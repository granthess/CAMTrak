using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.AvalonDock.Layout;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace CAMTrak.Model
{
    public class EditDocument : ViewModelBase
    {
        public int Width { get { return GetWidth(); } }
        public int Height { get { return GetHeight(); } }

        public ObservableCollection<ITrackItem> Data { get; private set; }

        public EditDocument()
        {
            Data = new ObservableCollection<ITrackItem>();
            _Width = 1500;
            _Height = 1200;

            // put a couple items in the Data

            ITrackItem item = new TrackItemGeneric();
            item.Left = 41;
            item.Top = 37;
            item.Width = 51;
            item.Height = 51;

            Data.Add(item);

            item = new TrackItemGeneric();
            item.Left = 100;
            item.Top = 97;
            item.Width = 94;
            item.Height = 31;

            Data.Add(item);
        }

        private int _Width;
        private int GetWidth()
        {

            return _Width;
        }

        private int _Height;
        private int GetHeight()
        {
            return _Height;
        }
    }
}
