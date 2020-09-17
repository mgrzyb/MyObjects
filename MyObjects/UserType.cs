using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace MyObjects
{
    public abstract class UserType<T> : IUserType
    {
        public T Instance => default(T);
        
        public Type ReturnedType => typeof(T);
        
        public abstract bool Equals(object x, object y);
        public abstract int GetHashCode(object x);
        public abstract object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner);
        public abstract void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session);
        public abstract object DeepCopy(object value);
        public abstract object Replace(object original, object target, object owner);
        public abstract object Assemble(object cached, object owner);
        public abstract object Disassemble(object value);
        public abstract SqlType[] SqlTypes { get; }
        public abstract bool IsMutable { get; }
    }
}