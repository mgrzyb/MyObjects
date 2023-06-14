using System.Linq;
using System.Threading;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using MyObjects.Demo.Functions.Model;
using MyObjects.NHibernate;

namespace MyObjects.Demo.Functions.GraphQL.ProductCategories;

public class ProductCategoryProjectionType : ObjectType<ProductCategoryProjectionDto>
{
    private static readonly SemaphoreSlim sem = new SemaphoreSlim(1, 1);
    
    protected override void Configure(IObjectTypeDescriptor<ProductCategoryProjectionDto> descriptor)
    {
        descriptor.Field(dto => dto.Name).Resolve(context => context.Parent<Demo.Model.Products.ProductCategory>().Name);
        descriptor.Field(dto => dto.Children).Type<ListType<ProductCategoryProjectionType>>().Resolve(async context =>
        {
            var session = context.Services.GetService<IReadonlySession<global::NHibernate.ISession>>();

            // Resolvers are run parallel, Session is not thread save  
            await sem.WaitAsync();
            try
            {
                return await context.Parent<Demo.Model.Products.ProductCategory>().Children.Initialized();
            }
            finally
            {
                sem.Release();
            }
        });
    }
}