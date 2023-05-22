namespace MyObjects.Demo.Model.Products.Commands;

public partial class MoveProductCategory
{
    public partial class Handler : CommandHandler<MoveProductCategory>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }
    }
}
