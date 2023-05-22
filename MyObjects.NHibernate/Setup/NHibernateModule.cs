using System.Reflection;
using Autofac;
using NHibernate;
using Module = Autofac.Module;

namespace MyObjects.NHibernate;

[Obsolete]
public class NHibernateModule : Module
{
    private readonly NHibernateConfigurationBuilder configurationBuilder;
    
    public NHibernateModule(IPersistenceStrategy persistenceStrategy, Assembly modelAssembly) : this(persistenceStrategy)
    {
        this.configurationBuilder.AddEntitiesFromAssembly(modelAssembly);
    }

    public NHibernateModule(IPersistenceStrategy persistenceStrategy, Action<NHibernateConfigurationBuilder> configure) : this(persistenceStrategy)
    {
        configure(this.configurationBuilder);
    }

    private NHibernateModule(IPersistenceStrategy persistenceStrategy)
    {
        this.configurationBuilder = new NHibernateConfigurationBuilder(persistenceStrategy);
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(c => this.configurationBuilder.Build().BuildSessionFactory())
            .SingleInstance();
        builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
            .InstancePerLifetimeScope();
        builder.Register(c => new NHibernateSession(c.Resolve<global::NHibernate.ISession>()))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        builder.RegisterType<SessionAggregateRootLocator>().AsImplementedInterfaces();
        builder.RegisterType<NHibernateTransactionRunner>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}