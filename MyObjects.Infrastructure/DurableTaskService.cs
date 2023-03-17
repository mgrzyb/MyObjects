using MediatR;

namespace MyObjects.Infrastructure;

public interface IDurableTaskService
{
    Task RetryDurableTask(Reference<DurableTask> taskRef);
}

public class DurableTaskService : IDurableTaskService
{
    private readonly IMediator mediator;
    private readonly IReadonlySession session;

    public DurableTaskService(IMediator mediator, IReadonlySession session)
    {
        this.mediator = mediator;
        this.session = session;
    }

    public async Task RetryDurableTask(Reference<DurableTask> taskRef)
    {
        var task = await this.session.TryResolve(taskRef);
        if (task == null)
            return;

        await this.mediator.Send(new RunDurableTask(taskRef, c => task.CreateHandler(c).Run(task.GetArgs())));
    }
}