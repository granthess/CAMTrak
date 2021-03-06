﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using CAMTrak.Model.CADElements.Parts;
using CAMTrak.Model.Controls;
using CAMTrak.Utilities;

namespace CAMTrak.Model.CADElements
{
    public class CADElementBaseboard : CADElementBaseCommon
    {



        private  CADElementBaseboard()
            : base()
        {
            
        }

        public CADElementBaseboard(int Count, Vector2 Location)
            : this ()
        {
            if (Count < 3 | Count > 4)
                throw new ArgumentOutOfRangeException("Count", "Count must be either 3 or 4");

            SetupPosition(Location - (Vector2.UnitX * 20) - (Vector2.UnitY * 20));
            SetupPosition(Location - (Vector2.UnitX * 20) + (Vector2.UnitY * 20));
            SetupPosition(Location + (Vector2.UnitX * 20) + (Vector2.UnitY * 20));
            if (Count == 4)
            {
                SetupPosition(Location + (Vector2.UnitX * 20) - (Vector2.UnitY * 20));
            }
        }


        private void SetupPosition(Position pos)
        {
            Positions.Add(pos);
            pos.PositionConstrainDrag += new Position.PositionDragEventHandler(PositionConstrainDrag);
            pos.PositionSnapDrag += new Position.PositionDragEventHandler(PositionSnapDrag);
        }

        private void SetupPosition( Vector2 pos)
        {            
            SetupPosition(new Position(pos));
        }

        public CADElementBaseboard(Position V1, Position V2, Position V3, Position V4)
        {
            SetupPosition(V1);
            SetupPosition(V2);
            SetupPosition(V3);
            SetupPosition(V4);            
       }

        public CADElementBaseboard(Position V1, Position V2, Position V3)
            : this()
        {
            SetupPosition(V1);
            SetupPosition(V2);
            SetupPosition(V3);
        }

        protected override void PositionSnapDrag(object sender, Events.PositionDragEventArgs e)
        {
            double x = (int)(e.ActualPosition.x / 10) * 10;
            double y = (int)(e.ActualPosition.y / 10) * 10;
            e.RequestedPosition = new Vector2(x, y);
            e.Handled = true;
        }

        protected override void PositionConstrainDrag(object sender, Events.PositionDragEventArgs e)
        {
            
        }


    }
}
