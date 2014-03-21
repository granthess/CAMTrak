// HeightFieldShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using Microsoft.Xna.Framework;

namespace QuickStart.Physics
{
    /// <summary>
    /// Descriptor for height field physics shape.
    /// </summary>
    public class HeightFieldShapeDesc : ShapeDesc
    {
        private float[,] heightField;
        private float sizeX;
        private float sizeZ;

        /// <summary>
        /// Height array.
        /// </summary>
        public float[,] HeightField
        {
            get { return this.heightField; }
            set { this.heightField = value; }
        }

        /// <summary>
        /// Size of height field along the X axis, in world coordinates.
        /// </summary>
        public float SizeX
        {
            get { return this.sizeX; }
            set { this.sizeX = value; }
        }

        /// <summary>
        /// Size of height field along the Z axis, in world coordinates.
        /// </summary>
        public float SizeZ
        {
            get { return this.sizeZ; }
            set { this.sizeZ = value; }
        }
    }
}
