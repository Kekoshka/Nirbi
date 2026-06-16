using Microsoft.Extensions.DependencyInjection;
using MinorTaskService.DataAccess.Postgres.DomainEvents;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Interfaces;
using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using MinorTaskService.WebApi.DomainEvents.Handlers;
using MinorTaskService.WebApi.Services.BackgroundServices;

namespace MinorTaskService.WebApi.Common.Extensions;

public static class DomainEventsExtensions
{
    public static IServiceCollection AddMinorTaskDomainEvents(this IServiceCollection services)
    {
        services.AddHostedService<ConfirmationRespondConsumer>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<MinorTaskCreatedEvent>, MinorTaskCreatedEventHandler>();
        services.AddScoped<IDomainEventHandler<MinorTaskUpdatedEvent>, MinorTaskUpdatedEventHandler>();
        services.AddScoped<IDomainEventHandler<MinorTaskDeletedEvent>, MinorTaskDeletedEventHandler>();
        services.AddScoped<IDomainEventHandler<MinorTaskStatusUpdatedEvent>, MinorTaskStatusUpdatedEventHandler>();
        services.AddScoped<IDomainEventHandler<TaskParticipantAddedEvent>, TaskParticipantAddedEventHandler>();
        services.AddScoped<IDomainEventHandler<TaskParticipantRemovedEvent>, TaskParticipantRemovedEventHandler>();
        return services;
    }
}
