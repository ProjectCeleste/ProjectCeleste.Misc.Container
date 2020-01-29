namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IObservableConcurrentContainer<in TKey, TValue> : IConcurrentContainer<TKey, TValue>,
        IObservableContainer<TKey, TValue>
    {
    }
}