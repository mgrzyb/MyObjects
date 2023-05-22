using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Autofac.Diagnostics;
using NUnit.Framework;

namespace MyObjects.Testing;

public class ContainerBasedTestFixture
{
    private Lazy<IContainer> container;
    public IComponentContext Context => this.container.Value;
    
    private readonly IList<Action<ContainerBuilder>> scopeConfigActions = new List<Action<ContainerBuilder>>();

    protected ContainerBasedTestFixture()
    {
    }

    [SetUp]
    public void BeginTestScope()
    {
        this.container = new Lazy<IContainer>(() =>
        {
            var builder = new ContainerBuilder();
            foreach (var action in this.scopeConfigActions)
            {
                action(builder);
            }

            return this.CreateContainer(builder);
        });
    }

    [TearDown]
    public void DisposeTestScope()
    {
        this.container.Value.Dispose();
        this.container = null;
    }

    protected virtual IContainer CreateContainer(ContainerBuilder builder)
    {
        return builder.Build();
    }

    protected virtual void ConfigureTestScope(ContainerBuilder builder)
    {
    }

    protected ContainerBasedTestFixture With(Action<ContainerBuilder> configAction)
    {
        this.scopeConfigActions.Add(configAction);
        return this;
    }

    protected Task<K> RunInLifetimeScope<K>(Func<ILifetimeScope, Task<K>> given)
    {
        using (var scope = this.container.Value.BeginLifetimeScope(ConfigureActionScope))
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