using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GymTracker.Api.Auth;

public class AuthService : IAuthService
{
    private readonly JwtSettings _settings;
    private readonly RsaSecurityKey _rsaKey;

    public AuthService(IOptions<JwtSettings> options)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPem))
            throw new InvalidOperationException("JwtSettings.PrivateKeyPem must be configured (development only).");

        var rsa = RSA.Create();
        ImportPemPrivateKey(rsa, _settings.PrivateKeyPem);
        _rsaKey = new RsaSecurityKey(rsa);
    }

    public Task<AuthResponse> GenerateTokenAsync(string username)
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

        var resp = new AuthResponse(jwt, expires, claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value,
            "User");
        return Task.FromResult(resp);
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