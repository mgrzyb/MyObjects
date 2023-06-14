using System;

namespace MyObjects.Infrastructure;

public static class AggregateUpdated
{
    public static IDomainEvent From(Type type, AggregateRoot root)
    {
        return (IDomainEvent)Activator.CreateInstance(typeof(AggregateUpdated<>).MakeGenericType(type), root);
    }
}

public class AggregateUpdated<T> : IDomainEvent
{
    public T Root { get; }
    
    public AggregateUpdated(T root)
    {
        this.Root = root;
    }
}