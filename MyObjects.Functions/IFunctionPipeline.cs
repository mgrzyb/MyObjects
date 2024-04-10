namespace MyObjects.Functions;

public interface IFunctionPipeline<TArg, TResult>
{
    Task<TResult> Run(TArg arg, Func<Task<TResult>> action);
}

class FunctionPipeline<TArg, TResult> : IFunctionPipeline<TArg, TResult>
{
    public Task<TResult> Run(TArg arg, Func<Task<TResult>> action)
    {
        return action();
    }
}
