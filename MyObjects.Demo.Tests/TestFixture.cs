using MyObjects.Demo.Model.Products;
using MyObjects.Testing;

namespace MyObjects.Demo.UnitTests
{
    public class TestFixture : DomainModelTestFixtureBase
    {
        public TestFixture() : base(typeof(Product).Assembly)
        {
        }
    }
}