using System;

namespace MyObjects
{
    public interface IEntity
    {
        int Id { get; }
        int Version { get; }
        Type GetEntityType();
    }
    
    public abstract class Entity : IEntity
    {
        private readonly int id = -1;
        public virtual int Id => this.id;

        public virtual int Version { get; protected set; }

        public virtual string CreatedBy { get; protected set; }
        public virtual string UpdatedBy { get; protected set; }
        public virtual DateTimeOffset CreatedOn { get; protected set; }
        public virtual DateTimeOffset UpdatedOn { get; protected set; }

        protected Entity()
        {
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as Entity);
        }

        private static bool IsTransient(Entity obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }

        public virtual bool Equals(Entity? other)
        {
            if (other == null) 
                return false;
            
            if (ReferenceEquals(this, other)) 
                return true;

            if (IsTransient(this) || IsTransient(other))
                return false;

            if (Equals(this.Id, other.Id) == false) 
                return false;
            
            var otherType = other.GetEntityType();
            var thisType = this.GetEntityType();

            return thisType == otherType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetEntityType(), this.Id);
        }

        public override string ToString()
        {
            return $"{this.GetEntityType().Name}:{this.Id}";
        }

        public virtual Type GetEntityType()
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

        public static bool operator !=(Entity? left, object? right)
        {
            return !Equals(left, right);
        }
    }
}