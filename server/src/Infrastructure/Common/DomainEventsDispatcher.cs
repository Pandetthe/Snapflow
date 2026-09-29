using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snapflow.Common;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Snapflow.Infrastructure.Common;

internal sealed class DomainEventsDispatcher(
    IServiceProvider serviceProvider,
    ILogger<DomainEventsDispatcher> logger) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            Type domainEventType = domainEvent.GetType();
            Type handlerType = HandlerTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(IDomainEventHandler<>).MakeGenericType(et));

            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

            foreach (object? handler in handlers)
            {
                if (handler is null)
                {
                    continue;
                }

                await HandleGuardedAsync(handler, domainEvent, domainEventType, cancellationToken);
            }
        }
    }

    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "The change is already committed, so a failing side effect must not be "
                        + "reported to the caller as a failed operation.")]
    private async Task HandleGuardedAsync(
        object handler,
        IDomainEvent domainEvent,
        Type domainEventType,
        CancellationToken cancellationToken)
    {
        HandlerWrapper handlerWrapper = HandlerWrapper.Create(handler, domainEventType);

        try
        {
            await handlerWrapper.Handle(domainEvent, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "{Handler} failed to handle {DomainEvent}; the change it reacts to is already committed.",
                handler.GetType().Name,
                domainEventType.Name);
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            object instance = Activator.CreateInstance(wrapperType, handler)
                ?? throw new InvalidOperationException("Could not create handler wrapper instance.");
            return (HandlerWrapper)instance;
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

        public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            await _handler.Handle((T)domainEvent, cancellationToken);
        }
    }
}
