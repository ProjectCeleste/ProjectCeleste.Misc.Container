using System;
using System.Collections.Generic;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface ICachedContainer<in TKey, TValue> : IContainer<TKey, TValue>, IDisposable
    {
        TValue Get(Func<TValue, bool> criteria, bool useCache = true);
        IEnumerable<TValue> Gets(bool useCache = true);
        IEnumerable<TValue> Gets(Func<TValue, bool> criteria, bool useCache = true);
    }
}