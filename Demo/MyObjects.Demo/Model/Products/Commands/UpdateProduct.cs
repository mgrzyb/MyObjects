using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Products.Commands;

public partial class UpdateProduct : Command<VersionedReference<Product>>, IPartialDtoFor<Product>
{
    public readonly VersionedReference<Product> ProductRef;
    public IEnumerable<Reference<ProductCategory>>? Categories { get; set; }

    public UpdateProduct(VersionedReference<Product> productRef)
    {
        this.ProductRef = productRef;
    }

    public partial class Handler
    {
        public override async Task<VersionedReference<Product>> Handle(UpdateProduct command, CancellationToken cancellationToken)
        {
            var product = await this.Session.Resolve(command.ProductRef);

            await product.Update(async () => {            
                product.Name = command.Name ?? product.Name;
                product.Description = command.Description ?? product.Description;
                product.ExternalId = command.ExternalId ?? product.ExternalId;
                if (command.Categories != null)
                    product.Categories.Patch(await this.Session.ResolveMany(command.Categories));
            });

            return product.GetVersionedReference();
        }
    }
}

public static class CollectionExtensions
{
    public static void Patch<T>(this ICollection<T> collection, IEnumerable<T> patch)
    {
        foreach (var item in collection.ToList())
        {
            if (patch.Contains(item) == false)
                collection.Remove(item);
        }

        foreach (var item in patch)
        {
            if (collection.Contains(item) == false)
                collection.Add(item);
        }
    }
}