using Autofac;
using MediatR;

namespace MyObjects.Infrastructure.Setup;

public class RequestHandlerRegistration
{
    public readonly ContainerBuilder Builder;

    public RequestHandlerRegistration(ContainerBuilder builder)
    {
        Builder = builder;
    }

    public RequestHandlerRegistration Retry()
    {
        this.Builder.RegisterGenericDecorator(
            typeof(RetryRequestHandlerDecorator<,>),
            typeof(IRequestHandler<,>));

        return this;
    }
}