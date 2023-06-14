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
            get => this.Entity != null ? this.Entity.Id : this.id;
            private init { this.id = value; }
        }
        
        public Type EntityType => this.Entity != null ? this.Entity.GetEntityType() : typeof(TEntity);

        protected readonly TEntity? Entity;

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
            this.Entity = entity;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override string ToString()
        {
            if (this.Entity != null)
                return $"[{this.Entity.ToString()}]";
            
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
            
            if (obj is IReference other)
            {
                if (other.EntityType.IsAssignableFrom(this.EntityType) || this.EntityType.IsAssignableFrom(other.EntityType))
                    return this.Id == other.Id;
            }

            return false;
        }

        public static Expression<Func<TEntity, Reference<TEntity>>> FromEntity = (entity) =>
            new Reference<TEntity> {Id = entity.Id};

        public VersionedReference<TEntity> WithVersion(int version)
        {
            return this.Entity != null ? new VersionedReference<TEntity>(this.Entity) : new VersionedReference<TEntity>(this.id, version);
        }
    }

    public class VersionedReference<TEntity> : Reference<TEntity> where TEntity : IEntity
    {
        private readonly int version;
        public int Version => this.Entity != null ? this.Entity.Version : this.version;
        
        public Reference<TEntity> WithoutVersion => this.Entity != null ? new Reference<TEntity>(this.Entity) : new Reference<TEntity>(this.Id);

        public VersionedReference(TEntity entity) : base(entity)
        {
        }
        
        public VersionedReference(int id, int version) : base(id)
        {
            this.version = version;
        }

    }
}