using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

public interface IHttpFunctionPipeline
{
    Task<IActionResult> Run(Func<Task<IActionResult>> action);
}

class HttpFunctionPipeline : IHttpFunctionPipeline
{
    public Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        return action();
    }
}