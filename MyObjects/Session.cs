using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyObjects
{
    public interface IReadonlySession
    {
        Task<T> Resolve<T>(Reference<T> entityRef) where T : IEntity;
        Task<IReadOnlyList<T>> ResolveMany<T>(IEnumerable<Reference<T>> entityRefs) where T : IEntity;
        Task<T> TryResolve<T>(Reference<T> entityRef) where T : IEntity;
        IQueryable<T> Query<T>();
        void Clear();
    }

    public interface IReadonlySession<out TAdvanced> : IReadonlySession
    {
        TAdvanced Advanced { get; }
    }

    public interface ISession : IReadonlySession
    {
        Task<Reference<T>> Save<T>(T entity) where T : AggregateRoot;
        Task Delete<T>(T entity) where T : AggregateRoot;
    }

    public interface ISession<out TAdvanced> : ISession, IReadonlySession<TAdvanced>
    {
    }
}