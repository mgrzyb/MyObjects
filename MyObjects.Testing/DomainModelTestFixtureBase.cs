using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;
using MyObjects.Infrastructure;

namespace MyObjects.Testing;

public class DomainModelTestFixtureBase<TAdvancedSession> : ContainerBasedTestFixture, IDomainEventBus
{
    private readonly ITestCodeRunner<TAdvancedSession> runner;

    private IList<IDomainEvent> domainEvents;
    protected IEnumerable<IDomainEvent> DomainEvents => this.domainEvents;

    protected DomainModelTestFixtureBase(ITestCodeRunner<TAdvancedSession> runner, Assembly modelAssembly, IEnumerable<IModule> modules)
        : base(modules.Concat(new IModule[]
        {
            new MediatorModule(), 
            new CommandsAndEventsModule<TAdvancedSession>(modelAssembly, true, false, true, false)
        }))
    {
        this.runner = runner;
    }

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
        private readonly DomainModelTestFixtureBase<TAdvancedSession> fixture;
        private readonly Func<IReadonlySession, Task<TEvent>> e;

        public EventHandlerRunner(DomainModelTestFixtureBase<TAdvancedSession> fixture, Func<IReadonlySession, Task<TEvent>> e)
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

    protected override void ConfigureTestScope(ContainerBuilder builder)
    {
        this.domainEvents = new List<IDomainEvent>();
        builder.RegisterInstance(this).As<IDomainEventBus>();
    }

    public Task Publish(IDomainEvent e)
    {
        this.domainEvents.Add(e);
        return Task.CompletedTask;
    }
}