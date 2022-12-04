using System.Reflection;
using Autofac;
using Autofac.Core;
using MyObjects.NHibernate;

namespace MyObjects.Testing.NHibernate;

public class IntegrationTestFixture : IntegrationTestFixtureBase<global::NHibernate.ISession>
{
    private readonly Action<NHibernateConfigurationBuilder> nhConfig;

    protected IntegrationTestFixture(Action<NHibernateConfigurationBuilder> nhConfig, Assembly modelAssembly, params IModule[] modules) 
        : base(new NHibernateTestCodeRunner(), modelAssembly, modules)
    {
        this.nhConfig = nhConfig;
    }
    
    protected override void ConfigureTestScope(ContainerBuilder builder)
    {
        base.ConfigureTestScope(builder);
        builder.RegisterModule(new NHibernateModule(new TestPersistenceStrategy(), this.nhConfig));
    }
}