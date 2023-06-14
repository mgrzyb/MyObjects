using MediatR;

namespace MyObjects.Infrastructure;

class DurableTasksRunningDecorator<TRequest, TResult> : ICommandHandler<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly DurableTaskQueueFactory taskQueueFactory;
    private readonly ICommandHandler<TRequest, TResult> innerHandler;
    private readonly IMediator mediator;

    public DurableTasksRunningDecorator(ICommandHandler<TRequest, TResult> innerHandler, DurableTaskQueueFactory taskQueueFactory, IMediator mediator)
    {
        this.mediator = mediator;
        this.innerHandler = innerHandler;
        this.taskQueueFactory = taskQueueFactory;
    }

    public async Task<TResult> Handle(TRequest command, CancellationToken cancellationToken)
    {
        var result = await this.innerHandler.Handle(command, cancellationToken);

        while (this.taskQueueFactory.TryDequeueFromQueue(out var task))
        {
            try
            {
                await this.mediator.Send(new RunDurableTask(task.Item1, task.Item2), cancellationToken);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception when running durable task: {task.Item1}: {e.Message}");
            }
        }

        return result;
    }
}