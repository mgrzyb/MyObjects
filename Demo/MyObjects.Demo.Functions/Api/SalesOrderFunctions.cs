using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Functions.GraphQL;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Functions;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.Api;

[Route("api")]
public partial class SalesOrderFunctions : FunctionsBase<IActionResult>
{
    private readonly Mapper mapper;
    private readonly ProjectedQuery<SalesOrder, SalesOrderDto> salesOrderQuery;
    
    public SalesOrderFunctions(IDependencies dependencies, ProjectedQuery<SalesOrder, SalesOrderDto> salesOrderQuery, Mapper mapper) : base(dependencies)
    {
        this.salesOrderQuery = salesOrderQuery;
        this.mapper = mapper;
    }
    
    [HttpGet][Route("sales-orders")]
    public async Task<IActionResult> GetSalesOrders(string? number)
    {
        var query = this.salesOrderQuery.Run();
        if (string.IsNullOrEmpty(number) == false)
            query = query.Where(o => o.Number == number);
        
        return new OkObjectResult(await query.ToListAsync());
    }

    [HttpGet][Route("sales-orders/{orderRef:int}")]
    public async Task<IActionResult> GetSalesOrder(Reference<SalesOrder> orderRef)
    {
        return new OkObjectResult(this.mapper.Map<SalesOrderDto>(await this.Session.Resolve(orderRef)));    
    }
    
    [HttpPost][Route("sales-orders")]
    public async Task<IActionResult> CreateSalesOrder(CreateSalesOrderDto body)
    {
        var orderRef = await this.Mediator.Send(this.mapper.Map<CreateSalesOrder>(body));
        return new CreatedResult($"api/sales-order/{orderRef.Id}", null);
    }
}