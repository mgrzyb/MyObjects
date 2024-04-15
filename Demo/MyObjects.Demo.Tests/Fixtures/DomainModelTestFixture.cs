using MyObjects.Demo.Model.Products;
using MyObjects.Identity;
using MyObjects.Testing.NHibernate;

namespace MyObjects.Demo.UnitTests;

public class DomainModelTestFixture : NHibernateDomainModelTestFixture
{
    protected DomainModelTestFixture() : base(typeof(Product).Assembly, typeof(UserIdentity).Assembly)
    {
    }
}