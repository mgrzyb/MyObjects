using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;

namespace MyObjects.Demo.Functions.Model;

public class ProductDataDto
{
    public string Name { get; set; }
    public IEnumerable<string> Categories { get; set; }
}

public class ProductDto : ProductDataDto
{
    public int Id { get; set; }
}

public class CreateProductDto : ProductDataDto
{
}

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        this.CreateMap<Product, ProductDto>()
            .ForMember(dto => dto.Categories, _ => _.MapFrom(p => p.Categories.Select(c => c.Name)));

        this.CreateMap<CreateProductDto, CreateProduct>();
    }
}