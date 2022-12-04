namespace MyObjects;

public interface ISessionAggregateRootLocator
{
    IEnumerable<AggregateRoot> GetAggregateRoots();
}