using System;
using Mobile.ViewModels;
using MvvmCross;
using MvvmCross.Forms.Core;
using MvvmCross.IoC;
using MvvmCross.Localization;
using MvvmCross.ViewModels;
using Xamarin.Forms;

namespace Mobile
{
    public class BleMvxApplication : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            RegisterAppStart<DeviceListViewModel>();
        }
    }
}
