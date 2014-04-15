using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CAMTrak.ViewModel;
using CAMTrak.Model;

namespace CAMTrak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel VM { get { return DataContext as MainViewModel; } }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this);
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            VM.CurrentItem = ((sender as Rectangle).DataContext as ITrackItem);


            e.Handled = true;
        }
    }
}
