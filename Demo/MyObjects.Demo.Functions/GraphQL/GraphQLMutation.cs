using System.Threading.Tasks;
using AutoMapper;
using HotChocolate;
using MediatR;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products.Commands;

namespace MyObjects.Demo.Functions.GraphQL;

public class GraphQLMutation
{
    public async Task<ProductDto> CreateProduct([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, CreateProductDto product)
    {
        var productRef = await mediator.Send(mapper.Map<CreateProduct>(product));
        return mapper.Map<ProductDto>(await session.Resolve(productRef));
    }

    public async Task<SalesOrderDto> CreateSalesOrder([Service]IReadonlySession session, [Service]IMediator mediator, [Service]Mapper mapper, CreateSalesOrderDto order)
    {
        var orderRef = await mediator.Send(mapper.Map<CreateSalesOrder>(order));
        return mapper.Map<SalesOrderDto>(await session.Resolve(orderRef));
    }
}