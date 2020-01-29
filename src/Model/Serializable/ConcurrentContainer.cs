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
    [JsonConverter(typeof(ConcurrentContainerJsonConverter))]
    [JsonArray(Title = "ConcurrentContainer<TKey, TValue>",
        Id = "ConcurrentContainer<TKey, TValue>", AllowNullItems = false)]
    [XmlRoot("ConcurrentContainer&lt;TKey, TValue&gt;")]
    public sealed class SerializableConcurrentContainer<TKey, TValue> : ConcurrentContainer<TKey, TValue>,
        IEnumerable<TValue>, IConcurrentContainerJsonConverter where TValue : class
    {
        public SerializableConcurrentContainer() : base(ContainerUtils.GetKeySelector<TValue, TKey>(),
            Array.Empty<TValue>(), ContainerUtils.GetKeyEqualityComparer<TValue, TKey>())
        {
        }

        public SerializableConcurrentContainer([NotNull] [ItemNotNull] IEnumerable<TValue> values) : base(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values,
            ContainerUtils.GetKeyEqualityComparer<TValue, TKey>())
        {
        }

        #region IEnumerable<TValue> Members

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        [Pure]
        public IEnumerator<TValue> GetEnumerator()
        {
            return Gets(false).GetEnumerator();
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