using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAMTrak.Utilities;

namespace CAMTrak.Model.CADElements.Events
{
    public class PositionDragEventArgs : EventArgs
    {
        public Vector2 ActualPosition { get; private set; }
        public Vector2 RequestedPosition { get; set; }
        public bool Handled { get; set; }
        public PositionDragEventArgs(Vector2 ActualPosition, Vector2 RequestedPosition)
        {
            this.ActualPosition = ActualPosition;
            this.RequestedPosition = RequestedPosition;
            this.Handled = false;
        }
    }
}
