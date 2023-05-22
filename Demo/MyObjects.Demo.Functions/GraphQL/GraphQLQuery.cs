using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using MyObjects.Demo.Functions.GraphQL.ProductCategory;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Functions.GraphQL;

public class GraphQLQuery
{
    [UseProjection]
    [UseFiltering]
    public IQueryable<ProductDto> GetProducts([Service]ProjectedQuery<Product, ProductDto> query) => query.Run();
    
    [UseProjection]
    [UseFiltering]
    public IQueryable<SalesOrderDto> GetSalesOrders([Service]ProjectedQuery<SalesOrder, SalesOrderDto> query) => query.Run();
}

public class GraphQLQueryType : ObjectType<GraphQLQuery>
{
    protected override void Configure(IObjectTypeDescriptor<GraphQLQuery> descriptor)
    {
        descriptor.ProductCategories();
    }
}