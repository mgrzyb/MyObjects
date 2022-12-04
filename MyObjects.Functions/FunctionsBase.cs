using MediatR;

namespace MyObjects.Functions;

public class FunctionsBase
{
    protected readonly IMediator Mediator;
    protected readonly IReadonlySession Session;
    protected readonly IHttpFunctionPipeline HttpPipeline;

    public FunctionsBase(IDependencies dependencies)
    {
        this.HttpPipeline = dependencies.HttpPipeline;
        this.Mediator = dependencies.Mediator;
        this.Session = dependencies.Session;
    }

    public interface IDependencies
    {
        IMediator Mediator { get; }
        IReadonlySession Session { get; }
        IHttpFunctionPipeline HttpPipeline { get; }
    }
}