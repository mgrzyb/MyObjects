using Autofac;
using Newtonsoft.Json;

namespace MyObjects.Infrastructure;

public class DurableTask : AggregateRoot
{
    public virtual string Type { get; protected set; }
    public virtual string SerializedArgs { get; protected set; }

    protected DurableTask()
    {
    }

    public DurableTask(string? taskType, string args)
    {
        this.Type = taskType;
        this.SerializedArgs = args;
    }
}

internal interface IDurableTaskQueueFactory
{
    IDurableTaskQueue<T> Create<T>();
}

internal class DurableTaskQueueFactory : IDurableTaskQueueFactory
{
    private readonly ISession session;
    private readonly Queue<(Reference<DurableTask>, Func<IComponentContext, Task>)> innerQueue = new();

    public DurableTaskQueueFactory(ISession session)
    {
        this.session = session;
    }

    public IDurableTaskQueue<T> Create<T>()
    {
        return new DurableTaskQueue<T>(this.session, this.innerQueue);
    }

    public bool TryDequeue(out (Reference<DurableTask>, Func<IComponentContext, Task>) task)
    {
        return this.innerQueue.TryDequeue(out task);
    }
}

class RunDurableTask : Command
{
    public readonly Reference<DurableTask> TaskRef;
    public readonly Func<IComponentContext, Task> Run;

    public RunDurableTask(Reference<DurableTask> taskRef, Func<IComponentContext, Task> run)
    {
        TaskRef = taskRef;
        Run = run;
    }

    public class Handler : CommandHandler<RunDurableTask>
    {
        private readonly IComponentContext context;

        public Handler(IDependencies dependencies, IComponentContext context) : base(dependencies)
        {
            this.context = context;
        }

        public override async Task Handle(RunDurableTask command, CancellationToken cancellationToken)
        {
            await command.Run(this.context);
            await this.Session.Delete(await this.Session.Resolve(command.TaskRef));
        }
    }
}

internal class DurableTaskQueue<TArgs> : IDurableTaskQueue<TArgs>
{
    private readonly ISession session;
    private readonly Queue<(Reference<DurableTask>, Func<IComponentContext, Task>)> innerQueue;
        
    public DurableTaskQueue(ISession session, Queue<(Reference<DurableTask>, Func<IComponentContext, Task>)> innerQueue)
    {
        this.session = session;
        this.innerQueue = innerQueue;
    }

    public async Task Enqueue<T>(TArgs args) where T : IDurableTask<TArgs>
    {
        var taskRef = await this.session.Save(new DurableTask(typeof(T).FullName, JsonConvert.SerializeObject(args)));
        this.innerQueue.Enqueue((taskRef, (IComponentContext c) => c.Resolve<T>().Run(args)));
    }
}