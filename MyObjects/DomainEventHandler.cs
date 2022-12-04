using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects
{
    public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : INotification
    {
    }

    public abstract class DomainEventHandler
    {
        protected ISession Session { get; }
        protected IMediator Mediator { get; }
        protected DomainEventHandler(IDependencies dependencies)
        {
            this.Session = dependencies.Session;
            this.Mediator = dependencies.Mediator;
        }

        public interface IDependencies
        {
            ISession Session { get; }
            IMediator Mediator { get; }
        }
    }

    public abstract class DomainEventHandler<T> : DomainEventHandler, IDomainEventHandler<T> where T : INotification
    {
        protected DomainEventHandler(IDependencies dependencies) : base(dependencies)
        {
        }

        protected virtual bool CanHandle(T domainEvent)
        {
            return true;
        }

        public Task Handle(T domainEvent, CancellationToken cancellationToken)
        {
            if (this.CanHandle(domainEvent))
                return this.Handle(domainEvent);
            return Task.CompletedTask;
        }

        protected abstract Task Handle(T domainEvent);
    }
    
    public class DomainEventHandlerWithAdvancedSession<TAdvancedSession>
    {
        protected ISession<TAdvancedSession> Session { get; }
        protected IMediator Mediator { get; }

        protected DomainEventHandlerWithAdvancedSession(IDependencies dependencies)
        {
            this.Session = dependencies.Session;
            this.Mediator = dependencies.Mediator;
        }

        public interface IDependencies
        {
            ISession<TAdvancedSession> Session { get; }
            IMediator Mediator { get; }
        }
    }

    public abstract class DomainEventHandlerWithAdvancedSession<TAdvancedSession, T> : DomainEventHandlerWithAdvancedSession<TAdvancedSession>, IDomainEventHandler<T> where T : INotification
    {
        protected DomainEventHandlerWithAdvancedSession(IDependencies dependencies) : base(dependencies)
        {
        }

        protected virtual bool CanHandle(T domainEvent)
        {
            return true;
        }

        public Task Handle(T domainEvent, CancellationToken cancellationToken)
        {
            if (this.CanHandle(domainEvent))
                return this.Handle(domainEvent);
            return Task.CompletedTask;
        }

        protected abstract Task Handle(T domainEvent);
    }
}