using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Products.Commands;

public partial class MoveProductCategory : Command
{
    public Reference<ProductCategory> CategoryRef { get; }
    public Reference<ProductCategory> DestinationCategoryRef { get; }

    public partial class Handler
    {
        public override async Task Handle(MoveProductCategory command, CancellationToken cancellationToken)
        {
            var category = await this.Session.Resolve(command.CategoryRef);
            
            if (command.DestinationCategoryRef != null)
            {
                category.MoveUnder(await this.Session.Resolve(command.DestinationCategoryRef));
            }
            else
            {
                category.MoveToRoot();
            }

        }
    }
}