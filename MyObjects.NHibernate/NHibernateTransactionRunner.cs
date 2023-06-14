namespace MyObjects.NHibernate;

class NHibernateTransactionRunner : ITransactionRunner
{
    private readonly global::NHibernate.ISession session;

    public NHibernateTransactionRunner(global::NHibernate.ISession session)
    {
        this.session = session;
    }

    public Task RunInTransaction(Func<Task> a)
    {
        return this.RunInTransaction(async () =>
        {
            await a();
            return string.Empty;
        });
    }
    
    public async Task<T> RunInTransaction<T>(Func<Task<T>> a)
    {
        using (var transaction = this.session.BeginTransaction())
        {
            try
            {
                var result = await a();
                await transaction.CommitAsync();
                return result;
            }
            catch (global::NHibernate.StaleObjectStateException e)
            {
                throw new ConcurrencyViolationException(e.EntityName, e.Identifier, e);
            }
        }
    }
}