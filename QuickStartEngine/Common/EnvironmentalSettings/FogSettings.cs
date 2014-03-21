//
// FogSettings.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using Microsoft.Xna.Framework;

namespace QuickStart.EnvironmentalSettings
{
    public class FogSettings
    {
        /// <summary>
        /// Fog color for this scene
        /// </summary>
        public Vector4 FogColor = new Vector4(0.8f, 0.8f, 0.8f, 1.0f);

        /// <summary>
        /// Distance from the camera at which fog starts
        /// </summary>
        public float FogNear = 50.0f;

        /// <summary>
        /// Distance from the camera at which fog stops getting thicker
        /// </summary>
        public float FogFar = 230.0f;

        /// <summary>
        /// Altitude (Y coordinate) at which fog settles (depth-based fog).
        /// </summary>
        public float FogAltitude = 10.0f;

        /// <summary>
        /// How thin the fog is (lower value is thicker)
        /// </summary>
        public float FogThinning = 10.0f;

        /// <summary>
        /// This is set to true if this scene's fog has changed since the last time it was accessed by the renderer.
        /// </summary>
        public bool SceneFogChanged = true;
    }
}
