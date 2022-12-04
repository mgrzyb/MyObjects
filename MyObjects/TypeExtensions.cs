using System;
using System.Collections.Generic;
using System.Linq;

namespace MyObjects
{
    public static class TypeExtensions
    {
        public static bool IsEnumerable(this Type type, out Type elementType)
        {
            var enumerable = new [] {type}.Concat(type.GetInterfaces())
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            
            if (enumerable != null)
            {
                elementType = enumerable.GetGenericArguments()[0];
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