using ConfirmationService.WebApi.Common.Extensions;
using ConfirmationService.WebApi.Interfaces;
using ConfirmationService.WebApi.Services;
using ExceptionHandler;
using MinorTaskService.WebApi.Services;
using Nirbi.ServiceAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.UsePostgreSql(builder.Configuration);
builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.RegisterExecutingAsseblyServices();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddConfirmationDomainEvents();
builder.Services.AddSingleton<IKafkaService, KafkaService>();

var app = builder.Build();

app.UseExceptionHandling();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
