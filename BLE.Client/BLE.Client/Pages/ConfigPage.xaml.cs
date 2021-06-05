﻿using BLE.Client.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;

namespace BLE.Client.Pages
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
