using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

class ConvertConcurrencyViolationToConflictDecorator : IFunctionPipeline<IActionResult>
{
    private readonly IFunctionPipeline<IActionResult> innerPipeline;

    public ConvertConcurrencyViolationToConflictDecorator(IFunctionPipeline<IActionResult> innerPipeline)
    {
        this.innerPipeline = innerPipeline;
    }

    public async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try
        {
            return await this.innerPipeline.Run(action);
        }
        catch (ConcurrencyViolationException e)
        {
            return new ConflictResult();
        }
    }
}