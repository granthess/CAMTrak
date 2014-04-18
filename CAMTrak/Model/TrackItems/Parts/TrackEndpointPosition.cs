using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using CAMTrak.MathUtils;

namespace CAMTrak.Model.TrackItems.Parts
{
    public class TrackEndpointPosition : ViewModelBase 
    {
        #region notified properties
        private Vector2 _Position;
        private void SetPosition(Vector2 value)
        {
            Set<Vector2>("Position", ref _Position, value);
        }
        public Vector2 Position { get { return _Position; } set { SetPosition(value); } }

        private double _Altitude;
        private void SetAltitude(double value)
        {
            Set<double>("Altitude", ref _Altitude, value);
        }
        public double Altitude { get { return _Altitude; } set { SetAltitude(value); } }
        #endregion

        #region private fields
        #endregion

        #region Constructors
        public TrackEndpointPosition()
        {
        }

        public TrackEndpointPosition(Vector2 Position, double Altitude = 0.0)
            : this()
        {
            this.Position = new Vector2(Position);
            this.Altitude = Altitude;
        }
        #endregion

    }
}
