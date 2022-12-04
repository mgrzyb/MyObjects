using System.Collections.Generic;
using System.Linq;

namespace MyObjects
{
    public static class EntityExtensions
    {
        public static Reference<T> GetReference<T>(this T entity) where T : IEntity
        {
            return new Reference<T>(entity.Id);
        }
        
        public static IEnumerable<Reference<T>> AsReferences<T>(this IEnumerable<T> entities) 
            where T : IEntity
        {
            return entities.Select(GetReference);
        }

        public static IQueryable<Reference<TEntity>> AsReferenceList<TEntity>(this IQueryable<TEntity> entities)
            where TEntity : IEntity
        {
            return entities.Select(Reference<TEntity>.FromEntity);
        }
    }
}