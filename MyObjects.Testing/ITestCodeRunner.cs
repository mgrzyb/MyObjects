using System;
using System.Threading.Tasks;
using Autofac;

namespace MyObjects;

public interface ITestCodeRunner<TAdvancedSession>
{
    Task<K> Run<K>(ILifetimeScope container, Func<IReadonlySession<TAdvancedSession>, Task<K>> a);
    Task<K> Run<K>(ILifetimeScope container, Func<ISession<TAdvancedSession>, Task<K>> a);
}