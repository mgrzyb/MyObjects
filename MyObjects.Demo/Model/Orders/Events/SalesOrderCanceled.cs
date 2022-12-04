namespace MyObjects.Demo.Model.Orders;

public class SalesOrderCanceled : IDomainEvent
{
    public SalesOrder SalesOrder { get; }

    public SalesOrderCanceled(SalesOrder salesOrder)
    {
        this.SalesOrder = salesOrder;
    }
}