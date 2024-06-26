using Autofac;
using MediatR;

namespace MyObjects.Infrastructure;

public class LifetimeScopeCommandHandlerAdapter<TRequest, TResult> : IRequestHandler<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILifetimeScope context;
    private readonly IMediator mediator;

    public LifetimeScopeCommandHandlerAdapter(ILifetimeScope context, IMediator mediator)
    {
        this.context = context;
        this.mediator = mediator;
    }
            
    public async Task<TResult> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await using var scope = this.context.BeginLifetimeScope();
        var commandHandler = scope.Resolve<ICommandHandler<TRequest, TResult>>();
        return await commandHandler.Handle(request, cancellationToken);
    }
}