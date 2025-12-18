using System;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace server.Common.Settings;

public class JwtConfig
{
    public string Secret { get; }
    public string Issuer { get; }
    public string Audience { get; }
    public int ExpiryHours { get; }
    public int RefreshExpiryHours { get; }
    public string AccessTokenCookieName { get; }

    public JwtConfig(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JwtSettings");

        Secret = jwtSection["SecretKey"] ?? throw new ArgumentNullException("Jwt:Secret is missing in appsettings.json");
        Issuer = jwtSection["Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer is missing in appsettings.json");
        Audience = jwtSection["Audience"] ?? throw new ArgumentNullException("Jwt:Audience is missing in appsettings.json");
        AccessTokenCookieName = jwtSection["AccessTokenCookieName"] ?? "access_token";

        ExpiryHours = int.TryParse(jwtSection["AccessTokenExpirationHours"], out int exp) ? exp : 1;
        RefreshExpiryHours = int.TryParse(jwtSection["RefreshTokenExpirationMonths"], out int refresh) ? refresh : 24;
    }
}

public static class AuthenticationConfig
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = new JwtConfig(configuration);

        services.AddSingleton(jwtConfig);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies[jwtConfig.AccessTokenCookieName];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireOwnerAdminRole", policy => policy.RequireRole("owner", "admin"));
            options.AddPolicy("RequireOwnerRole", policy => policy.RequireRole("owner"));
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"));
            options.AddPolicy("RequireUserRole", policy => policy.RequireRole("user"));
        });

        return services;
    }
}