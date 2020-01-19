using System;
using System.Collections;
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
    [JsonArray(Title = "ObservableContainer<TKey, TValue>", Id = "ObservableContainer<TKey, TValue>",
        AllowNullItems = false)]
    [XmlRoot("ObservableContainer&lt;TKey, TValue&gt;")]
    public sealed class ObservableContainer<TKey, TValue> : IObservableContainer<TKey, TValue> where TValue : class
    {
        [XmlIgnore] [JsonIgnore] private readonly Func<TValue, TKey> _keySelector;
        [XmlIgnore] [JsonIgnore] private readonly IDictionary<TKey, TValue> _valuesDic;

        public ObservableContainer() : this(ContainerUtils.GetKeySelector<TValue, TKey>(), Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableContainer(IEnumerable<TValue> values) : this(ContainerUtils.GetKeySelector<TValue, TKey>(),
            values,
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableContainer(IEnumerable<TValue> values, IEqualityComparer<TKey> comparer) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values, comparer)
        {
        }

        public ObservableContainer(Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableContainer(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) : this(keySelector,
            Array.Empty<TValue>(), comparer)
        {
        }

        public ObservableContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(keySelector,
            values, ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values,
            IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            _valuesDic = new Dictionary<TKey, TValue>(comparer);
            var exceptions = new List<Exception>();
            foreach (var value in values)
            {
                try
                {
                    var key = _keySelector(value);
                    _valuesDic.Add(key, value);
                }
                catch (ArgumentException e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
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
            try
            {
                _valuesDic.Add(key, value);
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    value));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (ArgumentException)
            {
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types

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
                    _valuesDic.Add(key, value);
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
            if (!_valuesDic.TryGetValue(key, out var value) || !_valuesDic.Remove(key))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Remove(TKey key, out TValue value)
        {
            if (!_valuesDic.TryGetValue(key, out value) || !_valuesDic.Remove(key))
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

//        public bool ChangeKey(TKey oldKey, TKey newKey)
//        {
//            if (!_valuesDic.TryGetValue(oldKey, out var value))
//                throw new KeyNotFoundException($"KeyNotFoundException '{oldKey}'");

//            if (_valuesDic.ContainsKey(newKey))
//                throw new ArgumentException($"Key '{newKey}' already used", nameof(newKey));

//            _valuesDic.Remove(oldKey);

//            try
//            {
//                _valuesDic.Add(newKey, value);
//            }
//#pragma warning disable CA1031 // Do not catch general exception types
//            catch (ArgumentException)
//            {
//                _valuesDic.Add(oldKey, value);
//                return false;
//            }
//#pragma warning restore CA1031 // Do not catch general exception types

//            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
//                newKey,
//                oldKey));

//            return true;
//        }

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