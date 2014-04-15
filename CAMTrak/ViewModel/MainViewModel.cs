using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CAMTrak;
using GalaSoft.MvvmLight.Command;
using Xceed.Wpf.AvalonDock.Layout;
using CAMTrak.Model;

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
        public Model.Flippy George { get; set; }

        private EditDocument _CurrentDocument;
        public EditDocument CurrentDocument { get { return _CurrentDocument; } set { Set<EditDocument>("CurrentDocument", ref _CurrentDocument, value); } }

        private ITrackItem _CurrentItem;
        public ITrackItem CurrentItem { get { return _CurrentItem; } set { Set<ITrackItem>("CurrentItem", ref _CurrentItem, value); } }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(MainWindow window)
        {
            this.window = window;
            SetupCommands();
            George = new Model.Flippy();

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}




        }

        #region Commands
        public RelayCommand Button1Cmd { get; private set; }
        public RelayCommand CMDItemClick { get; private set; }
        private void SetupCommands()
        {
            Button1Cmd = new RelayCommand(Button1Go, Button1Can);
            CMDItemClick = new RelayCommand(CMDItemClickGo, CMDItemClickCan);
        }

        private void Button1Go()
        {
            EditDocument edoc = new EditDocument();
            LayoutDocument document = new LayoutDocument();
            document.Title = "New Document";
            document.Content = edoc;
            window.documentPane1.Children.Add(document);
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