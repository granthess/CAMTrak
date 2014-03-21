//
// QuadTree.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities.Heightfield;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    enum TreeSection
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    public class QuadTree
    {
        /// <summary>
        /// <see cref="Terrain"/> that is <see cref="QuadTree"/> belongs to.
        /// </summary>
        private TerrainComponent terrain;

        /// <summary>
        /// Bounding box that surrounds this <see cref="QuadTree"/> section.
        /// </summary>
        private BoundingBox boundingBox;    // Holds bounding box used for culling

        /// <summary>
        /// Top left <see cref="QuadTree"/> child section of this <see cref="QuadTree"/>.
        /// </summary>
        private QuadTree topLeft;

        /// <summary>
        /// Top right <see cref="QuadTree"/> child section of this <see cref="QuadTree"/>.
        /// </summary>
        private QuadTree topRight;

        /// <summary>
        /// Bottom left <see cref="QuadTree"/> child section of this <see cref="QuadTree"/>.
        /// </summary>
        private QuadTree bottomLeft;

        /// <summary>
        /// Bottom right <see cref="QuadTree"/> child section of this <see cref="QuadTree"/>.
        /// </summary>
        private QuadTree bottomRight;

        /// <summary>
        /// Array of <see cref="QuadTree"/> sections
        /// </summary>        
        private QuadTree[] childQuadTrees;

        /// <summary>
        /// Terrain patch for this <see cref="QuadTree"/> section. A single terrain patch can hold multiple
        /// LODs.
        /// </summary>
        private TerrainPatch leafPatch;

        /// <summary>
        /// Whether or not thie is a leaf node in the <see cref="QuadTree"/>. A leaf node has no child nodes
        /// branched off from it.
        /// </summary>
        private bool isLeaf = false;

        /// <summary>
        /// The first corner is the corner formed by the x, and z coordinates of the first vertex in the <see cref="QuadTree"/>, and
        /// the y coordinate of the lowest elevation value for the <see cref="QuadTree"/>.
        /// </summary>
        private Vector2 firstCorner = Vector2.Zero;

        /// <summary>
        /// The last corner is the corner formed by the x, and z coordinates of the last vertex in the <see cref="QuadTree"/> and the
        /// y coordinate of the highest elevation value for the <see cref="QuadTree"/>.
        /// </summary>
        private Vector2 lastCorner = Vector2.Zero;

        /// <summary>
        /// The dimensions (in the x and z axes) of this <see cref="QuadTree"/>.
        /// </summary>
        public int Width
        {
            get { return width; }
        }
        private int width;

        /// <summary>
        /// This is the offset along the X-axis for the beginning of this QuadTree section from the origin of the Terrain.
        /// </summary>
        public int offsetX;

        /// <summary>
        /// This is the offset along the Z-axis for the beginning of this QuadTree section from the origin of the Terrain.
        /// </summary>
        public int offsetZ;

        /// <summary>
        /// Contains the vertex offset for the first vertex in this <see cref="QuadTree"/> section.
        /// </summary>
        public int vertexBufferOffset = 0;

        /// <summary>
        /// Contains the vertex offset for the last vertex in this <see cref="QuadTree"/> section.
        /// </summary>
        public int vertexBufferOffsetEnd = 0;

        /// <summary>
        /// Holds mesh vertices that represent the bounding box created by this <see cref="QuadTree"/> section.
        /// </summary>
        public VertexPositionColor[] BoundingBoxMesh
        {
            get { return boundingBoxMesh; }
        }
        private VertexPositionColor[] boundingBoxMesh;

        private VertexBuffer boundingBoxVertBuffer;

        /// <summary>
        /// Minimum height of this <see cref="QuadTree"/> section's bounding box.
        /// </summary>
        private float minHeight = 1000000.0f;

        /// <summary>
        /// Maximum height of this <see cref="QuadTree"/> section's bounding box.
        /// </summary>
        private float maxHeight = 0.0f;

        /// <summary>
        /// Width of the root <see cref="QuadTree"/>.
        /// </summary>
        private int rootWidth = 0;

        static private Material boundingBoxMaterial;        
        static private IndexBuffer boundingBoxIndexBuffer;

        private QSGame Game;

        /// <summary>
        /// Creates a root <see cref="QuadTree"/> node. This should only be called from the
        /// parent Terrain section.
        /// </summary>
        /// <param name="game">QSGame reference</param>
        /// <param name="terrain">Parent terrain section</param>
        /// <param name="verticesLength">Length of vertices in this QuadTree</param>
        public QuadTree(QSGame game, TerrainComponent terrain, int verticesLength)            
        {
            this.Game = game;

            this.terrain = terrain;

            if (boundingBoxMaterial == null)
            {
                boundingBoxMaterial = game.Content.Load<Material>("Material/WireFrame");
            }

            // This truncation requires all heightmap images to be
            // a power of two in height and width
            this.width = (int)Math.Sqrt(verticesLength);
            this.rootWidth = width;

            // Vertices are only used for setting up the dimensions of
            // the bounding box. The vertices used in rendering are
            // located in the terrain class.
            SetupBoundingBox();

            // If this tree is the smallest allowable size, set it as a leaf
            // so that it will not continue branching smaller.
            if (verticesLength <= this.terrain.MinLeafSize)
            {
                this.isLeaf = true;

                CreateBoundingBoxMesh();
            }

            if (this.isLeaf)
            {
                SetupTerrainVertices();

                this.leafPatch = new TerrainPatch(this.Game, this.terrain, this);
            }
            else
            {
                BranchOffRoot();
            }
        }

        /// <summary>
        /// Creates a child <see cref="QuadTree"/> section.
        /// </summary>
        /// <param name="SourceTerrain">Parent <see cref="Terrain"/> section</param>
        /// <param name="verticesLength"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public QuadTree(QSGame game, TerrainComponent sourceTerrain, int verticesLength, int offsetX, int offsetZ)
        {
            this.Game = game;

            this.terrain = sourceTerrain;

            if (boundingBoxMaterial == null)
            {
                boundingBoxMaterial = game.Content.Load<Material>("Material/Terrain");
            }

            this.offsetX = offsetX;
            this.offsetZ = offsetZ;

            // This truncation requires all heightmap images to be
            // a power of two in height and width
            this.width = ((int)Math.Sqrt(verticesLength) / 2) + 1;

            SetupBoundingBox();

            // If this tree is the smallest allowable size, set it as a leaf
            // so that it will not continue branching smaller.
            int widthMinusOne = this.width - 1;
            if ( (widthMinusOne * widthMinusOne) <= this.terrain.MinLeafSize)
            {
                this.isLeaf = true;

                CreateBoundingBoxMesh();
            }

            if (this.isLeaf)
            {
                SetupTerrainVertices();

                this.leafPatch = new TerrainPatch(this.Game, this.terrain, this);
            }
            else
            {
                BranchOff();
            }
        }

        /// <summary>
        /// Setup <see cref="BoundingBox"/> for this <see cref="QuadTree"/> section.
        /// </summary>
        private void SetupBoundingBox()
        {
            this.firstCorner = new Vector2(this.offsetX * this.terrain.ScaleFactor, this.offsetZ * this.terrain.ScaleFactor);

            int widthMinusOne = this.width - 1;
            this.lastCorner = new Vector2((widthMinusOne + this.offsetX) * this.terrain.ScaleFactor,
                                          (widthMinusOne + this.offsetZ) * this.terrain.ScaleFactor);

            // Determine heights for use with the bounding box
            for (int x = 0; x < this.width; ++x)
            {
                for (int z = 0; z < this.width; ++z)
                {
                    float heightDataAtXZ = this.terrain.heightData[x + this.offsetX, z + this.offsetZ];

                    if (heightDataAtXZ < this.minHeight)
                    {
                        this.minHeight = heightDataAtXZ;
                    }
                    else if (heightDataAtXZ > this.maxHeight)
                    {
                        this.maxHeight = heightDataAtXZ;
                    }
                }
            }

            boundingBox = new BoundingBox(new Vector3(this.firstCorner.X, this.minHeight, this.firstCorner.Y),
                                          new Vector3(this.lastCorner.X, this.maxHeight, this.lastCorner.Y));
        }

        /// <summary>
        /// This sets up the vertices for all of the triangles in this quad-tree section
        /// passes them to the main terrain component.
        /// </summary>
        private void SetupTerrainVertices()
        {
            int offset = this.terrain.VertexList.Count;

            // Texture the level
            for (int x = 0; x < width; ++x)
                for (int z = 0; z < width; ++z)
                {
                    VertexTerrain tempVert = new VertexTerrain();
                    int offsetXtotal = offsetX + x;
                    int offsetZtotal = offsetZ + z;
                    tempVert.Position = new Vector3(offsetXtotal * terrain.ScaleFactor,
                                                    terrain.heightData[offsetXtotal, offsetZtotal],
                                                    offsetZtotal * terrain.ScaleFactor);

                    tempVert.Normal = terrain.normals[offsetXtotal, offsetZtotal];

                    this.terrain.VertexList.Add(tempVert);
                }

            this.vertexBufferOffset = offset;
            this.vertexBufferOffsetEnd = terrain.VertexList.Count;
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

            this.boundingBoxVertBuffer = new VertexBuffer(this.Game.GraphicsDevice, VertexPositionColor.VertexDeclaration, boundingBoxMesh.Length, BufferUsage.WriteOnly);
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

                boundingBoxIndexBuffer = new IndexBuffer(this.Game.GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
                boundingBoxIndexBuffer.SetData(indices);
            }
        }

        /// <summary>
        /// Branch the root <see cref="QuadTree"/> node into four child QuadTree nodes.
        /// </summary>
        /// <remarks>Only called from the main root node</remarks>
        private void BranchOffRoot()
        {
            int widthSqr = width * width;
            int halfWidthMinusOne = (width / 2) - 1;

            this.topLeft = new QuadTree(Game, terrain, widthSqr, 0, 0);
            this.bottomLeft = new QuadTree(Game, terrain, widthSqr, 0, halfWidthMinusOne);
            this.topRight = new QuadTree(Game, terrain, widthSqr, halfWidthMinusOne, 0);
            this.bottomRight = new QuadTree(Game, terrain, widthSqr, halfWidthMinusOne, halfWidthMinusOne);

            this.childQuadTrees = new QuadTree[4];

            this.childQuadTrees[(int)TreeSection.TopLeft] = topLeft;
            this.childQuadTrees[(int)TreeSection.TopRight] = topRight;
            this.childQuadTrees[(int)TreeSection.BottomLeft] = bottomLeft;
            this.childQuadTrees[(int)TreeSection.BottomRight] = bottomRight;
        }

        /// <summary>
        /// Branch a child <see cref="QuadTree"/> node into four more children QuadTree nodes
        /// </summary>
        private void BranchOff()
        {
            int widthSqr = width * width;
            int halfOfWidthMinusOne = ((width - 1) / 2);
            int widthAdjustWithZ = halfOfWidthMinusOne + offsetZ;
            int widthAdjustWithX = halfOfWidthMinusOne + offsetX;

            this.topLeft = new QuadTree(Game, terrain, widthSqr, offsetX, offsetZ);
            this.bottomLeft = new QuadTree(Game, terrain, widthSqr, offsetX, widthAdjustWithZ);
            this.topRight = new QuadTree(Game, terrain, widthSqr, widthAdjustWithX, offsetZ);
            this.bottomRight = new QuadTree(Game, terrain, widthSqr, widthAdjustWithX, widthAdjustWithZ);

            this.childQuadTrees = new QuadTree[4];

            this.childQuadTrees[(int)TreeSection.TopLeft] = topLeft;
            this.childQuadTrees[(int)TreeSection.TopRight] = topRight;
            this.childQuadTrees[(int)TreeSection.BottomLeft] = bottomLeft;
            this.childQuadTrees[(int)TreeSection.BottomRight] = bottomRight;
        }

        static public float GetDistanceBetweenCameraNearPlaneAndBoundingBox( BoundingFrustum frustum, BoundingBox boundingBox )
        {
            Vector3[] corners = new Vector3[8];
            boundingBox.GetCorners(corners);

            Vector3 nearPoint = frustum.GetCorners()[0];

            float distanceSqr = float.MaxValue;

            for (int i = 0; i < corners.Length - 1; ++i)
            {
                Vector3 diffVect = corners[i] - nearPoint;
                float sqrDiffDist = diffVect.LengthSquared();

                if (sqrDiffDist < distanceSqr)
                {
                    distanceSqr = sqrDiffDist;
                }
            }

            // The distances between the points of a near plane aren't large enough that it will be noticeable, so we
            // don't bother calculating the center of the near plane, we just use a corner.


            return (float)Math.Sqrt(distanceSqr);
        }

        public void QueryForRenderChunks( BoundingFrustum bFrustum, ref Plane clippingPlane, LOD DetailLevel, ref RenderPassDesc desc )
        {
            // If a clipping plane exists, check which side the box is on
            if (null != clippingPlane)
            {
                PlaneIntersectionType intersection = clippingPlane.Intersects(this.boundingBox);

                // If all geometry is on the back side of the plane, then no need to render it
                if (intersection == PlaneIntersectionType.Back)
                {
                    return;
                }
            }

            // Check if QuadTree bounding box intersection the current view frustum
            if (!bFrustum.Intersects(this.boundingBox))
            {
                return;
            }
            
            // Only draw leaves on the tree, never the main tree branches themselves.
            if(isLeaf)
            {
                GeometryChunk chunk = this.Game.Graphics.AllocateGeometryChunk();
                chunk.Indices = this.leafPatch.indexBuffers[(int)DetailLevel];
                chunk.VertexStreams.Add(terrain.VertexBuffer);                
                chunk.WorldTransform = Matrix.Identity;
                chunk.VertexCount = this.width * this.width;
                chunk.StartIndex = 0;
                chunk.VertexStreamOffset = this.vertexBufferOffset;
                chunk.PrimitiveCount = this.leafPatch.numTris[(int)DetailLevel];
                chunk.Material = this.terrain.Material;

                if (this.Game.Settings.GraphicsLevel > GraphicsLevel.Low && DetailLevel == LOD.High)
                {
                    float distanceToBoundingBox = GetDistanceBetweenCameraNearPlaneAndBoundingBox(bFrustum, this.boundingBox);

                    // If the entire terrain patch is far enough from the camera then we lower the shader quality.
                    if (distanceToBoundingBox < QSConstants.MinQuadTreeCubeCenterDistance / 2 )
                    {
                        chunk.RenderTechniqueName = "MultiTexturedNormaled";
                    }
                    else
                    {
                        chunk.RenderTechniqueName = "MultiTextured";
                    }
                }
                else
                {
                    chunk.RenderTechniqueName = "MultiTextured";
                }

                chunk.CanCreateShadows = false;  // Terrain does not cast shadows, it would be very expensive
                chunk.CanReceiveShadows = true;
                chunk.Type = PrimitiveType.TriangleList;

                if (this.terrain.DisplayBoundingBoxes)
                {
                    // Draw bounding boxes
                    GeometryChunk boxChunk = this.Game.Graphics.AllocateGeometryChunk();
                    boxChunk.Indices = boundingBoxIndexBuffer;
                    boxChunk.VertexStreams.Add(this.boundingBoxVertBuffer);                    
                    boxChunk.WorldTransform = Matrix.Identity;
                    boxChunk.VertexCount = this.BoundingBoxMesh.Length;
                    boxChunk.StartIndex = 0;
                    boxChunk.VertexStreamOffset = 0;
                    boxChunk.PrimitiveCount = 12;   // Number of trianges that are required to render a cube.
                    boxChunk.Material = boundingBoxMaterial;
                    boxChunk.RenderTechniqueName = boundingBoxMaterial.CurrentTechnique;
                    boxChunk.CanCreateShadows = false;
                    boxChunk.CanReceiveShadows = true;
                    boxChunk.Type = PrimitiveType.TriangleList;
                }
            }
            // If there are branches on this node, move down through them recursively
            else if(childQuadTrees.Length > 0)
            {
                for(int i = 0; i < childQuadTrees.Length; ++i)
                {
                    childQuadTrees[i].QueryForRenderChunks(bFrustum, ref clippingPlane, DetailLevel, ref desc);
                }
            }
        }

        /// <summary>
        /// This recursive function makes sure that every array of index buffers in each
        /// <see cref="QuadTree"/> node have proper pointers. This allows for the user to call any
        /// <see cref="LOD"/> draw on the terrain and the <see cref="Terrain"/> should draw that LOD or the next highest
        /// LOD possible available.
        /// </summary>
        public void SetupLODs()
        {
            // Only setup LODs for leaves on the tree, never the main tree branches themselves.
            if (this.isLeaf)
            {
                int highestLODUsed = 0;

                // Note: We compare with 'i > 0', rather than 'i >= 0', because index 0 is unused.
                for (int i = (int)LOD.NumOfLODs - 1; i > 0; --i)
                {
                    if (this.leafPatch.indexBuffers[i] != null)
                    {
                        highestLODUsed = i;
                    }
                }

                // We start at 1 because that's the lowest LOD value
                for (int i = 1; i < (int)LOD.NumOfLODs; ++i)
                {
                    if (i < highestLODUsed)
                    {
                        this.leafPatch.indexBuffers[i] = leafPatch.indexBuffers[highestLODUsed];
                        this.leafPatch.numTris[i] = leafPatch.numTris[highestLODUsed];
                    }
                    else if (i > highestLODUsed)
                    {
                        if (this.leafPatch.indexBuffers[i] == null)
                        {
                            int iMinusOne = i - 1;
                            this.leafPatch.indexBuffers[i] = leafPatch.indexBuffers[iMinusOne];
                            this.leafPatch.numTris[i] = leafPatch.numTris[iMinusOne];
                        }
                    }
                }
            }
            // If there are branches on this node, move down through them recursively
            else if (childQuadTrees.Length > 0)
            {
                for (int i = 0; i < childQuadTrees.Length; ++i)
                {
                    childQuadTrees[i].SetupLODs();
                }
            }
        }

        /// <summary>
        /// Adds a new LOD to the <see cref="TerrainPatch"/> owned by this <see cref="QuadTree"/>.
        /// </summary>
        /// <param name="detailLevel">LOD to add to the TerrainPatch owned by this QuadTree.</param>
        public void AddNewPatchLOD(LOD detailLevel)
        {
            // Only setup LODs for leaves on the tree, never the main tree branches themselves.
            if (isLeaf)
            {
                this.leafPatch.SetupTerrainIndices(this.width, detailLevel);

                SetupLODs();        // Update LOD array to account for new LOD
            }

            // If there are branches on this node, move down through them recursively
            else if (childQuadTrees.Length > 0)
            {
                for (int i = 0; i < childQuadTrees.Length; ++i)
                {
                    this.childQuadTrees[i].AddNewPatchLOD(detailLevel);
                }
            }

        }
    }
}

