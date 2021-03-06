using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders.Commands
{
    public class CreateSalesOrder : Command<Reference<SalesOrder>>
    {
        public IEnumerable<(Reference<Product> ProductRef, decimal Price, decimal Quantity)> Lines { get; }

        public CreateSalesOrder(IEnumerable<(Reference<Product>, decimal, decimal)> lines)
        {
            this.Lines = lines;
        }

        public class Handler : Handler<CreateSalesOrder>
        {
            public Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            public override async Task<Reference<SalesOrder>> Handle(CreateSalesOrder command, CancellationToken cancellationToken)
            {
                var order = new SalesOrder();
                
                foreach (var l in command.Lines)
                {
                    var product = await this.Session.Resolve(l.ProductRef);
                    order.AddLine(product, l.Price, l.Quantity);
                }

                return await this.Session.Save(order);
            }
        }
    }
}