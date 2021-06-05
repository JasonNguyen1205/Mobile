using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
{   
    public partial class DeviceListPage : MvxTabbedPage<DeviceListViewModel>
    {
        public DeviceListPage()
        {
            InitializeComponent();
        }
    }
}
