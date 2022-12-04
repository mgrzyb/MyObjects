namespace MyObjects.NHibernate;

public interface ITransactionRunner
{
    Task<T> RunInTransaction<T>(Func<Task<T>> a);
}