using System;
using System.Collections;
using System.Collections.Generic;

namespace ProjectCeleste.Misc.Container.Misc
{
    public static class ContainerEqualityComparer
    {
        private static readonly IDictionary<Type, IEqualityComparer> CustomDefaultEqualityComparer =
            new Dictionary<Type, IEqualityComparer>();

        static ContainerEqualityComparer()
        {
            CustomDefaultEqualityComparer.Add(typeof(string), StringComparer.OrdinalIgnoreCase);
        }

        public static IEqualityComparer<TKey> GetCustomDefaultEqualityComparer<TKey>()
        {
            var keyType = typeof(TKey);
            return CustomDefaultEqualityComparer.ContainsKey(keyType)
                ? (IEqualityComparer<TKey>) CustomDefaultEqualityComparer[keyType]
                : EqualityComparer<TKey>.Default;
        }

        public static void AddDefaultEqualityComparer(Type type, IEqualityComparer equalityComparer)
        {
            CustomDefaultEqualityComparer.Add(type, equalityComparer);
        }

        public static void RemoveDefaultEqualityComparer(Type type)
        {
            CustomDefaultEqualityComparer.Remove(type);
        }
    }
}