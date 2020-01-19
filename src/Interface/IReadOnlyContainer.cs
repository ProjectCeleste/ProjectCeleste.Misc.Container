using System;
using System.Collections.Generic;
using ProjectCeleste.Misc.Container.Misc;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IReadOnlyContainer<in TKey, out TValue> : IEnumerable<TValue>, IContainerJsonConverter
    {
        TValue this[TKey key] { get; }
        int Count { get; }
        bool ContainsKey(TKey key);
        TValue Get(Func<TValue, bool> criteria);
        TValue Get(TKey key);
        IEnumerable<TValue> Gets();
        IEnumerable<TValue> Gets(Func<TValue, bool> criteria);
    }
}