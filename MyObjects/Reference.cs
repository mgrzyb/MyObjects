using System;
using System.Linq.Expressions;
using MyObjects;

namespace MyObjects
{
    public interface IReference
    {
        int Id { get; }
        Type EntityType { get; }
    }

    public class Reference<TEntity> : IEquatable<Reference<TEntity>>, IReference where TEntity : IEntity
    {
        private int id;
        public int Id
        {
            get => this.entity != null ? this.entity.Id : this.id;
            private set { this.id = value; }
        }
        
        public Type EntityType => typeof(TEntity);

        private readonly TEntity? entity;

        // To make ToReference() working
        protected Reference()
        {
            
        }

        public Reference(int id)
        {
            this.id = id;
        }

        public Reference(TEntity entity)
        {
            this.entity = entity;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            if (this.entity != null)
                return $"[{this.entity.ToString()}]";
            
            return $"[{EntityType.Name}:{this.Id}]";
        }

        public static bool operator ==(Reference<TEntity>? ref1, Reference<TEntity>? ref2)
        {
            if (ReferenceEquals(ref1, ref2)) return true;
            if (ReferenceEquals(ref1, null)) return false;
            if (ReferenceEquals(ref2, null)) return false;
            return ref1.Id == ref2.Id;
        }

        public static bool operator !=(Reference<TEntity>? ref1, Reference<TEntity>? ref2)
        {
            return !(ref1 == ref2);
        }

        public bool Equals(Reference<TEntity>? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Reference<TEntity>) obj);
        }

        public static Expression<Func<TEntity, Reference<TEntity>>> FromEntity = (entity) =>
            new Reference<TEntity> {Id = entity.Id};
    }
}