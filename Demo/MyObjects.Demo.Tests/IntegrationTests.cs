using System.Threading.Tasks;
using Autofac;
using Moq;
using MyObjects.Demo.Model.Orders;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products.Commands;
using NUnit.Framework;

namespace MyObjects.Demo.UnitTests;

public class IntegrationTests : IntegrationTestFixture
{
    public IntegrationTests()
    {
        With(builder =>
        {
            builder.RegisterInstance(new Mock<INumberSequence>().Object).AsImplementedInterfaces();
            builder.RegisterType<EmailSender>().AsImplementedInterfaces();
            builder.RegisterType<DurableEmailSender>().AsSelf();
        });
    }

    [Test]
    public async Task Test()
    {
        var productRef = await this.When(new CreateProduct("Foo"));
        var orderRef = await this.When(new CreateSalesOrder
        {
            Lines = new[] {(productRef, 10m, 10m)}
        });
        await When(new CancelSalesOrder(orderRef));

        await Then(async s =>
        {
            var order = await s.Resolve(orderRef);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Canceled));
        });
    }
}

