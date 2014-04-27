using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CAMTrak.Model.CADElements;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;
using CAMTrak.Utilities;
using System.Windows.Controls;
using System.Windows;

namespace CAMTrak.Model.Controls
{
    public class CADControlPolygon : CADControl
    {
        private Pen PreviewPen;
        public CADControlPolygon()
            : base()
        {
            PreviewPen = new Pen(Brushes.Red, 1);
            PreviewPen.DashStyle = DashStyles.Dash;
            PreviewPen.Freeze();
        }


        private StreamGeometry DrawGeometry(bool Preview = false)
        {
            StreamGeometry stream = new StreamGeometry();
            bool started = false;
            PointCollection points = new PointCollection();

            using (StreamGeometryContext gc = stream.Open())
            {
                Vector2 Loc;
                foreach (Position Pos in Positions)
                {
                    Pos.Offset = new Vector2(E.Left, E.Top);

                    if (Preview)
                    {
                        Loc = Pos.PreviewLocalPosition;
                    }
                    else
                    {
                        Loc = Pos.LocalPosition;
                    }

                    if (!started)
                    {
                        gc.BeginFigure(Loc, true, true);
                        started = true;
                    }
                    else
                    {
                        points.Add(Loc);
                    }
                }

                gc.PolyLineTo(points, true, true);
            }

            return stream;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            bool ShowPreview = false;

            foreach (Position pos in Positions)
            {
                if (pos.IsDragging)
                {
                    ShowPreview = true;
                    break;
                }
            }

            // Draw the actual geometry            
            drawingContext.DrawGeometry(Brushes.Green, new Pen(Brushes.Green, 1), DrawGeometry());

            // Show the preview as an overlay
            if (ShowPreview)
            {
                drawingContext.DrawGeometry(null, PreviewPen, DrawGeometry(true));
            }
        }


    }
}