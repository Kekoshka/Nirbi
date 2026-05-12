using GatewayService.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("DataService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:DataService"]!));
builder.Services.AddHttpClient("MinorTaskService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:MinorTaskService"]!));
builder.Services.AddHttpClient("ConfirmationService", client =>
    client.BaseAddress = new Uri(builder.Configuration["Services:ConfirmationService"]!));

builder.Services.AddScoped<IMinorTaskAggregator, MinorTaskAggregator>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["*"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Nirbi API Gateway",
        Version = "v1",
        Description = "Единая точка входа для микросервисов платформы Nirbi"
    });

    // Поддержка Bearer токена в Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен. Пример: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        context.AddRequestTransform(async transformContext =>
        {
            var authHeader = transformContext.HttpContext.Request.Headers.Authorization;
            if (!string.IsNullOrEmpty(authHeader))
                transformContext.ProxyRequest.Headers
                    .TryAddWithoutValidation("Authorization", (string?)authHeader);
            await Task.CompletedTask;
        });
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Nirbi API Gateway v1");
        options.RoutePrefix = "swagger";
        // Показывать тело запроса/ответа по умолчанию
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

app.MapControllers();
app.MapReverseProxy();

app.Run();