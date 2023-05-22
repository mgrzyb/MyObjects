using System.Collections.Generic;
using AutoMapper;
using MyObjects.Demo.Model.Products;

namespace MyObjects.Demo.Functions.Model;

public class ProductCategoryDto
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string Name { get; set; }
    public IEnumerable<ProductCategoryDto> Children { get; set; }
}

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        this.CreateMap<ProductCategory, ProductCategoryDto>();
    }
}