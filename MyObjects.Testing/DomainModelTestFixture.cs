using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using MediatR;
using MyObjects.Demo.Functions;
using MyObjects.Infrastructure;
using MyObjects.Infrastructure.Setup;

namespace MyObjects.Testing;

public abstract class DomainModelTestFixture<TAdvancedSession> : ContainerBasedTestFixture, IDomainEventBus
{

    private IList<IDomainEvent> domainEvents;
    protected IEnumerable<IDomainEvent> DomainEvents => this.domainEvents;

    private readonly Assembly[] assemblies;
    private readonly ITestCodeRunner<TAdvancedSession> runner;
    
    protected DomainModelTestFixture(ITestCodeRunner<TAdvancedSession> runner, params Assembly[] assemblies)
    {
        this.runner = runner;
        this.assemblies = assemblies;
    }

    protected override IContainer CreateContainer(ContainerBuilder builder)
    {
        this.SetupMyObjectsRegistration(builder.AddMyObjects(this.assemblies))
            .AddCommandHandlers(c => c
                .EmitDomainEvents())
            .AddDomainEventHandlers();

        this.domainEvents = new List<IDomainEvent>();
        builder.RegisterInstance(this).As<IDomainEventBus>();
        
        return builder.Build();
    }

    protected abstract MyObjectsRegistration<TAdvancedSession> SetupMyObjectsRegistration(
        MyObjectsRegistration registration);
    
    protected Task<K> Given<K>(Func<ISession<TAdvancedSession>, Task<K>> given)
    {
        return this.RunInLifetimeScope(scope => this.runner.Run<K>(scope, given));
    }

    protected Task<K> When<K>(Func<ISession<TAdvancedSession>, Task<K>> when)
    {
        return this.RunInLifetimeScope(scope => this.runner.Run<K>(scope, when));
    }

    protected Task<K> When<K>(Func<IComponentContext, ISession<TAdvancedSession>, Task<K>> when)
    {
        return this.RunInLifetimeScope(scope => this.runner.Run<K>(scope, session => when(scope, session)));
    }

    protected Task When(Command command)
    {
        return this.RunInLifetimeScope(scope => scope.Resolve<IMediator>().Send(command));
    }

    protected Task<TResponse> When<TResponse>(Command<TResponse> command)
    {
        return this.RunInLifetimeScope(scope => scope.Resolve<IMediator>().Send(command));
    }

    protected EventHandlerRunner<TEvent> When<TEvent>(Func<IReadonlySession, Task<TEvent>> e) where TEvent : IDomainEvent
    {
        return new EventHandlerRunner<TEvent>(this, e);
    }

    protected Task Then(Func<IReadonlySession<TAdvancedSession>, Task> then)
    {
        return this.RunInLifetimeScope(scope => this.runner.Run(scope, async session =>
        {
            await then(session);
            return string.Empty;
        }));
    }

    protected class EventHandlerRunner<TEvent> where TEvent : IDomainEvent
    {
        private readonly DomainModelTestFixture<TAdvancedSession> fixture;
        private readonly Func<IReadonlySession, Task<TEvent>> e;

        public EventHandlerRunner(DomainModelTestFixture<TAdvancedSession> fixture, Func<IReadonlySession, Task<TEvent>> e)
        {
            this.fixture = fixture;
            this.e = e;
        }

        public async Task IsHandledBy<THandler>() where THandler : IDomainEventHandler<TEvent>
        {
            await this.fixture.When(async (c, s) =>
            {
                var e = await this.e(s);
                await c.Resolve<THandler>().Handle(e, CancellationToken.None);
                return string.Empty;
            });
        }
    }

    public Task Publish(IDomainEvent e)
    {
        this.domainEvents.Add(e);
        return Task.CompletedTask;
    }
}