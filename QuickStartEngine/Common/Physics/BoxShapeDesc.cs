// BoxShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using Microsoft.Xna.Framework;

namespace QuickStart.Physics
{
    /// <summary>
    /// Desciptor for box physics shape.
    /// </summary>
    public class BoxShapeDesc : ShapeDesc
    {
        private Vector3 extents;

        /// <summary>
        /// Side extents of the box, along all axes.  This should be the length of the sides.
        /// </summary>
        public Vector3 Extents
        {
            get { return this.extents; }
            set { this.extents = value; }
        }
    }
}
