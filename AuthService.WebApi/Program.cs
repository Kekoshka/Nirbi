using AuthService.WebApi.Data.Extensions;
using AuthService.WebApi.Extensions;
using AuthService.WebApi.External.Keycloak;
using ExceptionHandler;
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddKeycloakIntegration(builder.Configuration);
builder.Services.AddAuthServiceDependencies(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseExceptionHandling();
app.UseHttpsRedirection();

// Apply migrations
await app.Services.ApplyMigrationsAsync();

app.MapControllers();
app.Run();

