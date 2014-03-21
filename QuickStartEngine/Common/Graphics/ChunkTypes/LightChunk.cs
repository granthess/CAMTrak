// LightChunk.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

using Microsoft.Xna.Framework;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Definition of a chunk of light data from a single light source we can use for this frame.
    /// </summary>
    public class LightChunk
    {
        /// <summary>
        /// Position of the light, if it is directional then Vector3.Zero will suffice.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Direction of the light
        /// </summary>
        public Vector3 direction;
        
        /// <summary>
        /// Whether or not this light is directional
        /// </summary>
        public bool directional;

        public Vector4 ambientColor;

        public Vector4 diffuseColor;

        public Vector4 specularColor;

        public float specularPower;

        /// <summary>
        /// Recycles and prepares the <see cref="LightChunk"/> the reallocation.
        /// </summary>
        public void Recycle()
        {

        }
    }
}
