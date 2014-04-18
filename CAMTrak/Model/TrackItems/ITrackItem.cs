using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Windows.Controls;
using System.Windows;
using CAMTrak.Model.TrackItems.Parts;


namespace CAMTrak.Model.TrackItems
{
    public interface ITrackItem
    {


        double Width { get; set; }
        double Height { get; set; }
        double Left { get; set; }
        double Top { get; set; }

        bool IsActive { get; set; }

        UIElement Control { get; }

        // The Endpoints dict, stores the endpoints for the trackitem by ID.
        // With nested trackitems (groups, yards, crossovers, etc.) the endpoints
        // are prefixed with the trackitem's ID with the format "{0}.{1}" 
        Dictionary<string, TrackEndpoint> Endpoints { get; }

    }
}
