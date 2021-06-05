using Android.Util;
using System;
using System.Collections.Generic;
using System.Timers;
#if __ANDROID__

#endif

namespace BLE.Share.Data
{
    public interface IMarketDataService
    {
        void SubscribePriceUpdate(Action<BLE.Share.Data.PriceBar> callback);
        IEnumerable<BLE.Share.Data.PriceBar> GetHistoricalData(int numberBars);
        void ClearSubscriptions();
        BLE.Share.Data.PriceBar GetNextBar();
    }

    public delegate void OnNewData(BLE.Share.Data.PriceBar data);

    public class MarketDataService : IMarketDataService
    {
        private readonly object _syncRoot = new object();

        private volatile bool _isRunning = false;

        private readonly BLE.Share.Data.RandomPricesDataSource _generator;
        private readonly Timer _timer;

        public event OnNewData NewData;

        public MarketDataService(DateTime startDate, int timeFrameMinutes, int tickTimerIntervalms)
        {
            _generator = new RandomPricesDataSource(timeFrameMinutes, true, 25, 367367, 30, startDate);

            _timer = new Timer(tickTimerIntervalms);
        }

        public void SubscribePriceUpdate(Action<BLE.Share.Data.PriceBar> callback)
        {
            lock (_syncRoot)
            {
                if (_isRunning) return;

                NewData += (arg) => callback(arg);

                _isRunning = true;
                _timer.Elapsed += TimerElapsed;
                _timer.AutoReset = true;
                _timer.Start();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_syncRoot)
                {
                    OnTimerElapsed();
                }
            }
            catch (Exception exception)
            {
#if __ANDROID__
                Log.Error("RandomPricesDataSource", exception.Message, exception);
#elif __IOS__
                Console.WriteLine("RandomPricesDataSource exception message: {0}, exception {1}", exception.Message, exception);
#endif
            }
        }

        private void OnTimerElapsed()
        {
            if (!_isRunning) return;

            var priceBar = _generator.Tick();

            if (NewData != null)
            {
                NewData(priceBar);
            }
        }

        public IEnumerable<BLE.Share.Data.PriceBar> GetHistoricalData(int numberBars)
        {
            var prices = new List<BLE.Share.Data.PriceBar>(numberBars);
            for (var i = 0; i < numberBars; i++)
            {
                prices.Add(_generator.GetNextData());
            }

            return prices;
        }

        public void ClearSubscriptions()
        {
            lock (_syncRoot)
            {
                if (!_isRunning) return;

                _isRunning = false;
                _timer.Stop();

                NewData = null;
            }
        }

        public BLE.Share.Data.PriceBar GetNextBar()
        {
            return _generator.Tick();
        }
    }
}