using MyObjects.Demo.Model.Products;
using MyObjects.Testing;

namespace MyObjects.Demo.UnitTests
{
    public class TestFixture : TestFixtureBase
    {
        protected TestFixture()
            : base(options => options.AddEntitiesFromAssemblyOf<Product>())
        {
        }
    }
}