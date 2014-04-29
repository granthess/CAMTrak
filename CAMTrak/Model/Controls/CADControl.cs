using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Controls;
using CAMTrak.Model.CADElements;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;
using System.ComponentModel;
using CAMTrak.Utilities;
using CAMTrak.Model.Controls.Adorners;

namespace CAMTrak.Model.Controls
{
    public class CADControl : FrameworkElement
    {


        public ICADElement E { get { return DataContext as ICADElement; } }
        public ObservableCollection<Position> Positions { get { return E.Positions; } }

        private Pen thePen;
        private Brush theBrush;

        private CADAdorner Adorner;

        public CADControl()
            : base()
        {      

            thePen = new Pen(Brushes.Black, 1);
            theBrush = Brushes.LightSalmon;
            Width = 40;
            Height = 40;

            Loaded += new RoutedEventHandler(CADControl_Loaded);
                       
        }

        void pos_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDragging")
            {                
                InvalidateVisual();
                Adorner.InvalidateVisual();
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if ((Positions != null) && (Positions.Count > 0))
            {
                Vector2 Min = Positions[0];
                Vector2 Max = Positions[0];

                // Get the min/max positions
                foreach (Vector2 Pos in Positions)
                {
                    Min = Vector2.Min(Min, Pos);
                    Max = Vector2.Max(Max, Pos);
                }

                // Now adjust so that the Min becomes top/left
                foreach (Position Pos in Positions)
                {                    
                    Pos.Offset = Min;
                }

                E.Left = Min.x;
                E.Top = Min.y;

                Width = (Max - Min).x;
                Height = (Max - Min).y;
                return new Size(Width, Height);

            }
            else
            {
                return base.ArrangeOverride(finalSize);
            }
        }

        void CADControl_Loaded(object sender, RoutedEventArgs e)
        {
            AddAdorner();
            foreach (Position pos in Positions)
            {
                pos.PropertyChanged += new PropertyChangedEventHandler(pos_PropertyChanged);
                pos.PositionChanged += new EventHandler(pos_PositionChanged);
            }
        }

        void pos_PositionChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
            Adorner.InvalidateVisual();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (Adorner != null)
            {
                Adorner.Show();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (Adorner != null)
            {
                Adorner.Hide();
            }
        }
        public void AddAdorner()
        {
            Adorner = new CADAdorner(this, E.Positions);            
        }

        void inAdorner_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            InvalidateMeasure();
        }

        void inAdorner_DragDelta(object sender, DragDeltaEventArgs e)
        {
            E.Left += e.HorizontalChange;
            E.Top += e.VerticalChange;
        }

        private Point GetCenter()
        {            
            return new Point(Width / 2.0, Height / 2.0);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawEllipse(theBrush, thePen, GetCenter(), Width / 2.0, Height / 2.0);
            drawingContext.DrawLine(thePen, new Point(0,0), new Point(-Width, -Height));
         
        }
    }
}
