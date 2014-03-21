using System;

using Microsoft.Xna.Framework;

namespace QuickStart.Entities.NavMesh
{
    public class NavMeshNode
    {
        const int NodeVertexInvalid = -1;

        // Top/bottom and left/right are assuming 0,0 is the top left. The x coordinate
        //   increases as we go to the right, the z coordinate increases as we go down.
        // Example:
        //
        // 0,0   1,0   2,0   3,0
        //
        // 0,1   1,1   2,1   3,1
        //
        // 0,2,  1,2   2,2   3,2
        //
        public int VertexTopLeft
        {
            get { return vertexTopLeft; }
        }
        int vertexTopLeft = NodeVertexInvalid;

        public int VertexTopRight
        {
            get { return vertexTopRight; }
        }
        int vertexTopRight = NodeVertexInvalid;

        public int VertexBottomLeft
        {
            get { return vertexBottomLeft; }
        }
        int vertexBottomLeft = NodeVertexInvalid;

        public int VertexBottomRight
        {
            get { return vertexBottomRight; }
        }
        int vertexBottomRight = NodeVertexInvalid;

        public int XCoordTopLeft
        {
            get { return vertexTopLeft % width; }
        }

        public int ZCoordTopLeft
        {
            get { return vertexTopLeft / width; }
        }

        public int XCoordTopRight
        {
            get { return vertexTopRight % width; }
        }

        public int ZCoordTopRight
        {
            get { return vertexTopRight / width; }
        }

        public int XCoordBottomLeft
        {
            get { return vertexBottomLeft % width; }
        }

        public int ZCoordBottomLeft
        {
            get { return vertexBottomLeft / width; }
        }

        public int XCoordBottomRight
        {
            get { return vertexBottomRight % width; }
        }

        public int ZCoordBottomRight
        {
            get { return vertexBottomRight / width; }
        }

        private int width = 0;

        public Vector3 Normal
        {
            set
            {
                this.normal = value;

                // Normal has changed, so re-calculate steepness
                this.steepness = Vector3.Dot(this.normal, Vector3.UnitY);

                tooSteep = ( (float)Math.Acos(this.steepness) > QSConstants.SlopeSteepnessThreshold );
            }
        }
        private Vector3 normal = Vector3.Zero;

        /// <summary>
        /// How steep this node is. 1.0 is a sheer cliff (straight up/down). 0.0f is flat.
        /// </summary>
        public float Steepness
        {
            get { return steepness; }
        }
        float steepness = 0.0f;

        public bool TooSteep
        {
            get { return tooSteep; }
        }
        bool tooSteep = false;

        public NavMeshNode()
        {

        }

        public NavMeshNode(int topLeftVertex, int width)
        {
            InitializeNode(topLeftVertex, width);
        }

        public void InitializeNode(int topLeftVertex, int width)
        {
            this.width = width;

            vertexTopLeft = topLeftVertex;
            vertexTopRight = vertexTopLeft + 1;
            vertexBottomLeft = vertexTopLeft + width;
            vertexBottomRight = vertexBottomLeft + 1;
        }
    }
}
