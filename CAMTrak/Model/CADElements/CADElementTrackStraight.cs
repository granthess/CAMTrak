using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;

namespace CAMTrak.Model.CADElements
{
    public class CADElementTrackStraight : CADElementTrackCommon
    {




        public CADElementTrackStraight()
        {
            Positions = new ObservableCollection<Position>();
        }
    }
}
