using System;
using System.Threading.Tasks;

namespace MyObjects.Demo;

public interface IEmailSender
{
    Task<int> SendEmail(Email email, string recipient);
}

public class EmailSender : IEmailSender
{
    private static int count = 0;
    public Task<int> SendEmail(Email email, string recipient)
    {
        var c = count++;
        Console.WriteLine($"Sending email ({c}): {email.Subject}");
        if (c%3 == 0 || c%3 == 1)
            throw new InvalidOperationException($"Error when sending email ({c})");
        else
            Console.WriteLine($"Email sent ({c}): {email.Subject}");

        return Task.FromResult(c);
    }
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

    public class SendEmailHandler : IDurableTaskHandler<Tuple<Email, string>>
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