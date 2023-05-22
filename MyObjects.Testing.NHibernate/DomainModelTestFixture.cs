using System.Reflection;
using MyObjects.Infrastructure;
using MyObjects.NHibernate;

namespace MyObjects.Testing.NHibernate;

public class NHibernateDomainModelTestFixture : DomainModelTestFixture<global::NHibernate.ISession>
{
    protected NHibernateDomainModelTestFixture(params Assembly[] assemblies) : base(new NHibernateTestCodeRunner(), assemblies) 
    {
    }

    protected override MyObjectsRegistration<global::NHibernate.ISession> SetupMyObjectsRegistration(MyObjectsRegistration registration)
    {
        return registration.UseNHibernate(new TestPersistenceStrategy());
    }
}