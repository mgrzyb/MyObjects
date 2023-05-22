using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;
using MyObjects.Demo.Functions;
using MyObjects.Infrastructure;
using MyObjects.Infrastructure.Setup;

namespace MyObjects.Testing;

public abstract class IntegrationTestFixtureBase<TAdvancedSession> : ContainerBasedTestFixture
{
    private readonly ITestCodeRunner<TAdvancedSession> runner;
    private readonly Assembly[] assemblies;

    protected IntegrationTestFixtureBase(ITestCodeRunner<TAdvancedSession> runner, params Assembly[] assemblies)
        : base()
    {
        this.runner = runner;
        this.assemblies = assemblies;
    }

    protected override IContainer CreateContainer(ContainerBuilder builder)
    {
        this.SetupMyObjectsRegistration(builder.AddMyObjects(this.assemblies)
                .UseDomainEventBus()
                .UseDurableTasks())
            .AddCommandHandlers<TAdvancedSession>(c => c
                .EmitDomainEvents()
                .RunInTransaction()
                .RunDurableTasks()
                .RunInLifetimeScope())
            .AddDomainEventHandlers();

        return builder.Build();
    }

    protected abstract MyObjectsRegistration<TAdvancedSession> SetupMyObjectsRegistration(
        MyObjectsRegistration registration);

    protected Task When(Command command)
    {
        return this.Context.Resolve<IMediator>().Send(command);
    }

    protected Task<TResponse> When<TResponse>(Command<TResponse> command)
    {
        return this.Context.Resolve<IMediator>().Send(command);
    }

    protected Task Then(Func<IReadonlySession<TAdvancedSession>, Task> then)
    {
        return this.RunInLifetimeScope(scope => this.runner.Run(scope, async session =>
        {
            await then(session);
            return string.Empty;
        }));
    }
}