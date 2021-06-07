
using MvvmCross.Forms.Presenters.Attributes;
using Mobile.ViewModels;
using MvvmCross.Forms.Views;

namespace Mobile.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false)]
    public partial class LoadTempPage : MvxContentPage<LoadTempModel>
    {
        public  LoadTempPage()
        {
            InitializeComponent();
        }
    }
}
