using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            btnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };
            btnClose.Click += (s, e) => { this.Close(); };
            colorZone.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }
    }
}
