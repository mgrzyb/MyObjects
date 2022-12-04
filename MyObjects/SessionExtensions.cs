using System.Threading.Tasks;

namespace MyObjects;

public static class SessionExtensions
{
    public static Task<T> Resolve<T>(this IReadonlySession session, int id) where T : IEntity
    {
        return session.Resolve(new Reference<T>(id));
    }

    public static async Task<(T1, T2)> Resolve<T1, T2>(this IReadonlySession session, Reference<T1> r1, Reference<T2> r2) where T1 : IEntity where T2 : IEntity
    {
        return (await session.Resolve(r1), await session.Resolve(r2));
    }

    public static async Task<(T1, T2, T3)> Resolve<T1, T2, T3>(this IReadonlySession session, Reference<T1> r1, Reference<T2> r2,
        Reference<T3> r3) where T3 : IEntity where T1 : IEntity where T2 : IEntity
    {
        return (await session.Resolve(r1), await session.Resolve(r2), await session.Resolve(r3));
    }
}