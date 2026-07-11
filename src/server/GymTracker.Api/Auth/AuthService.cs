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
            Tidy(ref email);

            var result = await _userRepository.FindByEmailAsync(email);

            if (!result.IsSuccess) return ApiResponse<LoginResult>.Failure(ApiError.UserNotFound);

            var user = result.Data!;
            var ph = new PasswordHasher<User>();
            if (ph.VerifyHashedPassword(user, user.PasswordHashed, password) == PasswordVerificationResult.Failed)
            {
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidCredentials);
            }

            var loginResult = await GenerateTokensAsync(user);

            return ApiResponse<LoginResult>.Success(loginResult);
        }
        catch (Exception)
        {
            return ApiResponse<LoginResult>.Failure(ApiError.UnknownError);
        }
    }

    public async Task<ApiResponse<LoginResult>> RefreshToken(string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            var parts = refreshToken.Split(':', 2);
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out var userId))
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            var randomPart = parts[1];

            var userResult = await _userRepository.GetByIdAsync(userId);
            if (!userResult.IsSuccess)
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            var user = userResult.Data!;

            if (string.IsNullOrWhiteSpace(user.RefreshTokenHash) || user.RefreshTokenExpiresAt == null)
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            var randomPartHash = HashRandomPart(randomPart);
            var storedHash = Convert.FromBase64String(user.RefreshTokenHash);
            if (!CryptographicOperations.FixedTimeEquals(randomPartHash, storedHash))
                return ApiResponse<LoginResult>.Failure(ApiError.InvalidRefreshToken);

            if (string.IsNullOrWhiteSpace(user.Email))
                return ApiResponse<LoginResult>.Failure(ApiError.UnknownError);

            var loginResult = await GenerateTokensAsync(user);

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
            Tidy(ref email);

            var ph = new PasswordHasher<User>();
            var newUser = new User
            {
                Email = email,
                PasswordHashed =
                    ph.HashPassword(null!, password) // null! because it doesn't actually use the user object
            };

            var result = await _userRegistrationService.RegisterUserAsync(newUser);
            if (!result.IsSuccess)
            {
                return result.Status == DbResultStatus.DuplicateName
                    ? ApiResponse<RegisterResult>.Failure(ApiError.EmailTaken)
                    : ApiResponse<RegisterResult>.Failure(ApiError.UnknownError);
            }

            return ApiResponse<RegisterResult>.Success(new RegisterResult(result.Data.Id));
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

    public async Task<ApiResponse<ResetPasswordResult>> ResetPassword(string email, string oldPassword,
        string newPassword)
    {
        try
        {
            Tidy(ref email);

            var result = await _userRepository.FindByEmailAsync(email);

            if (!result.IsSuccess) return ApiResponse<ResetPasswordResult>.Failure(ApiError.UserNotFound);

            var user = result.Data!;
            var ph = new PasswordHasher<User>();
            if (ph.VerifyHashedPassword(user, user.PasswordHashed, oldPassword) == PasswordVerificationResult.Failed)
            {
                return ApiResponse<ResetPasswordResult>.Failure(ApiError.InvalidCredentials);
            }

            user.PasswordHashed = ph.HashPassword(user, newPassword);
            var updateResult = await _userRepository.UpdateAsync(user);
            return !updateResult.IsSuccess
                ? ApiResponse<ResetPasswordResult>.Failure(ApiError.UnknownError)
                : ApiResponse<ResetPasswordResult>.Success(new ResetPasswordResult(result.Data.Id));
        }
        catch (ArgumentException)
        {
            return ApiResponse<ResetPasswordResult>.Failure(ApiError.InvalidEmail);
        }
        catch (Exception)
        {
            return ApiResponse<ResetPasswordResult>.Failure(ApiError.UnknownError);
        }
    }

    private async Task<LoginResult> GenerateTokensAsync(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiresInMinutes > 0 ? _settings.ExpiresInMinutes : 15);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
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

        var refreshTokenRandom = GenerateRandomToken();
        var refreshTokenHash = HashRandomPart(refreshTokenRandom);
        var refreshTokenExpiresAt = now.AddDays(_settings.RefreshTokenExpiryDays > 0 ? _settings.RefreshTokenExpiryDays : 30);

        user.RefreshTokenHash = Convert.ToBase64String(refreshTokenHash);
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await _userRepository.UpdateAsync(user);

        var refreshToken = $"{user.Id}:{refreshTokenRandom}";

        return new LoginResult(jwt, expires, user.Id.ToString(), user.Email!, refreshToken, refreshTokenExpiresAt);
    }

    private static string GenerateRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[] HashRandomPart(string randomPart)
    {
        return SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(randomPart));
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

    private static void Tidy(ref string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        email = email.ToLower().Trim();

        // Validate Email format using a standard regex pattern
        if (!System.Text.RegularExpressions.Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            throw new ArgumentException("Invalid Email format.", nameof(email));
        }

        // Check for potentially dangerous characters that shouldn't be in valid emails
        if (System.Text.RegularExpressions.Regex.IsMatch(email, @"[;""\\<>(){}|\[\]]"))
        {
            throw new ArgumentException("Email contains invalid characters.", nameof(email));
        }

        // Defense-in-depth: escape apostrophes (even though EF Core parameterizes queries)
        email = email.Replace("'", "''");
    }
}