using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;
using MyObjects.Functions;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.Api;

[Route("api")]
public partial class FriscoProductFunctions : HttpFunctionsBase
{
    public FriscoProductFunctions(IDependencies dependencies) : base(dependencies)
    {
    }
    
    [HttpPost][Route("frisco-products")]
    public async Task<IActionResult> ImportFriscoProducts(IEnumerable<FriscoProductWrapperDto> body)
    {
        var roots = await this.GetFriscoProductCategories(body);
        await this.Mediator.Send(new CreateProductCategoryHierarchies(roots));

        var externalIds = body.Select(p => p.Product.Id).ToArray();
        var existingProducts = this.Session.Query<Product>()
            .Where(p => externalIds.Contains(p.ExternalId.Value))
            .Select(p => p.ExternalId).ToHashSet();

        var rootCategoryExternalIds = body.SelectMany(p =>
            p.Product.Categories.Where(c =>
                p.Product.Categories.Any(child => child.ParentId == c.CategoryId) == false))
            .Select(c => c.CategoryId)
            .Distinct()
            .ToArray();

        var categories = await this.Session.Query<ProductCategory>()
            .Where(c => rootCategoryExternalIds.Contains(c.ExternalId.Value))
            .Select(c => new {c.ExternalId, c.Id})
            .ToListAsync();
        
        foreach (var p in body.Select(p => p.Product).Where(p => existingProducts.Contains(p.Id) == false))
        {
            var categoryReferences = p.Categories.Join(categories, 
                c => c.CategoryId, 
                c=>c.ExternalId, 
                (_,c) => new Reference<ProductCategory>(c.Id));
            
            await this.Mediator.Send(new CreateProduct(p.Name.Pl, categoryReferences) { ExternalId = p.Id });
        }
        
        return new OkResult();
    }

    private async Task<IEnumerable<Foo>> GetFriscoProductCategories(IEnumerable<FriscoProductWrapperDto> body)
    {
        var categories = this.Session.Query<ProductCategory>().ToDictionary(c => c.ExternalId, c => c);
        var friscoCategories = body.SelectMany(p => p.Product.Categories).DistinctBy(c => c.CategoryId);

        var roots = new HashSet<Foo>();
        var d = new Dictionary<int, Foo>();

        foreach (var f in friscoCategories)
        {
            var foo = d.GetValueOrDefault(f.CategoryId) ?? new Foo
            {
                ExternalId = f.CategoryId,
                SelfRef = categories.GetValueOrDefault(f.CategoryId)?.GetReference(),
            };
            d[f.CategoryId] = foo;

            foo.Name = f.Name.Pl;

            if (f.ParentId.HasValue)
            {
                roots.Remove(foo);
                foo.ParentRef = categories.GetValueOrDefault(f.ParentId.Value)?.GetReference();

                if (d.TryGetValue(f.ParentId.Value, out var parent))
                {
                    parent.Children.Add(foo);
                }
                else
                {
                    var p = new Foo
                    {
                        Name = "???",
                        ExternalId = f.ParentId.Value,
                        SelfRef = categories.GetValueOrDefault(f.ParentId.Value)?.GetReference()
                    };
                    d.Add(f.ParentId.Value, p);
                    roots.Add(p);
                }
            }
            else
            {
                roots.Add(foo);
            }
        }

        return roots;
    }
}