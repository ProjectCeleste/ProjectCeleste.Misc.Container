using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using ProjectCeleste.Misc.Container.Interface;

namespace ProjectCeleste.Misc.Container.Model
{
    public class ReadOnlyContainer<TKey, TValue> : IReadOnlyContainer<TKey, TValue> where TValue : class
    {
        [NotNull] private protected readonly IReadOnlyDictionary<TKey, TValue> ValuesDic;

        public ReadOnlyContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values) : this(
            keySelector, values, EqualityComparer<TKey>.Default)
        {
        }

        public ReadOnlyContainer([NotNull] Func<TValue, TKey> keySelector,
            [NotNull] [ItemNotNull] IEnumerable<TValue> values,
            [NotNull] IEqualityComparer<TKey> comparer)
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

            ValuesDic = new ReadOnlyDictionary<TKey, TValue>(valuesDic);
        }

        [Pure]
        public bool ContainsKey(TKey key)
        {
            return ValuesDic.ContainsKey(key);
        }

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
        public TValue Get(Func<TValue, bool> criteria)
        {
            return Gets().FirstOrDefault(criteria);
        }

        [Pure]
        public IEnumerable<TValue> Gets(Func<TValue, bool> criteria)
        {
            return Gets().Where(criteria);
        }

        [Pure]
        public IEnumerable<TValue> Gets()
        {
            return ValuesDic.Select(key => key.Value);
        }
    }
}