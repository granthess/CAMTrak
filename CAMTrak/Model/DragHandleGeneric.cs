using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace CAMTrak.Model
{
	public class DragHandleGeneric : ViewModelBase,  IDragHandle
	{
        private int _Width;
        private int _Height;
        private int _Left;
        private int _Top;

        public int Width { get { return _Width; } set { Set<int>("Width", ref _Width, value); } }
        public int Height { get { return _Height; } set { Set<int>("Height", ref _Height, value); } }
        public int Left { get { return _Left; } set { Set<int>("Left", ref  _Left, value); } }
        public int Top { get { return _Top; } set { Set<int>("Left", ref  _Top, value); } }
	
	}
}
