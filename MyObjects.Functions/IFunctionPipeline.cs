namespace MyObjects.Functions;

public interface IFunctionPipeline<TResult>
{
    Task<TResult> Run(Func<Task<TResult>> action);
}

class FunctionPipeline<TResult> : IFunctionPipeline<TResult>
{
    public Task<TResult> Run(Func<Task<TResult>> action)
    {
        return action();
    }
}