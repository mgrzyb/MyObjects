using System.Reflection;
using Autofac;

namespace MyObjects.Infrastructure;

public class MyObjectsRegistration
{
    public readonly ContainerBuilder Builder;
    public readonly IEnumerable<Assembly> Assemblies;

    private readonly List<Type> entityTypes = new();
    public IEnumerable<Type> EntityTypes => this.entityTypes;
    
    public MyObjectsRegistration(ContainerBuilder builder, IEnumerable<Assembly> assemblies)
    {
        Builder = builder;
        Assemblies = assemblies;
    }

    public virtual MyObjectsRegistration AddEntityType<T>()
    {
        this.AddEntityType(typeof(T));
        return this;
    }

    public virtual MyObjectsRegistration AddEntityType(Type type)
    {
        this.entityTypes.Add(type);
        return this;
    }
}

public class MyObjectsRegistration<TAdvancedSession> : MyObjectsRegistration
{
    private readonly MyObjectsRegistration r;

    public MyObjectsRegistration(MyObjectsRegistration r) : base(r.Builder, r.Assemblies)
    {
        this.r = r;
    }
    
    public virtual MyObjectsRegistration AddEntityType(Type type)
    {
        this.r.AddEntityType(type);
        return this;
    }
}
