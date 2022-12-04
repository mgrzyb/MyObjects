using System.Collections.Generic;

namespace MyObjects
{
    public abstract class AggregateRoot : Entity
    {
        private readonly DomainEventQueue domainEvents;
        
        protected AggregateRoot()
        {
            this.domainEvents = new DomainEventQueue();
        }

        protected internal virtual void Publish(IDomainEvent domainEvent)
        {
            this.domainEvents.Enqueue(domainEvent);
        }

        public virtual bool TryDequeueDomainEvent(out IDomainEvent? domainEvent)
        {
            return this.domainEvents.TryDequeue(out domainEvent);
        }

        private class DomainEventQueue
        {
            private readonly Queue<IDomainEvent> events = new Queue<IDomainEvent>();

            public bool TryDequeue(out IDomainEvent? item)
            {
                return this.events.TryDequeue(out item);
            }

            public void Enqueue(IDomainEvent item)
            {
                this.events.Enqueue(item);
            }
        }
    }
}