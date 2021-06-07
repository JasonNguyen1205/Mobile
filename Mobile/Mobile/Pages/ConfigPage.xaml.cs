using Mobile.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace Mobile.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false)]
    public partial class ConfigPage : MvxContentPage<ConfigViewModel>
    {
        public ConfigPage()
        {
            InitializeComponent();
        }
    }
}
