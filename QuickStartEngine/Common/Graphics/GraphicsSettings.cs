//
// GraphicsSettings.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework.Graphics;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Defines configurable settings applied to a graphics device.
    /// </summary>
    public struct GraphicsSettings
    {
        /// <summary>
        /// The width of the back buffer, in pixels.
        /// </summary>
        public int BackBufferWidth;

        /// <summary>
        /// The height of the back buffer, in pixels.
        /// </summary>
        public int BackBufferHeight;

        /// <summary>
        /// Flag enabling/disabling synchronization with vertical retrace (vsync).
        /// </summary>
        public bool EnableVSync;

        /// <summary>
        /// Flag enabling/disabling multi-sample anti-aliasing.  If true, the highest quality MSAA setting will be used.
        /// </summary>
        public bool EnableMSAA;

        /// <summary>
        /// Flag enabling/disabling full-screen rendering.  Ignored on Xbox 360 platform.
        /// </summary>
        public bool EnableFullScreen;

        /// <summary>
        /// Flag enabling/disabling shadow mapping.
        /// </summary>
        public bool EnableShadowMapping;

        /// <summary>
        /// Level of detail for all rendering
        /// </summary>
        public GraphicsLevel GraphicsLevel;

        /// <summary>
        /// Stores whether or not the engine is using 32-bit index buffers or not, based on the capabilities
        /// of the user's video card or if the games default graphics profile is turned down.
        /// </summary>
        public bool Uses32BitIndices;

        /// <summary>
        /// Stores whether or not the engine is able to use Shader Model 3 or not, based on the capabilities
        /// of the user's video card or if the games default graphics profile is turned down.
        /// </summary>
        public bool SupportsSM3;

        /// <summary>
        /// Stores whether or not the engine is able to use The RGBA64 texture format or not, based on the 
        /// capabilities of the user's video card or if the games default graphics profile is turned down.
        /// </summary>
        public bool SupportsRGBA64TextureFormat;

        /// <summary>
        /// Stores the maximum texture size supported by the graphics profile.
        /// </summary>
        public int MaxTextureSize;

        /// <summary>
        /// Stores whether or not the graphics profile properly supports non-power-of-two textures.
        /// </summary>
        public bool SupportsNonPowerOfTwoTextures;

        /// <summary>
        /// Whether or not water reflection rendering is supported by the current Graphics Level setting.
        /// </summary>
        public bool EnableWaterReflections;

        /// <summary>
        /// Whether or not water reflects and refracts only the sky, based on the current Graphics Level setting.
        /// </summary>
        public bool EnableWaterUsesSkyOnly;

        /// <summary>
        /// Whether or not water refraction rendering is supported by the current Graphics Level setting.
        /// </summary>
        public bool EnableWaterRefractions;

        /// <summary>
        /// Whether or not water shoreline rendering is supported by the current Graphics Level setting.
        /// </summary>
        public bool EnableWaterShoreline;

        /// <summary>
        /// Whether or not particles will render in reflections or refractions, based on the current Graphics Level setting.
        /// </summary>
        public bool EnableParticleRenderWithWater;

        public LOD DesiredLOD;

        public void ProcessSettings(GraphicsDevice device)
        {
            // If the graphics profile for the engine is set to HiDef but that isn't supported by this user
            // then switch to the 'Reach' profile.
            if (device.GraphicsProfile == GraphicsProfile.HiDef)
            {
                if (!device.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
                {
                    //this.graphics.GraphicsProfile = GraphicsProfile.Reach;

                    // Currently the engine does not support scaling back to DX9 cards if you have setup
                    // your game to run in the 'HiDef' graphics profile. This support may be coming soon.
                    // Don't blame me, blame XNA, this engine supported all kinds of hardware just fine up until
                    // I had to upgrade to XNA 4.0 where they made artificial restrictions just to try and
                    // unify hardware requirements.
                    throw new Exception("Sorry, this game requires a DirectX 10 or higher video card to run.");
                }
            }

            bool usesHiDef = (device.GraphicsProfile == GraphicsProfile.HiDef);

            // These two will always be the same, we just keep two around for readability purposes
            this.Uses32BitIndices = usesHiDef;
            this.SupportsSM3 = usesHiDef;
            this.SupportsRGBA64TextureFormat = usesHiDef;            
            this.SupportsNonPowerOfTwoTextures = usesHiDef;
            //QSConstants.MinQuadTreeWidth = (usesHiDef ? 512 : 128);
            //QSConstants.MinQuadTreeCubeCenterDistance = (float)Math.Sqrt(QSConstants.MinQuadTreeWidth * QSConstants.MinQuadTreeWidth * 3);
            this.MaxTextureSize = (usesHiDef ? 4096 : 2048);

            // If shadow mapping is enabled but not properly supported by this graphics profile, disable it.
            if (this.EnableShadowMapping && !usesHiDef)
            {
                this.EnableShadowMapping = false;
            }

            this.EnableWaterReflections = true;
            this.EnableWaterUsesSkyOnly = false;
            this.EnableWaterRefractions = true;
            this.EnableWaterShoreline = true;
            this.EnableParticleRenderWithWater = true;

            switch (this.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                    this.DesiredLOD = LOD.High;
                    break;
                case GraphicsLevel.High:
                    this.DesiredLOD = LOD.Med;
                    break;
                case GraphicsLevel.Med:
                    this.DesiredLOD = LOD.Low;
                    this.EnableWaterRefractions = false;
                    this.EnableWaterShoreline = false;
                    this.EnableParticleRenderWithWater = false;
                    break;
                case GraphicsLevel.Low:
                    this.DesiredLOD = LOD.Low;
                    this.EnableWaterUsesSkyOnly = true;
                    this.EnableWaterRefractions = false;
                    this.EnableWaterShoreline = false;
                    this.EnableParticleRenderWithWater = false;
                    break;
            }
        }
    }
}
