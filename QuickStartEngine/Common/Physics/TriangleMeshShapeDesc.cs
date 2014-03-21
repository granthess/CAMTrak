// TriangleMeshShapeDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace QuickStart.Physics
{
    /// <summary>
    /// Descriptor for triangle mesh physics shape.
    /// </summary>
    public class TriangleMeshShapeDesc : ShapeDesc
    {
        private List<Vector3> vertices;
        private Vector3[] normals;
        private List<int> indices;

        /// <summary>
        /// Position array.
        /// </summary>
        public List<Vector3> Vertices
        {
            get { return this.vertices; }
            set { this.vertices = value; }
        }

        /// <summary>
        /// Normal array.
        /// </summary>
        public Vector3[] Normals
        {
            get { return this.normals; }
            set { this.normals = value; }
        }

        /// <summary>
        /// Triangle index array.
        /// </summary>
        public List<int> Indices
        {
            get { return this.indices; }
            set { this.indices = value; }
        }
    }
}
