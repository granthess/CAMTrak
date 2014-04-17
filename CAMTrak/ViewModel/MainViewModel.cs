using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CAMTrak;
using GalaSoft.MvvmLight.Command;
using Xceed.Wpf.AvalonDock.Layout;
using CAMTrak.Model;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Zoombox;
using System.Linq;

namespace CAMTrak.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public MainWindow window { get; private set; }
        
        private EditDocument _CurrentDocument;
        public EditDocument CurrentDocument { get { return _CurrentDocument; } set { Set<EditDocument>("CurrentDocument", ref _CurrentDocument, value); } }

        private ObservableCollection<EditDocument> _Documents;
        public ObservableCollection<EditDocument> Documents
        {
            get { return _Documents; }
            set { Set<ObservableCollection<EditDocument>>("Documents", ref _Documents, value); }
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(MainWindow window)
        {
            this.window = window;
            Documents = new ObservableCollection<EditDocument>();
            SetupCommands();
        
            window.dockingManager1.DocumentClosing += new System.EventHandler<Xceed.Wpf.AvalonDock.DocumentClosingEventArgs>(dockingManager1_DocumentClosing);

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}




        }

        void dockingManager1_DocumentClosing(object sender, Xceed.Wpf.AvalonDock.DocumentClosingEventArgs e)
        {

            // make sure the document isn't open
            EditDocument edoc = e.Document.Content as EditDocument;

            if (edoc.Changed)
            {
                string messageBoxText = "Do you want to save changes to this layout before closing?  Click Yes to save and close, No to close without saving, or Cancel to not close.";
                string caption = "Close Window";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Question;

                // Display message box
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                // Process message box results 
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        edoc.Changed = false;
                        e.Cancel = false;
                        break;
                    case MessageBoxResult.No:
                        edoc.Changed = true;
                        e.Cancel = false;
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
            else
            {
                e.Cancel = false;
            }            
        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        #region Commands
        public RelayCommand Button1Cmd { get; private set; }        
        public RelayCommand CMDItemClick { get; private set; }
        private void SetupCommands()
        {
            Button1Cmd = new RelayCommand(Button1Go, Button1Can);            
            CMDItemClick = new RelayCommand(CMDItemClickGo, CMDItemClickCan);
        }



        private void CreateNewWindow(string Title)
        {

            EditDocument Edoc = new EditDocument();
            Documents.Add(Edoc);
            
            var Ldoc = (from i in window.documentPane1.Children
                        where i.Content == Edoc
                        select i).First();
                        

            Edoc.SetLayout(Title, Ldoc);
            CurrentDocument = Edoc;
            

            //var x = Ldoc.Root.Manager.GetLayoutItemFromModel(Ldoc).View;
            //x.ApplyTemplate();
            //var y = x.Content;

            //Edoc.SetSetSet();
            
        }

 


        private void Button1Go()
        {
            CreateNewWindow("Untitled");
                       
        }

        
        private bool Button1Can()
        {
            return true;
        }


        private void CMDItemClickGo()
        {

        }

        private bool CMDItemClickCan()
        {
            return true;
        }
        #endregion
    }
}