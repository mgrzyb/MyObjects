using System.Linq;
using System.Threading.Tasks;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Products;
using NHibernate.Linq;
using NUnit.Framework;

namespace MyObjects.Demo.UnitTests
{
    public class SalesOrderTests : TestFixture
    {
        [Test]
        public async Task Given_Products_WhenOrderWithLinesIsCreated_Total_MakesSense()
        {
            var (aRef, bRef) = await Given(async session =>
            {
                var productA = new Product();
                var productB = new Product();
                return (await session.Save(productB), await session.Save(productA));
            });

            var orderRef = await When(async session =>
            {
                var productA = await session.Resolve(aRef);
                var productB = await session.Resolve(bRef);
                
                var salesOrder = new SalesOrder();
                salesOrder.AddLine(productA, 5);
                salesOrder.AddLine(productB, 3, 3);
                
                return await session.Save(salesOrder);
            });

            await Then(async session =>
            {
                var order = await session.Resolve(orderRef);
                
                Assert.That(order.Total, Is.EqualTo(14));

                var orders = await session
                    .Query<SalesOrder>()
                    .Where(o => o.Total > 10)
                    .ToListAsync();
                
                Assert.That(orders.Count, Is.EqualTo(1));
            });
        }
    }
}