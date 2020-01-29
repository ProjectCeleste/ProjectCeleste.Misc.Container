using System;
using JetBrains.Annotations;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IContainer<in TKey, TValue> : IReadOnlyContainer<TKey, TValue>
    {
        bool Add([NotNull] TValue value);
        void Clear();
        bool Remove([NotNull] TKey key);
        bool Remove([NotNull] TKey key, out TValue value);

        bool Update([NotNull] TValue value, [CanBeNull] Func<TValue, TValue, bool> equalityComparer = null);
        //bool ChangeKey(T1 newKey, T1 oldKey);
    }
}