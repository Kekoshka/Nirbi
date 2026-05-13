using ConfirmationService.WebApi.Interfaces;
using ConfirmationService.WebApi.Services;
using ExceptionHandler;
using MinorTaskService.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddScoped<IConfirmationService, ConfirmationService.WebApi.Services.ConfirmationService>();
//builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();
//builder.Services.AddScoped<IKafkaService, KafkaService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//app.UseExceptionHandling();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
