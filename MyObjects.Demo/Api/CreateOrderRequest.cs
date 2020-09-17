using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Api.Model;
using MyObjects.Demo.Model.Orders.Commands;

namespace MyObjects.Demo.Api
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
                var orderRef = await this.Mediator.Send(
                    new CreateSalesOrder(
                                request.Order.Lines.Select(l => (l.ProductId, l.Price, l.Quantity))
                            ));

                var order = await this.Session.Resolve(orderRef);
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