using AutoMapper;
using MyObjects.Demo.Functions.Infrastructure;
using MyObjects.Demo.Model.Products;
using MyObjects.Infrastructure;

namespace MyObjects.Demo.Functions.Model;

internal class ProductCreatedDto : DomainEventMessageDto
{
    public string EventType { get; } = "product-created";
    public int ProductId { get; set; } 
}

class DomainEventMessagesProfile : Profile
{
    public DomainEventMessagesProfile()
    {
        this.CreateMap<AggregateCreated<Product>, DomainEventMessageDto>()
            .Include<AggregateCreated<Product>, ProductCreatedDto>();
        
        this.CreateMap<AggregateCreated<Product>, ProductCreatedDto>()
            .ForMember(dto => dto.ProductId, _ => _.MapFrom(e => e.Root.Id));
    }
}