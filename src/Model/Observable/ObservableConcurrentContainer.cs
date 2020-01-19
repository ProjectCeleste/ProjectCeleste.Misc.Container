using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProjectCeleste.Misc.Container.Interface;
using ProjectCeleste.Misc.Container.Misc;

namespace ProjectCeleste.Misc.Container.Model.Observable
{
    [JsonConverter(typeof(ContainerJsonConverter))]
    [JsonArray(Title = "ObservableConcurrentContainer<TKey, TValue>",
        Id = "ObservableConcurrentContainer<TKey, TValue>",
        AllowNullItems = false)]
    [XmlRoot("ObservableConcurrentContainer&lt;TKey, TValue&gt;")]
    public sealed class ObservableConcurrentContainer<TKey, TValue> : IObservableContainer<TKey, TValue>
        where TValue : class
    {
        [XmlIgnore] [JsonIgnore] private readonly Func<TValue, TKey> _keySelector;
        [XmlIgnore] [JsonIgnore] private readonly ConcurrentDictionary<TKey, TValue> _valuesDic;

        public ObservableConcurrentContainer() : this(ContainerUtils.GetKeySelector<TValue, TKey>(),
            Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableConcurrentContainer(IEnumerable<TValue> values) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(),
            values,
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableConcurrentContainer(IEnumerable<TValue> values, IEqualityComparer<TKey> comparer) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values, comparer)
        {
        }

        public ObservableConcurrentContainer(Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableConcurrentContainer(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) : this(
            keySelector,
            Array.Empty<TValue>(), comparer)
        {
        }

        public ObservableConcurrentContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(
            keySelector,
            values, ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableConcurrentContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values,
            IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            var dic = values.Select(key => new KeyValuePair<TKey, TValue>(_keySelector(key), key));
            _valuesDic = new ConcurrentDictionary<TKey, TValue>(dic, comparer);
        }

        [XmlIgnore]
        [JsonIgnore]
        public TValue this[TKey key] => _valuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        [XmlIgnore] [JsonIgnore] public int Count => _valuesDic.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => { };

        public bool ContainsKey(TKey key)
        {
            return _valuesDic.ContainsKey(key);
        }

        public bool Add(TValue value)
        {
            var key = _keySelector(value);

            if (!_valuesDic.TryAdd(key, value))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                value));

            return true;
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
                        throw new ArgumentException($"Key= {key}");

                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                        value));
                }
                catch (ArgumentException e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);

            //CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
            //    values));
        }

        public bool Remove(TKey key)
        {
            if (!_valuesDic.TryRemove(key, out var value))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Remove(TKey key, out TValue value)
        {
            if (!_valuesDic.TryRemove(key, out value))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Update(TValue value)
        {
            var keyResult = _keySelector(value);

            if (!_valuesDic.TryGetValue(keyResult, out var item))
                throw new KeyNotFoundException($"KeyNotFoundException '{keyResult}'");

            if (ReferenceEquals(value, item) || Equals(value, item))
                return false;

            _valuesDic[keyResult] = value;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                value,
                item));

            return true;
        }

        //public bool ChangeKey(TKey oldKey, TKey newKey)
        //{
        //    if (_valuesDic.ContainsKey(newKey))
        //        throw new ArgumentException($"Key '{newKey}' already used", nameof(newKey));

        //    if (!_valuesDic.TryRemove(oldKey, out var value))
        //        throw new KeyNotFoundException($"KeyNotFoundException '{oldKey}'");

        //    if (_valuesDic.TryAdd(newKey, value))
        //    {
        //        CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
        //            newKey,
        //            oldKey));

        //        return true;
        //    }

        //    if (!_valuesDic.TryAdd(oldKey, value))
        //        throw new Exception();

        //    return false;
        //}

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
            return _valuesDic.ToArray().Select(p => p.Value);
        }

        public void Clear()
        {
            _valuesDic.Clear();
        }

        #region IEnumerable<TValue> Members

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return Gets().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Gets().GetEnumerator();
        }

        #endregion
    }
}