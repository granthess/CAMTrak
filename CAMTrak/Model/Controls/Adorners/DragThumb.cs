using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using CAMTrak.Model.CADElements.Parts;
using System.Windows.Input;

namespace CAMTrak.Model.Controls.Adorners
{
    public class DragThumb : Thumb
    {
        public Position LinkedPosition { get; private set; }

        public DragThumb(Position LinkedPosition)
            : base()
        {
            DefaultStyleKey = typeof(DragThumb);

            this.LinkedPosition = LinkedPosition;
            LinkedPosition.HookupEvent(this);
            
            Cursor = Cursors.Cross;
            Width = 10;
            Height = 10;
            
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            Pen myPen = new Pen(Brushes.Red, 2);
            Brush myBrush = Brushes.Red;
            drawingContext.DrawRectangle(myBrush, null, new Rect(new Size(Width, Height)));
        }

    }
}