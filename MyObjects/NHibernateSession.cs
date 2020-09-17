using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace MyObjects
{
    public class NHibernateSession : ISession
    {
        private readonly NHibernate.ISession session;

        public NHibernateSession(NHibernate.ISession session)
        {
            this.session = session;
        }

        public async Task<Reference<T>> Save<T>(T entity) where T : AggregateRoot
        {
            await this.session.SaveAsync(entity);
            return entity.GetReference();
        }

        public Task Delete<T>(T entity) where T : AggregateRoot
        {
            return this.session.DeleteAsync(entity);
        }

        public async Task<T> Resolve<T>(Reference<T> entityRef) where T : IEntity
        {
            var entity = await this.session.GetAsync<T>(entityRef.Id);
            if (entity == null)
                throw new InvalidReferenceException(entityRef);

            return entity;
        }

        public async Task<IReadOnlyList<T>> ResolveMany<T>(IEnumerable<Reference<T>> entityRefs) where T : IEntity
        {
            var ids = entityRefs.Select(r => r.Id).ToArray();
            var result = await this.session.Query<T>()
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();
            
            if (result.Count != entityRefs.Count())
                throw new InvalidReferenceException(entityRefs.First(r => result.Any(x => x.Id == r.Id) == false));
            
            return result;
        }

        public async Task<T> TryResolve<T>(Reference<T> entityRef) where T : IEntity
        {
            return await this.session.GetAsync<T>(entityRef.Id);
        }

        public IQueryable<T> Query<T>()
        {
            return this.session.Query<T>();
        }

        public void Clear()
        {
            this.session.Clear();
        }

        public NHibernate.ISession Advanced => this.session;
    }
}