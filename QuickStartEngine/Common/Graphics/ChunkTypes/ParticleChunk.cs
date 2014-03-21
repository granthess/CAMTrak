// ParticleChunk.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.ParticleSystem;
using QuickStart.Components;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Definition of a renderable chunk of geometry handled by the graphics system.
    /// </summary>
    public class ParticleChunk
    {
        /// <summary>
        /// The <see cref="VertexBuffer"/>s used for this particle chunk.
        /// </summary>
        public VertexBuffer vertices;

        /// <summary>
        /// The <see cref="IndexBuffer"/>s used for this particle chunk.
        /// </summary>
        public IndexBuffer indices;

        /// <summary>
        /// The starting vertex in the primitive list.
        /// </summary>
        public int StartVertexIndex;

        /// <summary>
        /// Number of vertices
        /// </summary>
        public int NumVerts;

        /// <summary>
        /// The starting index in the primitive list
        /// </summary>
        public int StartIndex;

        /// <summary>
        /// The number of primitives in the particle chunk.
        /// </summary>
        public int PrimitiveCount;

        /// <summary>
        /// The type of primitive defined in the particle chunk.
        /// </summary>
        public PrimitiveType Type = PrimitiveType.TriangleList;

        /// <summary>
        /// The material assigned to the particle chunk.
        /// </summary>
        public Material Material;

        /// <summary>
        /// Holds data about the effect settings for the particle system type this particle chunk derives from
        /// </summary>
        public ParticleEmitterComponentDefinition ParticleSettings;

        /// <summary>
        /// How much time has elapsed
        /// </summary>
        public float CurrentTime;

        /// <summary>
        /// Whether or not this chunk will be used in reflections on water planes
        /// </summary>
        public bool AllowsWaterReflection = true;

        /// <summary>
        /// Whether or not this chunk will be used in refractions through water planes
        /// </summary>
        public bool AllowsWaterRefraction = true;

        /// <summary>
        /// Recycles and prepares the <see cref="ParticleChunk"/> the reallocation.
        /// </summary>
        public void Recycle()
        {
            vertices = null;
            indices = null;
            StartVertexIndex = 0;
            NumVerts = 0;
            StartIndex = 0;
            PrimitiveCount = 0;
            Material = null;
            ParticleSettings = null;
        }
    }
}
