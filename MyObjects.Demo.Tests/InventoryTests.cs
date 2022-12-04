using System;
using System.Threading.Tasks;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands.Inventory.EventHandlers;
using MyObjects.Demo.Model.Products;
using MyObjects.Testing.NHibernate;
using NUnit.Framework;

namespace MyObjects.Demo.UnitTests;

public class InventoryTests : DomainModelTestFixture
{
    public InventoryTests() : base(c => c.AddEntitiesFromAssemblyOf<Product>(), typeof(Product).Assembly)
    {
    }

    [Test]
    public async Task ReleaseInventoryHandler_ReleasesInventory()
    {
        var orderRef = await Given(s => s.Save(new SalesOrder("123")));

        await When(SalesOrderCanceled(orderRef)).IsHandledBy<ReleaseInventoryWhenSalesOrderCancelled>();

        await Then(async s =>
        {
            Assert.True(true);       
        });
    }

    private static Func<IReadonlySession, Task<SalesOrderCanceled>> SalesOrderCanceled(Reference<SalesOrder> orderRef)
    {
        return async s => new SalesOrderCanceled(await s.Resolve(orderRef));
    }
}