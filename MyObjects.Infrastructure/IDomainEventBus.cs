using MediatR;

namespace MyObjects;

public interface IDomainEventBus
{
    Task Publish(IDomainEvent e);
}

class MediatorDomainEventBus : IDomainEventBus
{
    private readonly IMediator mediator;

    public MediatorDomainEventBus(IMediator mediator)
    {
        this.mediator = mediator;
    }

    public async Task Publish(IDomainEvent e)
    {
        await this.mediator.Publish(e);
    }
}