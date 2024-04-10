using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

public interface IOneOf
{
    public object Value { get; }
}

public class OneOf<T1, T2> : IOneOf
{
    public object Value { get; }
    
    public OneOf(T1 value)
    {
        this.Value = value;
    }
    
    public OneOf(T2 value)
    {
        this.Value = value;
    }

    public static implicit operator OneOf<T1, T2>(T1 value)
    {
        return new OneOf<T1, T2>(value);
    }

    public static implicit operator OneOf<T1, T2>(T2 value)
    {
        return new OneOf<T1, T2>(value);
    }
}

public class OneOf<T1, T2, T3> : IOneOf
{
    public object Value { get; }
    
    public OneOf(T1 value)
    {
        this.Value = value;
    }
    
    public OneOf(T2 value)
    {
        this.Value = value;
    }

    public OneOf(T3 value)
    {
        this.Value = value;
    }

    public static implicit operator OneOf<T1, T2, T3>(T1 value)
    {
        return new OneOf<T1, T2, T3>(value);
    }

    public static implicit operator OneOf<T1, T2, T3>(T2 value)
    {
        return new OneOf<T1, T2, T3>(value);
    }
    
    public static implicit operator OneOf<T1, T2, T3>(T3 value)
    {
        return new OneOf<T1, T2, T3>(value);
    }
}

public interface IHttpFunctionResult
{
    IActionResult ToActionResult();
}

public static class HttpOk
{
    public static HttpOk<T> WithValue<T>(T value)
    {
        return new HttpOk<T>(value);
    }
}

public class HttpOk<T> : IHttpFunctionResult
{
    public T Value { get; }
        
    public HttpOk(T value)
    {
        this.Value = value;
    }
        
    public IActionResult ToActionResult()
    {
        return new OkObjectResult(this.Value);
    }
}

public class HttpCreated : IHttpFunctionResult
{
    public string Location { get; }
    
    public HttpCreated(string location)
    {
        this.Location = location;
    }
    public IActionResult ToActionResult()
    {
        return new CreatedResult(this.Location, null);
    }
}

public class HttpConflict : IHttpFunctionResult
{
    public IActionResult ToActionResult()
    {
        return new ConflictResult();
    }
}

public class HttpUnauthorized : IHttpFunctionResult
{
    public IActionResult ToActionResult()
    {
        return new UnauthorizedResult();
    }
}

public class HttpFunctionsBase : FunctionsBase<HttpRequest, IActionResult>
{
    public HttpFunctionsBase(IDependencies dependencies) : base(dependencies)
    {
    }

    protected Task<IActionResult> Run(HttpRequest req, Func<Task<IActionResult>> f)
    {
        return this.Pipeline.Run(req, f);
    }

    protected IActionResult CreateActionResult(IActionResult result)
    {
        return result;
    }
    
    protected IActionResult CreateActionResult(IHttpFunctionResult result)
    {
        return result.ToActionResult();
    }

    protected IActionResult CreateActionResult<T1, T2>(OneOf<T1, T2> result) where T1 : IHttpFunctionResult where T2 : IHttpFunctionResult
    {
        return ((IHttpFunctionResult) result.Value).ToActionResult();
    }

    protected IActionResult CreateActionResult<T1, T2, T3>(OneOf<T1, T2, T3> result) where T1 : IHttpFunctionResult where T2 : IHttpFunctionResult where T3 : IHttpFunctionResult
    {
        return ((IHttpFunctionResult) result.Value).ToActionResult();
    }

    protected async Task<T> ResolveParameterValue<T>(HttpRequest req, string name)
    {
        foreach (var resolver in this.ArgumentResolvers)
        {
            var value = await resolver.TryResolve<T>(name, req);
            if (!Equals(value, default(T)))
            {
                return value;
            }
        }
        return default;
    }
}