using MediatR;

namespace MyObjects
{
    public class DomainEventEmittingCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> innerHandler;
        private readonly IDomainEventBus eventBus;
        private readonly ISessionAggregateRootLocator aggregateRootLocator;

        public DomainEventEmittingCommandHandlerDecorator(ICommandHandler<TCommand, TResult> innerHandler,
            IDomainEventBus eventBus, ISessionAggregateRootLocator aggregateRootLocator)
        {
            this.innerHandler = innerHandler;
            this.eventBus = eventBus;
            this.aggregateRootLocator = aggregateRootLocator;
        }

        public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var result = await this.innerHandler.Handle(request, cancellationToken);

            await this.PublishPendingDomainEvents();

            return result;
        }

        private async Task PublishPendingDomainEvents()
        {
            bool moreEvents;
            do
            {
                moreEvents = false;
                var aggregateRoots = this.aggregateRootLocator.GetAggregateRoots();


                foreach (var aggregateRoot in aggregateRoots.ToList())
                {
                    while (aggregateRoot.TryDequeueDomainEvent(out var e))
                    {
                        moreEvents = true;
                        await this.eventBus.Publish(e);
                    }
                }
            } while (moreEvents);
        }
    }
}