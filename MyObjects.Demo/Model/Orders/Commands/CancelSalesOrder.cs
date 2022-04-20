using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Orders.Commands;

public class CancelSalesOrder : Command
{
    public Reference<SalesOrder> OrderRef { get; }

    public CancelSalesOrder(Reference<SalesOrder> orderRef)
    {
        OrderRef = orderRef;
    }
    
    public class Handler : Handler<CancelSalesOrder>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }

        public override async Task Handle(CancelSalesOrder command, CancellationToken cancellationToken)
        {
            var order = await this.Session.Resolve(command.OrderRef);
            order.Cancel();                
        }
    }
    
}