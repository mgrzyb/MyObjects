using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace MyObjects.Testing.NHibernate
{
    public class SqliteDateTimeOffsetType : global::NHibernate.UserTypes.IUserType
    {
        bool global::NHibernate.UserTypes.IUserType.Equals(object x, object y)
        {
            return object.Equals(x, y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var obj = NHibernateUtil.UtcDateTimeNoMs.NullSafeGet(rs, names[0], session);
            if (obj == null)
                return null;

            return new DateTimeOffset(DateTime.SpecifyKind((DateTime)obj, DateTimeKind.Local).ToUniversalTime(), TimeSpan.Zero);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if(value == null)
            {
                NHibernateUtil.UtcDateTimeNoMs.NullSafeSet(cmd, null, index, session);
            }
            else
            {
                var dateTimeOffset = (DateTimeOffset) value;
                NHibernateUtil.UtcDateTimeNoMs.NullSafeSet(cmd, dateTimeOffset.UtcDateTime, index, session);
            }  
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            throw new NotImplementedException();
        }

        public object Assemble(object cached, object owner)
        {
            throw new NotImplementedException();
        }

        public object Disassemble(object value)
        {
            throw new NotImplementedException();
        }

        public SqlType[] SqlTypes => new[] {SqlTypeFactory.DateTime};
        public Type ReturnedType => typeof(DateTimeOffset);
        public bool IsMutable => false;
    }
}