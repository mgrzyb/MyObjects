using System.Linq;
using System.Threading;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using MyObjects.Demo.Functions.Model;
using NHibernate.Collection;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.GraphQL.ProductCategories;

public class ProductCategoryType : ObjectType<ProductCategoryDto>
{
    private static SemaphoreSlim sem = new SemaphoreSlim(1, 1);
    
    protected override void Configure(IObjectTypeDescriptor<ProductCategoryDto> descriptor)
    {
        descriptor.Field(dto => dto.Name).Resolve(context => context.Parent<Demo.Model.Products.ProductCategory>().Name);
        descriptor.Field(dto => dto.Children).Type<ListType<ProductCategoryType>>().Resolve(async context =>
        {
            var session = context.Services.GetService<IReadonlySession<global::NHibernate.ISession>>();

            await sem.WaitAsync();
            try
            {
                var children = context.Parent<Demo.Model.Products.ProductCategory>().Children;
                if (children is IPersistentCollection c)
                    await session.Advanced.GetSessionImplementation().InitializeCollectionAsync(c, false, CancellationToken.None);
                return children;
            }
            finally
            {
                sem.Release();
            }
        });
    }
}