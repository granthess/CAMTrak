// StaticModel.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.GeometricPrimitives;

namespace QuickStart.Graphics
{
    public struct VertexStaticModel : IVertexType
    {
        public Vector3 Position;
        public Vector2 Texture;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexStaticModel(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 textureCoords)
        {
            Position = position;
            Normal = normal;
            Tangent = tangent;
            Binormal = binormal;
            Texture = textureCoords;
        }
        
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexStaticModel.VertexDeclaration; }
        }
    }

    /// <summary>
    /// A model container that is optimized for static geometry.  No animation data is imported and all geometry is collapsed into single vertex and index buffers.
    /// </summary>
    public class StaticModel
    {
        // @todo: The interface to this class needs to be reworked to make it easy to use through the new render chunk system.

        /// <summary>
        /// Retrieves the number of vertices in the model.
        /// </summary>
        public int VertexCount
        {
            get { return numVertices; }
        }

        /// <summary>
        /// Retrieves the number of primitives (triangles) in the model.
        /// </summary>
        public int PrimitiveCount
        {
            get { return numPrimitives; }
        }

        /// <summary>
        /// Retrieves the type of geometry composing the model.
        /// </summary>
        public PrimitiveType GeometryType
        {
            get { return PrimitiveType.TriangleList; }
        }

        /// <summary>
        /// Retrieves the <see cref="VertexBuffer"/> for the model.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return this.vertexBuffer; }
        }

        /// <summary>
        /// Retrieves the <see cref="IndexBuffer"/> for the model.
        /// </summary>
        public IndexBuffer IndexBuffer
        {
            get { return this.indexBuffer; }
        }

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int numPrimitives;
        private int numVertices;


        /// <summary>
        /// Creates a new StaticModel instance from a content pipeline data file.
        /// </summary>
        /// <param name="reader">The content pipeline data file reader.</param>
        internal StaticModel(ContentReader reader)
        {
            vertexBuffer = reader.ReadObject<VertexBuffer>();
            indexBuffer = reader.ReadObject<IndexBuffer>();

            numPrimitives = indexBuffer.IndexCount  / 3;

            numVertices = vertexBuffer.VertexCount;
        }

        public StaticModel(GeometricPrimitive primitive, GraphicsDevice device)
        {
            // The vertex buffer that a GeometricPrimitive uses is not suitable for this engine, so we will
            // convert it to one that is.
            Vector2[] texCoords = 
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1)
            };

            int texCoordIndex = 0;

            List<VertexStaticModel> newVerts = new List<VertexStaticModel>();
            foreach (VertexPositionNormal vertex in primitive.Vertices)
            {
                VertexStaticModel newVert;
                ConvertVertexPositionNormalToStaticModelVertex(vertex, texCoords[texCoordIndex], out newVert);
                newVerts.Add(newVert);

                ++texCoordIndex;
                if (texCoordIndex > (texCoords.Length - 1))
                {
                    texCoordIndex = 0;
                }
            }

            vertexBuffer = new VertexBuffer(device, typeof(VertexStaticModel), newVerts.Count, BufferUsage.None);
            vertexBuffer.SetData(newVerts.ToArray());
            indexBuffer = primitive.IndexBuffer;
            numPrimitives = primitive.Indices.Count / 3;
            numVertices = primitive.Vertices.Count;
        }

        /// <summary>
        /// Binds the vertex buffer, index buffer, and vertex declaration to the current graphics device.
        /// </summary>
        /// <param name="stream">The vertex stream index for the data.</param>
        public void BindBuffers(int stream)
        {
            vertexBuffer.GraphicsDevice.SetVertexBuffer(vertexBuffer);                            
            indexBuffer.GraphicsDevice.Indices = indexBuffer;            
        }

        /// <summary>
        /// Renders the associated geometry in one call.  Equivalent to calling BindBuffers(0) followed by DrawIndexedPrimitive().
        /// </summary>
        public void DrawGeometry()
        {
            BindBuffers(0);

            vertexBuffer.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitives);
        }

        public List<Vector3> GetModelVertices(float Scale)
        {
            List<Vector3> verts = new List<Vector3>();

            VertexStaticModel[] vertices = new VertexStaticModel[numVertices];
            vertexBuffer.GetData(vertices);
            foreach (VertexStaticModel vertex in vertices)
            {
                verts.Add(vertex.Position * Scale);
            }

            return verts;
        }

        public List<int> GetModelIndices()
        {
            if (indexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
            {
                throw new Exception("Model uses 32-bit indices, which are not supported.");
            }

            int TriVertsCount = numPrimitives * 3;
            short[] s = new short[TriVertsCount];
            indexBuffer.GetData<short>(0, s, 0, TriVertsCount);
            List<int> indices = new List<int>();
            for (int i = 0; i != numPrimitives; ++i)
            {
                indices.Add(s[i * 3 + 0]);
                indices.Add(s[i * 3 + 1]);
                indices.Add(s[i * 3 + 2]);
            }

            return indices;
        }

        public void ConvertVertexPositionNormalToStaticModelVertex(VertexPositionNormal vertex, Vector2 texCoords, out VertexStaticModel outVertex)
        {
            // @NOTE: Since we don't have UVs we don't have a relative up, so all this does is give
            // us two vectors orthagonal to the vertex.Normal

            Vector3 referenceVector = Vector3.UnitY;
            float refDot = Vector3.Dot(referenceVector, vertex.Normal);
            if ( refDot == 1.0f || refDot == -1.0f )            
            {
                referenceVector = Vector3.UnitZ;
            }
            Vector3 tangent = Vector3.Cross(vertex.Normal, referenceVector);
            Vector3 binormal = Vector3.Cross(vertex.Normal, -tangent);

            outVertex = new VertexStaticModel(vertex.Position, vertex.Normal, tangent, binormal, texCoords);
        }
    }
}
