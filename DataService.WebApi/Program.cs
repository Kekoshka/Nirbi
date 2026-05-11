using DataService.WebApi.Common.Extensions;
using ExceptionHandler;
using Microsoft.OpenApi;
using Nirbi.ServiceAuth.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddNirbiServiceAuth(builder.Configuration);

//builder.Services.AddDataServiceJwtAuthentication(builder.Configuration);
builder.Services.UsePostgreSql(builder.Configuration);
builder.Services.AddS3ObjectStorage(builder.Configuration);
builder.Services.AddDataServices();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
var app = builder.Build();
//app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
