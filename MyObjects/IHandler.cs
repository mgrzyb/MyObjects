using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects
{
    public interface IHandler<in TRequest, TResult> where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest command, CancellationToken cancellationToken);
    }
}