using System.Linq;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace MyObjects.Demo.Functions.GraphQL.ProductCategories;

public static class ProductCategoriesExtensions
{
    public static void ProductCategories(this IObjectTypeDescriptor<GraphQLQuery> descriptor)
    {
        descriptor.Field("productCategories")
            .Type<ListType<ProductCategoryProjectionType>>()
            .Resolve(context => context.Services.GetService<IReadonlySession>().Query<Demo.Model.Products.ProductCategory>()
                .Where(c => c.Parent == null).ToList());
    }
}