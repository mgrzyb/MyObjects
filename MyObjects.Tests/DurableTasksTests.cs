using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MyObjects.Infrastructure;
using MyObjects.Testing.NHibernate;
using NUnit.Framework;

namespace MyObjects.Tests;

[TestFixture]
public class DurableTasksTests : IntegrationTestFixture
{
    public DurableTasksTests() 
        : base(builder => builder.AddEntity<DurableTask>(), typeof(DurableTasksTests).Assembly)
    {
    }

    [Test]
    public async Task Test()
    {
        await this.Context.Resolve<IMediator>().Send(new ConfirmOrder());
    }
}

public class ConfirmOrder : Command
{
    public class Handler : CommandHandler<ConfirmOrder>
    {
        private readonly IDurableTaskQueue<Email> durableTaskQueue;

        public Handler(IDependencies dependencies, IDurableTaskQueue<Email> durableTaskQueue) : base(dependencies)
        {
            this.durableTaskQueue = durableTaskQueue;
        }

        public override async Task Handle(ConfirmOrder command, CancellationToken cancellationToken)
        {
            await this.durableTaskQueue.Enqueue<SendEmail>(new Email { Subject = "Foo" });
        }
    }
}

public class Email
{
    public string Subject { get; set; }
}

class SendEmail : IDurableTask<Email>
{
    public async Task Run(Email args)
    {
        Console.WriteLine("Sending email");
    }
}