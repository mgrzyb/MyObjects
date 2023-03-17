using System.Threading.Tasks;
using MyObjects.Demo.Model.Orders;
using MyObjects.Infrastructure;

namespace MyObjects.Demo;

public class SendEmailWhenSalesOrderCreated : DomainEventHandler<AggregateCreated<SalesOrder>>
{
    private readonly DurableEmailSender emailSender;
    private readonly Durable<IEmailSender> durableEmailSender;

    public SendEmailWhenSalesOrderCreated(IDependencies dependencies, DurableEmailSender emailSender, Durable<IEmailSender> durableEmailSender) : base(dependencies)
    {
        this.emailSender = emailSender;
        this.durableEmailSender = durableEmailSender;
    }

    protected override Task Handle(AggregateCreated<SalesOrder> domainEvent)
    {
        return this.durableEmailSender.Enqueue(s => s.SendEmail(new Email {Subject = "Order created"}, "Maciek"));
        // return this.emailSender.EnqueueSendEmail(new Email {Subject = "Order created"}, "Maciek");
    }
}