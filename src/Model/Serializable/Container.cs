using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ProjectCeleste.Misc.Container.Utils;
using ProjectCeleste.Misc.Container.Utils.JsonConverter;

namespace ProjectCeleste.Misc.Container.Model.Serializable
{
    [JsonConverter(typeof(ContainerJsonConverter))]
    [JsonArray(Title = "Container<TKey, TValue>", Id = "Container<TKey, TValue>",
        AllowNullItems = false)]
    [XmlRoot("Container&lt;TKey, TValue&gt;")]
    public sealed class SerializableContainer<TKey, TValue> : Container<TKey, TValue>, IEnumerable<TValue>,
        IContainerJsonConverter where TValue : class
    {
        public SerializableContainer() : base(ContainerUtils.GetKeySelector<TValue, TKey>(), Array.Empty<TValue>(),
            ContainerUtils.GetKeyEqualityComparer<TValue, TKey>())
        {
        }

        public SerializableContainer([NotNull] [ItemNotNull] IEnumerable<TValue> values) : base(
            ContainerUtils.GetKeySelector<TValue, TKey>(),
            values, ContainerUtils.GetKeyEqualityComparer<TValue, TKey>())
        {
        }

        #region IEnumerable<TValue> Members

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        [Pure]
        public IEnumerator<TValue> GetEnumerator()
        {
            return Gets().GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the
        ///     collection.
        /// </returns>
        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}