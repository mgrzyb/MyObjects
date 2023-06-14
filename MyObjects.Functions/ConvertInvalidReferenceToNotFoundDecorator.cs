using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

class ConvertInvalidReferenceToNotFoundDecorator : IFunctionPipeline<IActionResult>
{
    private readonly IFunctionPipeline<IActionResult> innerPipeline;

    public ConvertInvalidReferenceToNotFoundDecorator(IFunctionPipeline<IActionResult> innerPipeline)
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