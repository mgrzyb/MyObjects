using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;

namespace MyObjects.Demo.Functions.Model;

public partial class ProductProjectionDto : IDtoFor<Product>
{
    public int Id { get; set; }
    public IEnumerable<string> Categories { get; set; }
}

public partial class ProductDto : IDtoFor<Product>
{
    public VersionedReferenceDto<Product> Ref { get; set; }
    public IEnumerable<int> Categories { get; set; }
}

public partial class CreateProductDto : IDtoFor<CreateProduct>
{
    public IEnumerable<int>? Categories { get; set; }
}

public partial class UpdateProductDto : IDtoFor<UpdateProduct>
{
    public IEnumerable<int>? Categories { get; set; }
}

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        this.CreateMap<Product, ProductProjectionDto>()
            .ForMember(dto => dto.Categories, _ => _.MapFrom(p => p.Categories.Select(c => c.Name)));

        this.CreateMap<Product, ProductDto>()
            .ForMember(dto => dto.Ref, _=>_.MapFrom(p => p.GetVersionedReference().ToDto()))
            .ForMember(dto => dto.Categories, _ => _.MapFrom(p => p.Categories.Select(c => c.Id)));

        this.CreateMap<CreateProductDto, CreateProduct>()
            .ConstructUsing(dto => dto.Categories == null ? new CreateProduct(dto.Name) : new CreateProduct(dto.Name, dto.Categories.Select(id => new Reference<ProductCategory>(id))))
            .ForMember(c => c.Categories, _=>_.Ignore());

        this.CreateMap<UpdateProductDto, UpdateProduct>()
            .ForMember(c => c.Categories, _=>_.MapFrom(dto => dto.Categories == null ? null : dto.Categories.Select(id => new Reference<ProductCategory>(id))));
    }
}