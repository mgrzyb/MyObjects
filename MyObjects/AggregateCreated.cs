namespace MyObjects.Infrastructure;

public class AggregateCreated<T> : IDomainEvent
{
    public T Root { get; }

    public AggregateCreated(T root)
    {
        this.Root = root;
    }
}