using NHibernate;

namespace MyObjects.NHibernate;

public static class EnumerableExtensions
{
    public static async Task<T> Initialized<T>(this T collection)
    {
        await NHibernateUtil.InitializeAsync(collection);
        return collection;
    }
}