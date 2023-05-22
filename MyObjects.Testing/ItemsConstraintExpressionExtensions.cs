using System;
using NUnit.Framework.Constraints;

namespace MyObjects.Tests;

public static class ItemsConstraintExpressionExtensions
{
    public static IConstraint Event<T>(this ItemsConstraintExpression _, Predicate<T> predicate = null)
    {
        return _.Append(new PredicateConstraint<T>(predicate ?? (_ => true)));
    }
}