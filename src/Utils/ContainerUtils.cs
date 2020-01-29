using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using ProjectCeleste.Misc.Container.Utils.CustomAttribute;

namespace ProjectCeleste.Misc.Container.Utils
{
    internal static class ContainerUtils
    {
        [NotNull]
        [Pure]
        internal static Func<TValue, TKey> GetKeySelector<TValue, TKey>()
        {
            var properties = typeof(TValue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(key => key.PropertyType == typeof(TKey));
            var propertiesInfo = properties.Where(property =>
                Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is KeyAttribute).ToArray();

            if (propertiesInfo.Length == 0)
                throw new Exception("KeyAttribute not found");

            if (propertiesInfo.Length > 1)
                throw new Exception("Multiple KeyAttribute found");

            var propertyInfo = propertiesInfo[0];

            return obj => (TKey) propertyInfo.GetValue(obj);
        }

        [NotNull]
        [Pure]
        internal static Action<TValue, TKey> GetKeySetMethod<TValue, TKey>()
        {
            var properties = typeof(TValue)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(key => key.PropertyType == typeof(TKey));
            var propertiesInfo = properties.Where(property =>
                Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is KeyAttribute).ToArray();

            if (propertiesInfo.Length == 0)
                throw new Exception("KeyAttribute not found");

            if (propertiesInfo.Length > 1)
                throw new Exception("Multiple KeyAttribute found");

            var propertyInfo = propertiesInfo[0];

            return (obj, newKey) => propertyInfo.SetValue(obj, newKey);
        }

        [NotNull]
        [Pure]
        internal static IEqualityComparer<TKey> GetKeyEqualityComparer<TValue, TKey>()
        {
            if (typeof(TValue).GetCustomAttribute(typeof(ContainerEqualityComparer)) is ContainerEqualityComparer
                attribute)
            {
                return attribute.EqualityComparer is IEqualityComparer<TKey> equalityComparer
                    ? equalityComparer
                    : throw new Exception("Invalid EqualityComparer");
            }

            return EqualityComparer<TKey>.Default;
        }
    }
}