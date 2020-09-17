using System.Threading.Tasks;
using MyObjects.Demo.Model.Products;
using NUnit.Framework;

namespace MyObjects.Demo.UnitTests
{
    public class ProductTests : TestFixture
    {
        [Test]
        public async Task CanCreateProduct()
        {
            var productRef = await When(session => session.Save(new Product
            {
                Name = "My first product"
            }));

            await Then(async session =>
            {
                var product = await session.Resolve(productRef);

                Assert.That(product.Name, Is.EqualTo("My first product"));
            });
        }
    }
}