using System;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Azure.Storage.Queues;
using Microsoft.Azure.WebJobs;
using MyObjects.Infrastructure;

namespace MyObjects.Demo.Functions.Infrastructure;

public class DurableTaskFunctions
{
    private readonly IDurableTaskService durableTaskService;

    public DurableTaskFunctions(IDurableTaskService durableTaskService)
    {
        this.durableTaskService = durableTaskService;
    }

    [FunctionName("DurableTaskRetry")]
    public async Task RunAsync([QueueTrigger("durable-tasks", Connection = "DurableTaskQueue")] string myQueueItem)
    {
        Console.WriteLine("Attempt to retry a task: " + myQueueItem);

        var taskRef = new Reference<DurableTask>(Int32.Parse(myQueueItem));
        if (taskRef == null)
            return;

        await this.durableTaskService.RetryDurableTask(taskRef);
    }

    public class ScheduleRetryWhenDurableTaskCreated : DomainEventHandler<AggregateCreated<DurableTask>>
    {
        private readonly QueueClient queue;

        public ScheduleRetryWhenDurableTaskCreated(IDependencies dependencies, IIndex<string, QueueClient> queues) : base(dependencies)
        {
            this.queue = queues["durable-tasks"];
        }

        protected override Task Handle(AggregateCreated<DurableTask> domainEvent)
        {
            Console.WriteLine("Scheduling retry of task: " + domainEvent.Root);
            this.queue.SendMessage(Convert.ToBase64String(Encoding.UTF8.GetBytes(domainEvent.Root.GetReference().Id.ToString())), visibilityTimeout: TimeSpan.FromSeconds(30));
            return Task.CompletedTask;
        }
    }    
}