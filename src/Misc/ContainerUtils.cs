using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ProjectCeleste.Misc.Container.Misc
{
    public static class ContainerUtils
    {
        public static string GetKeyPropertyName<TValue, TKey>()
        {
            var properties = typeof(TValue).GetProperties().Where(key => key.PropertyType == typeof(TKey));
            var propertiesInfo = properties.Where(property =>
                Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is KeyAttribute).ToArray();

            if (propertiesInfo.Length == 0)
                throw new Exception("KeyAttribute not found");

            if (propertiesInfo.Length > 1)
                throw new Exception("Multiple KeyAttribute found");

            return propertiesInfo[0].Name;
        }

        public static Func<TValue, TKey> GetKeySelector<TValue, TKey>()
        {
            var properties = typeof(TValue).GetProperties().Where(key => key.PropertyType == typeof(TKey));
            var propertiesInfo = properties.Where(property =>
                Attribute.GetCustomAttribute(property, typeof(KeyAttribute)) is KeyAttribute).ToArray();

            if (propertiesInfo.Length == 0)
                throw new Exception("KeyAttribute not found");

            if (propertiesInfo.Length > 1)
                throw new Exception("Multiple KeyAttribute found");

            var propertyInfo = propertiesInfo[0];

            return obj => (TKey) propertyInfo.GetValue(obj);
        }
    }
}