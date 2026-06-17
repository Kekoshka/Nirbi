using CommunicationService.DataAccess.Postgres.Context;
using CommunicationService.WebApi.Common.DataSeed;
using CommunicationService.WebApi.Common.Extensions;
using CommunicationService.WebApi.Common.Options;
using CommunicationService.WebApi.Interfaces;
using CommunicationService.WebApi.Services;
using ExceptionHandler;
using Microsoft.EntityFrameworkCore;
using Nirbi.ServiceAuth.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<ExternalServicesOptions>(
    builder.Configuration.GetSection(nameof(ExternalServicesOptions)));
builder.Services.Configure<KafkaConsumersOptions>(
    builder.Configuration.GetSection(nameof(KafkaConsumersOptions)));


builder.Services.AddSchemaRegistryClient(builder.Configuration);
builder.Services.AddHostedService<HostedBase>();
builder.Services.AddNirbiServiceAuth(builder.Configuration);
builder.Services.AddRefit(builder.Configuration);

builder.Services.UsePostgreSql(builder.Configuration);
builder.Services.AddDomainEvents();

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatUserService, ChatUserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IKafkaService, KafkaService>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!await db.ChatTypes.AnyAsync())
    {
        await db.ChatTypes.AddRangeAsync(ChatTypesSeed.ChatTypes);
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandling();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
