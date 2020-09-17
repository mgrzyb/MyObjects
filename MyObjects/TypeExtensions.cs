using System;
using System.Collections.Generic;
using Autofac;

namespace MyObjects
{
    public static class TypeExtensions
    {
        public static bool IsEnumerable(this Type type, out Type elementType)
        {
            if (type.IsClosedTypeOf(typeof(IEnumerable<>)))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            else
            {
                elementType = default(Type);
                return false;
            }
        }

        public static bool IsEnumerableOf<T>(this Type type)
        {
            return type.IsEnumerable(out var elementType) && elementType.IsSubclassOf(typeof(T));
        }
    }
}