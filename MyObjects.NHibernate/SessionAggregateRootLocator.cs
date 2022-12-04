namespace MyObjects.NHibernate;

public class SessionAggregateRootLocator : ISessionAggregateRootLocator
{
    private readonly global::NHibernate.ISession session;

    public SessionAggregateRootLocator(global::NHibernate.ISession session)
    {
        this.session = session;
    }

    public IEnumerable<AggregateRoot> GetAggregateRoots()
    {
        var sessionImplementation = this.session.GetSessionImplementation();

        var aggregateRoots = sessionImplementation.PersistenceContext.EntityEntries.Keys
            .OfType<AggregateRoot>();
        return aggregateRoots;
    }
}