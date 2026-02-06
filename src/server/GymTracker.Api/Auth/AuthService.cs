using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using GymTracker.Api.Endpoints.Responses.Results;
using GymTracker.Api.Endpoints.Responses.Structure;
using GymTracker.Core.Entities;
using GymTracker.Core.Interfaces;
using GymTracker.Core.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymTracker.Api.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _settings;
    private readonly RsaSecurityKey _rsaKey;
    private readonly IUserRepository _userRepository;
    private readonly IUserRegistrationService _userRegistrationService;

    public AuthService(
        IOptions<JwtSettings> options,
        IUserRepository userRepository,
        IUserRegistrationService userRegistrationService)
    {
        _userRepository = userRepository;
        _userRegistrationService = userRegistrationService;
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPem))
            throw new InvalidOperationException("JwtSettings.PrivateKeyPem must be configured (development only).");

        var rsa = RSA.Create();
        ImportPemPrivateKey(rsa, _settings.PrivateKeyPem);
        _rsaKey = new RsaSecurityKey(rsa);
    }

    public async Task<ApiResponse<LoginResult>> Login(string email, string password)
    {
        try
        {
            var result = await _userRepository.FindByEmailAsync(email);

            if (!result.IsSuccess) return ApiResponse<LoginResult>.Failure(ApiError.UserNotFound);
            
            var user = result.Data!;
            var ph = new PasswordHasher<User>();
            if (ph.VerifyHashedPassword(user, user.PasswordHashed, password) == PasswordVerificationResult.Failed)
            {
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidCredentials);
            }
            
            var loginResult = GenerateTokenAsync(user.Id, email);
            
            return ApiResponse<LoginResult>.Success(loginResult);
        }
        catch (Exception)
        {
            return ApiResponse<LoginResult>.Failure(ApiError.UnknownError);
        }
    }

    public async Task<ApiResponse<RegisterResult>> Register(string email, string password)
    {
        try
        {
            var ph = new PasswordHasher<User>();
            var newUser = new User
            {
                Email = email,
                PasswordHashed = ph.HashPassword(null!, password) // null! because it doesn't actually use the user object
            };
            
            var result = await _userRegistrationService.RegisterUserAsync(newUser);
            if (!result.IsSuccess)
            {
                return result.Status == DbResultStatus.DuplicateName 
                    ? ApiResponse<RegisterResult>.Failure(ApiError.EmailTaken)
                    : ApiResponse<RegisterResult>.Failure(ApiError.UnknownError);
            }
            
            return ApiResponse<RegisterResult>.Success(new RegisterResult(result.Data));
        }
        catch (ArgumentException)
        {
            // Invalid input validation
            return ApiResponse<RegisterResult>.Failure(ApiError.InvalidEmail);
        }
        catch (Exception)
        {
            // Unknown error
            return ApiResponse<RegisterResult>.Failure(ApiError.UnknownError);
        }
    }
    
    private LoginResult GenerateTokenAsync(Guid userId, string email)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiresInMinutes > 0 ? _settings.ExpiresInMinutes : 60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
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

        var resp = new LoginResult(jwt, expires, userId.ToString(), email);
        return resp;
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