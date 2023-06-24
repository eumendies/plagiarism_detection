using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.models;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private ObservableCollection<MenuBar> menuBars;

        private readonly IRegionManager regionManager;

        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }

        // public DelegateCommand LoadedCommand { get; private set; }

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        
        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace)) 
                return; 
            regionManager.Regions["MainViewRegion"].RequestNavigate(obj.NameSpace);
        }

        public MainViewModel(IRegionManager regionManager)
        {
            menuBars = new ObservableCollection<MenuBar>();
            CreateMenuBars();
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            this.regionManager = regionManager;
        }

        void CreateMenuBars()
        {
            MenuBars.Add(new MenuBar() { Icon = "Typewriter", Title = "写作", NameSpace = "WritingView"});
            MenuBars.Add(new MenuBar() { Icon = "NoteSearch", Title = "查重", NameSpace = "DetectionView" });
            MenuBars.Add(new MenuBar() { Icon = "DataBase", Title = "论文库", NameSpace = "DataBaseView" });
            MenuBars.Add(new MenuBar() { Icon = "TextBox", Title = "文档", NameSpace = "DocView" });
        }
    }
}
