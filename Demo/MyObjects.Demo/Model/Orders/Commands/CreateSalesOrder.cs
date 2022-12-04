using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Model.Orders.Commands
{
    public class CreateSalesOrder : Command<Reference<SalesOrder>>
    {
        public IEnumerable<(Reference<Product> ProductRef, decimal Price, decimal Quantity)> Lines { get; init; }

        public class Handler : CommandHandler<CreateSalesOrder, Reference<SalesOrder>>
        {
            private readonly INumberSequence orderNumberSequence;
            
            public Handler(IDependencies dependencies, INumberSequence orderNumberSequence) : base(dependencies)
            {
                this.orderNumberSequence = orderNumberSequence;
            }

            public override async Task<Reference<SalesOrder>> Handle(CreateSalesOrder command, CancellationToken cancellationToken)
            {
                var order = new SalesOrder(this.orderNumberSequence.Next().ToString());
                
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