namespace GymTracker.Api.Auth;

public class JwtSettings
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    /// <summary>
    /// RSA private key in PEM format.
    /// </summary>
    public string? PrivateKeyPem { get; set; }

    /// <summary>
    /// Token lifetime in minutes.
    /// </summary>
    public int ExpiresInMinutes { get; set; } = 60;
}
