using Microsoft.AspNetCore.Mvc;
using MyObjects.AspNetCore.Mvc;
using MyObjects.Demo.Api.Model;
using MyObjects.Demo.Model.Orders.Commands;

namespace MyObjects.Demo.Server.Api
{
    public class CreateOrderRequest : ApiRequest<SalesOrderDto>
    {
        public SalesOrderDto Order { get; set; }
        
        public class Handler : Handler<CreateOrderRequest>
        {
            public Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            protected override async Task<ActionResult<SalesOrderDto>> Handle(CreateOrderRequest request)
            {
                var command = new CreateSalesOrder(
                    request.Order.Lines.Select(l => (l.ProductId, l.Price, l.Quantity))
                );
                
                var orderRef = await this.Mediator.Send(command);

                var order = await this.Session.Resolve(orderRef);
                
                // Could use AutoMapper
                return new SalesOrderDto
                {
                    Lines = order.Lines.Select(l => new SalesOrderLineDto
                    {
                        ProductId = l.Product.GetReference(),
                        Price = l.Price,
                        Quantity = l.Quantity,
                    })
                };
            }
        }
    }
}