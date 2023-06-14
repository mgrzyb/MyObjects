namespace MyObjects.Demo.Model.Products.Commands;

public partial class UpdateProduct
{
    public partial class Handler : CommandHandler<UpdateProduct, VersionedReference<Product>>
    {
        public Handler(IDependencies dependencies): base(dependencies)
        {
        }
    }
}