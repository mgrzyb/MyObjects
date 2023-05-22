namespace MyObjects.Demo.Model.Orders.Commands;

#nullable enable
public partial class CancelSalesOrder
{
    public partial class Handler : CommandHandler<CancelSalesOrder>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }
    }
}