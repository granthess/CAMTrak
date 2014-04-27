using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;
using CAMTrak.Model.Controls;
using CAMTrak.Utilities;

namespace CAMTrak.Model.CADElements
{
    public class CADElementBaseTri : ViewModelBase,  ICADElement
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
        public double Left { get { return _Left; } set {SetLeft(value);}}

        private double _Top;
        private void SetTop(double value)
        {
            Set<double>("Top", ref _Top, value);
        }
        public double Top { get { return _Top; } set {SetTop(value);}}

        private ObservableCollection<Position> _Positions;
        private void SetPositions(ObservableCollection<Position> value)
        {
            Set<ObservableCollection<Position>>("Positions", ref _Positions, value);
        }
        public ObservableCollection<Position> Positions { get { return _Positions; } set { SetPositions(value); } }


        public CADElementBaseTri()
        {
            Positions = new ObservableCollection<Position>();
        }

        public CADElementBaseTri(Position V1, Position V2, Position V3)
            : this()
        {
            Positions.Add(V1);
            Positions.Add(V2);
            Positions.Add(V3);

            V1.PositionConstrainDrag += new Position.PositionDragEventHandler(PositionConstrainDrag);
            V2.PositionConstrainDrag += new Position.PositionDragEventHandler(PositionConstrainDrag);
            V3.PositionConstrainDrag += new Position.PositionDragEventHandler(PositionConstrainDrag);

            V1.PositionSnapDrag += new Position.PositionDragEventHandler(PositionSnapDrag);
            V2.PositionSnapDrag += new Position.PositionDragEventHandler(PositionSnapDrag);
            V3.PositionSnapDrag += new Position.PositionDragEventHandler(PositionSnapDrag);
        }

        void PositionSnapDrag(object sender, Events.PositionDragEventArgs e)
        {
            double x = (int)(e.ActualPosition.x / 10) * 10;
            double y = (int)(e.ActualPosition.y / 10) * 10;
            e.RequestedPosition = new Vector2(x, y);
            e.Handled = true;
        }

        void PositionConstrainDrag(object sender, Events.PositionDragEventArgs e)
        {
            
        }


    }
}
