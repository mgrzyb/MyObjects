using MediatR;

namespace MyObjects.Functions;

public class FunctionsBase<TResult>
{
    protected readonly IMediator Mediator;
    protected readonly IReadonlySession Session;
    protected readonly IFunctionPipeline<TResult> Pipeline;

    public FunctionsBase(IDependencies dependencies)
    {
        this.Pipeline = dependencies.Pipeline;
        this.Mediator = dependencies.Mediator;
        this.Session = dependencies.Session;
    }

    public interface IDependencies
    {
        IMediator Mediator { get; }
        IReadonlySession Session { get; }
        IFunctionPipeline<TResult> Pipeline { get; }
    }
}