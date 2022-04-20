using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyObjects.NHibernate;

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

    public interface IReadonlySession<TAdvanced> : IReadonlySession
    {
        TAdvanced Advanced { get; }
    }
    
    public interface ISession<TAdvanced> : IReadonlySession<TAdvanced>
    {
        Task<Reference<T>> Save<T>(T entity) where T : AggregateRoot;
        Task Delete<T>(T entity) where T : AggregateRoot;
    }
    
    public static class SessionExtensions
    {
        public static async Task<(T1, T2, T3)> Resolve<T1, T2, T3>(this ISession session, Reference<T1> r1, Reference<T2> r2,
            Reference<T3> r3) where T3 : IEntity where T1 : IEntity where T2 : IEntity
        {
            return (await session.Resolve(r1), await session.Resolve(r2), await session.Resolve(r3));
        }

        public static Task<T> Resolve<T>(this IReadonlySession session, int id) where T : IEntity
        {
            return session.Resolve(new Reference<T>(id));
        }
    }
}