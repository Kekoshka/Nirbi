using System.Reflection;
using MinorTaskService.WebApi.Common.Extensions;
using MinorTaskService.WebApi.Common.Options;
using MinorTaskService.WebApi.Interfaces;
using MinorTaskService.WebApi.Services;
using Nirbi.ServiceAuth.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ExternalServicesOptions>(
    builder.Configuration.GetSection(nameof(ExternalServicesOptions)));
builder.Services.Configure<DefaultDataOptions>(
    builder.Configuration.GetSection(nameof(DefaultDataOptions)));

builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.AddRefit(builder.Configuration);

builder.Services.UsePostgreSql(builder.Configuration);
builder.Services.AddMinorTaskDomainEvents();

builder.Services.AddScoped<IMinorTaskService, global::MinorTaskService.WebApi.Services.MinorTaskService>();
builder.Services.AddScoped<ITaskParticipantService, TaskParticipantService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IKafkaService, KafkaService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

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
