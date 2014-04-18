using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAMTrak.MathUtils;
using CAMTrak.Model.TrackItems.Parts;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace CAMTrak.Model.TrackItems
{
    public class TrackItemStraight : TrackItemBase
    {

        #region Notified properties
        #endregion

        #region Constructors
        public TrackItemStraight(EditDocument Parent)
            : base(Parent)
        {
            Endpoints = new Dictionary<string, TrackEndpoint>(2);
        }

        public TrackItemStraight(EditDocument Parent, Vector2 PA0, Vector2 PA1)
            : this (Parent)
        {
            this.A0 = new TrackEndpoint(this, "A.0", PA0);
            this.A1 = new TrackEndpoint(this, "A.1", PA1);
            Endpoints.Add("A.0", this.A0);
            Endpoints.Add("A.1", this.A1 );
        }

        #endregion

        #region Calculated Properties
        new public double Angle { get { return GetAngle(); } set { SetAngle(value); } }
        new public Vector2 Center { get { return GetCenter(); } set { SetCenter(value); } }


        /// <summary>
        /// Angle is the angle from the center to the A.1 position from center to center.y -1
        /// </summary>
        /// <returns></returns>
        private double GetAngle()
        {
            Vector2 V1 = (A1.Position.Position - Center);

            double angle = MathUtils.MathUtils.Radian2Degree(Math.Atan2(V1.y, V1.x));
            return angle;
        }

        private void SetAngle(double value)
        {
            // get the distance from Center to an endpoint
            double Dist = (Center - A0.Position.Position).Length();
            double Ang = MathUtils.MathUtils.Degree2Radian(value);
            Vector2 NewVec = new Vector2(Math.Cos(Ang), Math.Sin(Ang));

            A0.Position.Position = -NewVec * Dist;
            A1.Position.Position = NewVec * Dist;

            RegenerateGeometry();
        }

        private Vector2 GetCenter()
        {
            return (A0.Position.Position + A1.Position.Position) / 2.0;
        }

        private void SetCenter(Vector2 value)
        {

        }
        #endregion

        #region Geometry
        
        public override void GenerateOutlineDrawing()
        {
            Pen OutlinePen;

            if (IsActive)
            {
                OutlinePen = new Pen(Brushes.Red, 0.5);
            }
            else
            {
                OutlinePen = new Pen(Brushes.DarkBlue, 0.5);
            }

            // Draw a box around the outside of the track item
            // the box should be 120 inches wide
            // TODO: use scale values, not fixed here
            GeometryGroup OutlineGeometry = new GeometryGroup();
            if ((A1 != null) && (A0 != null))
            {
                Vector2 CenterVector = A1.Position.Position - A0.Position.Position;
                Vector2 Normal = new Vector2(CenterVector.x, -CenterVector.y);
                double offset = 60 / 160.0;


                Vector2 tmp;

                tmp = A0.Position.Position + (Normal * offset);

                Vector2 begin = new Vector2(tmp);

                PathFigure myPathFigure = new PathFigure();
                myPathFigure.StartPoint = new Point(tmp.x, tmp.y);

                double minX = tmp.x;
                double minY = tmp.y;
                double maxX = tmp.x;
                double maxY = tmp.y;


                LineSegment myLineSegment = new LineSegment();
                PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

                tmp = A0.Position.Position + (-Normal * offset);
                myLineSegment.Point = new Point(tmp.x, tmp.y);

                minX = Math.Min(minX, tmp.x);
                minY = Math.Min(minY, tmp.y);
                maxX = Math.Max(maxX, tmp.x);
                maxY = Math.Max(maxX, tmp.y);
                myPathSegmentCollection.Add(myLineSegment);

                tmp = A1.Position.Position + (-Normal * offset);
                myLineSegment.Point = new Point(tmp.x, tmp.y);

                minX = Math.Min(minX, tmp.x);
                minY = Math.Min(minY, tmp.y);
                maxX = Math.Max(maxX, tmp.x);
                maxY = Math.Max(maxX, tmp.y);
                myPathSegmentCollection.Add(myLineSegment);

                tmp = A1.Position.Position + (Normal * offset);
                myLineSegment.Point = new Point(tmp.x, tmp.y);

                minX = Math.Min(minX, tmp.x);
                minY = Math.Min(minY, tmp.y);
                maxX = Math.Max(maxX, tmp.x);
                maxY = Math.Max(maxX, tmp.y);
                myPathSegmentCollection.Add(myLineSegment);

                myPathFigure.Segments = myPathSegmentCollection;

                PathFigureCollection myPathFigureCollection = new PathFigureCollection();
                myPathFigureCollection.Add(myPathFigure);

                PathGeometry myPathGeometry = new PathGeometry();
                myPathGeometry.Figures = myPathFigureCollection;
                OutlineGeometry.Children.Add(myPathGeometry);

                _Width = maxX - minX;
                _Height = maxY - minY;
                _Left = minX;
                _Top = minY;
            }
            else
            {
                OutlineGeometry.Children.Add(new RectangleGeometry(new Rect(0.0f, 0.0f, Width, Height)));
            }
            
                       
            OutlineDrawing = new GeometryDrawing(Brushes.Pink, OutlinePen, OutlineGeometry);
        }
        #endregion

    }
}
