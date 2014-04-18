using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAMTrak.Model.Trackbed;
using GalaSoft.MvvmLight;
using CAMTrak.Model.Events;

namespace CAMTrak.Model.TrackItems.Parts
{
    #region Documentation
    /// <summary>
    /// A TrackEndpoint specified a "connection" point at the end of a TrackItem
    /// and is often used to control the size and location of the TrackItem.
    /// 
    /// </summary>
    /// <remarks>
    /// Contains a TrackbedProperties instance that is shared between the two
    /// TrackItem intances.  When a connection is made, the TrackbedProperties value
    /// is copied (by reference) from the stationary item to the moving item.
    /// 
    /// Contains an X,Y, Altitude coordinate of where the endpoint is on the map.
    /// 
    /// Contains an X,Y coordinate of the B-Spline control handle for the point.
    /// If the track item is not a B-Spline type, the control handle is calculated
    /// and locked to provide a smooth connection.  This is how items are rotated
    /// to match when connecting.
    /// 
    /// Contains a reference to another endpoint which is null when not connected.
    /// 
    /// Contains an ID to allow for quick lookup in the TrackItem's Endpoints
    /// dictionary.  
    /// 
    /// Contains a bool specifying if it is a primary or secondary endpoint.  Only 
    /// two endpoints in a trackitem may be specified as primary.  When rotating a
    /// track item, the rotation is based on the line between the two primary endpoints.
    /// </remarks>
    #endregion
    public class TrackEndpoint : ViewModelBase
    {

        #region Notified Properties
        private TrackbedProperties _Trackbed;
        private void SetTrackbed(TrackbedProperties value) //TODO: rethink this
        {
            Set<TrackbedProperties>("Trackbed", ref _Trackbed, value);
            
            // If we are connected, update the other half of the connection, but don't
            // update if it is already set, otherwise it will infinite loop.
            if ((ConnectedEndpoint != null) && (ConnectedEndpoint.Trackbed != value))
            {
                ConnectedEndpoint.Trackbed = value;
            }
        }        
        public TrackbedProperties Trackbed { get { return _Trackbed; } set { SetTrackbed(value); } }


        private TrackEndpoint _ConnectedEndpoint;
        private void SetConnectedEndpoint(TrackEndpoint value)
        {
            Set<TrackEndpoint>("ConnectedEndpoint", ref _ConnectedEndpoint, value);
        }
        public TrackEndpoint ConnectedEndpoint { get { return _ConnectedEndpoint; } set { SetConnectedEndpoint(value); } }

        private string _ID = string.Empty;
        private void SetID(string value)
        {
            if (_ID == string.Empty)
            {
                _ID = value;
            }
        }
        public string ID { get { return _ID; } private set { SetID(value); } }

        #endregion 

        #region Private Fields
        private ITrackItem Parent;
        #endregion 

        #region Constructors

        public TrackEndpoint(ITrackItem Parent, string ID)
        {
            this.Parent = Parent;
            this.ID = ID;
            ConnectedEndpoint = null;
            Trackbed = null; // TODO: pull initial trackbedproperties from parent
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
