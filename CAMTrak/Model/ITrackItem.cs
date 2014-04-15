using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid;


namespace CAMTrak.Model
{
    public interface ITrackItem
    {


        int Width { get; set; }
        int Height { get; set; }
        int Left { get; set; }
        int Top { get; set; }

        ObservableCollection<IDragHandle> Handles { get; }
        
    }
}
