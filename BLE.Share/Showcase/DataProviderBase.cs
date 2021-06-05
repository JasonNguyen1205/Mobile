using System;
using System.Reactive.Linq;

namespace BLE.Share.Showcase
{
    public abstract class DataProviderBase<T> : BLE.Share.Showcase.IDataProvider<T>
    {
        private readonly TimeSpan _period;
        protected DataProviderBase(TimeSpan period)
        {
            _period = period;
        }

        public virtual IObservable<T> Data
        {
            get { return Observable.Interval(_period).Select(t => OnNext()); }
        }

        protected abstract T OnNext();
    }
}