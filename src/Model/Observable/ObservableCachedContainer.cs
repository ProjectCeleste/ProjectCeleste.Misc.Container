using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ProjectCeleste.Misc.Container.Interface;
using ProjectCeleste.Misc.Container.Misc;

namespace ProjectCeleste.Misc.Container.Model.Observable
{
    [JsonConverter(typeof(ContainerJsonConverter))]
    [JsonArray(Title = "ObservableCachedContainer<TKey, TValue>",
        Id = "ObservableCachedContainer<TKey, TValue>",
        AllowNullItems = false)]
    [XmlRoot("ObservableCachedContainer&lt;TKey, TValue&gt;")]
    public sealed class ObservableCachedContainer<TKey, TValue> : ICachedContainer<TKey, TValue>,
        IObservableContainer<TKey, TValue> where TValue : class
    {
        [XmlIgnore] [JsonIgnore] private readonly ConcurrentDictionary<TKey, TValue> _valuesDic;

        [XmlIgnore] [JsonIgnore] private readonly Func<TValue, TKey> _keySelector;

        public ObservableCachedContainer() : this(ContainerUtils.GetKeySelector<TValue, TKey>(), Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableCachedContainer(IEnumerable<TValue> values) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(),
            values,
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableCachedContainer(IEnumerable<TValue> values, IEqualityComparer<TKey> comparer) : this(
            ContainerUtils.GetKeySelector<TValue, TKey>(), values, comparer)
        {
        }

        public ObservableCachedContainer(Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableCachedContainer(Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer) : this(
            keySelector,
            Array.Empty<TValue>(), comparer)
        {
        }

        public ObservableCachedContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values) : this(keySelector,
            values, ContainerEqualityComparer.GetCustomDefaultEqualityComparer<TKey>())
        {
        }

        public ObservableCachedContainer(Func<TValue, TKey> keySelector, IEnumerable<TValue> values,
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

            StartCache();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => { };

        [XmlIgnore]
        [JsonIgnore]
        public TValue this[TKey key] => _valuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        [XmlIgnore] [JsonIgnore] public int Count => _valuesDic.Count;

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
            return Gets(true).FirstOrDefault(criteria);
        }

        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria)
        {
            return Gets(true).Where(criteria);
        }

        public IEnumerable<TValue> Gets()
        {
            return Gets(true);
        }

        public void Clear()
        {
            _valuesDic.Clear();

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public TValue Get(Func<TValue, bool> criteria, bool useCache)
        {
            return Gets(useCache).FirstOrDefault(criteria);
        }

        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria, bool useCache)
        {
            return Gets(useCache).Where(criteria);
        }

        public IEnumerable<TValue> Gets(bool useCache)
        {
            return useCache ? _valuesCache.ToArray() : _valuesDic.ToArray().Select(p => p.Value);
        }

        #region Cache

        [XmlIgnore] [JsonIgnore] private ICollection<TValue> _valuesCache;

        [XmlIgnore] [JsonIgnore] private Timer _snapshotTimer;

        private void StartCache(int cacheDelayInSec = 5)
        {
            if (_snapshotTimer != null)
                return;

            _valuesCache = Gets(false).ToArray();

            var interval = cacheDelayInSec * 1000;
            _snapshotTimer = new Timer(TakeSnapshot, new object(), interval, interval);
        }

        private void TakeSnapshot(object state)
        {
            if (!Monitor.TryEnter(state))
                return;

            try
            {
                Interlocked.Exchange(ref _valuesCache, Gets(false).ToArray());
            }
            finally
            {
                Monitor.Exit(state);
            }
        }

        #endregion

        #region IDisposable Support

        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                if (_snapshotTimer != null)
                {
                    _snapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _snapshotTimer.Dispose();
                    _snapshotTimer = null;
                }

                CollectionChanged = null;
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion


        #region IEnumerable<TValue> Members

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return Gets(false).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Gets(false).GetEnumerator();
        }

        #endregion
    }
}