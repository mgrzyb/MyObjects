namespace MyObjects.Demo.Model.Orders;

public class SalesOrderCanceled : DomainEvent
{
    public SalesOrder SalesOrder { get; }

    public SalesOrderCanceled(SalesOrder salesOrder)
    {
        this.SalesOrder = salesOrder;
    }
}