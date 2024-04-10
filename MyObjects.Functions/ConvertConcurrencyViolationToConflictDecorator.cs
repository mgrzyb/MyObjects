using Microsoft.AspNetCore.Mvc;

namespace MyObjects.Functions;

class ConvertConcurrencyViolationToConflictDecorator<TContext> : IFunctionPipeline<TContext, IActionResult>
{
    private readonly IFunctionPipeline<TContext, IActionResult> innerPipeline;

    public ConvertConcurrencyViolationToConflictDecorator(IFunctionPipeline<TContext, IActionResult> innerPipeline)
    {
        this.innerPipeline = innerPipeline;
    }

    public async Task<IActionResult> Run(TContext context, Func<Task<IActionResult>> action)
    {
        try
        {
            return await this.innerPipeline.Run(context, action);
        }
        catch (ConcurrencyViolationException e)
        {
            return new ConflictResult();
        }
    }
}