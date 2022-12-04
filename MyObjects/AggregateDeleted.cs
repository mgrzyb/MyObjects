namespace MyObjects.Infrastructure
{
    public class AggregateDeleted<T> : IDomainEvent
    {
        public T Root { get; }

        public AggregateDeleted(T root)
        {
            this.Root = root;
        }
    }
}