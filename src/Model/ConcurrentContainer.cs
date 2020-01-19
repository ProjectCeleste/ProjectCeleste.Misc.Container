using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProjectCeleste.Misc.Container.Interface;
using ProjectCeleste.Misc.Container.Misc;

namespace ProjectCeleste.Misc.Container.Model
{
    [JsonConverter(typeof(ContainerJsonConverter))]
    [JsonArray(Title = "ConcurrentContainer<TKey, TValue>",
        Id = "ConcurrentContainer<TKey, TValue>", AllowNullItems = false)]
    [XmlRoot("ConcurrentContainer&lt;TKey, TValue&gt;")]
    public sealed class ConcurrentContainer<TKey, TValue> : IContainer<TKey, TValue> where TValue : class
    {
        [XmlIgnore] [JsonIgnore] private readonly Func<TValue, TKey> _keySelector;
        [XmlIgnore] [JsonIgnore] private readonly ConcurrentDictionary<TKey, TValue> _valuesDic;

        public ConcurrentContainer() : this(ContainerUtils.GetKeySelector<TValue, TKey>(), Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ConcurrentContainer(IEnumerable<TValue> values) : this(ContainerUtils.GetKeySelector<TValue, TKey>(),
            values,
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ConcurrentContainer(IEnumerable<TValue> values, IEqualityComparer<TKey> comparer) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values, comparer)
        {
        }

        public ConcurrentContainer(Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ConcurrentContainer(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) : this(keySelector,
            Array.Empty<TValue>(), comparer)
        {
        }

        public ConcurrentContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(keySelector,
            values, ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ConcurrentContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values,
            IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            _valuesDic = new ConcurrentDictionary<TKey, TValue>(comparer);
            var exceptions = new List<Exception>();
            foreach (var value in values)
            {
                try
                {
                    var key = _keySelector(value);
                    if (!_valuesDic.TryAdd(key, value))
                        throw new ArgumentException($"Key={key}");
                }
                catch (ArgumentException e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        public bool ContainsKey(TKey key)
        {
            return _valuesDic.ContainsKey(key);
        }

        public bool Add(TValue value)
        {
            var key = _keySelector(value);
            return _valuesDic.TryAdd(key, value);
        }

        public bool Remove(TKey key)
        {
            return _valuesDic.TryRemove(key, out _);
        }

        public bool Remove(TKey key, out TValue value)
        {
            return _valuesDic.TryRemove(key, out value);
        }

        public bool Update(TValue value)
        {
            var keyResult = _keySelector(value);

            if (!_valuesDic.TryGetValue(keyResult, out var item))
                throw new KeyNotFoundException($"KeyNotFoundException '{keyResult}'");

            if (ReferenceEquals(value, item) || Equals(value, item))
                return false;

            _valuesDic[keyResult] = value;

            return true;
        }

        //public bool ChangeKey(TKey oldKey, TKey newKey)
        //{
        //    if (_valuesDic.ContainsKey(newKey))
        //        throw new ArgumentException($"Key '{newKey}' already used", nameof(newKey));

        //    if (!_valuesDic.TryRemove(oldKey, out var value))
        //        throw new KeyNotFoundException($"KeyNotFoundException '{oldKey}'");

        //    if (_valuesDic.TryAdd(newKey, value))
        //        return true;

        //    if (!_valuesDic.TryAdd(oldKey, value))
        //        throw new Exception();

        //    return false;
        //}

        [XmlIgnore]
        [JsonIgnore]
        public TValue this[TKey key] => _valuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        [XmlIgnore] [JsonIgnore] public int Count => _valuesDic.Count;

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

        public void Clear()
        {
            _valuesDic.Clear();
        }

        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria)
        {
            return Gets().Where(criteria);
        }

        public IEnumerable<TValue> Gets()
        {
            return _valuesDic.ToArray().Select(p => p.Value);
        }

        public void Add(IEnumerable<TValue> values)
        {
            var exceptions = new List<Exception>();
            foreach (var value in values)
            {
                try
                {
                    var key = _keySelector(value);
                    if (!_valuesDic.TryAdd(key, value))
                        throw new ArgumentException($"Key={key}");
                }
                catch (ArgumentException e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        #region IEnumerable<TValue> Members

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            //return _valuesDic.Values.GetEnumerator();
            return Gets().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //return _valuesDic.Values.GetEnumerator();
            return Gets().GetEnumerator();
        }

        #endregion
    }
}