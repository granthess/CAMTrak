// ConeShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

namespace QuickStart.Physics
{
    public class ConeShapeDesc : ShapeDesc
    {
        private float radius;
        private float height;

        /// <summary>
        /// Radius of the cylinder.
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        /// <summary>
        /// Height of the cylinder
        /// </summary>
        public float Height
        {
            get { return this.height; }
            set { this.height = value; }
        }
    }
}
