using System;

namespace BLE.Share.Showcase
{
    public interface IDataProvider<T>
    {
        IObservable<T> Data { get; }
    }
}
