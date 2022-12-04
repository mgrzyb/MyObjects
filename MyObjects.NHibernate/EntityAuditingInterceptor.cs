using System;
using NHibernate;
using NHibernate.Type;

namespace MyObjects.NHibernate
{
    public class EntityAuditingInterceptor : EmptyInterceptor
    {
        private readonly Func<string> currentUser;
        private readonly Func<DateTimeOffset> now;

        public EntityAuditingInterceptor(Func<string> currentUser, Func<DateTimeOffset> now)
        {
            this.currentUser = currentUser;
            this.now = now;
        }

        public override bool OnSave(object obj, object id, object[] state, string[] propertyNames, IType[] types)
        {            
            if (obj is Entity entity)
            {
                var time = this.now();
                var userName = this.currentUser();
                
                state[Array.IndexOf(propertyNames, nameof(Entity.CreatedBy))] = userName;
                state[Array.IndexOf(propertyNames,nameof(Entity.UpdatedBy))] = userName;
                state[Array.IndexOf(propertyNames,nameof(Entity.CreatedOn))] = time;
                state[Array.IndexOf(propertyNames,nameof(Entity.UpdatedOn))] = time;

                return true;
            }

            return base.OnSave(obj, id, state, propertyNames, types);
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames,
            IType[] types)
        {
            if (entity is Entity)
            {
                currentState[Array.IndexOf(propertyNames,nameof(Entity.UpdatedOn))] = this.now();
                currentState[Array.IndexOf(propertyNames,nameof(Entity.UpdatedBy))] = this.currentUser();
                return true;
            }
            
            return base.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);
        }
    }
}