using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using NHibernate;

namespace MyObjects
{
    public interface IApiRequestHandler
    {
    }

    public interface IApiRequestHandler<in TRequest, TResponse> : IApiRequestHandler
    {
        Task<ActionResult<TResponse>> HandleRequest(TRequest request);
    }

    public interface IApiRequestHandler<in TRequest> : IApiRequestHandler
    {
        Task<IActionResult> HandleRequest(TRequest request);
    }

    [ApiRequestHandler]
    [Route("api/[controller]")]
    public abstract class ApiRequestHandler : Controller
    {
        protected readonly IMediator Mediator;
        protected readonly ISession Session;
//        private readonly TelemetryClient telemetry;
        
        public ApiRequestHandler(IDependencies dependencies)
        {
            this.Session = dependencies.Session;
            this.Mediator = dependencies.Mediator;
  //          this.telemetry = dependencies.TelemetryClient;
        }

        /*
        protected new ActionResult BadRequest(ApiErrorResponse response)
        {
            return base.BadRequest(response);
        }
        */

        /*public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            try
            {
                var properties = context.ActionArguments
                    .SelectMany(a => Diagnostics.GetFormattedPropertyValues(a.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                this.telemetry.TrackEvent("ApiRequest", properties);
            }
            catch (Exception e)
            {
                this.telemetry.TrackException(e);
            }
        }*/

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.Exception is StaleObjectStateException)
            {
                context.Result = this.StatusCode((int) HttpStatusCode.Conflict);
                context.ExceptionHandled = true;
            }
        }
        
        public interface IDependencies
        {
            ISession Session { get; }
            IMediator Mediator { get; }
//            TelemetryClient TelemetryClient { get; }
        }
    }
    
    public class ApiRequestHandlerAttribute : Attribute, IControllerModelConvention
    {
        public void Apply(ControllerModel model)
        {
            var apiRequestInterface = model.ControllerType.GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(IApiRequestHandler<,>)));
            if (apiRequestInterface.Any() == false)
            {
                apiRequestInterface = model.ControllerType.GetInterfaces().Where(i => i.IsClosedTypeOf(typeof(IApiRequestHandler<>)));
            }
            var requestType = apiRequestInterface.Single().GetGenericArguments()[0];

            model.ControllerName = requestType.Name.EndsWith("Request") ? requestType.Name.Remove(requestType.Name.Length - "Request".Length) : requestType.Name;
        }
    }
}