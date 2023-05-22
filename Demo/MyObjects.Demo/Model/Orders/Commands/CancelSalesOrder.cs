using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Orders.Commands;

public partial class CancelSalesOrder : Command
{
    public Reference<SalesOrder> OrderRef { get; }

    public CancelSalesOrder(Reference<SalesOrder> orderRef)
    {
        OrderRef = orderRef;
    }
    
    public partial class Handler
    {
        public override async Task Handle(CancelSalesOrder command, CancellationToken cancellationToken)
        {
            var order = await this.Session.Resolve(command.OrderRef);
            order.Cancel();                
        }
    }
}