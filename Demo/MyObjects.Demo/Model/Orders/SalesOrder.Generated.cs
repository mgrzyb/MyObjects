using System.Collections.Generic;

namespace MyObjects.Demo.Model.Orders;

#nullable enable
partial class SalesOrder
{
    private readonly ICollection<SalesOrderLine> lines = new List<SalesOrderLine>();
        
    protected SalesOrder()
    {
    }
}