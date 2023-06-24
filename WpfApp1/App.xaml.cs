using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Views;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WritingView, WritingViewModel>();
            containerRegistry.RegisterForNavigation<DetectionView, DetectionViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, MainViewModel>();
            containerRegistry.RegisterForNavigation<DataBaseView, DataBaseViewModel>();
            containerRegistry.RegisterForNavigation<DocView, DocViewModel>();
        }
    }
}
