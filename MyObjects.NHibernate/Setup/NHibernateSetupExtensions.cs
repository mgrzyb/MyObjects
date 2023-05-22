using Autofac;
using MyObjects.Infrastructure;
using NHibernate;

namespace MyObjects.NHibernate;

public static class NHibernateSetupExtensions
{
    public static MyObjectsRegistration<global::NHibernate.ISession> UseNHibernate(
        this MyObjectsRegistration myObjectsRegistration,
        IPersistenceStrategy persistenceStrategy,
        Action<NHibernateConfigurationBuilder>? configure = null)
    {
        var configurationBuilder = new NHibernateConfigurationBuilder(persistenceStrategy);

        configure?.Invoke(configurationBuilder);

        var builder = myObjectsRegistration.Builder;

        builder.Register(c =>
            {
                foreach (var assembly in myObjectsRegistration.Assemblies)
                {
                    configurationBuilder.AddEntitiesFromAssembly(assembly);
                }

                configurationBuilder.AddEntities(myObjectsRegistration.EntityTypes);
                
                return configurationBuilder.Build().BuildSessionFactory();
            })
            .SingleInstance();
        builder.Register(c => c.Resolve<ISessionFactory>().OpenSession())
            .InstancePerLifetimeScope();
        builder.Register(c => new NHibernateSession(c.Resolve<global::NHibernate.ISession>()))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        builder.RegisterType<SessionAggregateRootLocator>().AsImplementedInterfaces();
        builder.RegisterType<NHibernateTransactionRunner>().AsImplementedInterfaces().InstancePerLifetimeScope();

        return new MyObjectsRegistration<global::NHibernate.ISession>(myObjectsRegistration);
    }
}