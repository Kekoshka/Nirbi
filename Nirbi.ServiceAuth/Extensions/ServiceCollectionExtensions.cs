using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nirbi.ServiceAuth.Authorization;
using Nirbi.ServiceAuth.Configuration;
using Nirbi.ServiceAuth.Http;
using Nirbi.ServiceAuth.Identity;

namespace Nirbi.ServiceAuth.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNirbiServiceAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        string configSection = ServiceAuthOptions.SectionName)
    {
        services.Configure<ServiceAuthOptions>(configuration.GetSection(configSection));

        var snapshot = configuration.GetSection(configSection).Get<ServiceAuthOptions>() ?? new ServiceAuthOptions();
        var useKeycloak = !string.IsNullOrWhiteSpace(snapshot.Keycloak?.Authority);

        services.AddHttpClient("Nirbi.ServiceAuth.Internal", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IServiceAccessTokenProvider, ServiceAccessTokenProvider>();
        services.AddTransient<ServiceAccessTokenDelegatingHandler>();

        var authenticationBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = useKeycloak ? "NirbiSmartJwt" : "NirbiServiceJwt";
            options.DefaultChallengeScheme = options.DefaultScheme;
        });

        if (useKeycloak)
        {
            authenticationBuilder.AddPolicyScheme("NirbiSmartJwt", "Nirbi smart JWT", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var token = GetBearerToken(context.Request);
                    if (string.IsNullOrEmpty(token))
                        return "NirbiServiceJwt";

                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        if (!handler.CanReadToken(token))
                            return "NirbiServiceJwt";

                        var jwt = handler.ReadJwtToken(token);
                        var iss = jwt.Issuer;
                        if (string.Equals(iss, snapshot.ServiceJwt.Issuer, StringComparison.Ordinal))
                            return "NirbiServiceJwt";

                        return "NirbiKeycloakJwt";
                    }
                    catch
                    {
                        return "NirbiServiceJwt";
                    }
                };
            });
        }

        authenticationBuilder.AddJwtBearer("NirbiServiceJwt", options =>
        {
            ConfigureServiceJwtBearer(options, snapshot);
        });

        if (useKeycloak)
        {
            authenticationBuilder.AddJwtBearer("NirbiKeycloakJwt", options =>
            {
                options.Authority = snapshot.Keycloak!.Authority;
                options.Audience = snapshot.Keycloak.Audience;
                options.RequireHttpsMetadata = snapshot.Keycloak.RequireHttpsMetadata;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.Headers["X-Auth-Error"] = context.Exception.Message;
                        Console.WriteLine($"JWT Auth failed: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("JWT Token validated successfully");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken)
                            && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, ServiceAuthPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, ScopeAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICallerContext, HttpCallerContext>();

        return services;
    }

    public static IHttpClientBuilder AddNirbiAuthedHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null)
    {
        return services.AddHttpClient(name, client => { configureClient?.Invoke(client); })
            .AddHttpMessageHandler<ServiceAccessTokenDelegatingHandler>();
    }

    private static void ConfigureServiceJwtBearer(JwtBearerOptions options, ServiceAuthOptions snapshot)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(snapshot.ServiceJwt.SigningKey ?? string.Empty);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("NirbiServiceAuth: ServiceJwt.SigningKey must be at least 32 bytes (UTF-8).");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = snapshot.ServiceJwt.Issuer,
            ValidateAudience = true,
            ValidAudience = snapshot.ServiceJwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(60)
        };

        options.MapInboundClaims = false;
    }

    private static string? GetBearerToken(HttpRequest request)
    {
        // 1. ����������� ��������� Authorization
        var header = request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(header)
            && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header["Bearer ".Length..].Trim();

        // 2. SignalR WebSocket/SSE � ����� � query string
        var queryToken = request.Query["access_token"].ToString();
        if (!string.IsNullOrEmpty(queryToken))
            return queryToken;

        return null;
    }
}
