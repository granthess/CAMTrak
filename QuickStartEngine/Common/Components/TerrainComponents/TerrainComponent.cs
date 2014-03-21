//
// TerrainComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Graphics;
using QuickStart.Interfaces;
using QuickStart.Mathmatics;

namespace QuickStart.Components
{
    /*
     * Shader Input Structure
     * 
     *   struct VS_INPUT
         {
             float4 Position            : POSITION0;     
             float3 Normal              : NORMAL0;    
         };
     * */

    /// <summary>
    /// Used to hold vertex information for <see cref="Terrain"/>.
    /// </summary>
    public struct VertexTerrain : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;

        public static int SizeInBytes = (3 + 3) * sizeof(float);
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
            new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 )           
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexTerrain.VertexDeclaration; }
        }
    }

    /// <summary>
    /// The terrain class wraps up the entire terrain system, including all <see cref="QuadTree"/> sections it holds, and
    /// all <see cref="TerrainPatch"/> sections within the QuadTrees. The <see cref="Terrain"/> class also performs all
    /// operations dealing with terrain information, like determining the height of a point on the terrain.
    /// </summary>
    public class TerrainComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.TerrainComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            TerrainComponentDefinition compDef = content.Load<TerrainComponentDefinition>(definitionPath);

            TerrainComponent newComponent = new TerrainComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Retrieves the <see cref="VertexBuffer"/> associated with this terrain.
        /// </summary>
        public VertexBuffer VertexBuffer
        {
            get { return this.vertexBuffer; }
        }

        /// <summary>
        /// Holds entire <see cref="VertexBuffer"/> for this entire terrain section.
        /// </summary>
        private VertexBuffer vertexBuffer;

        /// <summary>
        /// Terrain's parent entity. This is exposed so QuadTree chunks can use it.
        /// </summary>
        public BaseEntity Parent
        {
            get { return this.parentEntity; }
        }

               

        /// <summary>
        /// Holds vertices for entire <see cref="Terrain"/> section until all <see cref="QuadTree"/> sections and
        /// <see cref="TerrainPatch"/> sections have finished loading, and then this list is released.
        /// </summary>
        public List<VertexTerrain> VertexList
        {
            get { return this.vertexList; }
            set
            {
                this.vertexList = value;
            }
        }
        private List<VertexTerrain> vertexList;

        /// <summary>
        /// The root <see cref="QuadTree"/> node for the entire <see cref="Terrain"/> section.
        /// </summary>
        public QuadTree RootQuadTree
        {
            get { return this.rootQuadTree; }
        }
        private QuadTree rootQuadTree;

        /// <summary>
        /// Height/Width of the <see cref="Terrain"/> heightfield. Must be power-of-two.
        /// </summary>
        public int Size
        {
            get { return this.size; }
            set
            {
                // Anytime size is changed we make sure it is being set to a power-of-two value.
                if (QSMath.IsPowerOfTwo(value))
                {
                    this.size = value;
                }
                else
                {
                    throw new Exception("Terrain size (height and width) must be a power-of-two!");
                }
            }
        }
        private int size;

        /// <summary>
        /// <see cref="Terrain"/> scaling factor. Larger values will result in a larger terrain.
        /// </summary>
        public int ScaleFactor
        {
            get { return this.scaleFactor; }
        }
        private int scaleFactor = 1;

        /// <summary>
        /// Elevation strength of <see cref="Terrain"/>. Larger value will give you taller, and steeper <see cref="Terrain"/>.
        /// </summary>
        public float ElevationStrength
        {
            get { return this.elevationStrength; }
            set 
            {
                if (value < float.Epsilon)
                {
                    value = QSConstants.DefaultTerrainElevStr;
                }

                this.elevationStrength = value; 
            }
        }
        private float elevationStrength = 1.0f;

        private float MaximumHeight = 0;
        private float MinimumHeight = 10000;

        /// <summary>
        /// Holds the height (Y coordinate) of each [x, z] coordinate. Height data is loaded from a heightmap image.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public float[,] heightData;

        /// <summary>
        /// Holds information about the texture type, which is stored in each vertex after the vertex buffers 
        /// are created.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Color[,] textureTypeData;

        /// <summary>
        /// Holds the normal vectors for each vertex in the terrain.
        /// The normals for lighting are later stored in each vertex, but
        /// we want to store these values permanentally for proper physics
        /// collisions with the ground.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Vector3[,] normals;

        /// <summary>
        /// Holds information about billboards in on the terrain.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public int[,] billboardData;

        /// <summary>
        /// Heightmap image which is used to setup the heightfield for this <see cref="Terrain"/>
        /// </summary>
        /// <remarks>Can be set to <see cref="null"/> after heightfield is created</remarks>
        private Texture2D heightMap;

         /// <summary>
        /// Must be power of two values.
        /// ALL <see cref="QuadTree"/> sections leafs will result in this size.
        /// </summary>
        public int MinLeafSize
        {
            get { return this.minLeafSize; }
            private set
            {
                if (QSMath.IsPowerOfTwo(value))
                {
                    this.minLeafSize = value;
                }
                else
                {
                    throw new Exception("Leaf size must be set to a power-of-two");
                }
            }
        }
        private int minLeafSize = QSConstants.DefaultQuadTreeWidth * QSConstants.DefaultQuadTreeWidth;

        /// <summary>
        /// Default <see cref="Terrain"/> level-of-detail setting.
        /// </summary>
        public LOD DetailDefault
        {
            get { return this.detailDefault; }
            set { this.detailDefault = value; }
        }
        private LOD detailDefault = LOD.High;

        /// <summary>
        /// Current <see cref="Terrain"/> level-of-detail setting.
        /// </summary>
        public LOD Detail
        {
            get { return this.detail; }
        }
        private LOD detail = LOD.High;

        /// <summary>
        /// Retrieves the material used for the terrain.
        /// </summary>
        public Material Material
        {
            get { return this.material; }
        }
        private Material material;

        public bool DisplayBoundingBoxes
        {
            get { return this.displayBoundingBoxes; }
        }
        private bool displayBoundingBoxes = false;

        /// <summary>
        /// Create an entire <see cref="Terrain"/> section.
        /// </summary>
        /// <param name="game">QSGame reference</param>
        public TerrainComponent(BaseEntity entity, TerrainComponentDefinition compDef)
            : base(entity)
        {
            // Register the terrain entity with the game.
            this.parentEntity.Game.TerrainID = this.parentEntity.UniqueID;

            this.vertexList = new List<VertexTerrain>();

            this.detailDefault = LOD.High;
            this.detail = this.detailDefault;

            if (compDef.ElevationStrength < 1.0f)
            {
                throw new Exception("TerrainComponentDefinition's 'ElevationStrength' must be 1.0 or greater");
            }

            this.elevationStrength = compDef.ElevationStrength;

            InitializeTerrain(compDef.HeightMapImagePath, compDef.ScaleFactor, compDef.SmoothingPasses);

            // Sets up the LOD arrays to allow for new LODs later on.
            SetupLODS();

            // Setup a very low LOD terrain. This will allow the water
            // component to use a very low detail terrain for faster
            // rendering. This terrain is not the default unless specified
            // seperately, so the terrain will continue to render at
            // its default setting, this merely sets up a second LOD terrain
            // for use by other components.
            switch (this.parentEntity.Game.Settings.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                    // No LOD needed at highest settings
                    break;
                case GraphicsLevel.High:
                    AddNew(LOD.Med);
                    break;
                case GraphicsLevel.Med:
                case GraphicsLevel.Low:
                    AddNew(LOD.Low);
                    break;
            }
        }

        /// <summary>
        /// Create an entire <see cref="Terrain"/> section.
        /// </summary>
        /// <param name="game">QSGame reference</param>
        /// <param name="detailLevel">Level-of-detail of this terrain. Higher is better quality, lower is better performance.</param>
        public TerrainComponent(BaseEntity entity, LOD detailLevel)
            : base(entity)
        {
            this.parentEntity.Game.TerrainID = this.parentEntity.UniqueID;

            this.detailDefault = detailLevel;
            this.detail = detailDefault;

            this.vertexList = new List<VertexTerrain>();

            // Sets up the LOD arrays to allow for new LODs later on.
            SetupLODS();

            // Setup a very low LOD terrain. This will allow the water
            // component to use a very low detail terrain for faster
            // rendering. This terrain is not the default unless specified
            // seperately, so the terrain will continue to render at
            // its default setting, this merely sets up a second LOD terrain
            // for use by other components.
            switch (this.parentEntity.Game.Settings.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                    // No LOD needed at highest settings
                    break;
                case GraphicsLevel.High:
                    AddNew(LOD.Med);
                    break;
                case GraphicsLevel.Med:
                case GraphicsLevel.Low:
                    AddNew(LOD.Low);
                    break;
            }
        }

        public override void QueryForChunks(ref RenderPassDesc desc)
        {
            if (desc.GeometryChunksExcludedThisPass)
                return;

            switch (desc.Type)
            {
                case RenderPassType.ShadowMapCreate:
                case RenderPassType.SemiTransparentOnly:
                case RenderPassType.SkyOnly:                
                    {
                        return;
                    }                    
            }

            var msgGetWaterElev = ObjectPool.Aquire<MsgGetGraphicsWaterElevation>();
            this.parentEntity.Game.SendInterfaceMessage(msgGetWaterElev, InterfaceType.Graphics);

            desc.WaterElevation = msgGetWaterElev.Elevation;

            this.rootQuadTree.QueryForRenderChunks(desc.ViewFrustum, ref desc.ClippingPlane, desc.RequestedLOD, ref desc);            
        }

        /// <summary>
        /// Initialize main terrain settings.
        /// </summary>
        /// <param name="heightImagePath">File path for heightmap image, image must be a power of 2 in height and width</param>
        /// <param name="terrainTexturePath">File path for terrain texture image, image must be a power of 
        /// 2 in height and width. Texture image defines where multi-texture splatting occurs. You can use
        /// this to draw out paths or sections in the <see cref="Terrain"/>.</param>
        /// <param name="scaleFactor">Scale (size) of <see cref="Terrain"/>.</param>
        /// <param name="smoothingPasses">Smoothes out the terrain using averages of height. The number of
        /// smoothing passes you choose to make is up to you. If you have sharp elevations on your map, 
        /// you have the elevation strength turned up then you may want a higher value. If your terrain 
        /// is already smooth or has very small elevation strength you may not need any passes. Default 
        /// value is 5. Use value of 0 to skip smoothing.</param>
        public void InitializeTerrain(string heightImagePath, int scaleFactor, int? smoothingPasses)
        {
            this.scaleFactor = (int)MathHelper.Clamp(scaleFactor, 1, QSConstants.MaxTerrainScale);

            LoadHeightData(heightImagePath);                                // Load heightfield from heightmap image

            SmoothTerrain(smoothingPasses);                                 // Smooth out the Terrain
            SetupTerrainNormals();                                          // Setup the normals for each terrain vertex

            this.rootQuadTree = new QuadTree(this.parentEntity.Game, this, this.normals.Length); // Initialize the root quad-tree node

            SetupTerrainVertexBuffer();             // QuadTree sections have setup the vertex list, now this creates a VertexBuffer.

            this.textureTypeData = null;                 // Free terrain data to GC now that each quad-tree section has its own data.
            
            this.vertexList.Clear();                // Free terrain vertex list, as all vertex data is loaded into vertex buffer.

            this.material = this.parentEntity.Game.Content.Load<Material>("Material/Terrain");
        }

        /// <summary>
        /// Load the heightfield data from the heightmap image
        /// </summary>
        /// <param name="heightImagePath">Path of heightmap image to read from</param>
        private void LoadHeightData(string heightImagePath)
        {
            this.heightMap = this.parentEntity.Game.Content.Load<Texture2D>(heightImagePath);

            if ((QSMath.IsPowerOfTwo(this.heightMap.Width) && QSMath.IsPowerOfTwo(this.heightMap.Height) == false))
            {
                throw new Exception("Height maps must have a width and height that is a power of two.");
            }

            this.Size = this.heightMap.Width;             // Sets the map width to the same as the heightmap texture.

            // We setup the map for colors so we can use the color to determine elevations of the map
            Color[] heightMapColors = new Color[size * size];

            // XNA Built-in feature automatically copies pixel data into the heightmap.
            this.heightMap.GetData(heightMapColors);

            this.heightData = new float[size, size];  // Create an array to hold elevations from heightMap

            // Find minimum and maximum values for the heightmap file we read in
            for (int x = 0; x < size; ++x)
                for (int z = 0; z < size; ++z)
                {
                    Color heightMapColorAtPoint = heightMapColors[x + z * size];
                    float heightAtXZ = heightMapColorAtPoint.R + heightMapColorAtPoint.G + heightMapColorAtPoint.B;

                    if (heightAtXZ < this.MinimumHeight)
                        this.MinimumHeight = heightAtXZ;
                    else if (heightAtXZ > this.MaximumHeight)
                        this.MaximumHeight = heightAtXZ;
                }


            // Set height by color, and then alter height by min and max amounts
            for (int x = 0; x < size; ++x)
                for (int z = 0; z < size; ++z)
                {
                    Color heightMapColorAtPoint = heightMapColors[z + x * size];
                    float heightAtXZ = heightMapColorAtPoint.R + heightMapColorAtPoint.G + heightMapColorAtPoint.B;
                    this.heightData[z, x] = (heightAtXZ - this.MinimumHeight) / (this.MaximumHeight - this.MinimumHeight) * this.elevationStrength * this.scaleFactor;
                }
        }

        /// <summary>
        /// Smooths the terrain.
        /// </summary>
        /// <param name="smoothingPasses">Number of smoothing passes to use. More passes will result in a smoother terrain,
        /// however more smoothing takes more loading time.</param>
        private void SmoothTerrain(int? smoothingPasses)
        {
            if (smoothingPasses < 0 || smoothingPasses == null)
            {
                smoothingPasses = QSConstants.DefaultTerrainSmoothing;
            }

            float[,] newHeightData;

            for ( int passes = (int)smoothingPasses; passes > 0; --passes )
            {
                newHeightData = new float[size, size];

                for (int x = 0; x < this.size; ++x)
                {
                    for (int z = 0; z < this.size; ++z)
                    {
                        int adjacentSections = 0;
                        float sectionsTotal = 0.0f;

                        int xMinusOne = x - 1;
                        int zMinusOne = z - 1;
                        int xPlusOne = x + 1;
                        int zPlusOne = z + 1;
                        bool bAboveIsValid = zMinusOne > 0;
                        bool bBelowIsValid = zPlusOne < size;

                        // =================================================================
                        if (xMinusOne > 0)            // Check to left
                        {
                            sectionsTotal += this.heightData[xMinusOne, z];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the left
                            {
                                sectionsTotal += this.heightData[xMinusOne, zMinusOne];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the left
                            {
                                sectionsTotal += this.heightData[xMinusOne, zPlusOne];
                                ++adjacentSections;
                            }
                        }
                        // =================================================================

                        // =================================================================
                        if (xPlusOne < size)     // Check to right
                        {
                            sectionsTotal += this.heightData[xPlusOne, z];
                            ++adjacentSections;

                            if (bAboveIsValid)        // Check up and to the right
                            {
                                sectionsTotal += this.heightData[xPlusOne, zMinusOne];
                                ++adjacentSections;
                            }

                            if (bBelowIsValid)        // Check down and to the right
                            {
                                sectionsTotal += this.heightData[xPlusOne, zPlusOne];
                                ++adjacentSections;
                            }
                        }
                        // =================================================================

                        // =================================================================
                        if (bAboveIsValid)            // Check above
                        {
                            sectionsTotal += this.heightData[x, zMinusOne];
                            ++adjacentSections;
                        }
                        // =================================================================

                        // =================================================================
                        if (bBelowIsValid)    // Check below
                        {
                            sectionsTotal += this.heightData[x, zPlusOne];
                            ++adjacentSections;
                        }
                        // =================================================================

                        newHeightData[x, z] = (this.heightData[x, z] + (sectionsTotal / adjacentSections)) * 0.5f;
                    }
                }

                // Overwrite the HeightData info with our new smoothed info
                for (int x = 0; x < this.size; ++x)
                    for (int z = 0; z < this.size; ++z)
                    {
                        this.heightData[x, z] = newHeightData[x, z];
                    }
            }
        }

        /// <summary>
        /// Setup <see cref="Terrain"/> normals. Normals are used for lighting, normal mapping, and physics with terrain.
        /// </summary>
        private void SetupTerrainNormals()
        {
            VertexTerrain[] terrainVertices = new VertexTerrain[this.size * this.size];
            this.normals = new Vector3[this.size, this.size];

            // Determine vertex positions so we can figure out normals in section below.
            for(int x = 0; x < this.size; ++x)
                for(int z = 0; z < this.size; ++z)
                {
                    terrainVertices[x + z * this.size].Position = new Vector3(x * this.scaleFactor, this.heightData[x, z], z * this.scaleFactor);
                }

            // Setup normals for lighting and physics (Credit: Riemer's method)
            int sizeMinusOne = this.size - 1;
            for (int x = 1; x < sizeMinusOne; ++x)
                for (int z = 1; z < sizeMinusOne; ++z)
                {
                    int ZTimesSize = (z * this.size);
                    Vector3 normX = new Vector3((terrainVertices[x - 1 + ZTimesSize].Position.Y - terrainVertices[x + 1 + ZTimesSize].Position.Y) / 2, 1, 0);
                    Vector3 normZ = new Vector3(0, 1, (terrainVertices[x + (z - 1) * this.size].Position.Y - terrainVertices[x + (z + 1) * this.size].Position.Y) / 2);

                    // We inline the normalize method here since it is used alot, this is faster than calling Vector3.Normalize()
                    Vector3 normal = normX + normZ;
                    float length = (float)Math.Sqrt( (float)((normal.X * normal.X) + (normal.Y * normal.Y) + (normal.Z * normal.Z)) );
                    float num = 1f / length;
                    normal.X *= num;
                    normal.Y *= num;
                    normal.Z *= num;

                    this.normals[x, z] = terrainVertices[x + ZTimesSize].Normal = normal;    // Stored for use in physics and for the
                                                                                             // quad-tree component to reference.
                }
        }

        /// <summary>
        /// Setup vertex buffer for entire terrain section.
        /// </summary>
        private void SetupTerrainVertexBuffer()
        {
            VertexTerrain[] verticesArray = new VertexTerrain[vertexList.Count];

            this.vertexList.CopyTo(verticesArray);

            int SizePlusSizeDivWidth = (this.size + (this.size / QSConstants.DefaultQuadTreeWidth));
            this.vertexBuffer = new VertexBuffer(this.parentEntity.Game.GraphicsDevice, verticesArray[0].GetType(), SizePlusSizeDivWidth * SizePlusSizeDivWidth, BufferUsage.WriteOnly);
            this.vertexBuffer.SetData(verticesArray);
        }

        /// <summary>
        /// Get the height of the terrain at given horizontal coordinates.
        /// </summary>
        /// <param name="xPos">X coordinate</param>
        /// <param name="zPos">Z coordinate</param>
        /// <returns>Height at given coordinates</returns>
        public float GetTerrainHeight(float xPos, float zPos)
        {
            // we first get the height of four points of the quad underneath the point
            // Check to make sure this point is not off the map at all
            int x = (int)(xPos / this.scaleFactor);
            int sizeMinusTwo = this.size - 2;

            if (x > sizeMinusTwo)
            {
                return -10000.0f;      // Terrain height is considered -10000 (or any really low number will do)
                                       // if it is outside the heightmap.
            }
            else if (x < 0)
            {
                return -10000.0f;
            }

            int z = (int)(zPos / this.scaleFactor);
            if (z > sizeMinusTwo)
            {
                return -10000.0f;
            }
            else if (z < 0)
            {
                return -10000.0f;
            }

            int xPlusOne = x + 1;
            int zPlusOne = z + 1;

            float triZ0 = (this.heightData[x, z]);
            float triZ1 = (this.heightData[xPlusOne, z]);
            float triZ2 = (this.heightData[x, zPlusOne]);
            float triZ3 = (this.heightData[xPlusOne, zPlusOne]);

            float height = 0.0f;
            float sqX = (xPos / this.scaleFactor) - x;
            float sqZ = (zPos / this.scaleFactor) - z;
            if ((sqX + sqZ) < 1)
            {
                height = triZ0;
                height += (triZ1 - triZ0) * sqX;
                height += (triZ2 - triZ0) * sqZ;
            }
            else
            {
                height = triZ3;
                height += (triZ1 - triZ3) * (1.0f - sqZ);
                height += (triZ2 - triZ3) * (1.0f - sqX);
            }
            return height;
        }

        /// <summary>
        /// Checks if a position is above terrain.
        /// </summary>
        /// <param name="xPos">X Coordinate of position</param>
        /// <param name="zPos">Z Coordinate of position</param>
        /// <returns>True if position is above terrain.</returns>
        public bool IsAboveTerrain(float xPos, float zPos)
        {
            float XDivScaleFactor = (xPos / this.scaleFactor);

            // Keep object from going off the edge of the map
            if ( (XDivScaleFactor > this.size) || (XDivScaleFactor < 0) )
            {
                return false;
            }

            float ZDivScaleFactor = (zPos / this.scaleFactor);

            // Keep object from going off the edge of the map
            if (ZDivScaleFactor > this.size || (ZDivScaleFactor < 0) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the normal of a position on the heightmap.
        /// </summary>
        /// <param name="xPos">X position on the map</param>
        /// <param name="zPos">Z position on the map</param>
        /// <returns>Normal vector of this spot on the terrain</returns>
        public Vector3 GetNormal(float xPos, float zPos)
        {
            int x = (int)(xPos / this.scaleFactor);
            int sizeMinusTwo = this.size - 2;

            if (x > sizeMinusTwo)
            {
                x = sizeMinusTwo;
            }
            // if it is outside the heightmap.
            else if (x < 0)
            {
                x = 0;
            }

            int z = (int)(zPos / this.scaleFactor);

            if (z > sizeMinusTwo)
            {
                z = sizeMinusTwo;
            }
            else if (z < 0)
            {
                z = 0;
            }

            int xPlusOne = x + 1;
            int zPlusOne = z + 1;

            Vector3 triZ0 = (this.normals[x, z]);
            Vector3 triZ1 = (this.normals[xPlusOne, z]);
            Vector3 triZ2 = (this.normals[x, zPlusOne]);
            Vector3 triZ3 = (this.normals[xPlusOne, zPlusOne]);

            Vector3 avgNormal;
            float sqX = (xPos / this.scaleFactor) - x;
            float sqZ = (zPos / this.scaleFactor) - z;
            if ((sqX + sqZ) < 1)
            {
                avgNormal = triZ0;
                avgNormal += (triZ1 - triZ0) * sqX;
                avgNormal += (triZ2 - triZ0) * sqZ;
            }
            else
            {
                avgNormal = triZ3;
                avgNormal += (triZ1 - triZ3) * (1.0f - sqZ);
                avgNormal += (triZ2 - triZ3) * (1.0f - sqX);
            }
            return avgNormal;
        }

        /// <summary>
        /// Sets the minimum leaf size for quad-tree patches. Must be a power of two value.
        /// </summary>
        /// <param name="Width">Minimum leaf size width (also sets height to match)</param>
        public void SetLeafSize(int width)
        {
            this.MinLeafSize = (width * width);
        }

        /// <summary>
        /// Sets the <see cref="TerrainDetail"/> level
        /// </summary>
        /// <param name="detailLevel">Detail level setting</param>
        public void SetDetailLevel(LOD detailLevel)
        {
            this.detail = detailLevel;
        }

        /// <summary>
        /// Recursively sets up all LODs lookups for all quad-tree leaves.
        /// </summary>
        public void SetupLODS()
        {
            this.RootQuadTree.SetupLODs();
        }

        /// <summary>
        /// Adds a new level of detail to all quad-tree leaves.
        /// </summary>
        /// <param name="detailLevel">Detail level to add to all terrain patches in the Terrain</param>
        public void AddNew(LOD detailLevel)
        {
            this.RootQuadTree.AddNewPatchLOD(detailLevel);
        }

        /// <summary>
        /// Send a message directly to an Entity through this method.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Returns true if message was handled</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            if (message.UniqueTarget != this.parentEntity.UniqueID)
                return false;

            switch (message.Type)
            {
                case MessageType.GetTerrainHeightAtXZ:
                    {
                        MsgGetTerrainHeight getHeightMsg = message as MsgGetTerrainHeight;
                        message.TypeCheck(getHeightMsg);

                        getHeightMsg.PositionAboveTerrain = IsAboveTerrain(getHeightMsg.XPos, getHeightMsg.ZPos);

                        if (getHeightMsg.PositionAboveTerrain)
                            getHeightMsg.OutHeight = GetTerrainHeight(getHeightMsg.XPos, getHeightMsg.ZPos);
                    }
                    return true;
                case MessageType.GetTerrainEntity:
                    {
                        MsgGetTerrainEntity getTerrainMsg = message as MsgGetTerrainEntity;
                        message.TypeCheck(getTerrainMsg);

                        getTerrainMsg.TerrainEntity = this.parentEntity;
                    }
                    return true;
                case MessageType.ToggleTerrainDisplayBoundingBoxes:
                    {
                        MsgToggleDisplayBoundingBoxes msgDisplayBoxes = message as MsgToggleDisplayBoundingBoxes;
                        message.TypeCheck(msgDisplayBoxes);

                        // Toggle display of bounding boxes
                        displayBoundingBoxes = !displayBoundingBoxes;
                    }
                    return true;
                case MessageType.GetTerrainProperties:
                    {
                        MsgGetTerrainProperties msgTerrain = message as MsgGetTerrainProperties;
                        message.TypeCheck(msgTerrain);

                        msgTerrain.HeightMap = this.heightMap;
                        msgTerrain.MinHeight = this.MinimumHeight;
                        msgTerrain.MaxHeight = this.MaximumHeight;
                        msgTerrain.TerrainScale = this.ScaleFactor;
                        msgTerrain.ElevationStrength = this.ElevationStrength;
                        msgTerrain.Size = this.size;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
