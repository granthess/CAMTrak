using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;
using CAMTrak.Model.Controls;

namespace CAMTrak.Model.CADElements
{
    public interface ICADElement
    {
        bool IsActive { get; set; }

        double Left { get; set; }
        double Top { get; set; }

        string Name { get; set; }

        ObservableCollection<Position> Positions { get; }        
    }
}
