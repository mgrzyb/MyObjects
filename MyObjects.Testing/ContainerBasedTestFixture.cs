using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Diagnostics;
using NUnit.Framework;

namespace MyObjects.Testing;

public class ContainerBasedTestFixture
{
    private readonly IContainer container;
    private ILifetimeScope testScope;
    protected IComponentContext Context => this.testScope;
    
    private readonly IList<Action<ContainerBuilder>> scopeConfigActions = new List<Action<ContainerBuilder>>();
    protected ContainerBasedTestFixture(IEnumerable<IModule> modules)
    {
        var builder = new ContainerBuilder();
        foreach (var module in modules)
        {
            builder.RegisterModule(module);
        }

        this.container = builder.Build();
        
        /*var tracer = new DefaultDiagnosticTracer();
        tracer.OperationCompleted += (sender, args) =>
        {
            Trace.WriteLine(args.TraceContent);
        };

        // Subscribe to the diagnostics with your tracer.
        this.Container.SubscribeToDiagnostics(tracer);*/
    }

    [SetUp]
    public void BeginTestScope()
    {
        this.testScope = this.container.BeginLifetimeScope(this.ConfigureTestScope);
    }

    protected virtual void ConfigureTestScope(ContainerBuilder builder)
    {
    }

    [TearDown]
    public void DisposeTestScope()
    {
        this.testScope.Dispose();
    }

    protected ContainerBasedTestFixture With(Action<ContainerBuilder> configAction)
    {
        this.scopeConfigActions.Add(configAction);
        return this;
    }

    protected Task<K> RunInLifetimeScope<K>(Func<ILifetimeScope, Task<K>> given)
    {
        using (var scope = this.testScope.BeginLifetimeScope(ConfigureActionScope))
        {
            return given(scope);
        }
    }

    private void ConfigureActionScope(ContainerBuilder builder)
    {
        foreach (var action in this.scopeConfigActions)
        {
            action(builder);
        }
    }
}