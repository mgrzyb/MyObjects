using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyObjects.NHibernate;

namespace MyObjects
{
    public class EventEmittingSession<TAdvanced> : ISession<TAdvanced>
    {
        private readonly ISession<TAdvanced> session;

        public EventEmittingSession(ISession<TAdvanced> session)
        {
            this.session = session;
        }

        public Task<T> Resolve<T>(Reference<T> entityRef) where T : IEntity
        {
            return this.session.Resolve(entityRef);
        }

        public Task<IReadOnlyList<T>> ResolveMany<T>(IEnumerable<Reference<T>> entityRefs) where T : IEntity
        {
            return this.session.ResolveMany(entityRefs);
        }

        public Task<T> TryResolve<T>(Reference<T> entityRef) where T : IEntity
        {
            return this.session.TryResolve(entityRef);
        }

        public IQueryable<T> Query<T>()
        {
            return this.session.Query<T>();
        }

        public void Clear()
        {
            this.session.Clear();
        }

        public TAdvanced Advanced => this.session.Advanced;

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
    
    public class AggregateCreated<T> : DomainEvent
    {
        public T Root { get; }

        public AggregateCreated(T root)
        {
            this.Root = root;
        }
    }

    public class AggregateDeleted<T> : DomainEvent
    {
        public T Root { get; }

        public AggregateDeleted(T root)
        {
            this.Root = root;
        }
    }
}