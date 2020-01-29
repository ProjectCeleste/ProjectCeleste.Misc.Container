using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IReadOnlyContainer<in TKey, out TValue>
    {
        [NotNull] TValue this[[NotNull] TKey key] { get; }

        int Count { get; }

        [Pure]
        bool ContainsKey([NotNull] TKey key);

        [CanBeNull]
        [Pure]
        TValue Get([NotNull] Func<TValue, bool> criteria);

        [CanBeNull]
        [Pure]
        TValue Get([NotNull] TKey key);

        [NotNull]
        [ItemNotNull]
        [Pure]
        IEnumerable<TValue> Gets();

        [NotNull]
        [ItemNotNull]
        [Pure]
        IEnumerable<TValue> Gets([NotNull] Func<TValue, bool> criteria);
    }
}