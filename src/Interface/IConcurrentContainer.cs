using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IConcurrentContainer<in TKey, TValue> : IContainer<TKey, TValue>
    {
        [CanBeNull]
        [Pure]
        TValue Get([NotNull] Func<TValue, bool> criteria, bool useCache = true);

        [NotNull]
        [ItemNotNull]
        [Pure]
        IEnumerable<TValue> Gets(bool dirtyRead = true);

        [NotNull]
        [ItemNotNull]
        [Pure]
        IEnumerable<TValue> Gets([NotNull] Func<TValue, bool> criteria, bool dirtyRead = true);
    }
}