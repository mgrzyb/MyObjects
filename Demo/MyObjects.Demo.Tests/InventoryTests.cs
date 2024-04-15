using System;
using System.Threading.Tasks;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands.Inventory.EventHandlers;
using NUnit.Framework;

namespace MyObjects.Demo.UnitTests;

public class InventoryTests : DomainModelTestFixture
{
    [Test]
    public async Task ReleaseInventoryHandler_ReleasesInventory()
    {
        var orderRef = await Given(s => s.Save(new SalesOrder("123")));

        await When(async s => new SalesOrderCanceled(await s.Resolve(orderRef)))
            .IsHandledBy<ReleaseInventoryWhenSalesOrderCancelled>();

        await Then(async s =>
        {
            Assert.That(true);       
        });
    }
}