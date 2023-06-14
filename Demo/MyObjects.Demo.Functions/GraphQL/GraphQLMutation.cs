using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using MediatR;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;

namespace MyObjects.Demo.Functions.GraphQL;

public class GraphQLMutation
{
    public async Task<ProductDto> CreateProduct([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, CreateProductDto product)
    {
        var productRef = await mediator.Send(mapper.Map<CreateProduct>(product));
        return mapper.Map<ProductDto>(await session.Resolve(productRef));
    }

    public async Task<ProductDto> UpdateProduct([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, VersionedReferenceDto<Product> @ref, UpdateProductDto product)
    {
        var updatedRef = await mediator.Send(mapper.Map(product, new UpdateProduct(@ref)));
        return mapper.Map<ProductDto>(await session.Resolve(updatedRef.WithoutVersion));
    }

    public async Task<ProductCategoryDto> CreateProductCategory([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, CreateProductCategoryDto product)
    {
        var @ref = await mediator.Send(mapper.Map<CreateProductCategory>(product));
        return mapper.Map<ProductCategoryDto>(await session.Resolve(@ref));
    }
    
    public async Task<SalesOrderDto> CreateSalesOrder([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, CreateSalesOrderDto order)
    {
        var @ref = await mediator.Send(mapper.Map<CreateSalesOrder>(order));
        return mapper.Map<SalesOrderDto>(await session.Resolve(@ref));
    }
}