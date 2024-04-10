using MediatR;
using Microsoft.AspNetCore.Http;
using MyObjects.Infrastructure;
using Newtonsoft.Json;

namespace MyObjects.Functions;

public class FunctionsBase<TContext, TResult>
{
    protected readonly IMediator Mediator;
    protected readonly IReadonlySession Session;
    protected readonly IFunctionPipeline<TContext, TResult> Pipeline;
    protected readonly IEnumerable<IFunctionArgumentResolver<TContext>> ArgumentResolvers;

    protected static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();
    
    public FunctionsBase(IDependencies dependencies)
    {
        this.Pipeline = dependencies.Pipeline;
        this.Mediator = dependencies.Mediator;
        this.Session = dependencies.Session;
        this.ArgumentResolvers = dependencies.ArgumentResolvers;
        JsonSerializerSettings.Converters.Add(new NewtonsoftReferenceConverter());
    }

    public interface IDependencies
    {
        IMediator Mediator { get; }
        IReadonlySession Session { get; }
        IFunctionPipeline<TContext, TResult> Pipeline { get; }
        IEnumerable<IFunctionArgumentResolver<TContext>> ArgumentResolvers { get; }
    }
}