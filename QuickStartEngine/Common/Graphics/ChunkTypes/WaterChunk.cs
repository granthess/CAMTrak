// WaterChunk.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Components;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Data about water plane that is passed to the GraphicsSystem
    /// </summary>
    public class WaterChunk : IVertexType
    {
        /// <summary>
        /// The <see cref="VertexBuffer"/>s used for this water chunk.
        /// </summary>
        public VertexWater[] Vertices;

        /// <summary>
        /// The <see cref="VertexDeclaration"/> used for the water chunk.
        /// </summary>
        public VertexDeclaration VertexDeclaration {get; set;}

        /// <summary>
        /// The number of primitives in the water chunk.
        /// </summary>
        public int PrimitiveCount;

        /// <summary>
        /// Elevation of the water chunk.
        /// </summary>
        public float Elevation;

        /// <summary>
        /// The type of primitive defined in the water chunk.
        /// </summary>
        public PrimitiveType Type = PrimitiveType.LineList;

        /// <summary>
        /// The material assigned to the water chunk.
        /// </summary>
        public Material Material;

        /// <summary>
        /// How reflective this water is.
        /// </summary>
        public float Reflectivity = 1.0f;

        /// <summary>
        /// Color of shallowest water
        /// </summary>
        public Vector4 WaterColorLight = new Vector4(0.0f, 0.235f, 0.44f, 1.0f);

        /// <summary>
        /// Color of deep water
        /// </summary>
        public Vector4 WaterColorDark = new Vector4(0.0f, 0.126f, 0.367f, 1.0f);

        /// <summary>
        /// Recycles and prepares the <see cref="WaterChunk"/> the reallocation.
        /// </summary>
        public void Recycle()
        {
            Vertices = null;
            Material = null;
        }
    }
}
