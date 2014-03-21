using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Graphics;

namespace QuickStart.Entities.NavMesh
{
    public class NavMeshChunk
    {
        private NavMesh parent;
        private const int SizeInvalid = -1;

        private Dictionary<int, NavMeshNode> meshNodes;    // List of nodes that belong to this chunk.

        int Height
        {
            get 
            {
                if (height == SizeInvalid)
                {
                    CalculateHeight();
                }

                return height; 
            }
        }
        private int height = SizeInvalid;

        int Width
        {
            get 
            {
                if (height == SizeInvalid)
                {
                    CalculateWidth();
                }

                return width; 
            }
        }
        private int width = SizeInvalid;

        /// <summary>
        /// Bounding box that surrounds this <see cref="QuadTree"/> section.
        /// </summary>
        private BoundingBox boundingBox;

        private VertexPositionColor[] boundingBoxMesh;                
        private VertexBuffer boundingBoxVertBuffer;        
        static private IndexBuffer boundingBoxIndexBuffer;
        static private Material boundingBoxMaterial;
        static private VertexDeclaration lineVertexDeclaration;

        public bool BoundingBoxDrawnThisFrame
        {
            get { return boundingBoxDrawnThisFrame; }
            set { boundingBoxDrawnThisFrame = value; }
        }
        private bool boundingBoxDrawnThisFrame = false;

        private int minNodeValue = SizeInvalid;
        private int maxNodeValue = SizeInvalid;

        public NavMeshChunk(NavMesh parent)
        {
            this.parent = parent;

            meshNodes = new Dictionary<int, NavMeshNode>();

            boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);

            if (lineVertexDeclaration == null)
            {
                lineVertexDeclaration = new VertexDeclaration(null);
            }

            if (boundingBoxMaterial == null)
            {
                boundingBoxMaterial = this.parent.Game.Content.Load<Material>("Material/WireFrameRed");
            }
        }

        public void AddNode( NavMeshNode node, bool skipAABBUpdate )
        {
            bool boundingBoxUpdateNeeded = false;

            if (node.VertexTopLeft < minNodeValue || minNodeValue == SizeInvalid)
            {
                minNodeValue = node.VertexTopLeft;
                boundingBoxUpdateNeeded = true;
            }

            if (node.VertexTopLeft > maxNodeValue || maxNodeValue == SizeInvalid)
            {
                maxNodeValue = node.VertexTopLeft;
                boundingBoxUpdateNeeded = true;
            }

            if (!skipAABBUpdate && boundingBoxUpdateNeeded)
            {
                boundingBox.Min = this.parent.TerrainComp.VertexList[minNodeValue].Position;
                boundingBox.Min.Y = 0.0f;

                NavMeshNode maxNode;
                meshNodes.TryGetValue(maxNodeValue, out maxNode);

                if (maxNode != null)
                {
                    boundingBox.Max = this.parent.TerrainComp.VertexList[maxNode.VertexBottomRight].Position;
                    boundingBox.Max.Y = 1000.0f;
                }
            }

            meshNodes.Add(node.VertexTopLeft, node);
        }

        public void RemoveNode(NavMeshNode node)
        {
            if (meshNodes.Count == 0)
                return;

            meshNodes.Remove(node.VertexTopLeft);

            if (node.VertexTopLeft == minNodeValue || node.VertexTopLeft == maxNodeValue)
            {     
                // We need to find the new lowest and highest values before RemoveNode() will function properly
                //minNodeValue = meshNodes.Keys.Min();
                //maxNodeValue = meshNodes.Keys.Max();

                UpdateAABB();
            }
        }

        public void UpdateAABB()
        {
            boundingBox.Min = this.parent.TerrainComp.VertexList[minNodeValue].Position;
            boundingBox.Min.Y -= 10.0f;

            NavMeshNode maxNode;
            meshNodes.TryGetValue(maxNodeValue, out maxNode);

            if (maxNode != null)
            {
                boundingBox.Max = this.parent.TerrainComp.VertexList[maxNode.VertexBottomRight].Position;
                boundingBox.Max.Y += 10.0f;
            }

            CreateBoundingBoxMesh();
        }

        public void CalculateHeight()
        {
            if (this.meshNodes.Count == 0)
                return;

            NavMeshNode minNode;
            this.meshNodes.TryGetValue(this.minNodeValue, out minNode);
            int minsRow = minNode.ZCoordTopLeft;

            NavMeshNode maxNode;
            this.meshNodes.TryGetValue(this.maxNodeValue, out maxNode);
            int maxsRow = maxNode.ZCoordTopLeft;

            height = ((maxsRow - minsRow) + 1);
        }

        public void CalculateWidth()
        {
            if (this.meshNodes.Count == 0)
                return;

            NavMeshNode minNode;
            this.meshNodes.TryGetValue(this.minNodeValue, out minNode);
            int minsColumn = minNode.XCoordTopLeft;

            NavMeshNode maxNode;
            this.meshNodes.TryGetValue(this.maxNodeValue, out maxNode);
            int maxsColumn = maxNode.XCoordTopLeft;

            height = ((maxsColumn - minsColumn) + 1);
        }

        public int GetMinRow()
        {
            if (this.meshNodes.Count == 0)
                return 0;

            NavMeshNode minNode;
            this.meshNodes.TryGetValue(this.minNodeValue, out minNode);

            if (minNode == null)
                return 0;

            return minNode.ZCoordTopLeft;
        }

        public int GetMaxRow()
        {
            if (this.meshNodes.Count == 0)
                return 0;

            NavMeshNode maxNode;
            this.meshNodes.TryGetValue(this.maxNodeValue, out maxNode);

            if (maxNode == null)
                return 0;

            return maxNode.ZCoordTopLeft;
        }

        public int GetMinColumn()
        {
            if (this.meshNodes.Count == 0)
                return 0;

            NavMeshNode minNode;
            this.meshNodes.TryGetValue(this.minNodeValue, out minNode);

            if (minNode == null)
                return 0;

            return minNode.XCoordTopLeft;
        }

        public int GetMaxColumn()
        {
            if (this.meshNodes.Count == 0)
                return 0;

            NavMeshNode maxNode;
            this.meshNodes.TryGetValue(this.maxNodeValue, out maxNode);

            if (maxNode == null)
                return 0;

            return maxNode.XCoordTopLeft;
        }

        public void QueryForRenderChunks(ref RenderPassDesc desc)
        {
            boundingBoxDrawnThisFrame = true;

            if (!desc.RenderCamera.ViewFrustum.Intersects(boundingBox))
                return;

            // Draw bounding boxes
            GeometryChunk boxChunk = this.parent.Game.Graphics.AllocateGeometryChunk();
            boxChunk.Indices = boundingBoxIndexBuffer;
            boxChunk.VertexStreams.Add(this.boundingBoxVertBuffer);            
            boxChunk.WorldTransform = Matrix.Identity;
            boxChunk.VertexCount = this.boundingBoxMesh.Length;
            boxChunk.StartIndex = 0;
            boxChunk.VertexStreamOffset = 0;
            boxChunk.PrimitiveCount = 12;   // Number of trianges that are required to render a cube.
            boxChunk.Material = boundingBoxMaterial;
            boxChunk.RenderTechniqueName = boundingBoxMaterial.CurrentTechnique;
            boxChunk.Type = PrimitiveType.TriangleList;
        }

        /// <summary>
        /// Create a simple bounding box mesh so that we can show a QuadTree's <see cref="BoundingBox"/>
        /// </summary>
        private void CreateBoundingBoxMesh()
        {
            this.boundingBoxMesh = new VertexPositionColor[8];
            short[] indices = new short[36];

            Vector3[] corners = boundingBox.GetCorners();

            for (int i = 0; i < 8; ++i)
            {
                boundingBoxMesh[i].Position = corners[i];
            }

            this.boundingBoxVertBuffer = new VertexBuffer(this.parent.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, boundingBoxMesh.Length, BufferUsage.WriteOnly);
            this.boundingBoxVertBuffer.SetData(boundingBoxMesh);

            if (boundingBoxIndexBuffer == null)
            {
                // Front
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                indices[3] = 0;
                indices[4] = 2;
                indices[5] = 3;

                // Back
                indices[6] = 4;
                indices[7] = 5;
                indices[8] = 6;
                indices[9] = 4;
                indices[10] = 6;
                indices[11] = 7;

                // Top
                indices[12] = 4;
                indices[13] = 5;
                indices[14] = 1;
                indices[15] = 4;
                indices[16] = 1;
                indices[17] = 0;

                // Bottom
                indices[18] = 7;
                indices[19] = 6;
                indices[20] = 2;
                indices[21] = 7;
                indices[22] = 2;
                indices[23] = 3;

                // Left
                indices[24] = 0;
                indices[25] = 4;
                indices[26] = 7;
                indices[27] = 0;
                indices[28] = 7;
                indices[29] = 3;

                // Right
                indices[30] = 1;
                indices[31] = 5;
                indices[32] = 6;
                indices[33] = 1;
                indices[34] = 6;
                indices[35] = 2;

                boundingBoxIndexBuffer = new IndexBuffer(this.parent.Game.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
                boundingBoxIndexBuffer.SetData(indices);
            }
        }
    }
}
