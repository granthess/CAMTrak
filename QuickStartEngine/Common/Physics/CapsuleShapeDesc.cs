// CylinderShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

namespace QuickStart.Physics
{
    public class CapsuleShapeDesc : ShapeDesc
    {
        private float radius;
        private float height;

        /// <summary>
        /// Radius of the capsule.
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }

        /// <summary>
        /// Length of the capsule.
        /// </summary>
        public float Length
        {
            get { return this.height; }
            set { this.height = value; }
        }
    }
}
