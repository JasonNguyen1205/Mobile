using System.Collections.ObjectModel;
using Acr.UserDialogs;
using Mobile.Function;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Extensions;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Android.Content;
using Android.App;
using SciChart.Charting.Visuals;
using Mobile;

namespace Mobile
{
    partial class SciChartSurfaceViewModel:BaseViewModel
    {
        // General
        private readonly IMvxNavigationService _navigation;
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ICharacteristic Characteristic { get; private set; }
        private Context context;

        public IDescriptor Descriptor { get; set; }
        public string CharacteristicValue => Characteristic?.Value.ToHexString().Replace("-", " ");

        private readonly IUserDialogs _userDialogs;
        public Core core = new Core();
        public bool _updatesStarted;


        public SciChartSurfaceViewModel(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;
        }

        public override void Prepare()
        {
            //// Set our view from the "main" layout resource
            //this.SetContentView(Resource.Layout.Main);
            SciChartSurface.SetRuntimeLicenseKey("p4EHEHIJEMU5vlOWuaMd0iAfxvucSkiRat2vlZqeGnsnJNO/IIzxhwboFp/KIV5OUtI/vvXkR9ZZMJUA1MECdq8Qcht8PyANsApS3hQ+ERfgSUSYay3Z/RCP1cYBzWPU3Svp3ApH6Lk9ECb5GIaaKMmVGaDc/cavUCE7LsmDfgBXBnYQPiQFGxjNMXE7HKGri8tsNKmFluPfslol1mGZTiPQjh4d6oNdrz47ao9F8xwrh6GkdqLhdSeZKqpMS/E+MSJURWqC2NwEGGDMqqSHcgjF4jQPXhyCpdCx5SD6RhcUsCXpbuW0prjqEUylvUDgh8jdWKfOCSjkhvfUZO+q+BdaN7Oi9CkQbr11EFDHp6DXJqLEpZjZ0A39F2KjWemKmS/ePIf5FEncIq1RkUCynPnnyJW1gTMjvYWJxsLOdd3IEqwq14yqVvm7aLFtUghTNbWPqQTX//c+FQxjs8EzEPZKM+qJIR7hDy3Yi8iT9Bt31Y8T8q8mn2fubfY2sFEnsDCBxabShyG1xCYfyqUYnxbIrMtbZYENcQ==");
            // Get our chart from the layout resource,
            //var chart = Resource.Id.Chart;

            // Get our chart from the layout resource,
            //var chart = 
            // Create a numeric X axis
            var xAxis = new NumericAxis(context) { AxisTitle = "Number of Samples (per Series)" };

            // Create a numeric Y axis
            var yAxis = new NumericAxis(context)
            {
                AxisTitle = "Value",
                VisibleRange = new DoubleRange(-1, 1)
            };

            // Add xAxis to the XAxes collection of the chart
            chart.XAxes.Add(xAxis);

            // Add yAxis to the YAxes collection of the chart
            chart.YAxes.Add(yAxis);
        }


    }
}
