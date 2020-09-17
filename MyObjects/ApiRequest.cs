using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MyObjects
{
    public interface IApiRequest : IApiRequest<Unit>
    {
    }

    public class ApiRequest : IApiRequest
    {
        public abstract class Handler<TRequest> : ApiRequestHandler, IApiRequestHandler<TRequest> where TRequest : IApiRequest
        {
            public Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            public Task<IActionResult> HandleRequest([FromBody] TRequest request)
            {
                return this.Handle(request);
            }

            protected abstract Task<IActionResult> Handle(TRequest request);
        }
    }

    public interface IApiRequest<TResponse>
    {
    }

    public class ApiRequest<TResponse> : IApiRequest<TResponse>
    {
        public abstract class Handler<TRequest> : ApiRequestHandler, IApiRequestHandler<TRequest, TResponse>
        {
            public Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            public Task<ActionResult<TResponse>> HandleRequest([FromBody]TRequest request)
            {
                return this.Handle(request);
            }

            protected abstract Task<ActionResult<TResponse>> Handle(TRequest request);
        }
    }
}