using MediatR;
using MyObjects.Infrastructure;
using Newtonsoft.Json;

namespace MyObjects.Functions;

public class FunctionsBase<TResult>
{
    protected readonly IMediator Mediator;
    protected readonly IReadonlySession Session;
    protected readonly IFunctionPipeline<TResult> Pipeline;
    protected static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();
    
    public FunctionsBase(IDependencies dependencies)
    {
        this.Pipeline = dependencies.Pipeline;
        this.Mediator = dependencies.Mediator;
        this.Session = dependencies.Session;
        JsonSerializerSettings.Converters.Add(new NewtonsoftReferenceConverter());
    }

    public interface IDependencies
    {
        IMediator Mediator { get; }
        IReadonlySession Session { get; }
        IFunctionPipeline<TResult> Pipeline { get; }
    }
}