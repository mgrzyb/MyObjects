using Microsoft.AspNetCore.Mvc;
using MyObjects.AspNetCore.Mvc;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands;

namespace MyObjects.Demo.Api;

public class CancelSalesOrderRequest : ApiRequest
{
    public Reference<SalesOrder> OrderId { get; set; }
    
    public class Handler : Handler<CancelSalesOrderRequest>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }

        protected override async Task<IActionResult> Handle(CancelSalesOrderRequest request)
        {
            await this.Mediator.Send(new CancelSalesOrder(request.OrderId));

            return NoContent();
        }
    }

}