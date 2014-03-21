// SphereShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

namespace QuickStart.Physics
{
    /// <summary>
    /// Descriptor for sphere physics shape.
    /// </summary>
    public class SphereShapeDesc : ShapeDesc
    {
        private float radius;

        /// <summary>
        /// Radius of the sphere.
        /// </summary>
        public float Radius
        {
            get { return this.radius; }
            set { this.radius = value; }
        }
    }
}
