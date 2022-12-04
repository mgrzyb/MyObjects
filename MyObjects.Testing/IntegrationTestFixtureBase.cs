
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using MediatR;
using MyObjects.Infrastructure;

namespace MyObjects.Testing;

public class IntegrationTestFixtureBase<TAdvancedSession> : ContainerBasedTestFixture
{
    private readonly ITestCodeRunner<TAdvancedSession> runner;

    protected IntegrationTestFixtureBase(ITestCodeRunner<TAdvancedSession> runner, Assembly modelAssembly, IEnumerable<IModule> modules)
        : base(modules.Concat(new IModule[]
        {
            new MediatorModule(), 
            new CommandsAndEventsModule<TAdvancedSession>(modelAssembly, true, true, true, false),
            new DurableTasksModule(modelAssembly)
        }))
    {
        this.runner = runner;
    }
    
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