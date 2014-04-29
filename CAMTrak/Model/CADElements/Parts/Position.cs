using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAMTrak.Utilities;
using GalaSoft.MvvmLight;
using System.Windows;
using CAMTrak.Model.CADElements.Events;
using System.Windows.Controls.Primitives;

namespace CAMTrak.Model.CADElements.Parts
{
    /// <summary>
    /// Position class to record the location of various control points
    /// for CADElements and CADControls
    /// </summary>
    public class Position : ViewModelBase
    {
        #region Notified Properties
        private bool _IsDragging;
        private void SetIsDragging(bool value)
        {
            Set<bool>("IsDragging", ref _IsDragging, value);
        }
        public bool IsDragging { get { return _IsDragging; } set { SetIsDragging(value); } }
        
        

        private Vector2 _Offset;
        private void SetOffset(Vector2 value)
        {
            Set<Vector2>("Offset", ref _Offset, value);
        }
        public Vector2 Offset { get { return _Offset; } set { SetOffset(value); } }


        public Vector2 LocalPosition { get { return AbsolutePosition - Offset; } }
        public Vector2 AbsolutePosition { get { return _AbsolutePosition; } set { _AbsolutePosition = value; _PreviewPosition = value; } }

        public Vector2 PreviewPosition { get { return GetPreviewPosition(); } set { _PreviewPosition = value; } }
        public Vector2 PreviewLocalPosition { get { return PreviewPosition - Offset; } }

        public Vector2 RequestedPosition { get; private set; }

        private Vector2 GetPreviewPosition()
        {
            if (IsDragging)
            {
                return _PreviewPosition;
            }
            else
            {
                return AbsolutePosition;
            }
        }

        #endregion

        #region Private fields
        Vector2 _AbsolutePosition;
        Vector2 _PreviewPosition;
        #endregion

        #region Eventing
        public delegate void PositionDragEventHandler(object sender, PositionDragEventArgs e);

        public event PositionDragEventHandler PositionConstrainDrag;
        public event PositionDragEventHandler PositionSnapDrag;

        public event EventHandler PositionChanged;

        protected virtual void OnPositionChanged()
        {

            if (PositionChanged != null)
            {
                PositionChanged(this, new EventArgs());
            }
        }

        protected virtual Vector2 OnPositionContrainDrag(Vector2 ActualPosition)
        {
            Vector2 Preview = new Vector2(ActualPosition);
            PositionDragEventArgs e = new PositionDragEventArgs(ActualPosition, Preview);
            if (PositionConstrainDrag != null)
            {
                PositionConstrainDrag(this, e);
            }

            if (e.Handled)
            {
                return e.RequestedPosition;
            }
            else
            {
                return ActualPosition;
            }
        }

        protected virtual Vector2 OnPositionSnapDrag(Vector2 ActualPosition)
        {
            Vector2 Preview = new Vector2(ActualPosition);
            PositionDragEventArgs e = new PositionDragEventArgs(ActualPosition, Preview);
            if (PositionSnapDrag != null)
            {
                PositionSnapDrag(this, e);
            }

            if (e.Handled)
            {
                return e.RequestedPosition;
            }
            else
            {
                return ActualPosition;
            }
        }
        #endregion

        #region DragDelta support
        private Vector2 DragDelta(Vector2 Offset)
        {
            // First calculate the new position
            RequestedPosition += Offset;
            Vector2 Preview;

            // Then do a constrain pass            
            Preview = new Vector2(OnPositionContrainDrag(RequestedPosition));

            // And a snap pass
            Preview = new Vector2(OnPositionSnapDrag(Preview));

            // finaly return the result
            return Preview;
        }

        public void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            RequestedPosition = AbsolutePosition;
            Vector2 Offset = new Vector2(e.HorizontalOffset, e.VerticalOffset);
            PreviewPosition = DragDelta(Offset);
            IsDragging = true;
            e.Handled = true;
        }

        public void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {            
            e.Handled = true;
            AbsolutePosition = PreviewPosition;
            IsDragging = false;
            OnPositionChanged();
        }

        public void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector2 Offset = new Vector2(e.HorizontalChange, e.VerticalChange);
            PreviewPosition = DragDelta(Offset);
            IsDragging = true;
            //e.Handled = true;
            OnPositionChanged();
        }
        #endregion

        #region Constructors
        public Position()
        {
            AbsolutePosition = Vector2.Zero;
            PreviewPosition = Vector2.Zero;
            Offset = Vector2.Zero;
            IsDragging = false;
        }

        public Position(Position Source)
        {
            this.AbsolutePosition = Source.AbsolutePosition;
        }

        public Position(Vector2 Source)
        {
            this.AbsolutePosition = Source;
        }

        public Position(Point Source)
        {
            this.AbsolutePosition = new Vector2(Source);
        }

        public Position(double X, double Y)
        {
            AbsolutePosition = new Vector2(X, Y);
        }
        #endregion

        #region Helpers
        public void HookupEvent(Thumb source)
        {
            source.DragStarted += new DragStartedEventHandler(Thumb_DragStarted);
            source.DragCompleted += new DragCompletedEventHandler(Thumb_DragCompleted);
            source.DragDelta +=new DragDeltaEventHandler(Thumb_DragDelta);

        }
        #endregion

        #region Operators and Conversions
        public static implicit operator Vector2(Position Source)
        {
            return new Vector2(Source.AbsolutePosition);
        }

        public static Position operator - (Position a)
        {
            return new Position (-(Vector2)a);
        }

        public static Position operator - (Position a, double scalar)
        {
            return new Position ((Vector2)a - scalar);
        }

        public static Position operator - (Position a, Position b)
        {
            return new Position ((Vector2)a - (Vector2)b);
        }
        #endregion
    }
}
