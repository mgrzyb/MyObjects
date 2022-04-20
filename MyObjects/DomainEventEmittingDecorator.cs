using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects
{
    public class DomainEventEmittingDecorator
    {
        private readonly global::NHibernate.ISession session;
        private readonly IMediator mediator;

        protected DomainEventEmittingDecorator(global::NHibernate.ISession session, IMediator mediator)
        {
            this.session = session;
            this.mediator = mediator;
        }

        protected async Task EmitPendingDomainEvents()
        {
            var sessionImplementation = this.session.GetSessionImplementation();

            var aggregateRoots = sessionImplementation.PersistenceContext.EntityEntries.Keys
                .OfType<AggregateRoot>();
            
            foreach (var aggregateRoot in aggregateRoots.ToList())
            {
                while (aggregateRoot.TryDequeueDomainEvent(out var e))
                {
                    await this.mediator.Publish(e);
                }
            }
        }
    }
    
    public class DomainEventEmittingDecorator<TCommand, TResult> : DomainEventEmittingDecorator, IHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IHandler<TCommand, TResult> innerHandler;
        private readonly global::NHibernate.ISession session;
        private readonly IMediator mediator;

        public DomainEventEmittingDecorator(IHandler<TCommand, TResult> innerHandler,
            global::NHibernate.ISession session, IMediator mediator) : base(session, mediator)
        {
            this.innerHandler = innerHandler;
            this.session = session;
            this.mediator = mediator;
        }

        public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var result = await this.innerHandler.Handle(request, cancellationToken);

            await this.EmitPendingDomainEvents();

            return result;
        }
    }
    
    public class DomainEventEmittingDecorator<TEvent> : DomainEventEmittingDecorator, IDomainEventHandler<TEvent> where TEvent : INotification
    {
        private readonly INotificationHandler<TEvent> innerHandler;

        public DomainEventEmittingDecorator(INotificationHandler<TEvent> innerHandler,
            global::NHibernate.ISession session, IMediator mediator) : base(session, mediator)
        {
            this.innerHandler = innerHandler;
        }

        public async Task Handle(TEvent domainEvent, CancellationToken cancellationToken)
        {
            await this.innerHandler.Handle(domainEvent, cancellationToken);

            await this.EmitPendingDomainEvents();
        }
    }
}