using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MyObjects.Demo.Model.Products;
using MyObjects.Demo.Model.Products.Commands;

namespace MyObjects.Demo.Functions.Model;

public partial class ProductCategoryProjectionDto : IDtoFor<ProductCategory>
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public IEnumerable<ProductCategoryProjectionDto> Children { get; set; }
}

public partial class ProductCategoryDto : IDtoFor<ProductCategory>
{
    public VersionedReferenceDto<ProductCategory> Ref { get; set; }
    public int ParentId { get; set; }
    public IEnumerable<int> Children { get; set; }
}

public partial class CreateProductCategoryDto : IPartialDtoFor<ProductCategory>
{
    public string Name { get; set; }
}

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        this.CreateMap<ProductCategory, ProductCategoryProjectionDto>();
        
        this.CreateMap<ProductCategory, ProductCategoryDto>()
            .ForMember(dto => dto.Ref, _=>_.MapFrom(c => c.GetVersionedReference().ToDto()))
            .ForMember(dto => dto.Children, _=>_.MapFrom(c => c.Children.Select(c => c.Id)));

        this.CreateMap<CreateProductCategoryDto, CreateProductCategory>()
            .ConstructUsing(dto => new CreateProductCategory(dto.Name));
    }
}