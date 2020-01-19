using System.Collections.Specialized;

namespace ProjectCeleste.Misc.Container.Interface
{
    public interface IObservableContainer<in TKey, TValue> : IContainer<TKey, TValue>, INotifyCollectionChanged
    {
    }
}