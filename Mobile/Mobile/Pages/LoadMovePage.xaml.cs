using Mobile.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace Mobile.Pages
{
    [MvxContentPagePresentation(WrapInNavigationPage = true, NoHistory = false)]
    public partial class LoadMovePage : MvxContentPage<LoadMoveModel>
    {
        public LoadMovePage()
        {
            InitializeComponent();
        }
    }
}
