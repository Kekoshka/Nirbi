using GatewayService.WebApi.Services;
using Microsoft.OpenApi.Models;
using Yarp.ReverseProxy.Transforms;
using Nirbi.ServiceAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Configuration.GetSection("Services");
builder.Services.AddHttpClient(
    "ConfirmationService",
    c => c.BaseAddress = new Uri(services["ConfirmationService"]!));
builder.Services.AddHttpClient(
    "MinorTaskService",
    c => c.BaseAddress = new Uri(services["MinorTaskService"]!));
builder.Services.AddHttpClient(
    "DataService",
    c => c.BaseAddress = new Uri(services["DataService"]!));
builder.Services.AddNirbiAuthedHttpClient(
    "AuthService",
    c => c.BaseAddress = new Uri(services["AuthService"]!));

builder.Services.AddScoped<IMinorTaskAggregator, MinorTaskAggregator>();
builder.Services.AddScoped<IConfirmationsAggregator, ConfirmationsAggregator>();

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
builder.Services.AddSwaggerGen();

//Auth
builder.Services.AddHostedService<HostedBase>();
builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
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
        // ѕоказывать тело запроса/ответа по умолчанию
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelExpandDepth(2);
    });
}

app.MapControllers();
app.MapReverseProxy();

app.Run();