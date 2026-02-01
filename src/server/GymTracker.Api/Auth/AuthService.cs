using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using GymTracker.Api.Auth.Responses;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymTracker.Api.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _settings;
    private readonly RsaSecurityKey _rsaKey;
    private readonly IUserRepository _userRepository;

    public AuthService(IOptions<JwtSettings> options, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPem))
            throw new InvalidOperationException("JwtSettings.PrivateKeyPem must be configured (development only).");

        var rsa = RSA.Create();
        ImportPemPrivateKey(rsa, _settings.PrivateKeyPem);
        _rsaKey = new RsaSecurityKey(rsa);
    }

    public async Task<LoginResponse> GenerateTokenAsync(string username)
    {
        // For now, no validation; create token with sub, name, role
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiresInMinutes > 0 ? _settings.ExpiresInMinutes : 60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new("role", "User")
        };

        var creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.WriteToken(token);

        var resp = new LoginResponse(jwt, expires, claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value,
            "User");
        return resp;
    }

    public async Task<RegisterResponse> Register(string username, string password)
    {
        User newUser;
        try
        {
            var ph = new PasswordHasher<User>();
            newUser = new User
            {
                Username = username,
                PasswordHashed = ph.HashPassword(null!, password) // null! because it doesn't actually use the user object
            };
            await _userRepository.AddAsync(newUser);
        }
        catch (DbUpdateException e)
        {
            // Username unique constraint violation
            return new RegisterResponse(false, Error: RegisterError.UsernameTaken, ErrorMessage: "Username is already taken.");
        }
        catch (ArgumentException e)
        {
            // Invalid input validation
            return new RegisterResponse(false, Error: RegisterError.InvalidUsername, ErrorMessage: e.Message);
        }
        catch (Exception e)
        {
            // Unknown error
            return new RegisterResponse(false, Error: RegisterError.UnknownError, ErrorMessage: e.Message);
        }
        
        return new RegisterResponse(true, newUser.Id);
    }

    private static void ImportPemPrivateKey(RSA rsa, string pem)
    {
        const string pkcs1Header = "-----BEGIN RSA PRIVATE KEY-----";
        const string endRsaPrivateKey = "-----END RSA PRIVATE KEY-----";

        if (!pem.Contains(pkcs1Header)) throw new InvalidOperationException("Unsupported PEM format for private key.");
        
        var base64 = pem.Replace(pkcs1Header, string.Empty).Replace(endRsaPrivateKey, string.Empty)
            .Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        var bytes = Convert.FromBase64String(base64);
        rsa.ImportRSAPrivateKey(bytes, out _);
    }
}