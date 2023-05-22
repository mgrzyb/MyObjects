using Autofac;

namespace MyObjects.Infrastructure.Setup;

public class DomainEventHandlerRegistration
{
    public ContainerBuilder Builder { get; }

    public DomainEventHandlerRegistration(ContainerBuilder builder)
    {
        Builder = builder;
    }
}