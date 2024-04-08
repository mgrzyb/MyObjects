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

    public interface IVersionedReference : IReference
    {
        int Version { get; }        
    }

    public class ReferenceBase<TEntity> : IReference where TEntity : IEntity
    {
        private int id;
        public int Id
        {
            get => this.Entity != null ? this.Entity.Id : this.id;
            protected init { this.id = value; }
        }
        
        public Type EntityType => this.Entity != null ? this.Entity.GetEntityType() : typeof(TEntity);

        protected readonly TEntity? Entity;
        
        protected ReferenceBase()
        {
        }
        
        public ReferenceBase(int id)
        {
            this.id = id;
        }

        public ReferenceBase(TEntity entity)
        {
            this.Entity = entity;
        }
        
        public override int GetHashCode()
        {
            return this.Id;
        }
        
        protected bool Equals(IReference? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            if (other.EntityType.IsAssignableFrom(this.EntityType) || this.EntityType.IsAssignableFrom(other.EntityType))
                return this.Id == other.Id;

            return false;
        }
    }

    public class Reference<TEntity> : ReferenceBase<TEntity>, IEquatable<Reference<TEntity>> where TEntity : IEntity
    {
        // To make ToReference() working
        protected Reference()
        {
        }

        public Reference(int id) : base(id)
        {
        }

        public Reference(TEntity entity) : base(entity)
        {
        }
        
        public override bool Equals(object? obj)
        {
            return base.Equals(obj as IReference);
        }
        
        public bool Equals(Reference<TEntity>? other)
        {
            return base.Equals(other);
        }
        
        public override string ToString()
        {
            return $"[{EntityType.Name}:{this.Id}]";
        }
        
        public static Expression<Func<TEntity, Reference<TEntity>>> FromEntity = (entity) =>
            new Reference<TEntity> {Id = entity.Id};

        public VersionedReference<TEntity> WithVersion(int version)
        {
            return this.Entity != null ? new VersionedReference<TEntity>(this.Entity) : new VersionedReference<TEntity>(this.Id, version);
        }
        
        public static bool operator ==(Reference<TEntity>? ref1, Reference<TEntity>? ref2)
        {
            if (ReferenceEquals(ref1, ref2)) return true;
            return ref1?.Equals(ref2) == true;
        }

        public static bool operator !=(Reference<TEntity>? ref1, Reference<TEntity>? ref2)
        {
            return !(ref1 == ref2);
        }        
    }

    public class VersionedReference<TEntity> : ReferenceBase<TEntity>, IVersionedReference, IEquatable<VersionedReference<TEntity>> where TEntity : IEntity
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

        public override int GetHashCode()
        {
            return this.Id;
        }
        
        public bool Equals(VersionedReference<TEntity>? other)
        {
            return base.Equals(other) && other?.Version == this.Version;
        }
        
        public override bool Equals(object? obj)
        {
            var other = obj as IVersionedReference;
            return base.Equals(other) && other?.Version == this.Version;
        }
        
        public override string ToString()
        {
            return $"[{EntityType.Name}:{this.Id}({this.Version})]";
        }
        
        public static bool operator ==(VersionedReference<TEntity>? ref1, VersionedReference<TEntity>? ref2)
        {
            if (ReferenceEquals(ref1, ref2)) return true;
            return ref1?.Equals(ref2) == true;
        }

        public static bool operator !=(VersionedReference<TEntity>? ref1, VersionedReference<TEntity>? ref2)
        {
            return !(ref1 == ref2);
        }

        public static implicit operator Reference<TEntity>(VersionedReference<TEntity> versioned)
        {
            return versioned.WithoutVersion;
        }
    }
}