using GymTracker.Api.Auth;

namespace GymTracker.Api.Setup;

public static class AuthSetup
{

    public static void ConfigureAuth(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.Configure<JwtSettings>(webApplicationBuilder.Configuration.GetSection("JwtSettings"));
        var envPrivateKey = Environment.GetEnvironmentVariable("TRAKR_PRIVATE_KEY_PEM");
        if (!string.IsNullOrWhiteSpace(envPrivateKey))
        {
            webApplicationBuilder.Services.PostConfigure<JwtSettings>(opts => opts.PrivateKeyPem = envPrivateKey);
        }
        else throw new Exception("TRAKR_PRIVATE_KEY_PEM environment variable is not set." +
                                 " If running locally, please create a key with 'https://cryptotools.net/rsagen' and set " +
                                 "$env:TRAKR_PRIVATE_KEY_PEM and $env:TRAKR_PUBLIC_KEY_PEM for the user.");
        webApplicationBuilder.Services.AddScoped<IAuthService, AuthService>();
    }
}