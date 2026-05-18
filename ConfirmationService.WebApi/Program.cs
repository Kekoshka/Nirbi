using ConfirmationService.WebApi.Common.Extensions;
using ConfirmationService.WebApi.Interfaces;
using ConfirmationService.WebApi.Services;
using ExceptionHandler;
using MinorTaskService.WebApi.Services;
using Nirbi.ServiceAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHostedService<HostedBase>();
builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.UsePostgreSql(builder.Configuration);
builder.Services.ConfigureOptions(builder.Configuration);
builder.Services.RegisterExecutingAsseblyServices();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

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
