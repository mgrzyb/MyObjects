using Autofac;

namespace MyObjects.Testing.NHibernate;

public class NHibernateTestCodeRunner : ITestCodeRunner<global::NHibernate.ISession>
{
    public Task<K> Run<K>(ILifetimeScope container, Func<IReadonlySession<global::NHibernate.ISession>, Task<K>> a)
    {
        return a(container.Resolve<IReadonlySession<global::NHibernate.ISession>>());
    }

    public async Task<K> Run<K>(ILifetimeScope container, Func<ISession<global::NHibernate.ISession>, Task<K>> a)
    {
        var s = container.Resolve<global::NHibernate.ISession>();
        using var t = s.BeginTransaction();
        var result = await a(container.Resolve<ISession<global::NHibernate.ISession>>());
        await t.CommitAsync();
        return result;
    }
}