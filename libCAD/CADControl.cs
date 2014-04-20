using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CAMTrak.libCAD
{
    public class CADControl : FrameworkElement 
    {
        private Pen thePen;
        private Brush theBrush;

        public CADControl()
            : base()
        {
            thePen = new Pen(Brushes.Black, 1);
            theBrush = Brushes.LightSalmon;
        }

        private Point GetCenter()
        {
            return new Point(Width / 2.0, Height / 2.0);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawEllipse(theBrush, thePen, GetCenter(), Width / 2.0, Height / 2.0);
        }
    }
}
