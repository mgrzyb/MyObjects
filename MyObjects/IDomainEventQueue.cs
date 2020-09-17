namespace MyObjects
{
    public interface IDomainEventQueue
    {
        void Enqueue(DomainEvent domainEvent);
    }
}