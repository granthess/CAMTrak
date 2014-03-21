//
// RenderComponent.cs
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
using QuickStart.GeometricPrimitives;

namespace QuickStart.Components
{
    /// <summary>
    /// Creates a render component. All render information of this entity will come from this component.
    /// </summary>
    public class RenderComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.RenderComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            RenderComponentDefinition compDef = content.Load<RenderComponentDefinition>(definitionPath);

            RenderComponent newComponent = new RenderComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// StaticModel is a basic model for rendering.
        /// </summary>
        public StaticModel Model
        {
            get { return model; }
        }
        private StaticModel model = null;

        /// <summary>
        /// This render component's material
        /// </summary>
        private Material material;

        /// <summary>
        /// Holds scaling information for this render component to use when submitting a <see cref="GeometryChunk"/>.
        /// </summary>
        private Matrix scaleMatrix;

        /// <summary>
        /// Holds a matrix that includes rotation and position for the <see cref="GeometryChunk"/>.
        /// </summary>
        private Matrix worldTransform;

        /// <summary>
        /// Whether or not this object renders when only a sky should be rendered
        /// </summary>
        public bool RendersAsSky
        {
            get { return rendersAsSky; }
            set { rendersAsSky = value; }
        }
        private bool rendersAsSky;

        /// <summary>
        /// Bounding sphere the surrounds the model.
        /// </summary>
        private BoundingSphere boundingSphere;

        public PreferredRenderOrder RenderOrder
        {            
            get { return renderOrder; }
            set { renderOrder = value; }
        }
        private PreferredRenderOrder renderOrder = PreferredRenderOrder.RenderNormal;

        public bool CanCreateShadows
        {            
            get { return canCreateShadows; }
            set { canCreateShadows = value; }
        }
        public bool canCreateShadows = false;

        public bool CanReceiveShadows
        {
            get { return canReceiveShadows; }
            set { canReceiveShadows = value; }
        }
        public bool canReceiveShadows = false;

        public Color ModelColor
        {
            get { return modelColor; }
            set
            {
                modelColor = value;
                opacity = (modelColor.A / 255.0f);
            }
        }
        public Color modelColor = Color.LightGray;

        /// <summary>
        /// Opacity of this entity, between 0 and 1, 0 being completely transparent, 1 being completely opaque.
        /// </summary>
        public float Opacity
        {
            get { return opacity; }
            set 
            { 
                opacity = MathHelper.Clamp(value, 0.0f, 1.0f);
                modelColor.A = (byte)(MathHelper.Clamp(value, 0.0f, 1.0f) * 255);
            }
        }
        public float opacity = 1.0f;

        /// <summary>
        /// Create a render component. All rendering info for an entity comes from here.
        /// </summary>
        /// <param name="parent">Entity this component will be attached to</param>
        public RenderComponent(BaseEntity parent, RenderComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            if (compDef.PrimitiveType != GeometricPrimitiveType.Invalid)
            {
                LoadModelFromPrimitive(compDef.PrimitiveType, compDef.Height, compDef.Width, compDef.Depth, compDef.Diameter);
            }
            else if (compDef.ModelPath.Length > 0)
            {
                LoadModel(compDef.ModelPath);
            }
            else
            {
                throw new Exception("A RenderComponentDefinition must contain either a valid primitive type, or a path to a model asset file");
            }

            if (compDef.MaterialPath.Length > 0)
            {
                LoadMaterial(compDef.MaterialPath);
            }
            else
            {
                throw new Exception("A RenderComponentDefintion must contain a valid material path");
            }

            this.renderOrder = compDef.RenderOrder;
            this.canCreateShadows = compDef.CanCreateShadows;
            this.canReceiveShadows = compDef.CanReceiveShadows;
            this.ModelColor = new Color(compDef.MeshColor);
            this.rendersAsSky = compDef.RendersAsSky;
            this.Opacity = compDef.Opacity;
        }

        /// <summary>
        /// Create a render component. All rendering info for an entity comes from here.
        /// </summary>
        /// <param name="parent">Entity this component will be attached to</param>
        public RenderComponent(BaseEntity parent, string modelPath, string materialPath)
            : base(parent)
        {
            ActivateComponent();

            LoadModel(modelPath);
            LoadMaterial(materialPath);
        }

        /// <summary>
        /// Create a render component. All rendering info for an entity comes from here.
        /// </summary>
        /// <param name="parent">Entity this component will be attached to</param>
        public RenderComponent(BaseEntity parent, string materialPath, GeometricPrimitiveType primitiveType)
            : base(parent)
        {
            ActivateComponent();

            LoadModelFromPrimitive(primitiveType);
            LoadMaterial(materialPath);
        }

        /// <summary>
        /// Creates a StaticModel from a GeometricPrimitive.
        /// </summary>
        /// <param name="primitiveType">Type of primitive to create</param>
        private void LoadModelFromPrimitive( GeometricPrimitiveType primitiveType )
        {
            LoadModelFromPrimitive(primitiveType, 1.0f, 1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Creates a StaticModel from a GeometricPrimitive and specified dimensions.
        /// </summary>
        /// <param name="primitiveType">Type of primitive to create</param>
        /// <param name="height">Height of primitive, used by cubes and cylinders.</param>
        /// <param name="width">Width of primitive, used by cubes.</param>
        /// <param name="depth">Depth of primitive, used by cubes.</param>
        /// <param name="diameter">Diameter of primitive, used by cylinders, spheres, toruses, and teapots.</param>
        private void LoadModelFromPrimitive(GeometricPrimitiveType primitiveType, float height, float width, float depth, float diameter)
        {
            GeometricPrimitive primitive;

            switch (primitiveType)
            {
                case GeometricPrimitiveType.Box:
                    primitive = new CubePrimitive(this.parentEntity.Game.GraphicsDevice, height, width, depth);
                    break;
                case GeometricPrimitiveType.Sphere:
                    primitive = new SpherePrimitive(this.parentEntity.Game.GraphicsDevice, diameter, 16);
                    break;
                case GeometricPrimitiveType.Cylinder:
                    primitive = new CylinderPrimitive(this.parentEntity.Game.GraphicsDevice, height, diameter, 16);
                    break;
                case GeometricPrimitiveType.Cone:
                    primitive = new ConePrimitive(this.parentEntity.Game.GraphicsDevice, height, diameter, 16);
                    break;
                case GeometricPrimitiveType.Torus:
                    primitive = new TorusPrimitive(this.parentEntity.Game.GraphicsDevice, diameter, 0.3333f, 16);
                    break;
                case GeometricPrimitiveType.Teapot:
                    primitive = new TeapotPrimitive(this.parentEntity.Game.GraphicsDevice, diameter, 8);
                    break;
                default:
                    throw new Exception("LoadPrimitive does not handle this type of GeometricPrimitive. Was a new primitive type made and not handled here?");
            }

            if (null != primitive)
            {
                model = new StaticModel(primitive, this.parentEntity.Game.GraphicsDevice);
            }
        }

        /// <summary>
        /// Initialize the component, which will attach it to the entity
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            this.scaleMatrix = Matrix.CreateScale(this.parentEntity.Scale);
        }

        public void SetShadowingProperties( bool canCreateShadows, bool canReceiveShadows )
        {
            this.canCreateShadows = canCreateShadows;
            this.canReceiveShadows = canReceiveShadows;
        }

        /// <summary>
        /// Allows the renderer to query this component for render information
        /// </summary>
        /// <param name="desc">Descriptor reference from the renderer</param>
        public override void QueryForChunks(ref RenderPassDesc desc)
        {            
            if (desc.GeometryChunksExcludedThisPass)
                return;

            switch ( desc.Type )
            {
                case RenderPassType.OpaqueOnly:
                    {
                        if (this.modelColor.A != 255)
                            return;
                    }
                    break;
                case RenderPassType.SemiTransparentOnly:
                    {
                        if (this.modelColor.A == 0 || this.modelColor.A == 255 )
                            return;
                    }
                    break;
                case RenderPassType.SkyOnly:                
                    {
                        if (!this.rendersAsSky)
                            return;
                    }
                    break;
            }

            // If a clipping plane exists, check which side the sphere is on
            if (null != desc.ClippingPlane)
            {
                PlaneIntersectionType intersection = desc.ClippingPlane.Intersects(this.boundingSphere);
                
                // If all geometry is on the back side of the plane, then no need to render it
                if (intersection == PlaneIntersectionType.Back)
                {
                    return;
                }
            }

            // If all geometry lies outside the view frustum, don't render it
            if (!desc.ViewFrustum.Intersects(this.boundingSphere))
            {
                return;
            }

            GeometryChunk chunk;
            chunk = this.parentEntity.Game.Graphics.AllocateGeometryChunk();
            chunk.StartIndex = 0;
            chunk.Material = this.material;
            chunk.RenderTechniqueName = this.material.CurrentTechnique;
            chunk.VertexStreamOffset = 0;            
            chunk.Indices = this.model.IndexBuffer;
            chunk.PrimitiveCount = this.model.PrimitiveCount;
            chunk.Type = this.model.GeometryType;
            chunk.VertexCount = this.model.VertexCount;
            chunk.VertexStreams.Add(this.model.VertexBuffer);
            chunk.WorldTransform = worldTransform;
            chunk.RenderOrder = this.renderOrder;
            chunk.CanCreateShadows = this.canCreateShadows;
            chunk.CanReceiveShadows = this.canReceiveShadows;
            chunk.ModelColor = this.modelColor;
        }

        /// <summary>
        /// Loads an entity's model
        /// </summary>
        /// <param name="modelPath">File path to the model file</param>
        protected void LoadModel(string modelPath)
        {
            this.model = this.parentEntity.Game.ModelLoader.LoadStaticModel(modelPath);

            ProcessBoundingRadius();
        }

        /// <summary>
        /// Create a bounding sphere for this entity's model
        /// </summary>
        protected void ProcessBoundingRadius()
        {
            List<Vector3> vertices = this.model.GetModelVertices(this.parentEntity.Scale);

            this.boundingSphere = GetBoundingSphere(vertices);
            this.boundingSphere.Center = this.parentEntity.Position;
        }

        /// <summary>
        /// Loads a entity's material
        /// </summary>
        /// <param name="materialPath">File path to the material file</param>
        protected void LoadMaterial(string materialPath)
        {
            this.material = this.parentEntity.Game.Content.Load<Material>(materialPath);            
        }

        /// <summary>
        /// Creates the bounding box for the model. This should only be called during model
        /// initialization, as it is much too slow to be used during runtime, or each frame for each model.
        /// </summary>
        public BoundingSphere GetBoundingSphere(List<Vector3> vertices)
        {
            BoundingSphere sphere = BoundingSphere.CreateFromPoints(vertices);

            return sphere;
        }

        /// <summary>
        /// Sent to a component when its parent entity is added to a scene
        /// </summary>
        /// <param name="manager">The <see cref="SceneManager"/> that added the entity to the scene.</param>
        public override void AddedToScene(SceneManager manager)
        {
            this.worldTransform = this.scaleMatrix * this.parentEntity.Rotation * Matrix.CreateTranslation(this.parentEntity.Position);
            this.boundingSphere.Center = this.parentEntity.Position;
        }

        /// <summary>
        /// Update the component, at a fixed rate in sync with the physics world
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void FixedUpdate(GameTime gameTime)
        {
            this.worldTransform = this.scaleMatrix * this.parentEntity.Rotation * Matrix.CreateTranslation(this.parentEntity.Position);
            this.boundingSphere.Center = this.parentEntity.Position;

            MsgGetIsInMotion msgInMotion = ObjectPool.Aquire<MsgGetIsInMotion>();
            msgInMotion.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendMessage(msgInMotion);

            // If the entity isn't moving any longer, no need to continue updating this component.
            if (!msgInMotion.InMotion)
            {
                DeactivateComponent();
            }
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
                case MessageType.GetModelVertices:
                    {
                        MsgGetModelVertices msgGetVerts = message as MsgGetModelVertices;
                        message.TypeCheck(msgGetVerts);

                        if (model == null)
                        {
                            return false;
                        }

                        msgGetVerts.Vertices = model.GetModelVertices(this.parentEntity.Scale);
                    }
                    return true;
                case MessageType.GetModelIndices:
                    {
                        MsgGetModelIndices msgGetInds = message as MsgGetModelIndices;
                        message.TypeCheck(msgGetInds);

                        if (model == null)
                        {
                            return false;
                        }

                        msgGetInds.Indices = model.GetModelIndices();
                    }
                    return true;
                case MessageType.SetModelColor:
                    {
                        MsgSetModelColor msgSetColor = message as MsgSetModelColor;
                        message.TypeCheck(msgSetColor);

                        this.ModelColor = msgSetColor.Color;
                    }
                    return true;
                case MessageType.GetModelColor:
                    {
                        MsgGetModelColor msgGetColor = message as MsgGetModelColor;
                        message.TypeCheck(msgGetColor);

                        msgGetColor.Color = this.modelColor;
                    }
                    return true;
                case MessageType.SetModelOpacity:
                    {
                        MsgSetModelOpacity msgSetOpacity = message as MsgSetModelOpacity;
                        message.TypeCheck(msgSetOpacity);

                        msgSetOpacity.Opacity = this.opacity;
                    }
                    return true;
                case MessageType.GetModelOpacity:
                    {
                        MsgGetModelOpacity msgGetOpacity = message as MsgGetModelOpacity;
                        message.TypeCheck(msgGetOpacity);

                        this.Opacity = msgGetOpacity.Opacity;
                    }
                    return true;
                case MessageType.PositionChanged:
                    {
                        MsgPositionChanged msgPosChanged = message as MsgPositionChanged;
                        message.TypeCheck(msgPosChanged);

                        // We update the render mesh's transform (position, rotation),
                        // and its bounding sphere's position as well.
                        this.worldTransform = this.scaleMatrix * this.parentEntity.Rotation * Matrix.CreateTranslation(msgPosChanged.Position);
                        this.boundingSphere.Center = this.parentEntity.Position;
                    }
                    return true;
                case MessageType.RotationChanged:
                    {
                        MsgRotationChanged msgRotChanged = message as MsgRotationChanged;
                        message.TypeCheck(msgRotChanged);

                        // We update the render mesh's transform (position, rotation).                        
                        this.worldTransform = this.scaleMatrix * msgRotChanged.Rotation * Matrix.CreateTranslation(this.parentEntity.Position);                        
                    }
                    return true;
                case MessageType.BodyActivated:
                    {
                        MsgPhysicsBodyActivated msgBodyActive = message as MsgPhysicsBodyActivated;
                        message.TypeCheck(msgBodyActive);

                        ActivateComponent();
                    }
                    return true;
                case MessageType.BodyDeactivated:
                    {
                        MsgPhysicsBodyDeactivated msgBodyDeactivated = message as MsgPhysicsBodyDeactivated;
                        message.TypeCheck(msgBodyDeactivated);

                        DeactivateComponent();
                    }
                    return true;                
                default:
                    return false;
            }
        }
    }
}
