using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Windows.Controls;
using System.Windows;


namespace CAMTrak.Model
{
    public interface ITrackItem
    {


        double Width { get; set; }
        double Height { get; set; }
        double Left { get; set; }
        double Top { get; set; }

        UIElement Control { get; }

    }
}
