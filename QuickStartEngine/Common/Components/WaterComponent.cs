//
// WaterComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities;
using QuickStart.Graphics;
using QuickStart.Interfaces;

namespace QuickStart.Components
{
    /*
         * Water Input Structure:
         *   struct ADV_VS_INPUT
             {
                 float4 Position            : POSITION0;
                 float2 TextureCoords       : TEXCOORD0;
             };
         * */

    public struct VertexWater : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoords;

        public static int SizeInBytes = (3 + 2) * sizeof(float);
        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        public VertexWater(Vector3 pos, Vector2 texCoord)
            : this()
        {
            Position = pos;
            TexCoords = texCoord;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexWater.VertexDeclaration; }
        }
    }

    public class WaterComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.WaterComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            WaterComponentDefinition compDef = content.Load<WaterComponentDefinition>(definitionPath);

            WaterComponent newComponent = new WaterComponent(parent, compDef);
            return newComponent;
        }

        // Vertex declaration describes the format of our ParticleVertex structure.
        static private VertexDeclaration vertexDeclaration;

        private VertexWater[] waterVertices;

        /// <summary>
        /// This water component's material
        /// </summary>
        private Material material;

        /// <summary>
        /// Last recorded position of the water plane, this is used to keep track of changes to update things like
        /// the graphics system and bounding box.
        /// </summary>
        private Vector3 lastPosition;

        /// <summary>
        /// Bounding box that surrounds the water plane, used for frustum culling.
        /// </summary>
        private BoundingBox boundingBox;

        /// <summary>
        /// Color of the lightest water (shallow water)
        /// </summary>
        public Vector4 WaterColorLight
        {
            get { return this.waterColorLight; }
            set { this.waterColorLight = value; }
        }
        private Vector4 waterColorLight = new Vector4(0.0f, 0.235f, 0.44f, 1.0f);

        /// <summary>
        /// Color of the darkest water (deeper water)
        /// </summary>
        public Vector4 WaterColorDark
        {
            get { return this.waterColorDark; }
            set { this.waterColorDark = value; }
        }
        private Vector4 waterColorDark = new Vector4(0.0f, 0.126f, 0.367f, 1.0f);

        /// <summary>
        /// How reflective the surface of the water is.
        /// </summary>
        /// <remarks>A value of 0.0f means no reflection, and 1.0f means a perfect reflection.</remarks>
        public float Reflectivity
        {
            get { return this.reflectivity; }
            set { this.reflectivity = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }
        private float reflectivity = 0.6f;

        /// <summary>
        /// Width dimension of water plane
        /// </summary>
        public float Width
        {
            get { return this.width; }
        }
        private float width;

        /// <summary>
        /// Length dimension of water plane
        /// </summary>
        public float Length
        {
            get { return this.length; }
        }
        private float length;

        /// <summary>
        /// Elevation of the surface of the water
        /// </summary>
        public float WaterElevation
        {
            get { return waterVertices[0].Position.Y; }
        }

        /// <summary>
        /// Sets the colors of the water, which are a <see cref="Vector4"/>, from <see cref="Color"/> values.
        /// </summary>
        /// <param name="waterColor">Color to set water to.</param>
        public void SetWaterColors(Color lightColor, Color darkColor)
        {
            this.waterColorLight = lightColor.ToVector4();
            this.waterColorDark = darkColor.ToVector4();
        }

        public WaterComponent(BaseEntity parent, WaterComponentDefinition compDef) :
            base(parent)
        {
            ActivateComponent();

            this.width = compDef.Width;
            this.length = compDef.Length;

            SetupWaterVertices();

            // The vertex declaration is static, so we only need to create it once, and all water planes can use it
            if (vertexDeclaration == null)
            {
                vertexDeclaration = VertexWater.VertexDeclaration;
            }

            if (compDef.MaterialPath.Length > 0)
            {
                this.material = this.parentEntity.Game.Content.Load<Material>(compDef.MaterialPath);
            }
            else
            {
                throw new Exception("A WaterComponentDefinition must contain a valid material file path");
            }

            switch (this.parentEntity.Game.Graphics.Settings.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                case GraphicsLevel.High:
                    this.material.CurrentTechnique = "Water";
                    break;
                case GraphicsLevel.Med:
                    this.material.CurrentTechnique = "WaterReflectOnly";
                    break;
                case GraphicsLevel.Low:
                    this.material.CurrentTechnique = "WaterReflectOnly";
                    break;
            }

            this.WaterColorLight = compDef.WaterColorLight;
            this.WaterColorDark = compDef.WaterColorDark;
            this.Reflectivity = compDef.Reflectivity;
        }

        public WaterComponent(BaseEntity parent, int width, int length, string materialPath) :
            base(parent)
        {
            ActivateComponent();

            this.width = width;
            this.length = length;

            SetupWaterVertices();

            // The vertex declaration is static, so we only need to create it once, and all water planes can use it
            if (vertexDeclaration == null)
            {
                vertexDeclaration = VertexWater.VertexDeclaration;
            }

            this.material = this.parentEntity.Game.Content.Load<Material>(materialPath);

            switch (this.parentEntity.Game.Graphics.Settings.GraphicsLevel)
            {
                case GraphicsLevel.Highest:
                case GraphicsLevel.High:
                    this.material.CurrentTechnique = "Water";
                    break;
                case GraphicsLevel.Med:
                    this.material.CurrentTechnique = "WaterReflectOnly";
                    break;
                case GraphicsLevel.Low:
                    this.material.CurrentTechnique = "WaterReflectOnly";
                    break;
            }            
        }

        public void SetupWaterVertices(float width, float length)
        {
            this.width = width;
            this.length = length;

            SetupWaterVertices();
        }

        /// <summary>
        /// Setup the two triangles that makeup the water plane.
        /// </summary>
        public void SetupWaterVertices()
        {
            this.waterVertices = new VertexWater[6];

            this.waterVertices[0] = new VertexWater(Vector3.Zero, new Vector2(0, 1));
            this.waterVertices[1] = new VertexWater(new Vector3(0, 0, this.length), new Vector2(0, 0));
            this.waterVertices[2] = new VertexWater(new Vector3(this.width, 0, 0), new Vector2(1, 0));

            this.waterVertices[3] = new VertexWater(new Vector3(this.width, 0, 0), new Vector2(0, 1));
            this.waterVertices[4] = new VertexWater(new Vector3(0, 0, this.length), new Vector2(1, 1));
            this.waterVertices[5] = new VertexWater(new Vector3(this.width, 0, this.length), new Vector2(1, 0));

            this.boundingBox.Min = this.waterVertices[0].Position;
            this.boundingBox.Min.Y -= 1;
            this.boundingBox.Max = this.waterVertices[5].Position;
            this.boundingBox.Max.Y += 1;

            // Let graphics system know about the new elevation
            var msgSetElev = ObjectPool.Aquire<MsgSetGraphicsWaterElevation>();
            msgSetElev.Elevation = this.WaterElevation;
            this.parentEntity.Game.SendInterfaceMessage(msgSetElev, InterfaceType.Graphics);
        }

        /// <summary>
        /// Used to update the two triangles that make up the water plane, they must be
        /// updated when their position changes.
        /// </summary>
        private void UpdateWaterVertices()
        {
            var msgGetPos = ObjectPool.Aquire<MsgGetPosition>();
            msgGetPos.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendMessage(msgGetPos);

            if (lastPosition != msgGetPos.Position)
            {
                Vector3 positionChange = msgGetPos.Position - lastPosition;

                lastPosition = msgGetPos.Position;

                for (int i = 0; i < waterVertices.Length; ++i)
                {
                    waterVertices[i].Position += positionChange;
                }

                // We need to update the bounding box as well
                boundingBox.Min.Y += positionChange.Y;
                boundingBox.Max.Y += positionChange.Y;

                // Let graphics system know about the new elevation
                var msgSetElev = ObjectPool.Aquire<MsgSetGraphicsWaterElevation>();
                msgSetElev.Elevation = this.WaterElevation;
                this.parentEntity.Game.SendInterfaceMessage(msgSetElev, InterfaceType.Graphics);
            }
        }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void Update(GameTime gameTime)
        {
            UpdateWaterVertices();
        }        

        /// <summary>
        /// This is called by the <seealso cref="GraphicsSystem"/> to query the WaterComponent for information
        /// needed to render the water plane.
        /// </summary>
        /// <param name="desc"></param>
        public override void QueryForChunks(ref RenderPassDesc desc)
        {
            // We don't render water on geometry render passes
            if (desc.GeometryChunksOnlyThisPass)
            {
                return;
            }

            // Check to see if the water plane is in the view frustum
            if (!desc.ViewFrustum.Intersects(this.boundingBox))
            {
                return;
            }

            WaterChunk chunk = this.parentEntity.Game.Graphics.AllocateWaterChunk();
            chunk.Vertices = this.waterVertices;
            chunk.VertexDeclaration = vertexDeclaration;
            chunk.Material = this.material;
            chunk.Type = PrimitiveType.TriangleList;
            chunk.PrimitiveCount = 2;   // The water plane is only two triangles.
            chunk.Elevation = this.WaterElevation;
            chunk.Reflectivity = this.reflectivity;
            chunk.WaterColorLight = this.waterColorLight;
            chunk.WaterColorDark = this.waterColorDark;
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            if (message.UniqueTarget != this.parentEntity.UniqueID)
                return false;

            switch (message.Type)
            {
                case MessageType.SetWaterColors:
                    {
                        var msgSetColor = message as MsgSetWaterColors;
                        message.TypeCheck(msgSetColor);

                        this.WaterColorLight = msgSetColor.WaterColorLight.ToVector4();
                        this.WaterColorDark = msgSetColor.WaterColorDark.ToVector4();
                    }
                    return true;
                case MessageType.SetWaterReflectivity:
                    {
                        var msgSetReflectivity = message as MsgSetWaterReflectivity;
                        message.TypeCheck(msgSetReflectivity);

                        this.reflectivity = msgSetReflectivity.Reflectivity;
                    }
                    return true;
                case MessageType.GetWaterElevation:
                    {
                        var msgGetElevation = message as MsgGetWaterElevation;
                        message.TypeCheck(msgGetElevation);

                        msgGetElevation.Elevation = this.WaterElevation;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
