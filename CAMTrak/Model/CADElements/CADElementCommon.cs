using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;

namespace CAMTrak.Model.CADElements
{
    public class CADElementCommon : ViewModelBase, ICADElement
    {
        public bool IsActive { get; set; }

        private string _Name;
        private void SetName(string value)
        {
            Set<string>("Name", ref _Name, value);
        }
        public string Name { get { return _Name; } set { SetName(value); } }
        private double _Left;
        private void SetLeft(double value)
        {
            Set<double>("Left", ref _Left, value);
        }
        public double Left { get { return _Left; } set { SetLeft(value); } }

        private double _Top;
        private void SetTop(double value)
        {
            Set<double>("Top", ref _Top, value);
        }
        public double Top { get { return _Top; } set { SetTop(value); } }

        private ObservableCollection<Position> _Positions;
        private void SetPositions(ObservableCollection<Position> value)
        {
            Set<ObservableCollection<Position>>("Positions", ref _Positions, value);
        }
        public ObservableCollection<Position> Positions { get { return _Positions; } set { SetPositions(value); } }

        public CADElementCommon()
        {
            Positions = new ObservableCollection<Position>();
        }

        protected virtual void PositionSnapDrag(object sender, Events.PositionDragEventArgs e)
        {
            throw new NotImplementedException();
        }


        protected virtual void PositionConstrainDrag(object sender, Events.PositionDragEventArgs e)
        {
            throw new NotImplementedException();
        }
        

    }
}
