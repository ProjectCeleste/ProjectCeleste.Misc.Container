using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using ProjectCeleste.Misc.Container.Interface;
using ProjectCeleste.Misc.Container.Misc;

namespace ProjectCeleste.Misc.Container.Model
{
    [JsonConverter(typeof(ContainerJsonConverter))]
    [JsonArray(Title = "ReadOnlyContainer<TKey, TValue>", Id = "ReadOnlyContainer<TKey, TValue>",
        AllowNullItems = false)]
    public sealed class ReadOnlyContainer<TKey, TValue> : IReadOnlyContainer<TKey, TValue> where TValue : class
    {
        [JsonIgnore] private readonly IReadOnlyDictionary<TKey, TValue> _valuesDic;

        public ReadOnlyContainer(IEnumerable<TValue> values) : this(ContainerUtils.GetKeySelector<TValue, TKey>(),
            values,
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ReadOnlyContainer(IEnumerable<TValue> values, IEqualityComparer<TKey> comparer) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values, comparer)
        {
        }

        public ReadOnlyContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(
            keySelector,
            values, ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ReadOnlyContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values,
            IEqualityComparer<TKey> comparer)
        {
            var valuesDic = new Dictionary<TKey, TValue>(comparer);
            var exceptions = new List<Exception>();
            foreach (var value in values)
            {
                try
                {
                    var key = keySelector(value);
                    valuesDic.Add(key, value);
                }
                catch (ArgumentException e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            _valuesDic = new ReadOnlyDictionary<TKey, TValue>(valuesDic);
        }

        public bool ContainsKey(TKey key)
        {
            return _valuesDic.ContainsKey(key);
        }

        [JsonIgnore]
        public TValue this[TKey key] => _valuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        [JsonIgnore] public int Count => _valuesDic.Count;

        public TValue Get(TKey key)
        {
            return _valuesDic.TryGetValue(key, out var value)
                ? value
                : default;
        }

        public TValue Get(Func<TValue, bool> criteria)
        {
            return Gets().FirstOrDefault(criteria);
        }

        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria)
        {
            return Gets().Where(criteria);
        }

        public IEnumerable<TValue> Gets()
        {
            return _valuesDic.Values;
        }

        #region IEnumerable<TValue> Members

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _valuesDic.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _valuesDic.Values.GetEnumerator();
        }

        #endregion
    }
}