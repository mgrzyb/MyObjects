using Autofac;
using MediatR;
using MyObjects.NHibernate;

namespace MyObjects.Infrastructure.Setup;

public class CommandHandlerRegistration
{
    public readonly ContainerBuilder Builder;

    private bool completed = false;

    public CommandHandlerRegistration(ContainerBuilder builder)
    {
        Builder = builder;
    }

    public CommandHandlerRegistration EmitDomainEvents()
    {
        this.Builder.RegisterGenericDecorator(
            typeof(DomainEventEmittingCommandHandlerDecorator<,>),
            typeof(ICommandHandler<,>));
        
        return this;
    }

    public CommandHandlerRegistration RunInTransaction()
    {
        this.Builder.RegisterGenericDecorator(
            typeof(TransactionalCommandHandlerDecorator<,>),
            typeof(ICommandHandler<,>));
        
        return this;
    }

    public CommandHandlerRegistration RunDurableTasks()
    {
        this.Builder.RegisterGenericDecorator(
            typeof(DurableTasksRunningDecorator<,>),
            typeof(ICommandHandler<,>));
        
        return this;
    }

    public RequestHandlerRegistration RunInLifetimeScope()
    {
        this.Builder.RegisterGeneric(typeof(LifetimeScopeCommandHandlerAdapter<,>)).AsImplementedInterfaces();

        return this.Complete(true);
    }
    
    public RequestHandlerRegistration Retry()
    {
        this.Builder.RegisterGeneric(typeof(CommandHandlerAdapter<,>)).AsImplementedInterfaces();        
        this.Builder.RegisterGenericDecorator(
            typeof(RetryRequestHandlerDecorator<,>),
            typeof(IRequestHandler<,>));

        return this.Complete(true);
    }

    internal RequestHandlerRegistration Complete(bool completedAlready = false)
    {
        if (this.completed || completedAlready)
        {
            this.completed = true;
            return new RequestHandlerRegistration(this.Builder);
        }

        this.Builder.RegisterGeneric(typeof(CommandHandlerAdapter<,>)).AsImplementedInterfaces();
        this.completed = true;
        return new RequestHandlerRegistration(this.Builder);
    }
}