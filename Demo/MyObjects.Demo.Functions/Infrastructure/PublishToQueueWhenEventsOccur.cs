using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using AutoMapper;
using Azure.Storage.Queues;
using MediatR;
using MyObjects.Infrastructure.Setup;
using Newtonsoft.Json;

namespace MyObjects.Demo.Functions.Infrastructure;

internal class StorageQueueAdapter<T> : IDomainEventHandler<T> where T : IDomainEvent
{
    private readonly IDurableQueue queue;
    private readonly IEnumerable<IDomainEventMessageMapper<T>> mappers;

    public StorageQueueAdapter(IDurableQueue queue, IEnumerable<IDomainEventMessageMapper<T>> mappers)
    {
        this.queue = queue;
        this.mappers = mappers;
    }

    public async Task Handle(T e, CancellationToken cancellationToken)
    {
        foreach (var mapper in this.mappers)
        {
            await this.queue.SendMessage(mapper.Map(e));
        }
    }
}

public class DomainEventMessageDto
{
}

internal interface IDomainEventMessageMapper<in TSource> where TSource : IDomainEvent
{
    DomainEventMessageDto Map(TSource source);
}

class AutoDomainEventMessageMapper<TSource> : IDomainEventMessageMapper<TSource> where TSource : IDomainEvent
{
    private readonly Mapper mapper;

    public AutoDomainEventMessageMapper(Mapper mapper)
    {
        this.mapper = mapper;
    }

    public DomainEventMessageDto Map(TSource source)
    {
        return this.mapper.Map<DomainEventMessageDto>(source);
    }
}

interface IDurableQueue
{
    Task SendMessage(object body);
}

public static class CommandHandlerRegistrationExtensions
{
    public static CommandHandlerRegistration FlushQueues(this CommandHandlerRegistration registration)
    {
        registration.Builder.RegisterGenericDecorator(
            typeof(QueueFlushingDecorator<,>),
            typeof(ICommandHandler<,>));
        
        return registration;
    }
}

class QueueFlushingDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : IRequest<TResult>
{
    private readonly ICommandHandler<TCommand, TResult> innerHandler;
    private readonly IEnumerable<BatchingQueueDecorator> batchingQueues;
    
    public QueueFlushingDecorator(ICommandHandler<TCommand, TResult> innerHandler, IEnumerable<BatchingQueueDecorator> batchingQueues)
    {
        this.innerHandler = innerHandler;
        this.batchingQueues = batchingQueues;
    }
    
    public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var result = await this.innerHandler.Handle(command, cancellationToken);
        
        foreach (var queue in this.batchingQueues)
        {
            await queue.Flush();
        }

        return result;
    }
}

class BatchingQueueDecorator : IDurableQueue
{
    private readonly int batchSize;
    private readonly IDurableQueue innerQueue;
    private readonly Queue<object> messages = new Queue<object>();

    public BatchingQueueDecorator(IDurableQueue innerQueue, int batchSize)
    {
        this.batchSize = batchSize;
        this.innerQueue = innerQueue;
    }
    public Task SendMessage(object body)
    {
        this.messages.Enqueue(body);
        return Task.CompletedTask;
    }

    public async Task Flush()
    {
        while(this.messages.Any())
        {
            var batch = new List<object>();
            for (var i = 0; i < this.batchSize && this.messages.TryDequeue(out var m) ; i++)
            {
                batch.Add(m);
            }
            await this.innerQueue.SendMessage(batch.ToArray());
        }
    }
}

class DurableStorageQueue : IDurableQueue
{
    private readonly string queueName;
    private readonly IDurableTaskQueue<(string, string)> durableTaskQueue;

    public DurableStorageQueue(IDurableTaskQueue<(string, string)> durableTaskQueue, string queueName)
    {
        this.queueName = queueName;
        this.durableTaskQueue = durableTaskQueue;
    }

    public Task SendMessage(object body)
    {
        var messageBody = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body)));
        return this.durableTaskQueue.Enqueue<Handler>((this.queueName, messageBody));
    }

    public class Handler : IDurableTaskHandler<(string, string)>
    {
        private readonly IIndex<string, QueueClient> queues;

        public Handler(IIndex<string, QueueClient> queues)
        {
            this.queues = queues;
        }

        public Task Run((string, string) args)
        {
            return this.queues[args.Item1].SendMessageAsync(args.Item2);
        }
    }
}