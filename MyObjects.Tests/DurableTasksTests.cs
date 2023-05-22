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
public class DurableTasksTests : NHibernateIntegrationTestFixture
{
    public DurableTasksTests() 
        : base(typeof(DurableTasksTests).Assembly)
    {
        With(builder => builder.RegisterType<DurableEmailSender>().AsSelf());
    }

    [Test]
    public async Task Test()
    {
        await When(new ConfirmOrder());
    }
}

public class ConfirmOrder : Command
{
    public class Handler : CommandHandler<ConfirmOrder>
    {
        private readonly DurableEmailSender emailSender;

        public Handler(IDependencies dependencies, DurableEmailSender emailSender) : base(dependencies)
        {
            this.emailSender = emailSender;
        }

        public override async Task Handle(ConfirmOrder command, CancellationToken cancellationToken)
        {
            await this.emailSender.EnqueueSendEmail(new Email {Subject = "Hello world!"}, "macio@example.com");
        }
    }
}

public interface IEmailSender
{
    Task SendEmail(Email email, string recipient);
}

public class DurableEmailSender
{
    private readonly IDurableTaskQueue<Tuple<Email, string>> queue;

    public DurableEmailSender(IDurableTaskQueue<Tuple<Email, string>> queue)
    {
        this.queue = queue;
    }

    public Task EnqueueSendEmail(Email email, string recipient)
    {
        return this.queue.Enqueue<SendEmailHandler>(new Tuple<Email, string>(email, recipient));
    }

    private class SendEmailHandler : IDurableTaskHandler<Tuple<Email, string>>
    {
        private readonly IEmailSender service;

        public SendEmailHandler(IEmailSender service)
        {
            this.service = service;
        }

        public async Task Run(Tuple<Email, string> args)
        {
            this.service.SendEmail(args.Item1, args.Item2);
        }
    }
}

public class Email
{
    public string Subject { get; set; }
}