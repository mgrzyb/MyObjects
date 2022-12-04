namespace MyObjects.NHibernate;

class NHibernateTransactionRunner : ITransactionRunner
{
    private readonly global::NHibernate.ISession session;

    public NHibernateTransactionRunner(global::NHibernate.ISession session)
    {
        this.session = session;
    }

    public async Task<T> RunInTransaction<T>(Func<Task<T>> a)
    {
        using (var transaction = this.session.BeginTransaction())
        {
            var result = await a();                
            await transaction.CommitAsync();
            return result;
        }
    }
}