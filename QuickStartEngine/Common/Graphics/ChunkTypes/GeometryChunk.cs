// GeometryChunk.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Definition of a renderable chunk of geometry handled by the graphics system.
    /// </summary>
    public class GeometryChunk
    {
        /// <summary>
        /// The <see cref="VertexBuffer"/>s used for the geometry chunk.
        /// </summary>
        public List<VertexBuffer> VertexStreams = new List<VertexBuffer>(1);

        /// <summary>
        /// The <see cref="IndexBuffer"/> used for the geometry chunk.
        /// </summary>
        public IndexBuffer Indices;

        /// <summary>
        /// The starting index in the index buffer used for the geometry chunk.
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// The number of vertices in the geometry chunk.
        /// </summary>
        public int VertexCount;

        /// <summary>
        /// The number of primitives in the geometry chunk.
        /// </summary>
        public int PrimitiveCount;

        /// <summary>
        /// The offset into the vertex stream for the geometry chunk.
        /// </summary>
        public int VertexStreamOffset;

        /// <summary>
        /// The type of primitive defined in the geometry chunk.
        /// </summary>
        public PrimitiveType Type = PrimitiveType.TriangleList;

        /// <summary>
        /// The world transformation matrix of the geometry chunk.
        /// </summary>
        public Matrix WorldTransform;

        /// <summary>
        /// The material assigned to the geometry chunk.
        /// </summary>
        public Material Material;

        /// <summary>
        /// Name of the render technique to be used. We store this hear rather than in the Material
        /// because many pieces of geometry may be sharing a material but could require a different
        /// technique, and we don't want them each stomping on each others' technique names.
        /// </summary>
        public String RenderTechniqueName;

        /// <summary>
        /// Whether or not this chunk can cast a shadow onto surfaces that are able to receive them.
        /// </summary>
        public bool CanCreateShadows = true;

        /// <summary>
        /// Whether or not this chunk can cast a shadow onto surfaces that are able to receive them.
        /// </summary>
        public bool CanReceiveShadows = true;

        /// <summary>
        /// This geometry's opacity
        /// </summary>
        public Color ModelColor = Color.White;

        /// <summary>
        /// The rendering order of the chunk.
        /// </summary>
        public PreferredRenderOrder RenderOrder = PreferredRenderOrder.RenderNormal;

        /// <summary>
        /// Recycles and prepares the <see cref="GeometryChunk"/> the reallocation.
        /// </summary>
        public void Recycle()
        {
            Indices = null;            
            VertexStreams.Clear();
            Material = null;
        }
    }
}
