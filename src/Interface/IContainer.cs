namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IContainer<in TKey, TValue> : IReadOnlyContainer<TKey, TValue>
    {
        bool Add(TValue value);
        void Clear();
        bool Remove(TKey key);
        bool Remove(TKey key, out TValue value);

        bool Update(TValue value);
        //bool ChangeKey(T1 newKey, T1 oldKey);
    }
}