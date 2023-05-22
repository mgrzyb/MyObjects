using MyObjects.Demo.Model.Products;
using MyObjects.Testing.NHibernate;

namespace MyObjects.Demo.UnitTests;

public class IntegrationTestFixture : NHibernateIntegrationTestFixture
{
    protected IntegrationTestFixture() : base(typeof(Product).Assembly)
    {
    }
}