using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products;
using MyObjects.Functions;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.Api;

partial class SalesOrderFunctions : FunctionsBase
{
    public SalesOrderFunctions(IDependencies dependencies) : base(dependencies)
    {
    }

    [HttpGet][Route("sales-orders")]
    public async Task<IActionResult> GetSalesOrders()
    {
        var orders = await this.Session.Query<SalesOrder>().ToListAsync();
        return new OkObjectResult(orders.Select(o => new { Id = o.Id, Number = o.Number }));
    }

    [HttpGet][Route("sales-orders/{orderRef:int}")]
    public async Task<IActionResult> GetSalesOrder(Reference<SalesOrder> orderRef)
    {
        var o = await this.Session.Resolve(orderRef);
        return new OkObjectResult(new { Id = o.Id, Number = o.Number });    
    }
    
    [HttpPost][Route("sales-orders")]
    public async Task<IActionResult> CreateSalesOrder(SalesOrderDto body)
    {
        var orderRef = await this.Mediator.Send(new CreateSalesOrder
            { 
                Lines = body.Lines.Select(l =>(
                    l.ProductRef, 
                    l.Price, 
                    l.Quantity))
            });
        
        return new CreatedResult($"sales-order/{orderRef.Id}", orderRef);
    }
}

public class SalesOrderDto
{
    public IEnumerable<SalesOrderLineDto> Lines { get; set; }
}

public class SalesOrderLineDto
{
    public Reference<Product> ProductRef { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}