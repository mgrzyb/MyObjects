using System.Threading.Tasks;

namespace MyObjects.Infrastructure;

public partial class EventEmittingSessionDecorator : ISession
{
    private readonly ISession session;

    public EventEmittingSessionDecorator(ISession session)
    {
        this.session = session;
    }

    public Task<VersionedReference<T>> Save<T>(T entity) where T : AggregateRoot
    {
        var save = this.session.Save(entity);
        entity.Publish(new AggregateCreated<T>(entity));
        return save;
    }

    public Task Delete<T>(T entity) where T : AggregateRoot
    {
        var delete = this.session.Delete(entity);
        entity.Publish(new AggregateDeleted<T>(entity));
        return delete;
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