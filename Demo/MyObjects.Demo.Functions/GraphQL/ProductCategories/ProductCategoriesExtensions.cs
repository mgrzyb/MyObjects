using System.Linq;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using MyObjects.Demo.Functions.GraphQL.ProductCategories;

namespace MyObjects.Demo.Functions.GraphQL.ProductCategory;

public static class ProductCategoriesExtensions
{
    public static void ProductCategories(this IObjectTypeDescriptor<GraphQLQuery> descriptor)
    {
        descriptor.Field("productCategories")
            .Type<ListType<ProductCategoryType>>()
            .Resolve(context => context.Services.GetService<IReadonlySession>().Query<Demo.Model.Products.ProductCategory>()
                .Where(c => c.Parent == null).ToList());
    }
}