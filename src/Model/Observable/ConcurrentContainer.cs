using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using ProjectCeleste.Misc.Container.Interface;

namespace ProjectCeleste.Misc.Container.Model.Observable
{
    public class ObservableConcurrentContainer<TKey, TValue> : IObservableConcurrentContainer<TKey, TValue>
        where TValue : class
    {
        private readonly Func<TValue, TKey> _keySelector;
        protected internal readonly ConcurrentDictionary<TKey, TValue> ValuesDic;

        public ObservableConcurrentContainer([NotNull] Func<TValue, TKey> keySelector) : this(keySelector,
            Array.Empty<TValue>(),
            EqualityComparer<TKey>.Default)
        {
        }

        public ObservableConcurrentContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] IEqualityComparer<TKey> comparer) : this(
            keySelector,
            Array.Empty<TValue>(), comparer)
        {
        }

        public ObservableConcurrentContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values) : this(
            keySelector, values, EqualityComparer<TKey>.Default)
        {
        }

        public ObservableConcurrentContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values,
            IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            ValuesDic = new ConcurrentDictionary<TKey, TValue>(comparer);
            Add(values);
        }

        public TValue this[TKey key] => ValuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        public int Count => ValuesDic.Count;

        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => { };

        public bool ContainsKey(TKey key)
        {
            return ValuesDic.ContainsKey(key);
        }

        public bool Add(TValue value)
        {
            var key = _keySelector(value);

            if (!ValuesDic.TryAdd(key, value))
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
                    if (!ValuesDic.TryAdd(key, value))
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
            if (!ValuesDic.TryRemove(key, out var value))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Remove(TKey key, out TValue value)
        {
            if (!ValuesDic.TryRemove(key, out value))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Update(TValue value, Func<TValue, TValue, bool> equalityComparer = null)
        {
            var keyResult = _keySelector(value);

            if (!ValuesDic.TryGetValue(keyResult, out var item))
                throw new KeyNotFoundException($"KeyNotFoundException '{keyResult}'");

            if (ReferenceEquals(value, item) || Equals(value, item))
                return false;

            if (equalityComparer != null && equalityComparer(value, item))
                return false;

            ValuesDic[keyResult] = value;

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
            return ValuesDic.TryGetValue(key, out var value)
                ? value
                : default;
        }

        public TValue Get(Func<TValue, bool> criteria, bool dirtyRead)
        {
            return Gets(dirtyRead).FirstOrDefault(criteria);
        }

        public TValue Get(Func<TValue, bool> criteria)
        {
            return Gets().FirstOrDefault(criteria);
        }

        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria, bool dirtyRead)
        {
            return Gets(dirtyRead).Where(criteria);
        }

        public IEnumerable<TValue> Gets(bool dirtyRead)
        {
            return dirtyRead ? ValuesDic.Select(p => p.Value) : ValuesDic.ToArray().Select(p => p.Value);
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
            ValuesDic.Clear();

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}