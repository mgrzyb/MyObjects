using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NHibernate;

namespace MyObjects
{
    public interface IDomainEventHandler<in T> : INotificationHandler<T> where T : INotification
    {
    }
    
    public class DomainEventHandler
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
}