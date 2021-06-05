using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross.Forms.Platforms.Android.Views;
using SciChart.Charting.Visuals;
using System;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace BLE.Client.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait
        , ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity
        : MvxFormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;
            base.OnCreate(bundle);

            try
            {
                SciChartSurface.SetRuntimeLicenseKey("p4EHEHIJEMU5vlOWuaMd0iAfxvucSkiRat2vlZqeGnsnJNO/IIzxhwboFp/KIV5OUtI/vvXkR9ZZMJUA1MECdq8Qcht8PyANsApS3hQ+ERfgSUSYay3Z/RCP1cYBzWPU3Svp3ApH6Lk9ECb5GIaaKMmVGaDc/cavUCE7LsmDfgBXBnYQPiQFGxjNMXE7HKGri8tsNKmFluPfslol1mGZTiPQjh4d6oNdrz47ao9F8xwrh6GkdqLhdSeZKqpMS/E+MSJURWqC2NwEGGDMqqSHcgjF4jQPXhyCpdCx5SD6RhcUsCXpbuW0prjqEUylvUDgh8jdWKfOCSjkhvfUZO+q+BdaN7Oi9CkQbr11EFDHp6DXJqLEpZjZ0A39F2KjWemKmS/ePIf5FEncIq1RkUCynPnnyJW1gTMjvYWJxsLOdd3IEqwq14yqVvm7aLFtUghTNbWPqQTX//c+FQxjs8EzEPZKM+qJIR7hDy3Yi8iT9Bt31Y8T8q8mn2fubfY2sFEnsDCBxabShyG1xCYfyqUYnxbIrMtbZYENcQ==");
               
            }
            catch (Exception e)
            {
                Debug.WriteLine("SciChart", "Error when setting the license", e);
            }

        }
    }
}