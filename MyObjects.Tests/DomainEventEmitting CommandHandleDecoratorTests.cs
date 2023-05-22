using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using MyObjects.Infrastructure;
using MyObjects.NHibernate;
using MyObjects.Testing;
using NUnit.Framework;

namespace MyObjects.Tests;

public class DomainEventEmitting_CommandHandleDecoratorTests
{
    [Test]
    public async Task Given_Mo()
    {
        var a1 = new A();
        var a2 = new A();

        var rootLocator = new Mock<ISessionAggregateRootLocator>();
        rootLocator.Setup(l => l.GetAggregateRoots()).Returns(
            new [] { a1 }, 
            new [] { a2 }, 
            Enumerable.Empty<AggregateRoot>());

        var eventBus = new Mock<IDomainEventBus>();
        
        var handler = new DomainEventEmittingCommandHandlerDecorator<Foo, Unit>(
            new Mock<ICommandHandler<Foo, Unit>>().Object, 
            eventBus.Object, 
            rootLocator.Object);

        await handler.Handle(new Foo(), CancellationToken.None);
        
        eventBus.Verify(b => b.Publish(a1.DomainEvent));
        eventBus.Verify(b => b.Publish(a2.DomainEvent));
    }

    public class A : AggregateRoot
    {
        [Transient]
        public virtual AggregateCreated<A> DomainEvent { get; }

        public A()
        {
            this.Publish(this.DomainEvent = new AggregateCreated<A>(this));
        }
    }

    public class Foo : Command
    {
    }
}