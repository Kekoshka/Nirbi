using GatewayService.WebApi.Services;
using Microsoft.OpenApi.Models;
using Yarp.ReverseProxy.Transforms;
using Nirbi.ServiceAuth.Extensions;

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