using System;

namespace MyObjects
{
    public interface IEntity
    {
        int Id { get; }
        int Version { get; }
    }
    
    public abstract class Entity : IEntity
    {
        private readonly int id;
        public virtual int Id => this.id;

        public virtual int Version { get; protected set; }

        public virtual string CreatedBy { get; protected set; }
        public virtual string UpdatedBy { get; protected set; }
        public virtual DateTimeOffset CreatedOn { get; protected set; }
        public virtual DateTimeOffset UpdatedOn { get; protected set; }

        protected Entity()
        {
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Entity);
        }

        private static bool IsTransient(Entity obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }

        public virtual bool Equals(Entity other)
        {
            if (other == null) 
                return false;
            
            if (ReferenceEquals(this, other)) 
                return true;

            if (IsTransient(this) || IsTransient(other))
                return false;

            if (Equals(this.Id, other.Id) == false) 
                return false;
            
            var otherType = other.GetUnproxiedType();
            var thisType = this.GetUnproxiedType();

            return thisType == otherType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetUnproxiedType(), this.Id);
        }

        public override string ToString()
        {
            return $"{this.GetUnproxiedType().Name}:{this.Id}";
        }

        protected virtual Type GetUnproxiedType()
        {
            //
            // This method looks like an equivalent of GetType() but since it is protected virtual,
            // when called on a proxy it is getting forwarded to a "real" entity thus making it return
            // a correct entity type.
            //
            return this.GetType();
        }

        public static bool operator ==(Entity left, object right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity left, object right)
        {
            return !Equals(left, right);
        }
    }
}