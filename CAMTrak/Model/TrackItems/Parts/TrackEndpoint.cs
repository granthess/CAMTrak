using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using CAMTrak.Model.Events;
using CAMTrak.Utilities;

namespace CAMTrak.Model.TrackItems.Parts
{
    #region Documentation
    /// <summary>
    /// A TrackEndpoint specified a "connection" point at the end of a TrackItem
    /// and is often used to control the size and location of the TrackItem.
    /// 
    /// </summary>

    #endregion
    public class TrackEndpoint : ViewModelBase
    {

        #region Notified Properties
        
        private string _ID = string.Empty;
        private void SetID(string value)
        {
            if (_ID == string.Empty)
            {
                _ID = value;
            }
        }
        public string ID { get { return _ID; } private set { SetID(value); } }

        private TrackEndpointPosition _Position;
        private void SetPosition(TrackEndpointPosition value)
        {
            Set<TrackEndpointPosition>("Position", ref _Position, value);
        }
        public TrackEndpointPosition Position { get { return _Position; } set { SetPosition(value); } }


        #endregion 

        #region Private Fields
        private ITrackItem Parent;
        #endregion 

        #region Constructors

        public TrackEndpoint(ITrackItem Parent, string ID)
        {
            this.Parent = Parent;
            this.ID = ID;        
        }

        public TrackEndpoint(ITrackItem Parent, string ID, Vector2 Position, double Altitude = 0.0)
            : this(Parent, ID)
        {
            this.Position = new TrackEndpointPosition(Position, Altitude);

        }

        #endregion 

        #region Events
        public delegate void EndpointConnectionEventHandler(object sender, EndpointConnectionEventArgs e);
        public delegate void EndpointRotationEventHandler(object sender, EndpointRotationEventArgs e);
        public delegate void EndpointMoveEventHandler(object sender, EndpointMoveEventArgs e);

        public event EndpointConnectionEventHandler ConnectionChanged;
        public event EndpointRotationEventHandler ConnectionRotating;
        public event EndpointRotationEventHandler ConnectionRotated;
        public event EndpointMoveEventHandler ConnectionMoving;
        public event EndpointMoveEventHandler ConnectionMoved;

        protected virtual void OnConnectionChanged(EndpointConnectionEventArgs e)
        {
            if (ConnectionChanged != null)
                ConnectionChanged(this, e);
        }

        protected virtual void OnConnectionRotating(EndpointRotationEventArgs e)
        {
            if (ConnectionRotating != null)
                ConnectionRotating(this, e);
        }

        protected virtual void OnConnectionRotated(EndpointRotationEventArgs e)
        {
            if (ConnectionRotated != null)
                ConnectionRotated(this, e);
        }

        protected virtual void OnConnectionMoving(EndpointMoveEventArgs e)
        {
            if (ConnectionMoving != null)
                ConnectionMoving(this, e);
        }

        protected virtual void OnConnectionMoved(EndpointMoveEventArgs e)
        {
            if (ConnectionMoved != null)
                ConnectionMoved(this, e);
        }
        #endregion

        #region Move and Rotate

        #endregion

        #region Connect / Disconnect
        #endregion
    }
}
