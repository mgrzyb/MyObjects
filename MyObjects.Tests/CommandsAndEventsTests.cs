using System.Threading;
using System.Threading.Tasks;
using MyObjects.Infrastructure;
using MyObjects.Testing.NHibernate;
using NUnit.Framework;

namespace MyObjects.Tests;

[TestFixture]
public class CommandsAndEventsTests : NHibernateIntegrationTestFixture
{
    public CommandsAndEventsTests() : base(typeof(CommandsAndEventsTests).Assembly)
    {
    }

    [Test]
    public async Task Test()
    {
        var aRef = await When(new CreateA());

        await Then(async s => {
            var a = await s.Resolve(aRef);
            Assert.That(a.Foo, Is.EqualTo("Event Handled"));
            Assert.That(a.Bar, Is.EqualTo("Event Handled"));
        });
    }

    class A : AggregateRoot
    {
        public virtual string Foo { get; protected set; }
        public virtual string Bar { get; set; }

        public virtual void UpdateFoo(string foo)
        {
            this.Foo = foo;
            this.Publish(new FooUpdated { A = this });
        }
    }

    class FooUpdated : IDomainEvent
    {
        public A A { get; init; }
    }

    class CreateA : Command<Reference<A>>
    {
        public class Handler : CommandHandler<CreateA, Reference<A>>
        {
            public Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            public override async Task<Reference<A>> Handle(CreateA command, CancellationToken cancellationToken)
            {
                return await this.Session.Save(new A());
            }
        }
    }

    class UpdateFooWhenACreated : DomainEventHandler<AggregateCreated<A>>
    {
        public UpdateFooWhenACreated(IDependencies dependencies) : base(dependencies)
        {
        }

        protected override async Task Handle(AggregateCreated<A> domainEvent)
        {
            domainEvent.Root.UpdateFoo("Event Handled");
        }
    }

    class UpdateAWhenFooUpdated : DomainEventHandler<FooUpdated>
    {
        public UpdateAWhenFooUpdated(IDependencies dependencies) : base(dependencies)
        {
        }

        protected override async Task Handle(FooUpdated domainEvent)
        {
            domainEvent.A.Bar = "Event Handled";
        }
    }
}