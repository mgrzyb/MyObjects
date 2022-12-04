using MediatR;

namespace MyObjects.Infrastructure;

public class CommandHandlerAdapter<T, K> : IRequestHandler<T, K> where T : IRequest<K>
{
    private readonly ICommandHandler<T, K> handler;

    public CommandHandlerAdapter(ICommandHandler<T, K> handler)
    {
        this.handler = handler;
    }

    public Task<K> Handle(T request, CancellationToken cancellationToken)
    {
        return this.handler.Handle(request, cancellationToken);
    }
}