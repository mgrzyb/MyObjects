using System.Threading.Tasks;

namespace MyObjects.Infrastructure;

public partial class EventEmittingSessionDecorator : ISession
{
    private readonly ISession session;

    public EventEmittingSessionDecorator(ISession session)
    {
        this.session = session;
    }

    public Task<Reference<T>> Save<T>(T entity) where T : AggregateRoot
    {
        entity.Publish(new AggregateCreated<T>(entity));
        return this.session.Save(entity);
    }

    public Task Delete<T>(T entity) where T : AggregateRoot
    {
        entity.Publish(new AggregateDeleted<T>(entity));
        return this.session.Delete(entity);
    }
}

public class EventEmittingSessionDecorator<TAdvanced> : EventEmittingSessionDecorator, ISession<TAdvanced>
{
    private readonly ISession<TAdvanced> session;

    public EventEmittingSessionDecorator(ISession<TAdvanced> session) : base(session)
    {
        this.session = session;
    }

    public TAdvanced Advanced => this.session.Advanced;
}