using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace CAMTrak.Model
{
    public class TrackItemGeneric :   ViewModelBase, ITrackItem
    {
        private int _Width;
        private int _Height;
        private int _Left;
        private int _Top;
                
        public int Width { get { return _Width; } set { Set<int>("Width", ref _Width, value); } }
        public int Height { get { return _Height; } set { Set<int>("Height", ref _Height, value); } }
        public int Left { get { return _Left; } set { Set<int>("Left", ref  _Left, value); } }
        public int Top { get { return _Top; } set { Set<int>("Left", ref  _Top, value); } }

        private ObservableCollection<IDragHandle> _Handles;
        public ObservableCollection<IDragHandle> Handles
        {
            get { return _Handles; }
            private set { Set<ObservableCollection<IDragHandle>>("Handles", ref  _Handles, value); }
        }

        public TrackItemGeneric()
        {
            Handles = new ObservableCollection<IDragHandle>();
            IDragHandle Handle = new DragHandleGeneric();
            Handles.Add(Handle);
        }
    }
}
