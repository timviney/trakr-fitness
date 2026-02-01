﻿using System.Security.Cryptography;
using GymTracker.Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
        
        var envPublicKey = Environment.GetEnvironmentVariable("TRAKR_PUBLIC_KEY_PEM");
        if (!string.IsNullOrWhiteSpace(envPublicKey))
        {
            webApplicationBuilder.Services.PostConfigure<JwtSettings>(opts => opts.PublicKeyPem = envPublicKey);
        }
        else throw new Exception("TRAKR_PUBLIC_KEY_PEM environment variable is not set." +
                                 " If running locally, please create a key with 'https://cryptotools.net/rsagen' and set " +
                                 "$env:TRAKR_PRIVATE_KEY_PEM and $env:TRAKR_PUBLIC_KEY_PEM for the user.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(envPublicKey);
        var rsaKey = new RsaSecurityKey(rsa);

        webApplicationBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = rsaKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        webApplicationBuilder.Services.AddAuthorization();
        webApplicationBuilder.Services.AddHttpContextAccessor();
        webApplicationBuilder.Services.AddScoped<IAuthService, AuthService>();
        webApplicationBuilder.Services.AddScoped<IAuthContext, AuthContext>();
    }
}