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

        protected internal virtual void Publish(DomainEvent domainEvent)
        {
            this.domainEvents.Enqueue(domainEvent);
        }

        public virtual bool TryDequeueDomainEvent(out DomainEvent? domainEvent)
        {
            return this.domainEvents.TryDequeue(out domainEvent);
        }

        private class DomainEventQueue
        {
            private readonly Queue<DomainEvent> events = new Queue<DomainEvent>();

            public bool TryDequeue(out DomainEvent? item)
            {
                return this.events.TryDequeue(out item);
            }

            public void Enqueue(DomainEvent item)
            {
                this.events.Enqueue(item);
            }
        }
    }
}