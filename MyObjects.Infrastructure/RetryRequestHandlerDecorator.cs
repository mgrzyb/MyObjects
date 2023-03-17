using MediatR;

namespace MyObjects.Infrastructure;


public class RetryRequestHandlerDecorator<T, K> : IRequestHandler<T, K> where T : IRequest<K>
{
    private readonly IRequestHandler<T, K> inner;

    public RetryRequestHandlerDecorator(IRequestHandler<T, K> inner)
    {
        this.inner = inner;
    }

    public async Task<K> Handle(T request, CancellationToken cancellationToken)
    {
        try
        {
            return await this.inner.Handle(request, cancellationToken);
        }
        catch
        {
            Console.WriteLine("Retrying command: " + request);
            return await this.inner.Handle(request, cancellationToken);
        }
    }
}