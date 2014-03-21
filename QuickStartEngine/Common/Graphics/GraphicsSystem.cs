// GraphicsSystem.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart;
using QuickStart.Components;
using QuickStart.GUI;
using QuickStart.Interfaces;
using QuickStart.Entities;
using QuickStart.Mathmatics;
using QuickStart.Physics;

namespace QuickStart.Graphics
{
    public enum PreferredRenderOrder
    {
        RenderFirst = 0,
        RenderNormal = 1,        
        RenderLast = 2,
        
        NumOfRenderOrders
    }

    /// <summary>
    /// Used for level-of-detail values throughout the <see cref="Terrain"/> and <see cref="TerrainPatch"/> system.
    /// </summary>
    public enum LOD
    {
        NumOfLODs = 9,
        Minimum = 8,
        Low = 4,
        Med = 2,
        High = 1
    }

    /// <summary>
    /// The QuickStart graphics system.  Manages all rendering-related tasks, including device configuration.
    /// </summary>
    public class GraphicsSystem : QSInterface
    {
        /// <summary>
        /// Enumeration of all queriable variable matrix types.
        /// </summary>
        public enum VariableMatrixID
        {
            World = 0,
            View,
            LightView,
            LightProjection,
            Projection,
            WorldView,
            ReflectionView,
            ViewProjection,
            WorldViewProjection,
            LightViewProjection,         
            WindDirection,

            Identity,
        }

        /// <summary>
        /// Enumeration of all queriable variable float types.
        /// </summary>
        public enum VariableFloatID
        {
            TotalTime = 0,
            SpecularPower,
            Opacity,            
            Duration,
            DurationRandomness,
            EndVelocity,
            LightBrightness,
            Reflectivity,
            MinTerrainHeight,
            MaxTerrainHeight,
            TerrainHeightRange,
            TerrainScale,
            TerrainWidth,
            TerrainElevationModifier,
            WaterElevation,
            FogNear,
            FogFar,
            FogDistanceRange,
            FogThinning,
            FogAltitude,

            Unity,
        }

        /// <summary>
        /// Enumeration of all queriable variable float2 types.
        /// </summary>
        public enum VariableFloat2ID
        {
            RotateSpeed = 0,
            StartSize,
            EndSize,
            ViewportScale,

            Unity,
        }

        /// <summary>
        /// Enumeration of all queriable variable float3 types.
        /// </summary>
        public enum VariableFloat3ID
        {
            Gravity = 0,            

            Unity,
        }

        /// <summary>
        /// Enumeration of all queriable variable float4 types.
        /// </summary>
        public enum VariableFloat4ID
        {
            ViewPos = 0,
            ViewForward,
            LightDir,
            ModelColor,
            AmbientColor,
            DiffuseColor,
            SpecularColor,            
            MinColor,
            MaxColor,
            FogColor,
            WaterColorLight,
            WaterColorDark,
            ClippingPlane,

            Unity,
        }

        /// <summary>
        /// Enumeration of all queriable variable texture2D types.
        /// </summary>
        public enum VariableTexture2D
        {
            ShadowMap = 0,
            ReflectionMap,
            RefractionMap,
            CubeMap,
            HeightMap,
            NormalMap,

            Unity,
        }

        /// <summary>
        /// Enumeration of all queriable variable bool types.
        /// </summary>
        public enum VariableBoolID
        {            
            UsingClippingPlane = 0,            
            ClippingAboveWater,
            CameraUnderwater,

            Unity,
        }

        public GraphicsDeviceManager graphics;

        public GraphicsSettings Settings
        {
            get { return this.settings; }
        }
        private GraphicsSettings settings;

        private Matrix[] variableMatrix;
        private float[] variableFloat;
        private Vector2[] variableFloat2;
        private Vector3[] variableFloat3;
        private Vector4[] variableFloat4;
        private Texture2D[] variableTexture2D;
        private bool[] variableBool;
        
        bool shadowMappingEnabled = false;
        RenderTarget2D shadowRenderTarget;        
        SpriteBatch spriteBatch;    // Used for drawing shadow map to screen

        private List<GeometryChunk> geometryChunkList;
        private List<GeometryChunk> deadGeometryChunkList;

        private List<LightChunk> lightChunkList;
        private List<LightChunk> deadLightChunkList;

        private List<ParticleChunk> particleChunkList;
        private List<ParticleChunk> deadParticleChunkList;

        private List<WaterChunk> waterChunkList;
        private List<WaterChunk> deadWaterChunkList;

        private List<IRenderChunkProvider> renderChunkProviders;

        private Texture2D CubeMapTexture;

        private RenderTarget2D RefractionMap;        
        private Vector3 RefractNormalDirection = Vector3.Down;   // Normal of water plane

        private RenderTarget2D ReflectionMap;
        private Vector3 ReflectNormalDirection = Vector3.Up;    // Normal of water plane reflection
        private Matrix ReflectionViewMatrix;

        private const int numChunksToPreallocate = 30;
        private const int numLightChunksToPreallocate = 3;
        private const int numParticleChunksToPreallocate = 3;
        private const int numWaterChunksToPreallocate = 1;

        public int TrianglesProcessedLastFrame
        {
            get { return trianglesProcessedLastFrame; }
        }
        private int trianglesProcessedLastFrame = 0;

        public int GeometryChunksRenderedLastFrame
        {
            get { return geometryChunksRenderedLastFrame; }            
        }
        private int geometryChunksRenderedLastFrame = 0;

        public int ParticleChunksRenderedLastFrame
        {
            get { return particleChunksRenderedLastFrame; }
        }
        private int particleChunksRenderedLastFrame = 0;

        public int ParticlesRenderedLastFrame
        {
            get { return particlesRenderedLastFrame; }
        }
        private int particlesRenderedLastFrame = 0;              

        /// <summary>
        /// Initializes a new instance of the graphics system.
        /// </summary>
        /// <param name="gameInstance">The instance of QSGame for the game.</param>
        public GraphicsSystem(QSGame gameInstance)
            : base(gameInstance, InterfaceType.Graphics)
        {
            this.game = gameInstance;

            this.game.GameMessage += this.Game_GameMessage;

            this.graphics = new GraphicsDeviceManager(game);
            this.graphics.PreparingDeviceSettings += PreparingDeviceSettings;            

            this.variableMatrix = new Matrix[(int)VariableMatrixID.Identity + 1];
            for(int i = 0; i < this.variableMatrix.Length; ++i)
            {
                this.variableMatrix[i] = Matrix.Identity;
            }

            this.variableFloat = new float[(int)VariableFloatID.Unity + 1];
            for(int i = 0; i < this.variableFloat.Length; ++i)
            {
                this.variableFloat[i] = 0.0f;
            }

            this.variableFloat2 = new Vector2[(int)VariableFloat2ID.Unity + 1];
            for (int i = 0; i < this.variableFloat2.Length; ++i)
            {
                this.variableFloat2[i] = Vector2.Zero;
            }

            this.variableFloat3 = new Vector3[(int)VariableFloat3ID.Unity + 1];
            for (int i = 0; i < this.variableFloat3.Length; ++i)
            {
                this.variableFloat3[i] = Vector3.Zero;
            }

            this.variableFloat4 = new Vector4[(int)VariableFloat4ID.Unity + 1];
            for(int i = 0; i < this.variableFloat4.Length; ++i)
            {
                this.variableFloat4[i] = Vector4.Zero;
            }

            this.variableTexture2D = new Texture2D[(int)VariableTexture2D.Unity + 1];
            for (int i = 0; i < this.variableTexture2D.Length; ++i)
            {
                this.variableTexture2D[i] = null;
            }

            this.variableBool = new bool[(int)VariableBoolID.Unity + 1];
            for (int i = 0; i < this.variableBool.Length; ++i)
            {
                this.variableBool[i] = false;
            }

            this.geometryChunkList = new List<GeometryChunk>(numChunksToPreallocate);
            this.deadGeometryChunkList = new List<GeometryChunk>();
            for(int i = 0; i < numChunksToPreallocate; ++i)
            {
                GeometryChunk chunk = new GeometryChunk();
                this.deadGeometryChunkList.Add(chunk);
            }

            this.lightChunkList = new List<LightChunk>(numLightChunksToPreallocate);
            this.deadLightChunkList = new List<LightChunk>();
            for (int i = 0; i < numLightChunksToPreallocate; ++i)
            {
                LightChunk chunk = new LightChunk();
                this.deadLightChunkList.Add(chunk);
            }

            this.particleChunkList = new List<ParticleChunk>(numParticleChunksToPreallocate);
            this.deadParticleChunkList = new List<ParticleChunk>();
            for (int i = 0; i < numParticleChunksToPreallocate; ++i)
            {
                ParticleChunk chunk = new ParticleChunk();
                this.deadParticleChunkList.Add(chunk);
            }

            this.waterChunkList = new List<WaterChunk>(numWaterChunksToPreallocate);
            this.deadWaterChunkList = new List<WaterChunk>();
            for (int i = 0; i < numWaterChunksToPreallocate; ++i)
            {
                WaterChunk chunk = new WaterChunk();
                this.deadWaterChunkList.Add(chunk);
            }

            this.renderChunkProviders = new List<IRenderChunkProvider>();
        }

        public override void Shutdown()
        {
            ClearChunks();
        }

        public override void Update( GameTime gameTime )
        {
            PhysicsRenderManager manager = this.game.PhysicsRenderer;
            if (null != manager)
            {
                if (manager.Enabled)
                {
                    manager.Update(gameTime);
                }
            }
        }

        /// <summary>
        /// Toggles between fullscreen and windowed mode
        /// </summary>
        public void ToggleFullscreen()
        {
            this.graphics.IsFullScreen = !this.graphics.IsFullScreen;
            this.graphics.ApplyChanges();
        }        

        /// <summary>
        /// Recreates the graphics device with the given settings.
        /// </summary>
        /// <param name="newSettings">The settings to use when recreating the device.</param>
        public void ApplySettings(GraphicsSettings newSettings)
        {
            this.settings = newSettings;
            this.settings.ProcessSettings(this.graphics.GraphicsDevice);

            this.shadowMappingEnabled = this.settings.EnableShadowMapping;

            this.graphics.PreferredBackBufferWidth = this.settings.BackBufferWidth;
            this.graphics.PreferredBackBufferHeight = this.settings.BackBufferHeight;

            this.graphics.PreferMultiSampling = this.settings.EnableMSAA;

            this.graphics.IsFullScreen = this.settings.EnableFullScreen;

            this.graphics.SynchronizeWithVerticalRetrace = this.settings.EnableVSync;            
            
            this.graphics.ApplyChanges();

            // Let the camera interface know about the graphics change(s).
            MsgGraphicsSettingsChanged msgChange = ObjectPool.Aquire<MsgGraphicsSettingsChanged>();            
            this.game.SendInterfaceMessage(msgChange, InterfaceType.Camera);
        }

        /// <summary>
        /// Applies changes based on the user settings to the device descriptor before device creation.
        /// </summary>
        /// <param name="obj">The calling GraphicsDeviceManager instance.</param>
        /// <param name="args">The device creation event arguments.</param>
        private void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {            
            args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        /// <summary>
        /// Loads all global content needed by the graphics system.
        /// </summary>
        /// <param name="reload">This is true is LoadContent</param>
        public void LoadContent(bool isReload)
        {
            this.settings.ProcessSettings(this.graphics.GraphicsDevice);

            spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);                     

            PresentationParameters pp = this.game.GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;           

            int RefractionRenderTargetSize = 256;
            int ReflectionRenderTargetSize = 512;
            int shadowMapWidthHeight = this.settings.MaxTextureSize;

            switch (this.game.Settings.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                    RefractionRenderTargetSize = 1024;
                    ReflectionRenderTargetSize = 1024;                    
                    break;
                case GraphicsLevel.High:
                    RefractionRenderTargetSize = 256;
                    ReflectionRenderTargetSize = 512;
                    shadowMapWidthHeight = 2048;                
                    break;
                case GraphicsLevel.Med:
                    RefractionRenderTargetSize = 256;
                    ReflectionRenderTargetSize = 256;
                    shadowMapWidthHeight = 1024;
                    break;
                case GraphicsLevel.Low:
                    ReflectionRenderTargetSize = 128;
                    shadowMapWidthHeight = 1024;
                    break;
            }

            int minimumDimension = Math.Min(this.game.Settings.Resolution.X, this.game.Settings.Resolution.Y);

            if (this.game.Settings.IsShadowMapping)
            {                
                SurfaceFormat shadowMapFormat = SurfaceFormat.Color;                

                // Check to see if the device supports a 32 or 16 bit 
                // floating point render target
                if (this.settings.SupportsRGBA64TextureFormat)
                {
                    shadowMapFormat = SurfaceFormat.Rgba64;
                }

                // Create new floating point render target
                shadowRenderTarget = new RenderTarget2D(this.graphics.GraphicsDevice, shadowMapWidthHeight, shadowMapWidthHeight, true, shadowMapFormat, DepthFormat.Depth24);
            }

            if (this.game.Settings.GraphicsLevel != GraphicsLevel.Low)
            {
                if (this.settings.SupportsNonPowerOfTwoTextures)
                {
                    RefractionRenderTargetSize = QSMath.Clamp(RefractionRenderTargetSize, 1, minimumDimension);
                }
                
                RefractionMap = new RenderTarget2D(this.graphics.GraphicsDevice, RefractionRenderTargetSize, RefractionRenderTargetSize, true, format, DepthFormat.Depth24);                
            }

            if (this.settings.SupportsNonPowerOfTwoTextures)
            {
                ReflectionRenderTargetSize = QSMath.Clamp(ReflectionRenderTargetSize, 1, minimumDimension);
            }

            ReflectionMap = new RenderTarget2D(this.graphics.GraphicsDevice, ReflectionRenderTargetSize, ReflectionRenderTargetSize, true, format, DepthFormat.Depth24);

            this.variableFloat[(int)VariableFloatID.WaterElevation] = -100000;
        }

        /// <summary>
        /// Unloads all previously loaded global content needed by the graphics system.
        /// </summary>
        public void UnloadContent()
        {
        }
        
        /// <summary>
        /// Retrieves the current value of the specified queriable variable texture2D.
        /// </summary>
        /// <param name="textureID">The enumerated ID of the variable texture2D to retrieve.</param>
        /// <param name="texture">The texture value.</param>
        public void GetVariableTexture2D(VariableTexture2D textureID, out Texture2D texture)
        {
            texture = this.variableTexture2D[(int)textureID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable matrix.
        /// </summary>
        /// <param name="matID">The enumerated ID of the variable matrix to retrieve.</param>
        /// <param name="mat">The matrix value.</param>
        public void GetVariableMatrix(VariableMatrixID matID, out Matrix mat)
        {
            mat = this.variableMatrix[(int)matID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable float.
        /// </summary>
        /// <param name="varID">The enumerated ID of the variable float to retrieve.</param>
        /// <param name="val">The float value.</param>
        public void GetVariableFloat(VariableFloatID varID, out float val)
        {
            val = this.variableFloat[(int)varID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable float2.
        /// </summary>
        /// <param name="varID">The enuemrated ID of the variable float2 to retrieve.</param>
        /// <param name="val">The float2 value.</param>
        public void GetVariableFloat2(VariableFloat2ID varID, out Vector2 val)
        {
            val = this.variableFloat2[(int)varID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable float3.
        /// </summary>
        /// <param name="varID">The enuemrated ID of the variable float3 to retrieve.</param>
        /// <param name="val">The float3 value.</param>
        public void GetVariableFloat3(VariableFloat3ID varID, out Vector3 val)
        {
            val = this.variableFloat3[(int)varID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable float4.
        /// </summary>
        /// <param name="varID">The enuemrated ID of the variable float4 to retrieve.</param>
        /// <param name="val">The float4 value.</param>
        public void GetVariableFloat4(VariableFloat4ID varID, out Vector4 val)
        {
            val = this.variableFloat4[(int)varID];
        }

        /// <summary>
        /// Retrieves the current value of the specified queriable variable bool.
        /// </summary>
        /// <param name="varID">The enuemrated ID of the variable bool to retrieve.</param>
        /// <param name="val">The bool value.</param>
        public void GetVariableBool(VariableBoolID boolID, out bool val)
        {
            val = this.variableBool[(int)boolID];
        }

        /// <summary>
        /// Allocate a new <see cref="GeometryChunk"/> and inserts into the render chunk queue.  A recycling scheme is used to minimize garbage creation.
        /// </summary>
        /// <returns>The allocated instance of <see cref="GeometryChunk"/></returns>
        public GeometryChunk AllocateGeometryChunk()
        {
            if(this.deadGeometryChunkList.Count > 0)
            {
                GeometryChunk chunk = deadGeometryChunkList[this.deadGeometryChunkList.Count - 1];
                this.deadGeometryChunkList.RemoveAt(this.deadGeometryChunkList.Count - 1);

                this.geometryChunkList.Add(chunk);

                return chunk;
            }
            else
            {
                GeometryChunk chunk = new GeometryChunk();
                this.geometryChunkList.Add(chunk);
                return chunk;
            }
        }

        /// <summary>
        /// Deallocates an unused <see cref="GeometryChunk"/> instance.  Use this method if AllocateRenderChunk was called but the returned <see cref="GeometryChunk"/>
        /// instance should be ignored and deleted without being processed for rendering.  This is the "undo" method for AllocateGeometryChunk().
        /// </summary>
        /// <param name="chunk">The <see cref="GeometryChunk"/> to deallocate.</param>
        public void ReleaseGeometryChunk(GeometryChunk chunk)
        {
            chunk.Recycle();

            this.deadGeometryChunkList.Add(chunk);
        }

        /// <summary>
        /// Allocate a new <see cref="LightChunk"/> and inserts into the light chunk queue.  A recycling scheme is used to minimize garbage creation.
        /// </summary>
        /// <returns>The allocated instance of <see cref="LightChunk"/></returns>
        public LightChunk AllocateLightChunk()
        {
            if (this.deadLightChunkList.Count > 0)
            {
                LightChunk chunk = deadLightChunkList[this.deadLightChunkList.Count - 1];
                this.deadLightChunkList.RemoveAt(this.deadLightChunkList.Count - 1);

                this.lightChunkList.Add(chunk);

                return chunk;
            }
            else
            {
                LightChunk chunk = new LightChunk();
                this.lightChunkList.Add(chunk);
                return chunk;
            }
        }

        // <summary>
        /// Deallocates an unused <see cref="LightChunk"/> instance.  Use this method if AllocateLightChunk was called but the returned <see cref="LightChunk"/>
        /// instance should be ignored and deleted without being processed for rendering.  This is the "undo" method for AllocateLightChunk().
        /// </summary>
        /// <param name="chunk">The <see cref="LightChunk"/> to deallocate.</param>
        public void ReleaseLightChunk(LightChunk chunk)
        {
            chunk.Recycle();

            this.deadLightChunkList.Add(chunk);
        }

        /// <summary>
        /// Allocate a new <see cref="ParticleChunk"/> and inserts into the particle chunk queue.  A recycling scheme is used to minimize garbage creation.
        /// </summary>
        /// <returns>The allocated instance of <see cref="ParticleChunk"/></returns>
        public ParticleChunk AllocateParticleChunk()
        {
            if (this.deadParticleChunkList.Count > 0)
            {
                ParticleChunk chunk = this.deadParticleChunkList[this.deadParticleChunkList.Count - 1];
                this.deadParticleChunkList.RemoveAt(this.deadParticleChunkList.Count - 1);

                this.particleChunkList.Add(chunk);

                return chunk;
            }
            else
            {
                ParticleChunk chunk = new ParticleChunk();
                this.particleChunkList.Add(chunk);
                return chunk;
            }
        }

        // <summary>
        /// Deallocates an unused <see cref="ParticleChunk"/> instance.  Use this method if AllocateParticleChunk was called but the returned <see cref="ParticleChunk"/>
        /// instance should be ignored and deleted without being processed for rendering.  This is the "undo" method for AllocateParticleChunk().
        /// </summary>
        /// <param name="chunk">The <see cref="ParticleChunk"/> to deallocate.</param>
        public void ReleaseParticleChunk(ParticleChunk chunk)
        {
            chunk.Recycle();

            this.deadParticleChunkList.Add(chunk);
        }

        /// <summary>
        /// Allocate a new <see cref="WaterChunk"/> and inserts into the water chunk queue.  A recycling scheme is used to minimize garbage creation.
        /// </summary>
        /// <returns>The allocated instance of <see cref="WaterChunk"/></returns>
        public WaterChunk AllocateWaterChunk()
        {
            if (this.deadWaterChunkList.Count > 0)
            {
                WaterChunk chunk = this.deadWaterChunkList[this.deadWaterChunkList.Count - 1];
                this.deadWaterChunkList.RemoveAt(this.deadWaterChunkList.Count - 1);

                this.waterChunkList.Add(chunk);

                return chunk;
            }
            else
            {
                WaterChunk chunk = new WaterChunk();
                this.waterChunkList.Add(chunk);
                return chunk;
            }
        }

        // <summary>
        /// Deallocates an unused <see cref="WaterChunk"/> instance.  Use this method if AllocateWaterChunk was called but the returned <see cref="WaterChunk"/>
        /// instance should be ignored and deleted without being processed for rendering.  This is the "undo" method for AllocateWaterChunk().
        /// </summary>
        /// <param name="chunk">The <see cref="WaterChunk"/> to deallocate.</param>
        public void ReleaseWaterChunk(WaterChunk chunk)
        {
            chunk.Recycle();
            this.deadWaterChunkList.Add(chunk);
        }

        /// <summary>
        /// Inserts an IRenderChunkProvider instance into the per-frame processing list.  All registered IRenderChunkProviders
        /// will be queried for every rendering frame every frame.
        /// </summary>
        /// <param name="provider">The IRenderChunkProvider to register.</param>
        public void RegisterRenderChunkProvider(IRenderChunkProvider provider)
        {
            this.renderChunkProviders.Add(provider);
        }

        /// <summary>
        /// Removes an IRenderChunkProvider instance from the per-frame processing list.
        /// </summary>
        /// <param name="provider"></param>
        public void UnRegisterRenderChunkProvider(IRenderChunkProvider provider)
        {
            this.renderChunkProviders.Remove(provider);
        }

        /// <summary>
        /// Prepare for this frame, query for all chunks
        /// </summary>
        /// <param name="camera">Entity that is acting as a camera</param>
        /// <param name="gameTime">Snapshot of the time</param>
        private void PrepareFrame(BaseEntity camera, CameraComponent cameraComp, GameTime gameTime)
        {
            this.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            this.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            RenderPassDesc passDesc = new RenderPassDesc();
            passDesc.RenderCamera = cameraComp;
            passDesc.GeometryChunksOnlyThisPass = false;
            passDesc.GeometryChunksExcludedThisPass = true;
            passDesc.ViewFrustum = cameraComp.ViewFrustum;            
            passDesc.Type = RenderPassType.Normal;

            // Process initial view, grab all chunk types on this pass, except geometry
            for (int i = 0; i < this.renderChunkProviders.Count; ++i)
            {
                this.renderChunkProviders[i].QueryForChunks(ref passDesc);
            }

            // @todo: Implement real rendering algorithm
            this.variableMatrix[(int)VariableMatrixID.View] = cameraComp.viewMatrix;
            this.variableMatrix[(int)VariableMatrixID.Projection] = cameraComp.projectionMatrix;
            Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.View], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.ViewProjection]);

            this.variableFloat[(int)VariableFloatID.TotalTime] = (float)gameTime.TotalGameTime.Ticks / (float)TimeSpan.TicksPerSecond;

            this.variableFloat4[(int)VariableFloat4ID.ViewPos] = new Vector4(camera.Position, 0.0f);
            this.variableFloat4[(int)VariableFloat4ID.ViewForward] = new Vector4(cameraComp.viewMatrix.Forward, 0.0f);

            if (camera.Position.Y < this.variableFloat[(int)VariableFloatID.WaterElevation])
            {
                this.variableBool[(int)VariableBoolID.CameraUnderwater] = true;
            }
            else
            {
                this.variableBool[(int)VariableBoolID.CameraUnderwater] = false;
            }

            geometryChunksRenderedLastFrame = 0;
            trianglesProcessedLastFrame = 0;
        }

        /// <summary>
        /// Draws a single frame.
        /// </summary>
        /// <param name="camera">Entity that will be serving as the camera.</param>
        /// <param name="gameTime">Snapshot of the game timers.</param>
        public void DrawFrame(BaseEntity camera, GameTime gameTime)
        {
            // This will need to get a major cleanup someday when there is a cleanup pass in
            // the Graphics System.

            // Here's the summary of what is going on here:
            // - PrepareFrame gets all render chunk providers to report any non-geometry chunks 
            //   they may have, like light and water chunks.
            // - ProcessLighting updates light variables for use in shaders, and if shadow mapping
            //   is enabled, light matrices are also created for use in shadow mapping.
            // - ProcessFog updates fog variables for use in shaders.
            // - ProcessShadowMap renders the world (with no color or texture) to a shadow map texture,
            //   so it can get depth and opacity of models for shadowing.
            // - CreateWaterTextures renders the world once for reflection, and another for refraction
            //   (if refractions are enabled). These two render passes are stored in two textures that
            //   can be used by the water plane when it renders. This entire step is skipped if there
            //   is no water in the scene, or if the water plane is not in the view frustum.
            // - ProcessGeometryChunks renders all opaque geometry in the world.
            // - ProcessWaterChunks renders any water planes in the world (engine currently only supports one water plane).
            // - ProcessParticleChunks renders all particles in the world (except the ones in reflections, which are
            //   rendered during the CreateWaterTextures step, in which they make a call to ProcessParticleChunks as well.
            // - ProcessTransparentGeometryChunks renders any semi-transparent geometry in the world.
           
            // Currently the order of these functions cannot be changed, however, the engine is setup in such a way that you can
            // do not need a water plane in a scene, and you can turn off shadow mapping (through your settings.xml). In those cases
            // the engine will skip through those functions very quickly and efficiently.

            CameraComponent cameraComp = camera.GetComponentByType(ComponentType.CameraComponent) as CameraComponent;
            if (null == cameraComp)
            {
                throw new Exception("No CameraComponent was found on the entity that is currently assigned to be a camera");
            }

            PrepareFrame(camera, cameraComp, gameTime);

            ProcessLighting(camera);

            ProcessFog();

            ProcessShadowMap(cameraComp);

            CreateWaterTextures(camera, cameraComp);

            ProcessGeometryChunks(cameraComp);

            ProcessWaterChunks(cameraComp);            

            // And we always render particles last
            ProcessParticleChunks();

            ProcessTransparentGeometryChunks(cameraComp);            

            // Display the shadow map to the screen (if needed).
            DrawShadowMapToScreen();

            // Use this to debug water reflection and refraction
            //DrawReflectionMapToScreen();
            //DrawRefractionMapToScreen();

            DrawPhysics(gameTime);
            DrawGUI(gameTime);

            ClearChunks();
        }        

        private void ClearChunks()
        {
            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                this.geometryChunkList[i].Recycle();
                this.deadGeometryChunkList.Add(this.geometryChunkList[i]);
            }
            this.geometryChunkList.Clear();

            for (int i = 0; i < this.lightChunkList.Count; ++i)
            {
                this.lightChunkList[i].Recycle();
                this.deadLightChunkList.Add(this.lightChunkList[i]);
            }
            this.lightChunkList.Clear();

            this.particleChunksRenderedLastFrame = 0;
            for (int i = 0; i < this.particleChunkList.Count; ++i)
            {
                this.particleChunkList[i].Recycle();
                this.deadParticleChunkList.Add(this.particleChunkList[i]);

                ++this.particleChunksRenderedLastFrame;
            }
            this.particleChunkList.Clear();

            for (int i = 0; i < this.waterChunkList.Count; ++i)
            {
                this.waterChunkList[i].Recycle();
                this.deadWaterChunkList.Add(this.waterChunkList[i]);
            }
            this.waterChunkList.Clear();
        }

        /// <summary>
        /// Stores light direction, color, and processes the light-view-projection matrix for shadow mapping, if needed
        /// </summary>
        /// <param name="camera"></param>
        private void ProcessLighting(BaseEntity camera)
        {
            // @todo: Support more than one directional light source.
            bool directionFound = false;
            foreach ( LightChunk light in this.lightChunkList )
            {
                // We only care about directional lights
                if (!directionFound && light.directional)
                {                   
                    // Make sure direction is of valid length
                    if ( light.direction.LengthSquared() > float.Epsilon )
                    {
                        directionFound = true;

                        this.variableFloat4[(int)VariableFloat4ID.LightDir] = new Vector4(light.direction, 1.0f);
                        this.variableFloat4[(int)VariableFloat4ID.AmbientColor] = light.ambientColor;
                        this.variableFloat4[(int)VariableFloat4ID.DiffuseColor] = light.diffuseColor;
                        this.variableFloat4[(int)VariableFloat4ID.SpecularColor] = light.specularColor;
                        this.variableFloat[(int)VariableFloatID.SpecularPower] = light.specularPower;

                        if (this.shadowMappingEnabled)
                        {
                            // Now we calculate the lightViewProj matrix for shadow mapping

                            // Matrix with that will rotate in points the direction of the light
                            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero,
                                                                       light.direction,
                                                                       Vector3.Up);

                            CreateLightMatrices(camera, 500, ref light.direction, ref lightRotation, 
                                                out this.variableMatrix[(int)VariableMatrixID.LightProjection],
                                                out this.variableMatrix[(int)VariableMatrixID.LightView]);

                            Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.LightView], 
                                            ref this.variableMatrix[(int)VariableMatrixID.LightProjection], 
                                            out this.variableMatrix[(int)VariableMatrixID.LightViewProjection]);
                        }
                        
                        break;
                    }
                }
            }

            if (!directionFound)
            {
                this.variableFloat4[(int)VariableFloat4ID.LightDir] = new Vector4(0.5f, -0.4f, -0.5f, 1.0f);   // Default setting in case no light is created
            }       
        }

        private void CreateLightMatrices(BaseEntity camera, float desiredFarPlane, ref Vector3 lightDirection, 
                                        ref Matrix lightRotation, out Matrix LightProj, out Matrix LightView )
        {
            MsgCameraGetValues msgGetVals = ObjectPool.Aquire<MsgCameraGetValues>();
            msgGetVals.UniqueTarget = camera.UniqueID;
            this.game.SendMessage(msgGetVals);

            Matrix Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(msgGetVals.FOV), msgGetVals.AspectRatio,
                                                                    msgGetVals.NearPlane, desiredFarPlane);

            BoundingFrustum frustum = new BoundingFrustum(msgGetVals.ViewMatrix * Projection);

            // Get the corners of the frustum
            Vector3[] frustumCorners = frustum.GetCorners();

            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            // Find the smallest box around the points
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            // We need the position back in world coordinates so we transform 
            // the light position by the inverse of the lights rotation
            lightPosition = Vector3.Transform(lightPosition,
                                              Matrix.Invert(lightRotation));

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            LightProj = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                                                 -boxSize.Z, boxSize.Z);

            LightView = Matrix.CreateLookAt(lightPosition,
                                            lightPosition + lightDirection,
                                            Vector3.Up);
        }

        private void ProcessFog()
        {
            MsgGetFogSettings msgGetFog = ObjectPool.Aquire<MsgGetFogSettings>();
            this.game.SendInterfaceMessage(msgGetFog, InterfaceType.SceneManager);

            // We only need to get new fog settings if they've changed
            if (msgGetFog.Settings.SceneFogChanged)
            {
                this.variableFloat4[(int)VariableFloat4ID.FogColor] = msgGetFog.FogColor;
                this.variableFloat[(int)VariableFloatID.FogNear] = msgGetFog.FogNearDistance;
                this.variableFloat[(int)VariableFloatID.FogFar] = msgGetFog.FogFarDistance;
                this.variableFloat[(int)VariableFloatID.FogDistanceRange] = msgGetFog.FogFarDistance - msgGetFog.FogNearDistance;
                this.variableFloat[(int)VariableFloatID.FogThinning] = msgGetFog.FogThinning;
                this.variableFloat[(int)VariableFloatID.FogAltitude] = msgGetFog.FogAltitude;
                msgGetFog.Settings.SceneFogChanged = false;
            }
        }

        private void GetGeometryChunks(CameraComponent cameraComp, RenderPassType passType)
        {
            // We only process geometry chunks if we don't already have any
            if (this.geometryChunkList.Count > 0)
                return;

            RenderPassDesc passDesc = new RenderPassDesc();
            passDesc.RequestedLOD = LOD.High;
            passDesc.RenderCamera = cameraComp;
            passDesc.GeometryChunksOnlyThisPass = true;
            passDesc.GeometryChunksExcludedThisPass = false;
            passDesc.Type = passType;            
            passDesc.ViewFrustum = new BoundingFrustum(this.variableMatrix[(int)VariableMatrixID.View] * this.variableMatrix[(int)VariableMatrixID.Projection]);

            // Process initial view, grab ONLY geometry chunk types on this pass
            for (int i = 0; i < this.renderChunkProviders.Count; ++i)
            {
                this.renderChunkProviders[i].QueryForChunks(ref passDesc);
            }
        }

        private void GetGeometryChunks(CameraComponent cameraComp, ref Matrix ViewProjection, RenderPassType passType)
        {
            // We only process geometry chunks if we don't already have any
            if (this.geometryChunkList.Count > 0)
                return;            

            RenderPassDesc passDesc = new RenderPassDesc();
            passDesc.RequestedLOD = LOD.High;
            passDesc.RenderCamera = cameraComp;
            passDesc.GeometryChunksOnlyThisPass = true;
            passDesc.GeometryChunksExcludedThisPass = false;
            passDesc.Type = passType;
            passDesc.ViewFrustum = new BoundingFrustum(ViewProjection);

            // Process initial view, grab ONLY geometry chunk types on this pass
            for (int i = 0; i < this.renderChunkProviders.Count; ++i)
            {
                this.renderChunkProviders[i].QueryForChunks(ref passDesc);
            }
        }

        /// <summary>
        /// Processes a <see cref="GeometryChunk"/> list for shadow map.
        /// </summary>
        private void ProcessShadowMap(CameraComponent cameraComp)
        {
            if (!this.shadowMappingEnabled)
                return;

            GetGeometryChunks(cameraComp, ref this.variableMatrix[(int)VariableMatrixID.LightViewProjection], RenderPassType.ShadowMapCreate);

            this.graphics.GraphicsDevice.Clear(Color.Transparent);

            // Set our render target to our floating point render target
            this.graphics.GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            this.variableTexture2D[(int)VariableTexture2D.ShadowMap] = null;
            
            // Clear the render target to white or all 1's
            // We set the clear to white since that represents the 
            // furthest the object could be away
            this.graphics.GraphicsDevice.Clear(Color.White);

            PreferredRenderOrder orderToRender;
            for (orderToRender = PreferredRenderOrder.RenderFirst; orderToRender < PreferredRenderOrder.NumOfRenderOrders; ++orderToRender)
            {
                for (int i = 0; i < this.geometryChunkList.Count; ++i)
                {
                    GeometryChunk chunk = this.geometryChunkList[i];
                    chunk.Material.CurrentTechnique = chunk.RenderTechniqueName;  

                    if (chunk.RenderOrder != orderToRender)
                        continue;

                    if (!chunk.CanCreateShadows)
                            continue;

                    string defaultTechnique = chunk.Material.Effect.CurrentTechnique.Name;

                    this.variableMatrix[(int)VariableMatrixID.World] = chunk.WorldTransform;
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.World], ref this.variableMatrix[(int)VariableMatrixID.View], out this.variableMatrix[(int)VariableMatrixID.WorldView]);
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);

                    this.variableFloat4[(int)VariableFloat4ID.ModelColor] = chunk.ModelColor.ToVector4();

                    for (int j = 0; j < chunk.VertexStreams.Count; ++j)
                    {
                        this.graphics.GraphicsDevice.SetVertexBuffer(chunk.VertexStreams[j]);                        
                    }

                    this.graphics.GraphicsDevice.Indices = chunk.Indices;                    

                    chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques["CreateShadowMap"];

                    ++geometryChunksRenderedLastFrame;
                    trianglesProcessedLastFrame += chunk.PrimitiveCount;

                    int passes = chunk.Material.BindMaterial(this);
                    for (int j = 0; j < passes; ++j)
                    {
                        chunk.Material.BeginPass(j);

                        this.graphics.GraphicsDevice.DrawIndexedPrimitives(chunk.Type, chunk.VertexStreamOffset, 0, chunk.VertexCount, chunk.StartIndex, chunk.PrimitiveCount);

                        chunk.Material.EndPass();
                    }
                    chunk.Material.UnBindMaterial();

                    RasterizerState newState = new RasterizerState();
                    newState.FillMode = FillMode.Solid;
                    newState.CullMode = CullMode.CullCounterClockwiseFace;
                    this.graphics.GraphicsDevice.RasterizerState = newState; 

                    chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[defaultTechnique];
                }
            }

            // Set render target back to the back buffer
            this.graphics.GraphicsDevice.SetRenderTarget(null);

            // Return the shadow map as a texture
            this.variableTexture2D[(int)VariableTexture2D.ShadowMap] = shadowRenderTarget;

            // Shadow mapping required a different view frustum, which means the render chunks gathered
            // from that perspective are not useful for other rendering, so we clear them out here.
            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                this.geometryChunkList[i].Recycle();
                this.deadGeometryChunkList.Add(this.geometryChunkList[i]);
            }
            this.geometryChunkList.Clear();
        }

        /// <summary>
        /// Processes a <see cref="GeometryChunk"/> list.
        /// </summary>
        private void ProcessGeometryChunks(CameraComponent cameraComp)
        {
            GetGeometryChunks(cameraComp, RenderPassType.OpaqueOnly);   

            PreferredRenderOrder orderToRender;
            for (orderToRender = PreferredRenderOrder.RenderFirst; orderToRender < PreferredRenderOrder.NumOfRenderOrders; ++orderToRender)
            {
                for (int i = this.geometryChunkList.Count - 1; i >= 0; --i)
                {
                    GeometryChunk chunk = this.geometryChunkList[i];
                    chunk.Material.CurrentTechnique = chunk.RenderTechniqueName;

                    if (chunk.RenderOrder != orderToRender)
                        continue;

                    bool techniqueAltered = false;
                    string defaultTechnique = "";
                    if (this.variableBool[(int)VariableBoolID.CameraUnderwater] || !chunk.CanReceiveShadows || !this.shadowMappingEnabled)
                    {
                        techniqueAltered = true;
                        if (chunk.Material.CurrentTechnique == "")
                        {
                            chunk.Material.CurrentTechnique = chunk.Material.Effect.CurrentTechnique.Name;
                            defaultTechnique = chunk.Material.Effect.CurrentTechnique.Name;
                        }

                        defaultTechnique = chunk.Material.CurrentTechnique;

                        if (this.variableBool[(int)VariableBoolID.CameraUnderwater])
                        {
                            String underwaterTechnique = chunk.Material.CurrentTechnique + "CameraUnderwater";

                            // Check if technique exists
                            if (null != chunk.Material.Effect.Techniques[underwaterTechnique])
                            {
                                chunk.Material.CurrentTechnique += "CameraUnderwater";
                                chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                            }
                        }

                        if (!chunk.CanReceiveShadows || !this.shadowMappingEnabled)
                        {
                            chunk.Material.CurrentTechnique += "NoShadow";
                        }

                        chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                    }

                    this.variableMatrix[(int)VariableMatrixID.World] = chunk.WorldTransform;
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.World], ref this.variableMatrix[(int)VariableMatrixID.View], out this.variableMatrix[(int)VariableMatrixID.WorldView]);
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);

                    this.variableFloat4[(int)VariableFloat4ID.ModelColor] = chunk.ModelColor.ToVector4();    

                    for (int j = 0; j < chunk.VertexStreams.Count; ++j)
                    {
                        this.graphics.GraphicsDevice.SetVertexBuffer(chunk.VertexStreams[j]);
                    }

                    this.graphics.GraphicsDevice.Indices = chunk.Indices;                    

                    ++geometryChunksRenderedLastFrame;
                    trianglesProcessedLastFrame += chunk.PrimitiveCount;

                    int passes = chunk.Material.BindMaterial(this);
                    for (int j = 0; j < passes; ++j)
                    {
                        chunk.Material.BeginPass(j);

                        this.graphics.GraphicsDevice.DrawIndexedPrimitives(chunk.Type, chunk.VertexStreamOffset, 0, chunk.VertexCount, chunk.StartIndex, chunk.PrimitiveCount);

                        chunk.Material.EndPass();
                    }
                    chunk.Material.UnBindMaterial();

                    if (techniqueAltered)
                    {
                        chunk.Material.CurrentTechnique = defaultTechnique;
                        chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                    }

                    RasterizerState newState = new RasterizerState();
                    newState.FillMode = FillMode.Solid;
                    newState.CullMode = CullMode.CullCounterClockwiseFace;
                    this.graphics.GraphicsDevice.RasterizerState = newState;
                }                
            }
        }

        private void ProcessTransparentGeometryChunks(CameraComponent cameraComp)
        {
            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                this.geometryChunkList[i].Recycle();
                this.deadGeometryChunkList.Add(this.geometryChunkList[i]);
            }
            this.geometryChunkList.Clear();

            GetGeometryChunks(cameraComp, RenderPassType.SemiTransparentOnly);            
            this.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                GeometryChunk chunk = this.geometryChunkList[i];
                chunk.Material.CurrentTechnique = chunk.RenderTechniqueName;

                bool techniqueAltered = false;
                string defaultTechnique = "";
                if (!chunk.CanReceiveShadows || !this.shadowMappingEnabled)
                {
                    techniqueAltered = true;
                    if (chunk.Material.CurrentTechnique == "")
                    {
                        chunk.Material.CurrentTechnique = chunk.Material.Effect.CurrentTechnique.Name;
                    }

                    defaultTechnique = chunk.Material.CurrentTechnique;
                    chunk.Material.CurrentTechnique += "NoShadow";
                    chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                }

                this.variableMatrix[(int)VariableMatrixID.World] = chunk.WorldTransform;
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.World], ref this.variableMatrix[(int)VariableMatrixID.View], out this.variableMatrix[(int)VariableMatrixID.WorldView]);
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);

                this.variableFloat4[(int)VariableFloat4ID.ModelColor] = chunk.ModelColor.ToVector4();

                for (int j = 0; j < chunk.VertexStreams.Count; ++j)
                {
                    this.graphics.GraphicsDevice.SetVertexBuffer(chunk.VertexStreams[j]);
                }

                this.graphics.GraphicsDevice.Indices = chunk.Indices;
                
                ++geometryChunksRenderedLastFrame;
                trianglesProcessedLastFrame += chunk.PrimitiveCount;

                int passes = chunk.Material.BindMaterial(this);                
                for (int j = 0; j < passes; ++j)
                {
                    chunk.Material.BeginPass(j);

                    this.graphics.GraphicsDevice.DrawIndexedPrimitives(chunk.Type, chunk.VertexStreamOffset, 0, chunk.VertexCount, chunk.StartIndex, chunk.PrimitiveCount);

                    chunk.Material.EndPass();
                }
                chunk.Material.UnBindMaterial();

                if (techniqueAltered)
                {
                    chunk.Material.CurrentTechnique = defaultTechnique;
                    chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                }

                RasterizerState newState = new RasterizerState();
                newState.FillMode = FillMode.Solid;
                newState.CullMode = CullMode.CullCounterClockwiseFace;
                this.graphics.GraphicsDevice.RasterizerState = newState;
            }
        }

        /// <summary>
        /// Processes a <see cref="WaterChunk"/> list.
        /// </summary>
        private void CreateWaterTextures(BaseEntity camera, CameraComponent cameraComp)
        {            
            if (this.waterChunkList.Count > 0)
            {
                RasterizerState newState = new RasterizerState();
                newState.FillMode = FillMode.Solid;
                newState.CullMode = CullMode.None;
                this.graphics.GraphicsDevice.RasterizerState = newState;
            }

            for (int i = 0; i < this.waterChunkList.Count; ++i)
            {
                WaterChunk waterChunk = this.waterChunkList[i];

                this.variableFloat4[(int)VariableFloat4ID.WaterColorLight] = waterChunk.WaterColorLight;
                this.variableFloat4[(int)VariableFloat4ID.WaterColorDark] = waterChunk.WaterColorDark;

                if (this.settings.EnableWaterReflections)
                {
                    RenderPassType type = RenderPassType.WaterReflection;
                    if (this.settings.EnableWaterUsesSkyOnly)
                    {
                        type = RenderPassType.SkyOnly;
                    }                    

                    SetupReflectionMap(ref this.variableMatrix[(int)VariableMatrixID.Projection], camera, waterChunk.Elevation);
                    RenderSceneForWater(ref this.ReflectionViewMatrix, camera, cameraComp, type);
                    ProcessReflectionMap();

                    this.variableTexture2D[(int)VariableTexture2D.ReflectionMap] = this.ReflectionMap;
                    this.variableMatrix[(int)VariableMatrixID.ReflectionView] = this.ReflectionViewMatrix;
                }

                if (this.settings.EnableWaterRefractions)
                {
                    // We always render refractions second, so that we can use the geometry chunks from its render pass for everything else
                    SetupRefractionMap(ref cameraComp.viewMatrix, ref this.variableMatrix[(int)VariableMatrixID.Projection], camera, waterChunk.Elevation);
                    RenderSceneForWater(ref cameraComp.viewMatrix, camera, cameraComp, RenderPassType.WaterRefraction);
                    ProcessRefractionMap();

                    this.variableTexture2D[(int)VariableTexture2D.RefractionMap] = this.RefractionMap;
                }
                
                if (this.settings.EnableWaterShoreline)
                {
                    if (this.game.TerrainID != QSGame.UniqueIDEmpty)
                    {
                        MsgGetTerrainProperties msgGetTerrainProps = ObjectPool.Aquire<MsgGetTerrainProperties>();
                        msgGetTerrainProps.UniqueTarget = this.game.TerrainID;
                        this.game.SendMessage(msgGetTerrainProps);

                        this.variableTexture2D[(int)VariableTexture2D.HeightMap] = msgGetTerrainProps.HeightMap;
                        this.variableFloat[(int)VariableFloatID.MinTerrainHeight] = msgGetTerrainProps.MinHeight;
                        this.variableFloat[(int)VariableFloatID.MaxTerrainHeight] = msgGetTerrainProps.MaxHeight;
                        this.variableFloat[(int)VariableFloatID.TerrainHeightRange] = msgGetTerrainProps.MaxHeight - msgGetTerrainProps.MinHeight;
                        this.variableFloat[(int)VariableFloatID.TerrainElevationModifier] = msgGetTerrainProps.ElevationStrength;
                        this.variableFloat[(int)VariableFloatID.TerrainScale] = msgGetTerrainProps.TerrainScale;
                        this.variableFloat[(int)VariableFloatID.TerrainWidth] = msgGetTerrainProps.Size;
                    }
                }

                this.variableMatrix[(int)VariableMatrixID.Projection] = cameraComp.projectionMatrix;
                this.variableMatrix[(int)VariableMatrixID.World] = Matrix.Identity;
                this.variableMatrix[(int)VariableMatrixID.View] = cameraComp.viewMatrix;
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.View], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.ViewProjection]);
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);                    
            }
        }

        private void ProcessWaterChunks(CameraComponent cameraComp)
        {            
            string Technique = "Water";

            if (this.waterChunkList.Count > 0)
            {
                RasterizerState newState = new RasterizerState();
                newState.FillMode = FillMode.Solid;
                newState.CullMode = CullMode.None;
                this.graphics.GraphicsDevice.RasterizerState = newState;

                this.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                switch (this.settings.GraphicsLevel)
                {
                    case GraphicsLevel.Highest:
                    case GraphicsLevel.High:
                        break;
                    case GraphicsLevel.Med:
                    case GraphicsLevel.Low:
                        Technique = "WaterReflectOnly";
                        break;
                }

                if (!this.settings.EnableWaterShoreline)
                {
                    Technique += "NoShoreline";
                }

                this.variableMatrix[(int)VariableMatrixID.Projection] = cameraComp.projectionMatrix;
                this.variableMatrix[(int)VariableMatrixID.World] = Matrix.Identity;
                this.variableMatrix[(int)VariableMatrixID.View] = cameraComp.viewMatrix;
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.View], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.ViewProjection]);
                Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);
                this.variableFloat[(int)VariableFloatID.TotalTime] = ((float)this.game.GameTime.TotalGameTime.Ticks / (float)TimeSpan.TicksPerSecond) * 0.0006f;
            }

            // TODO: Properly support different water planes
            for (int i = 0; i < this.waterChunkList.Count; ++i)
            {
                WaterChunk waterChunk = this.waterChunkList[i];
                waterChunk.Material.CurrentTechnique = Technique;

                this.variableFloat[(int)VariableFloatID.Reflectivity] = waterChunk.Reflectivity;
                this.variableFloat[(int)VariableFloatID.WaterElevation] = waterChunk.Elevation;
                if (this.variableBool[(int)VariableBoolID.CameraUnderwater])
                {
                    this.variableFloat[(int)VariableFloatID.Reflectivity] = 1.0f - ( 1.0f - this.variableFloat[(int)VariableFloatID.Reflectivity] ) * 0.5f;
                }

                int passes = waterChunk.Material.BindMaterial(this);
                for (int k = 0; k < passes; ++k)
                {
                    waterChunk.Material.BeginPass(k);

                    this.graphics.GraphicsDevice.DrawUserPrimitives(waterChunk.Type, waterChunk.Vertices, 0, waterChunk.PrimitiveCount);

                    waterChunk.Material.EndPass();
                }
                waterChunk.Material.UnBindMaterial();
            }

            if (this.waterChunkList.Count > 0)
            {
                var newState = new RasterizerState();                
                newState.CullMode = CullMode.CullCounterClockwiseFace;
                this.graphics.GraphicsDevice.RasterizerState = newState;
            }
        }

        private void RenderSceneForWater(ref Matrix ViewMatrix, BaseEntity camera, CameraComponent cameraComp, RenderPassType renderType)
        {
            this.variableMatrix[(int)VariableMatrixID.View] = ViewMatrix;

            // Clean up any geometry chunks that might be remaining from a previous render pass
            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                this.geometryChunkList[i].Recycle();
                this.deadGeometryChunkList.Add(this.geometryChunkList[i]);
            }
            this.geometryChunkList.Clear();

            var passDesc = new RenderPassDesc();
            passDesc.RequestedLOD = this.settings.DesiredLOD;
            passDesc.RenderCamera = cameraComp;
            passDesc.GeometryChunksOnlyThisPass = true;
            passDesc.GeometryChunksExcludedThisPass = false;            
            passDesc.Type = renderType;
            passDesc.ViewFrustum = new BoundingFrustum(ViewMatrix * this.variableMatrix[(int)VariableMatrixID.Projection]);
            if (this.variableBool[(int)VariableBoolID.UsingClippingPlane])
            {
                Vector4 clipCopy = this.variableFloat4[(int)VariableFloat4ID.ClippingPlane];
                Vector4 clipPlane = new Vector4(clipCopy.X, clipCopy.Y, clipCopy.Z, clipCopy.W);
                    
                passDesc.ClippingPlane = new Plane(clipPlane);               
            }

            // Process initial view, grab ONLY geometry chunk types on this pass
            for (int i = 0; i < this.renderChunkProviders.Count; ++i)
            {
                this.renderChunkProviders[i].QueryForChunks(ref passDesc);
            }

            PreferredRenderOrder orderToRender;
            for (orderToRender = PreferredRenderOrder.RenderFirst; orderToRender < PreferredRenderOrder.NumOfRenderOrders; ++orderToRender)
            {
                for (int j = 0; j < this.geometryChunkList.Count; ++j)
                {
                    GeometryChunk chunk = this.geometryChunkList[j];
                    chunk.Material.CurrentTechnique = chunk.RenderTechniqueName;

                    if (chunk.RenderOrder != orderToRender)
                        continue;

                    bool techniqueAltered = false;
                    string defaultTechnique = "";
                    if (this.variableBool[(int)VariableBoolID.CameraUnderwater] || !chunk.CanReceiveShadows || !this.shadowMappingEnabled)
                    {
                        techniqueAltered = true;
                        if (chunk.Material.CurrentTechnique == "")
                        {
                            chunk.Material.CurrentTechnique = chunk.Material.Effect.CurrentTechnique.Name;
                            defaultTechnique = chunk.Material.Effect.CurrentTechnique.Name;
                        }

                        defaultTechnique = chunk.Material.CurrentTechnique;

                        if (this.variableBool[(int)VariableBoolID.CameraUnderwater])
                        {
                            String underwaterTechnique = chunk.Material.CurrentTechnique + "CameraUnderwater";
                                                        
                            // Check if technique exists
                            if (null != chunk.Material.Effect.Techniques[underwaterTechnique])
                            {
                                chunk.Material.CurrentTechnique += "CameraUnderwater";
                                chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                            }
                        }

                        if (!chunk.CanReceiveShadows || !this.shadowMappingEnabled)
                        {
                            chunk.Material.CurrentTechnique += "NoShadow";                            
                        }

                        chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[chunk.Material.CurrentTechnique];
                    }

                    this.variableMatrix[(int)VariableMatrixID.World] = chunk.WorldTransform;
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.World], ref this.variableMatrix[(int)VariableMatrixID.View], out this.variableMatrix[(int)VariableMatrixID.WorldView]);
                    Matrix.Multiply(ref this.variableMatrix[(int)VariableMatrixID.WorldView], ref this.variableMatrix[(int)VariableMatrixID.Projection], out this.variableMatrix[(int)VariableMatrixID.WorldViewProjection]);

                    this.variableFloat4[(int)VariableFloat4ID.ModelColor] = chunk.ModelColor.ToVector4();

                    for (int k = 0; k < chunk.VertexStreams.Count; ++k)
                    {
                        this.graphics.GraphicsDevice.SetVertexBuffer(chunk.VertexStreams[k]);
                    }

                    this.graphics.GraphicsDevice.Indices = chunk.Indices;
                    
                    ++geometryChunksRenderedLastFrame;
                    trianglesProcessedLastFrame += chunk.PrimitiveCount;

                    int passes = chunk.Material.BindMaterial(this);
                    for (int k = 0; k < passes; ++k)
                    {
                        chunk.Material.BeginPass(k);

                        this.graphics.GraphicsDevice.DrawIndexedPrimitives(chunk.Type, chunk.VertexStreamOffset, 0, chunk.VertexCount, chunk.StartIndex, chunk.PrimitiveCount);

                        chunk.Material.EndPass();
                    }
                    chunk.Material.UnBindMaterial();

                    if (techniqueAltered)
                    {
                        chunk.Material.CurrentTechnique = defaultTechnique;
                        chunk.Material.Effect.CurrentTechnique = chunk.Material.Effect.Techniques[defaultTechnique];
                    }

                    if ( null == this.graphics.GraphicsDevice.RasterizerState
                      || this.graphics.GraphicsDevice.RasterizerState.FillMode != FillMode.Solid
                      || this.graphics.GraphicsDevice.RasterizerState.CullMode != CullMode.None)
                    {
                        RasterizerState newState = new RasterizerState();
                        newState.FillMode = FillMode.Solid;
                        newState.CullMode = CullMode.None;
                        this.graphics.GraphicsDevice.RasterizerState = newState;
                    }
                }
            }

            // Clean up any geometry chunks that might be remaining from a previous render pass
            for (int i = 0; i < this.geometryChunkList.Count; ++i)
            {
                this.geometryChunkList[i].Recycle();
                this.deadGeometryChunkList.Add(this.geometryChunkList[i]);
            }
            this.geometryChunkList.Clear();

            if ( !this.settings.EnableParticleRenderWithWater || renderType == RenderPassType.SkyOnly )           
            {
                return;
            }
            // We don't render particles during reflections if the camera is underwater.
            else if (renderType == RenderPassType.WaterReflection && (camera.Position.Y <= this.variableFloat[(int)VariableFloatID.WaterElevation]))
            {
                return;
            }
            // We don't render particles during refractions if the camera is above water, because particles don't go underwater.
            else if (renderType == RenderPassType.WaterRefraction && (camera.Position.Y > this.variableFloat[(int)VariableFloatID.WaterElevation]))
            {
                return;
            } 
            else
            {
                ProcessParticleChunks();
            }
        }

        /// <summary>
        /// Draws refractions of items underwater.
        /// </summary>
        private void SetupReflectionMap(ref Matrix ProjectionMatrix, BaseEntity Camera, float Elevation)
        {
            Vector3 reflectPos = Camera.Position;
            reflectPos.Y = Elevation - (reflectPos.Y - Elevation);      // Flip camera position over the water plane

            Vector3 reflectForward = Camera.Rotation.Forward;
            reflectForward.Y *= -1;                                     // Flip camera's vertical angle

            Vector3 reflectOffset = reflectPos + reflectForward;
            Vector3 cameraUp = Vector3.Cross(Camera.Rotation.Right, reflectForward);
            Matrix.CreateLookAt(ref reflectPos, ref reflectOffset, ref cameraUp, out this.ReflectionViewMatrix);

            Vector3 reflectNormal = ReflectNormalDirection;
            // If underwater, flip elevation so camera acts as if it were underwater
            if (Camera.Position.Y < this.variableFloat[(int)VariableFloatID.WaterElevation])
            {
                Elevation *= -1;
                reflectNormal *= -1;
                this.variableBool[(int)VariableBoolID.ClippingAboveWater] = false;
            }
            else
            {
                this.variableBool[(int)VariableBoolID.ClippingAboveWater] = true;
            }

            // Store clipping plane in shader parameters
            this.variableBool[(int)VariableBoolID.UsingClippingPlane] = true;
            this.variableFloat4[(int)VariableFloat4ID.ClippingPlane] = new Vector4(reflectNormal, -Elevation);

            this.graphics.GraphicsDevice.SetRenderTarget(this.ReflectionMap);
            this.graphics.GraphicsDevice.Clear(Color.LightGray);
        }

        private void ProcessReflectionMap()
        {
            this.graphics.GraphicsDevice.SetRenderTarget(null);
            this.graphics.GraphicsDevice.Clear(Color.LightGray);

            // Turn off clipping plane
            this.variableBool[(int)VariableBoolID.UsingClippingPlane] = false;
            this.variableBool[(int)VariableBoolID.ClippingAboveWater] = false;
        }

        /// <summary>
        /// Draws refractions of items underwater.
        /// </summary>
        private void SetupRefractionMap(ref Matrix ViewMatrix, ref Matrix ProjectionMatrix, BaseEntity Camera, float Elevation)
        {
            Matrix camMatrix = ViewMatrix * ProjectionMatrix;
            Matrix invCamMatrix;
            Matrix.Invert(ref camMatrix, out invCamMatrix);
            Matrix.Transpose(ref invCamMatrix, out invCamMatrix);

            Vector3 reflectNormal = ReflectNormalDirection;

            // If underwater, flip elevation and normal so clipping plane reverses when the camera is underwater.
            if (Camera.Position.Y < this.variableFloat[(int)VariableFloatID.WaterElevation])
            {
                reflectNormal *= -1;
                Elevation -= 5.0f;  // We adjust the clipping plane to it doesn't line up perfectly with the water plane. This gives less isses
                                    // with artifacts near the places where the water meets geometry.
                Elevation *= -1;

                this.variableBool[(int)VariableBoolID.ClippingAboveWater] = true;
            }
            else
            {
                Elevation += 5.0f;  // We adjust the clipping plane to it doesn't line up perfectly with the water plane. This gives less isses
                                    // with artifacts near the places where the water meets geometry.

                this.variableBool[(int)VariableBoolID.ClippingAboveWater] = false;
            }           

            // Store clipping plane in shader parameters
            this.variableBool[(int)VariableBoolID.UsingClippingPlane] = true;
            this.variableFloat4[(int)VariableFloat4ID.ClippingPlane] = new Vector4(-reflectNormal, Elevation);
           
            this.graphics.GraphicsDevice.SetRenderTarget(this.RefractionMap);
            this.graphics.GraphicsDevice.Clear(Color.LightGray);
        }

        private void ProcessRefractionMap()
        {
            this.graphics.GraphicsDevice.SetRenderTarget(null);
            this.graphics.GraphicsDevice.Clear(Color.LightGray);

            // Turn off clipping plane
            this.variableBool[(int)VariableBoolID.UsingClippingPlane] = false;
            this.variableBool[(int)VariableBoolID.ClippingAboveWater] = false;
        }        

        /// <summary>
        /// Processes a <see cref="ParticleChunk"/> list.
        /// </summary>
        private void ProcessParticleChunks()
        {
            RasterizerState newState = new RasterizerState();
            newState.FillMode = FillMode.Solid;
            newState.CullMode = CullMode.CullCounterClockwiseFace;
            this.graphics.GraphicsDevice.RasterizerState = newState;

            particlesRenderedLastFrame = 0;

            if (this.particleChunkList.Count > 0)
            {
                this.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
                this.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;                

                this.variableFloat2[(int)VariableFloat2ID.ViewportScale] = new Vector2(0.5f / this.game.GraphicsDevice.Viewport.AspectRatio, -0.5f);
            }

            for (int i = 0; i < this.particleChunkList.Count; ++i)
            {
                ParticleChunk chunk = this.particleChunkList[i];

                if (chunk.ParticleSettings.SourceBlend != this.graphics.GraphicsDevice.BlendState.AlphaSourceBlend
                 || chunk.ParticleSettings.DestinationBlend != this.graphics.GraphicsDevice.BlendState.AlphaDestinationBlend)
                {
                    BlendState testState = new BlendState();
                    testState.AlphaSourceBlend = chunk.ParticleSettings.SourceBlend;
                    testState.AlphaDestinationBlend = chunk.ParticleSettings.DestinationBlend;
                    this.graphics.GraphicsDevice.BlendState = testState;
                }

                this.variableFloat[(int)VariableFloatID.Duration] = (float)chunk.ParticleSettings.Duration.TotalSeconds;
                this.variableFloat[(int)VariableFloatID.DurationRandomness] = chunk.ParticleSettings.DurationRandomness;
                this.variableFloat[(int)VariableFloatID.EndVelocity] = chunk.ParticleSettings.EndVelocity;
                this.variableFloat2[(int)VariableFloat2ID.RotateSpeed] = chunk.ParticleSettings.RotateSpeed;
                this.variableFloat2[(int)VariableFloat2ID.StartSize] = chunk.ParticleSettings.StartSize;
                this.variableFloat2[(int)VariableFloat2ID.EndSize] = chunk.ParticleSettings.EndSize;
                this.variableFloat3[(int)VariableFloat3ID.Gravity] = chunk.ParticleSettings.Gravity;
                this.variableFloat4[(int)VariableFloat4ID.MinColor] = chunk.ParticleSettings.MinColorVect;
                this.variableFloat4[(int)VariableFloat4ID.MaxColor] = chunk.ParticleSettings.MaxColorVect;
                this.variableFloat[(int)VariableFloatID.TotalTime] = chunk.CurrentTime; 

                this.graphics.GraphicsDevice.SetVertexBuffer(chunk.vertices);
                this.graphics.GraphicsDevice.Indices = chunk.indices;

                int passes = chunk.Material.BindMaterial(this);
                for (int j = 0; j < passes; ++j)
                {
                    chunk.Material.BeginPass(j);

                    this.graphics.GraphicsDevice.DrawIndexedPrimitives(chunk.Type,
                                                                       0,
                                                                       chunk.StartVertexIndex,
                                                                       chunk.NumVerts,
                                                                       chunk.StartIndex,
                                                                       chunk.PrimitiveCount);

                    particlesRenderedLastFrame += chunk.PrimitiveCount;

                    chunk.Material.EndPass();
                }
                chunk.Material.UnBindMaterial();
            }
           
            this.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            this.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

#if WINDOWS
        /// <summary>
        /// Draw physics shapes (if that feature is currently enabled)
        /// </summary>
        /// <param name="gameTime">Timing information</param>
        public void DrawPhysics(GameTime gameTime)
        {
            PhysicsRenderManager manager = this.game.PhysicsRenderer;
            if (null != manager)
            {
                if (manager.Enabled)
                {
                    manager.Draw(gameTime);
                }
            }
        }
#endif //WINDOWS

        /// <summary>
        /// Draw any necessary UI elements
        /// </summary>
        /// <param name="gameTime">Timing information</param>
        public void DrawGUI(GameTime gameTime)
        {
            GuiManager manager = this.game.Gui;
            if (null != manager)
            {
                manager.Draw(gameTime);
            }
        }

        /// <summary>
        /// Render the shadow map texture to the screen
        /// </summary>
        void DrawShadowMapToScreen()
        {
            if (this.shadowMappingEnabled && this.game.DrawShadowMapTextureToScreen)
            {
                if (this.variableTexture2D[(int)VariableTexture2D.ShadowMap] == null)
                    return;

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                spriteBatch.Draw(this.variableTexture2D[(int)VariableTexture2D.ShadowMap], new Rectangle(0, 0, 256, 256), Color.White);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Render the reflection map texture to the screen
        /// </summary>
        void DrawReflectionMapToScreen()
        {
            if (this.variableTexture2D[(int)VariableTexture2D.ReflectionMap] == null)
                return;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(this.variableTexture2D[(int)VariableTexture2D.ReflectionMap], new Rectangle(128, 0, 256, 256), Color.White);
            spriteBatch.End();
        }

        /// <summary>
        /// Render the refraction map texture to the screen
        /// </summary>
        void DrawRefractionMapToScreen()
        {
            if (this.variableTexture2D[(int)VariableTexture2D.RefractionMap] == null)
                return;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(this.variableTexture2D[(int)VariableTexture2D.RefractionMap], new Rectangle(384, 0, 256, 256), Color.White);
            spriteBatch.End();
        }


        /// <summary>
        /// Message listener for messages that are not directed at any particular Entity or Interface.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        protected virtual void Game_GameMessage(IMessage message)
        {
            ExecuteMessage(message);
        }

        /// <summary>
        /// Message handler for all incoming messages.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.SetCubeMapTexture:
                    {
                        var msgCubeMap = message as MsgSetGraphicsCubeMap;
                        message.TypeCheck(msgCubeMap);

                        this.CubeMapTexture = msgCubeMap.CubeMapTexture;
                        this.variableTexture2D[(int)VariableTexture2D.CubeMap] = this.CubeMapTexture;
                    }
                    return true;
                case MessageType.SetRenderWaterElevation:
                    {
                        var msgSetElev = message as MsgSetGraphicsWaterElevation;
                        message.TypeCheck(msgSetElev);

                        this.variableFloat[(int)VariableFloatID.WaterElevation] = msgSetElev.Elevation;
                    }
                    return true;
                case MessageType.GetRenderWaterElevation:
                    {
                        var msgGetElev = message as MsgGetGraphicsWaterElevation;
                        message.TypeCheck(msgGetElev);

                        msgGetElev.Elevation = this.variableFloat[(int)VariableFloatID.WaterElevation];
                    }
                    return true;
                case MessageType.GetViewport:
                    {
                        var msgGetViewPort = message as MsgGetViewport;
                        message.TypeCheck(msgGetViewPort);

                        msgGetViewPort.Viewport = this.graphics.GraphicsDevice.Viewport;                        
                    }
                    return true;

                default:
                    return false;
            }
        }
    }
}
