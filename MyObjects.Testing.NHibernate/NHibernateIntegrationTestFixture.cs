using System.Reflection;
using MyObjects.Infrastructure;
using MyObjects.NHibernate;

namespace MyObjects.Testing.NHibernate;

public class NHibernateIntegrationTestFixture : IntegrationTestFixtureBase<global::NHibernate.ISession>
{
    private readonly Action<NHibernateConfigurationBuilder> config = builder => { };
    
    protected NHibernateIntegrationTestFixture(params Assembly[] assemblies) : base(new NHibernateTestCodeRunner(), assemblies)
    {
    }

    protected NHibernateIntegrationTestFixture(Action<NHibernateConfigurationBuilder> config, params Assembly[] assemblies) : base(new NHibernateTestCodeRunner(), assemblies)
    {
        this.config = config;
    }
    
    protected override MyObjectsRegistration<global::NHibernate.ISession> SetupMyObjectsRegistration(MyObjectsRegistration registration)
    {
        return registration.UseNHibernate(new TestPersistenceStrategy(), this.config);
    }
}