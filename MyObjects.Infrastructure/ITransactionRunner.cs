namespace MyObjects.NHibernate;

public interface ITransactionRunner
{
    Task RunInTransaction(Func<Task> a);
    Task<T> RunInTransaction<T>(Func<Task<T>> a);
}