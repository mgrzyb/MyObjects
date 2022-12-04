using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extras.AggregateService;
using MediatR;
using MyObjects.NHibernate;
using Module = Autofac.Module;

namespace MyObjects.Infrastructure;

public class CommandsAndEventsModule<TAdvancedSession> : Module
{
    private readonly Assembly modelAssembly;
    private readonly bool emitEvents;
    private readonly bool scopeCommands;
    private readonly bool runCommandsInTransaction;
    private readonly bool retryCommands;

    public CommandsAndEventsModule(Assembly modelAssembly, bool emitEvents, bool scopeCommands, bool runCommandsInTransaction, bool retryCommands)
    {
        this.modelAssembly = modelAssembly;
        this.emitEvents = emitEvents;
        this.scopeCommands = scopeCommands;
        this.runCommandsInTransaction = runCommandsInTransaction;
        this.retryCommands = retryCommands;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterDecorator<EventEmittingSessionDecorator, ISession>();
        builder.RegisterGenericDecorator(typeof(EventEmittingSessionDecorator<>), typeof(ISession<>));
        
        builder.RegisterAggregateService<CommandHandlerBase.IDependencies>();
        builder.RegisterAggregateService<CommandHandlerWithAdvancedSession<TAdvancedSession>.IDependencies>();

        builder.RegisterAssemblyTypes(this.modelAssembly)
            .Where(type => type.IsClosedTypeOf(typeof(ICommandHandler<,>)))
            .AsSelf()
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                .Select(i => new TypedService(i)));

        builder.RegisterAggregateService<DomainEventHandler.IDependencies>();
        builder.RegisterAggregateService<DomainEventHandlerWithAdvancedSession<TAdvancedSession>.IDependencies>();

        builder.RegisterAssemblyTypes(this.modelAssembly)
            .Where(t => t.IsClosedTypeOf(typeof(IDomainEventHandler<>)))
            .AsSelf()
            .As(type => type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new TypedService(i)));

        if (this.emitEvents)
        {
            builder.RegisterGenericDecorator(
                typeof(DomainEventEmittingCommandHandlerDecorator<,>),
                typeof(ICommandHandler<,>));
        }

        if (this.runCommandsInTransaction)
        {
            builder.RegisterGenericDecorator(
                typeof(TransactionalCommandHandlerDecorator<,>),
                typeof(ICommandHandler<,>));
        }

        if (this.scopeCommands)
            builder.RegisterGeneric(typeof(LifetimeScopeCommandHandlerAdapter<,>)).AsImplementedInterfaces();
        else
            builder.RegisterGeneric(typeof(CommandHandlerAdapter<,>)).AsImplementedInterfaces();

        if (this.retryCommands)
            builder.RegisterGenericDecorator(
                typeof(RetryRequestHandlerDecorator<,>),
                typeof(IRequestHandler<,>));
    }
}