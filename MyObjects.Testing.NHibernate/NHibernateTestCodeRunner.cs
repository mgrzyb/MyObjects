using Autofac;
using MyObjects.NHibernate;

namespace MyObjects.Testing.NHibernate;

public class NHibernateTestCodeRunner : ITestCodeRunner<global::NHibernate.ISession>
{
    public Task Run(ILifetimeScope container, Func<IReadonlySession<global::NHibernate.ISession>, Task> a)
    {
        return a(container.Resolve<IReadonlySession<global::NHibernate.ISession>>());
    }

    public Task<K> Run<K>(ILifetimeScope container, Func<IReadonlySession<global::NHibernate.ISession>, Task<K>> a)
    {
        return a(container.Resolve<IReadonlySession<global::NHibernate.ISession>>());
    }

    public Task Run(ILifetimeScope container, Func<ISession<global::NHibernate.ISession>, Task> a)
    {
        var transactionRunner = container.Resolve<ITransactionRunner>();
        return transactionRunner.RunInTransaction(() => a(container.Resolve<ISession<global::NHibernate.ISession>>()));
    }
    
    public Task<K> Run<K>(ILifetimeScope container, Func<ISession<global::NHibernate.ISession>, Task<K>> a)
    {
        var transactionRunner = container.Resolve<ITransactionRunner>();
        return transactionRunner.RunInTransaction(() => a(container.Resolve<ISession<global::NHibernate.ISession>>()));
    }    
}