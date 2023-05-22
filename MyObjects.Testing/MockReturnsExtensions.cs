using System.Collections.Generic;
using Moq.Language;
using Moq.Language.Flow;

namespace MyObjects.Testing;

public static class MockReturnsExtensions
{
    public static IReturnsResult<TMock> Returns<TMock, TResult>(this IReturns<TMock, TResult> returns,
        params TResult[] results) where TMock : class
    {
        var r = new Queue<TResult>(results);
        return returns.Returns(() => r.Dequeue());
    }
}