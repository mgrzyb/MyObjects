using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Products.Commands;

public partial class CreateProductCategory : Command<Reference<ProductCategory>>, IPartialDtoFor<ProductCategory>
{
    public string Name { get; }
    public Reference<ProductCategory>? ParentRef { get; }

    public CreateProductCategory(string name)
    {
        Name = name;
    }

    public CreateProductCategory(string name, Reference<ProductCategory>? parentRef)
    {
        Name = name;
        ParentRef = parentRef;
    }

    public class Handler : CommandHandler<CreateProductCategory, Reference<ProductCategory>>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }

        public override async Task<Reference<ProductCategory>> Handle(CreateProductCategory command, CancellationToken cancellationToken)
        {
            if (command.ParentRef is null)
            {
                return await this.Session.Save(new ProductCategory(command.Name)
                {
                    ExternalId = command.ExternalId
                });
            }
            else
            {
                return await this.Session.Save(new ProductCategory(command.Name, await this.Session.Resolve(command.ParentRef))
                {
                    ExternalId = command.ExternalId
                });
            }
        }
    }
}

public record Foo
{
    public string Name { get; set; }
    public int ExternalId { get; init; }
    
    public Reference<ProductCategory>? SelfRef { get; init; }
    public Reference<ProductCategory>? ParentRef { get; set; }
    public List<Foo> Children { get; } = new();
}

public class CreateProductCategoryHierarchies : Command<IEnumerable<Reference<ProductCategory>>>
{
    public IEnumerable<Foo> Roots { get; }
    
    public CreateProductCategoryHierarchies(IEnumerable<Foo> roots)
    {
        Roots = roots;
    }

    public class Handler : CommandHandler<CreateProductCategoryHierarchies, IEnumerable<Reference<ProductCategory>>>
    {
        public Handler(IDependencies dependencies) : base(dependencies)
        {
        }
        
        public override async Task<IEnumerable<Reference<ProductCategory>>> Handle(CreateProductCategoryHierarchies command, CancellationToken cancellationToken)
        {
            var result = new List<Reference<ProductCategory>>();
            foreach (var r in command.Roots)
            {
                var parent = r.ParentRef != null ? await this.Session.Resolve(r.ParentRef) : null;
                await this.Create(r, parent, result);
            }

            return result;
        }

        private async Task Create(Foo f, ProductCategory parent, IList<Reference<ProductCategory>> result)
        {
            var category = await this.Ensure(f, parent);                        
            result.Add(category.GetReference());
            foreach (var child in f.Children)
            {
                await this.Create(child, category, result);
            }
        }

        private async Task<ProductCategory> Ensure(Foo f, ProductCategory parent)
        {
            if (f.SelfRef != null)
            {
                var c = await this.Session.Resolve(f.SelfRef);
                if (parent != null)
                    c.MoveUnder(parent);
                return c;
            }
            else
            {
                var c = new ProductCategory(f.Name, parent) { ExternalId = f.ExternalId };
                await this.Session.Save(c);
                return c;
            }
        }
    }
}