using System.Text;
using DataService.WebApi.Common.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DataService.WebApi.Common.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddDataServiceJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("NirbiServiceAuth").GetSection("ServiceJwt");
        var jwt = jwtSection.Get<ServiceJwtAuthOptions>() ?? new ServiceJwtAuthOptions();

        var keyBytes = Encoding.UTF8.GetBytes(jwt.SigningKey ?? string.Empty);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("NirbiServiceAuth:ServiceJwt:SigningKey must be at least 32 bytes (UTF-8).");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60),
                };
            });

        services.AddAuthorization();
        return services;
    }
}
