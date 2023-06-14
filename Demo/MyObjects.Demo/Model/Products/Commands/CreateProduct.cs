using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Products.Commands;

public partial class CreateProduct : Command<Reference<Product>>, IPartialDtoFor<Product>
{
    public string Name { get; }
    public IEnumerable<Reference<ProductCategory>> Categories { get; }

    public CreateProduct(string name) : this(name, Enumerable.Empty<Reference<ProductCategory>>())
    {
    }

    public CreateProduct(string name, IEnumerable<Reference<ProductCategory>> categories)
    {
        this.Name = name;
        this.Categories = categories;
    }


    public class Handler : CommandHandler<CreateProduct, Reference<Product>>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }

        public override async Task<Reference<Product>> Handle(CreateProduct command, CancellationToken cancellationToken)
        {
            var product = new Product(command.Name)
            {
                Description = command.Description,
                ExternalId = command.ExternalId, 
            };

            foreach (var category in await this.Session.ResolveMany(command.Categories))
            {
                product.Categories.Add(category);
            }
            
            return await this.Session.Save(product);
        }
    }

}