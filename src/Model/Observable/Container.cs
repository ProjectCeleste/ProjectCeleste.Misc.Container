using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using ProjectCeleste.Misc.Container.Interface;

namespace ProjectCeleste.Misc.Container.Model.Observable
{
    public class ObservableContainer<TKey, TValue> : IObservableContainer<TKey, TValue> where TValue : class
    {
        private readonly Func<TValue, TKey> _keySelector;
        private protected readonly IDictionary<TKey, TValue> ValuesDic;

        public ObservableContainer([NotNull] Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            EqualityComparer<TKey>.Default)
        {
        }

        public ObservableContainer([NotNull] Func<TValue, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer)
            : this(keySelector,
                Array.Empty<TValue>(), comparer)
        {
        }

        public ObservableContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values) : this(keySelector,
            values, EqualityComparer<TKey>.Default)
        {
        }

        public ObservableContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values,
            [NotNull] IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            ValuesDic = new Dictionary<TKey, TValue>(comparer);
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
            try
            {
                ValuesDic.Add(key, value);
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
                    ValuesDic.Add(key, value);
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
            if (!ValuesDic.TryGetValue(key, out var value) || !ValuesDic.Remove(key))
                return false;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                value));

            return true;
        }

        public bool Remove(TKey key, out TValue value)
        {
            if (!ValuesDic.TryGetValue(key, out value) || !ValuesDic.Remove(key))
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
            return ValuesDic.TryGetValue(key, out var value)
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
            return ValuesDic.Select(p => p.Value);
        }

        public void Clear()
        {
            ValuesDic.Clear();
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}