using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProjectCeleste.Misc.Container.Utils.JsonConverter
{
    public interface IConcurrentContainerJsonConverter
    {
    }

    public class ConcurrentContainerJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConcurrentContainerJsonConverter));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var methodGets = value.GetType().GetMethod("Gets", new[] {typeof(bool)});
            if (methodGets == null)
                throw new MissingMethodException(value.GetType().FullName, "Gets");
            var valueArray = methodGets.Invoke(value, new object[] {false});
            var ja = JArray.FromObject(valueArray);
            ja.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var stringType =
                $"System.Collections.Generic.IEnumerable`1[[{objectType.GenericTypeArguments[1].AssemblyQualifiedName}]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";
            var valuesType = Type.GetType(stringType, true);
            var constructorInfo = objectType.GetConstructor(new[] {valuesType});
            if (constructorInfo == null)
                throw new MissingMethodException(valuesType.FullName, "Constructor");

            var arrayObject = JArray.Load(reader).ToObject(valuesType);

            return constructorInfo.Invoke(new[] {arrayObject});
        }
    }
}