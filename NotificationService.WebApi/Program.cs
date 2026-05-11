using ExceptionHandler;
using NotificationService.WebApi.Common.Extensions;
using NotificationService.WebApi.Common.Hubs;
using NotificationService.WebApi.Common.Options;
using Nirbi.ServiceAuth.Extensions;
using NotificationService.WebApi.Services.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddJWTAuthentication();
builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.AddSchemaRegistryClient(builder.Configuration);
builder.Services.RegisterExecutingAsseblyServices();
builder.Services.RegisterMappers();
builder.Services.AddRefit(builder.Configuration);
var externalServicesOptions = builder.Configuration
    .GetSection(nameof(ExternalServicesOptions))
    .Get<ExternalServicesOptions>()!;

builder.Services.AddKafkaConsumers(externalServicesOptions);


var app = builder.Build();

app.UseExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
