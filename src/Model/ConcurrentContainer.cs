using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProjectCeleste.Misc.Container.Interface;

namespace ProjectCeleste.Misc.Container.Model
{
    public class ConcurrentContainer<TKey, TValue> : IConcurrentContainer<TKey, TValue> where TValue : class
    {
        [NotNull] private readonly Func<TValue, TKey> _keySelector;
        [NotNull] protected internal readonly ConcurrentDictionary<TKey, TValue> ValuesDic;

        public ConcurrentContainer([NotNull] Func<TValue, TKey> keySelector) : this(keySelector, Array.Empty<TValue>(),
            EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentContainer([NotNull] Func<TValue, TKey> keySelector, [NotNull] IEqualityComparer<TKey> comparer)
            : this(keySelector,
                Array.Empty<TValue>(), comparer)
        {
        }

        public ConcurrentContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values) : this(keySelector,
            values, EqualityComparer<TKey>.Default)
        {
        }

        public ConcurrentContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values,
            [NotNull] IEqualityComparer<TKey> comparer)
        {
            _keySelector = keySelector;
            ValuesDic = new ConcurrentDictionary<TKey, TValue>(comparer);
            Add(values);
        }

        [Pure]
        public bool ContainsKey(TKey key)
        {
            return ValuesDic.ContainsKey(key);
        }

        public bool Add(TValue value)
        {
            return ValuesDic.TryAdd(_keySelector(value), value);
        }

        public bool Remove(TKey key)
        {
            return ValuesDic.TryRemove(key, out _);
        }

        public bool Remove(TKey key, out TValue value)
        {
            return ValuesDic.TryRemove(key, out value);
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

        public TValue this[TKey key] => ValuesDic.TryGetValue(key, out var value)
            ? value
            : throw new KeyNotFoundException($"KeyNotFoundException '{key}'");

        public int Count => ValuesDic.Count;

        [Pure]
        public TValue Get(TKey key)
        {
            return ValuesDic.TryGetValue(key, out var value)
                ? value
                : default;
        }

        [Pure]
        public TValue Get(Func<TValue, bool> criteria, bool dirtyRead)
        {
            return Gets(dirtyRead).FirstOrDefault(criteria);
        }

        [Pure]
        public TValue Get(Func<TValue, bool> criteria)
        {
            return Gets(true).FirstOrDefault(criteria);
        }

        public void Clear()
        {
            ValuesDic.Clear();
        }

        [Pure]
        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria, bool dirtyRead)
        {
            return Gets(dirtyRead).Where(criteria);
        }

        [Pure]
        public IEnumerable<TValue> Gets(bool dirtyRead)
        {
            return dirtyRead ? ValuesDic.Select(p => p.Value) : ValuesDic.ToArray().Select(p => p.Value);
        }

        [Pure]
        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria)
        {
            return Gets(true).Where(criteria);
        }

        [Pure]
        public IEnumerable<TValue> Gets()
        {
            return Gets(true);
        }

        public void Add([NotNull] [ItemNotNull] IEnumerable<TValue> values)
        {
            var exceptions = new List<Exception>();
            foreach (var value in values)
            {
                try
                {
                    var key = _keySelector(value);
                    if (!ValuesDic.TryAdd(key, value))
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
    }
}