using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

class ConvertInvalidReferenceToNotFoundDecorator : IHttpFunctionPipeline
{
    private readonly IHttpFunctionPipeline innerPipeline;

    public ConvertInvalidReferenceToNotFoundDecorator(IHttpFunctionPipeline innerPipeline)
    {
        this.innerPipeline = innerPipeline;
    }

    public async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try
        {
            return await this.innerPipeline.Run(action);
        }
        catch (InvalidReferenceException e)
        {
            return new NotFoundResult();
        }
    }
}