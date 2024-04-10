namespace MyObjects.Functions;

public interface IFunctionArgumentResolver<TContext>
{
    Task<TValue> TryResolve<TValue>(string name, TContext context);
}
