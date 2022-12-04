using System;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Orders.Commands.Inventory.EventHandlers;

public class ReleaseInventoryWhenSalesOrderCancelled : DomainEventHandler<SalesOrderCanceled>
{
    public ReleaseInventoryWhenSalesOrderCancelled(IDependencies dependencies) : base(dependencies)
    {
    }

    protected async override Task Handle(SalesOrderCanceled domainEvent)
    {
        Console.WriteLine("Releasing inventory");
    }
}