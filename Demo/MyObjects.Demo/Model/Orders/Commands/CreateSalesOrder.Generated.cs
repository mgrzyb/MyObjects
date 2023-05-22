namespace MyObjects.Demo.Model.Orders.Commands;

#nullable enable
public partial class CreateSalesOrder
{
    public partial class Handler : CommandHandler<CreateSalesOrder, Reference<SalesOrder>>
    {
        public Handler(IDependencies dependencies, INumberSequence orderNumberSequence) : base(dependencies)
        {
            this.orderNumberSequence = orderNumberSequence;
        }
    }
}