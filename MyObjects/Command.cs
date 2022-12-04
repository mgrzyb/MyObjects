using MediatR;

namespace MyObjects
{
    public class Command : IRequest
    {
    }

    public class Command<TResponse> : IRequest<TResponse>
    {
    }
}