using Autofac;
using Autofac.Core;
using Autofac.Extras.AggregateService;
using MediatR;

namespace MyObjects.Infrastructure.Setup;

public static class CommandsAndEventsSetupExtensions
{
    public static MyObjectsRegistration<TAdvancedSession> AddCommandHandlers<TAdvancedSession>(
        this MyObjectsRegistration<TAdvancedSession> myObjectsRegistration,
        Action<CommandHandlerRegistration>? configure = null)
    {
        myObjectsRegistration.Builder.RegisterAggregateService<CommandHandlerBase.IDependencies>();
        myObjectsRegistration.Builder.RegisterAggregateService<CommandHandlerWithAdvancedSession<TAdvancedSession>.IDependencies>();

        myObjectsRegistration.Builder.RegisterAssemblyTypes(myObjectsRegistration.Assemblies.ToArray())
            .Where(type => type.IsClosedTypeOf(typeof(ICommandHandler<,>)))
            .AsSelf()
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                .Select(i => new TypedService(i)));

        var registration = new CommandHandlerRegistration(myObjectsRegistration.Builder);
        configure?.Invoke(registration);
        registration.Complete();
        
        return myObjectsRegistration;
    }

    public static MyObjectsRegistration<TAdvancedSession> AddDomainEventHandlers<TAdvancedSession>(
        this MyObjectsRegistration<TAdvancedSession> myObjectsRegistration,
        Action<DomainEventHandlerRegistration>? configure = null)
    {
        myObjectsRegistration.Builder.RegisterAggregateService<DomainEventHandler.IDependencies>();
        myObjectsRegistration.Builder.RegisterAggregateService<DomainEventHandlerWithAdvancedSession<TAdvancedSession>.IDependencies>();

        myObjectsRegistration.Builder.RegisterAssemblyTypes(myObjectsRegistration.Assemblies.ToArray())
            .Where(t => t.IsClosedTypeOf(typeof(IDomainEventHandler<>)))
            .AsSelf()
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new TypedService(i)));

        configure?.Invoke(new DomainEventHandlerRegistration(myObjectsRegistration.Builder));

        return myObjectsRegistration;
    }
}