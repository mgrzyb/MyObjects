using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Functions.Model;

public partial class ProductFooDto : IDtoFor<Product>
{
}

public class SalesOrderDataDto<TLine>
{
    public IEnumerable<TLine> Lines { get; set; }
}

public class SalesOrderDto : SalesOrderDataDto<SalesOrderLineDto>
{
    public string Number { get; set; }
    public int Id { get; set; }
}

public class SalesOrderLineDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreateSalesOrderDto : SalesOrderDataDto<SalesOrderLineDto>
{
}

public class SalesOrderProfile : Profile
{
    public SalesOrderProfile()
    {
        this.CreateMap<SalesOrder, SalesOrderDto>();
        this.CreateMap<SalesOrderLine, SalesOrderLineDto>();

        this.CreateMap<CreateSalesOrderDto, CreateSalesOrder>()
            .ForMember(order => order.Lines, _ => _.MapFrom((dto, _, _) => dto.Lines.Select(l => (new Reference<Product>(l.ProductId), l.Price, l.Quantity))));
    }
}