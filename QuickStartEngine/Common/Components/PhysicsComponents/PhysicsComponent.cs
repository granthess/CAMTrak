//
// PhysicsComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    /// <summary>
    /// Creates a physics component, which holds the physics actor for an entity to be used with the PhysicsManager.
    /// </summary>
    public class PhysicsComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.PhysicsComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            PhysicsComponentDefinition compDef = content.Load<PhysicsComponentDefinition>(definitionPath);

            PhysicsComponent newComponent = new PhysicsComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Physics actor holds all the information about the physics object.
        /// </summary>
        public IPhysicsActor PhysicsActor
        {
            get { return this.actor; }
        }
        protected IPhysicsActor actor;

        /// <summary>
        /// This components <seealso cref="ShapeType"/>
        /// </summary>
        public ShapeType ShapeType
        {
            get { return this.shapeType; }
        }
        protected ShapeType shapeType;

        public CollisionGroups CollisionGroupType
        {
            get { return this.collisionGroupType; }
        }
        protected CollisionGroups collisionGroupType;

        /// <summary>
        /// Whether or not this entity's physics are dynamic or not.
        /// </summary>
        public bool IsDynamic
        {
            get { return this.isDynamic; }
        }
        protected bool isDynamic;

        /// <summary>
        /// Mass of this entity's physics.
        /// </summary>
        public float Mass
        {
            get { return this.Mass; }
        }
        protected float mass;

        //@TODO: StaticModel has a lot of extra info made for rendering, we need PhysicsModel which would
        //       require less memory and load faster.
        public StaticModel PhysMesh
        {
            get { return this.physMesh; }
        }
        protected StaticModel physMesh;

        /// <summary>
        /// Height of physics primitive (used by box and capsule shapes). Height represents Y value.
        /// </summary>
        /// <remarks>This is unscaled until physics actor is loaded, then it will
        /// scale according to the parent entity's scale.</remarks>
        public float Height
        {
            get { return this.height; }
        }
        protected float height;

        /// <summary>
        /// Width of physics primitive (used by box shapes). Width represents X value.
        /// </summary>
        /// <remarks>This is unscaled until physics actor is loaded, then it will
        /// scale according to the parent entity's scale.</remarks>
        public float Width
        {
            get { return this.width; }
        }
        protected float width;

        /// <summary>
        /// Depth of physics primitive (used by box shapes).  Depth represents Z value.
        /// </summary>
        /// <remarks>This is unscaled until physics actor is loaded, then it will
        /// scale according to the parent entity's scale.</remarks>
        public float Depth
        {
            get { return this.depth; }
        }
        protected float depth;

        /// <summary>
        /// Diameter of physics primitive (used by sphere and capsule shapes).
        /// </summary>
        /// <remarks>This is unscaled until physics actor is loaded, then it will
        /// scale according to the parent entity's scale.</remarks>
        public float Diameter
        {
            get { return this.diameter; }
        }
        protected float diameter;

        public bool AffectedByGravity
        {
            get { return this.affectedByGravity; }
        }
        protected bool affectedByGravity;

        /// <summary>
        /// When true any changes to the physics position will update the entity position.
        /// </summary>
        public bool UpdatesEntityPosition
        {
            get { return this.updatesEntityPosition; }
        }
        protected bool updatesEntityPosition = true;

        /// <summary>
        /// Sets the physics mesh's position
        /// </summary>
        /// <param name="Position">Position to set physics to</param>
        public void SetPosition( Vector3 Position )
        {
            this.actor.Position = Position;
        }

        /// <summary>
        /// Sets the physics mesh's rotation
        /// </summary>
        /// <param name="Rotation">Rotation to set physics to</param>
        public void SetRotation( Matrix Rotation )
        {
            this.actor.Orientation = Rotation;
        }

        /// <summary>
        /// Create a physics component.
        /// </summary>        
        /// <remarks>This constructor should only be called by derived classes.</remarks>
        protected PhysicsComponent(BaseEntity parent)
            : base(parent)
        {            
        }

        /// <summary>
        /// Create a physics component.
        /// </summary>
        public PhysicsComponent(BaseEntity parent, PhysicsComponentDefinition compDef)
            : base(parent)
        {
            if ( (null != compDef.PhysicsModelPath) && (compDef.PhysicsModelPath.Length > 0) )
            {
                this.physMesh = this.parentEntity.Game.ModelLoader.LoadStaticModel(compDef.PhysicsModelPath);
            }

            this.shapeType = compDef.ShapeType;          
            this.mass = compDef.Mass;
            this.collisionGroupType = compDef.CollisionGroupType;
            this.isDynamic = compDef.IsDynamic;
            this.affectedByGravity = compDef.AffectedByGravity;

            this.height = compDef.Height;
            this.width = compDef.Width;
            this.depth = compDef.Depth;
            this.diameter = compDef.Diameter;

            InitializeActor();

            if (this.shapeType != ShapeType.TriangleMesh
             && this.shapeType != ShapeType.Heightfield
             && this.isDynamic)
            {
                ActivateComponent();
            }
        }

        /// <summary>
        /// Create a physics component
        /// </summary>
        /// <param name="parent">Entity this component will be attached to</param>
        /// <param name="type">Type of shape this object will be</param>
        /// <param name="density">Mass of physics object</param>
        /// <param name="isDynamic">Whether or not the physics will be dynamic (respond to gravity and collision)</param>
        public PhysicsComponent(BaseEntity parent, ShapeType type, float mass, bool isDynamic)
            : base(parent)
        {            
            this.shapeType = type;
            this.mass = mass;
            this.isDynamic = isDynamic;

            this.height = 1.0f;
            this.width = 1.0f;
            this.depth = 1.0f;
            this.diameter = 1.0f;

            InitializeActor();

            if (this.shapeType != ShapeType.TriangleMesh
             && this.shapeType != ShapeType.Heightfield
             && this.isDynamic)
            {
                ActivateComponent();
            }
        }

        /// <summary>
        /// Create a physics component
        /// </summary>
        /// <param name="parent">Entity this component will be attached to</param>
        /// <param name="physicsModelPath">Path of model to use for physics mesh</param>
        /// <param name="mass">Mass of physics object</param>
        /// <param name="isDynamic">Whether or not the physics will be dynamic (respond to gravity and collision)</param>
        /// <remarks>If you want to use the same model for physics as you're using for rendering, then use the constructor that
        /// requires a 'ShapeType' for input, and use ShapeType.TriangleMesh.</remarks>
        public PhysicsComponent(BaseEntity parent, ShapeType type, string physicsModelPath, float mass, bool isDynamic)
            : base(parent)
        {
            if (type != ShapeType.TriangleMesh
                && type != ShapeType.Box
                && type != ShapeType.Sphere)
            {
                throw new Exception("This constructor can only be called with a ShapeType of 'TriangleMesh', 'Box', or 'Sphere'");
            }

            this.shapeType = type;
            this.mass = mass;
            this.isDynamic = isDynamic;

            this.physMesh = this.parentEntity.Game.ModelLoader.LoadStaticModel(physicsModelPath);

            InitializeActor();

            if (this.shapeType != ShapeType.TriangleMesh             
             && this.isDynamic)
            {
                ActivateComponent();
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();

            // We need to tell the physics interface to remove this entity's physics from the world
            var remFromSceneMsg = ObjectPool.Aquire<MsgRemoveEntityFromPhysicsScene>();
            remFromSceneMsg.EntityID = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendInterfaceMessage(remFromSceneMsg, InterfaceType.Physics);
        }


        protected virtual void InitializeActor()
        {
            var getPhysSceneMsg = ObjectPool.Aquire<MsgGetPhysicsScene>();            
            getPhysSceneMsg.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendInterfaceMessage(getPhysSceneMsg, InterfaceType.Physics);

            IPhysicsScene physScene = getPhysSceneMsg.PhysicsScene;
            if (physScene != null)
            {
                CreateActor(physScene);

                if (this.actor != null)
                {
                    var addToSceneMsg = ObjectPool.Aquire<MsgAddEntityToPhysicsScene>();
                    addToSceneMsg.Actor = this.actor;
                    addToSceneMsg.EntityID = this.parentEntity.UniqueID;
                    this.parentEntity.Game.SendInterfaceMessage(addToSceneMsg, InterfaceType.Physics);

                    var setCollGroup = ObjectPool.Aquire<MsgSetActorToCollisionGroup>();
                    setCollGroup.Actor = this.actor;
                    setCollGroup.GroupType = this.collisionGroupType;
                    this.parentEntity.Game.SendInterfaceMessage(setCollGroup, InterfaceType.Physics);
                }
            }
        }

        /// <summary>
        /// Update the component, at a fixed rate in sync with the physics world
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void FixedUpdate(GameTime gameTime)
        {
            if (this.actor != null)
            {
                // If this component's physics controls the position of the entity, then update the entity's
                // position now.
                if (this.updatesEntityPosition)
                {
                    this.parentEntity.UpdateFromPhysics(this.actor.Position, this.actor.Orientation);
                }

                this.actor.UpdateCollisions();
            }
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        protected virtual void CreateActor(IPhysicsScene PhysicsScene)
        {
            ShapeDesc newShape = CreateShapeFromType(shapeType);

            if (newShape == null)
            {
                throw new Exception("Shape did not load properly");
            }

            ActorDesc desc = new ActorDesc();
            desc.Orientation = this.parentEntity.Rotation;
            desc.Mass = this.mass;
            desc.Dynamic = this.isDynamic;
            desc.AffectedByGravity = this.affectedByGravity;
            desc.Position = this.parentEntity.Position;
            desc.Shapes.Add(newShape);
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Game = this.parentEntity.Game;
            desc.Type = ActorType.Basic;

            if (newShape is TriangleMeshShapeDesc)
            {
                var triDesc = newShape as TriangleMeshShapeDesc;
                if (triDesc.Vertices.Count > 100)
                {
                    desc.Type = ActorType.Static;
                }
            }

            this.actor = PhysicsScene.CreateActor(desc);
        }        

        /// <summary>
        /// Create a shape from a <seealso cref="ShapeType"/> and the BaseEntity information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected ShapeDesc CreateShapeFromType(ShapeType type)
        {
            float scale = this.parentEntity.Scale;

            this.height *= scale;
            this.width *= scale;
            this.depth *= scale;
            this.diameter *= scale;

            switch (type)
            {
                case ShapeType.Box:
                    {
                        if (null == this.physMesh)
                        {                            
                            var shape = new BoxShapeDesc();
                            shape.Extents = new Vector3(this.width, this.height, this.depth);
                            return shape;
                        }
                        else
                        {
                            return CreateBoxShapeFromMesh(this.physMesh, scale);                            
                        }                        
                    }
                case ShapeType.Sphere:
                    {
                        if (null == this.physMesh)
                        {
                            var shape = new SphereShapeDesc();
                            shape.Radius = this.diameter / 2.0f;
                            return shape;
                        }
                        else
                        {
                            return CreateSphereShapeFromMesh(this.physMesh, scale);
                        }
                    }
                case ShapeType.Heightfield:
                    {
                        // Unsupported by this method, use CreateHeightfieldShape()
                        return null;
                    }
                case ShapeType.Capsule:
                    {
                        var shape = new CapsuleShapeDesc();
                        shape.Radius = this.diameter / 2.0f;
                        shape.Length = this.height;
                        return shape;
                    }
                case ShapeType.Cylinder:
                    {
                        var shape = new CylinderShapeDesc();
                        shape.Height = this.height;
                        shape.Radius = this.diameter / 2.0f;
                        return shape;
                    }
                case ShapeType.Cone:
                    {
                        var shape = new ConeShapeDesc();
                        shape.Height = this.height;
                        shape.Radius = this.diameter / 2.0f;
                        return shape;
                    }
                case ShapeType.TriangleMesh:
                    {
                        if (isDynamic)
                        {
                            throw new Exception("Triangle Mesh shapes do not support dynamic physics");
                        }

                        TriangleMeshShapeDesc shape = new TriangleMeshShapeDesc();

                        if (this.physMesh == null)
                        {
                            MsgGetModelVertices msgGetVerts = ObjectPool.Aquire<MsgGetModelVertices>();                            
                            msgGetVerts.UniqueTarget = this.parentEntity.UniqueID;
                            this.parentEntity.Game.SendMessage(msgGetVerts);

                            shape.Vertices = msgGetVerts.Vertices;

                            MsgGetModelIndices msgGetInds = ObjectPool.Aquire<MsgGetModelIndices>();
                            msgGetInds.UniqueTarget = this.parentEntity.UniqueID;
                            this.parentEntity.Game.SendMessage(msgGetInds);

                            shape.Indices = msgGetInds.Indices;
                        }
                        else
                        {
                            shape.Vertices = physMesh.GetModelVertices(this.parentEntity.Scale);
                            shape.Indices = physMesh.GetModelIndices();
                        }

                        if ((shape.Vertices.Count == 0) || (shape.Indices.Count == 0))
                            return null;

                        return shape;
                    }
                default:
                    // Throw exception
                    return null;
            };
        }

        static public BoxShapeDesc CreateBoxShapeFromMesh(StaticModel mesh, float scale)
        {
            List<Vector3> verts = mesh.GetModelVertices(scale);

            Vector3 negExtent = Vector3.Zero;
            Vector3 posExtent = Vector3.Zero;

            foreach (Vector3 vertex in verts)
            {
                if (vertex.X < negExtent.X)
                {
                    negExtent.X = vertex.X;
                }
                
                if (vertex.X > posExtent.X)
                {
                    posExtent.X = vertex.X;
                }
                
                if (vertex.Y < negExtent.Y)
                {
                    negExtent.Y = vertex.Y;
                }
                
                if (vertex.Y > posExtent.Y)
                {
                    posExtent.Y = vertex.Y;
                }

                if (vertex.Z < negExtent.Z)
                {
                    negExtent.Z = vertex.Z;
                }
                
                if (vertex.Z > posExtent.Z)
                {
                    posExtent.Z = vertex.Z;
                }
            }

            BoxShapeDesc shape = new BoxShapeDesc();
            shape.Extents = (posExtent - negExtent);

            return shape;
        }

        static public SphereShapeDesc CreateSphereShapeFromMesh(StaticModel mesh, float scale)
        {
            List<Vector3> verts = mesh.GetModelVertices(scale);

            float largestSquareLength = 0.0f;

            foreach (Vector3 vertex in verts)
            {
                float vertSqrLng = vertex.LengthSquared();
                if (vertSqrLng > largestSquareLength)
                {
                    largestSquareLength = vertSqrLng;
                }
            }

            SphereShapeDesc shape = new SphereShapeDesc();
            shape.Radius = (float)Math.Sqrt(largestSquareLength);

            return shape;
        }

        protected virtual void BodyActivated()
        {
            ActivateComponent();
        }

        protected virtual void BodyDeactivated()
        {
            DeactivateComponent();
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
                case MessageType.SetPosition:
                    {
                        var msgSetPos = message as MsgSetPosition;
                        message.TypeCheck(msgSetPos);

                        SetPosition(msgSetPos.Position);
                    }
                    return true;
                case MessageType.ModifyPosition:
                    {
                        var msgModPos = message as MsgModifyPosition;
                        message.TypeCheck(msgModPos);

                        SetPosition(this.actor.Position + msgModPos.Position);
                    }
                    return true;                
                case MessageType.SetRotation:
                    {
                        var setRotMsg = message as MsgSetRotation;
                        message.TypeCheck(setRotMsg);

                        SetRotation(setRotMsg.Rotation);
                    }
                    return true;
                case MessageType.ModifyRotation:
                    {
                        var modRotMsg = message as MsgModifyRotation;
                        message.TypeCheck(modRotMsg);

                        SetRotation(this.actor.Orientation *= modRotMsg.Rotation);
                    }
                    return true;
                case MessageType.SetLinearVelocity:
                    {
                        var setLinVelMsg = message as MsgSetLinearVelocity;
                        message.TypeCheck(setLinVelMsg);

                        this.actor.LinearVelocity = setLinVelMsg.LinearVelocity;
                    }
                    return true;
                case MessageType.GetLinearVelocity:
                    {
                        var getLinVelMsg = message as MsgGetLinearVelocity;
                        message.TypeCheck(getLinVelMsg);

                        getLinVelMsg.LinearVelocity = this.actor.LinearVelocity;
                    }
                    return true;
                case MessageType.SetAngularVelocity:
                    {
                        var setAngVelMsg = message as MsgSetAngularVelocity;
                        message.TypeCheck(setAngVelMsg);

                        this.actor.AngularVelocity = setAngVelMsg.AngularVelocity;
                    }
                    return true;
                case MessageType.GetAngularVelocity:
                    {
                        var getAngVelMsg = message as MsgGetAngularVelocity;
                        message.TypeCheck(getAngVelMsg);

                        getAngVelMsg.AngularVelocity = this.actor.AngularVelocity;
                    }
                    return true;
                case MessageType.AddLinearForce:
                    {
                        var addForceMsg = message as MsgAddLinearForce;
                        message.TypeCheck(addForceMsg);

                        // We do not handle this message for static physics types
                        if (this.shapeType == ShapeType.TriangleMesh
                         || this.shapeType == ShapeType.Heightfield)
                        {
                            return true;
                        }

                        if (addForceMsg.LinearVelocity.LengthSquared() > 0.0f)
                        {
                            this.actor.LinearVelocity += addForceMsg.LinearVelocity;
                        }
                    }
                    return true;
                case MessageType.AddAngularForce:
                    {
                        var addForceMsg = message as MsgAddAngularForce;
                        message.TypeCheck(addForceMsg);

                        this.actor.AngularVelocity += addForceMsg.AngularVelocity;
                    }
                    return true;
                case MessageType.ListenForCollision:
                    {
                        var msgListenColl = message as MsgListenForCollision;
                        message.TypeCheck(msgListenColl);

                        if (msgListenColl.ListenForCollisions == true)
                        {
                            this.actor.EnableCollisionListening();
                        }
                        else
                        {
                            this.actor.DisableCollisionListening();
                        }
                    }
                    return true;
                case MessageType.GetHasDynamicPhysics:
                    {
                        var msgGetDynPhys = message as MsgGetHasDynamicPhysics;
                        message.TypeCheck(msgGetDynPhys);

                        msgGetDynPhys.HasDynamicPhysics = this.isDynamic;
                    }
                    return true;
                case MessageType.SetPhysicsMovableState:
                    {
                        var msgSetDynPhys = message as MsgSetPhysicsMovableState;
                        message.TypeCheck(msgSetDynPhys);

                        if (this.actor == null)
                        {
                            return false;
                        }

                        // We only attempt this on shapes that are able to be dynamic
                        if (this.shapeType != ShapeType.TriangleMesh
                         && this.shapeType != ShapeType.Heightfield)
                        {
                            this.actor.SetMovable(msgSetDynPhys.Movable);
                            this.isDynamic = msgSetDynPhys.Movable; // TODO: Disable this for characters.
                            this.affectedByGravity = msgSetDynPhys.Movable;
                        }                                                
                    }
                    return true;
                case MessageType.GetDensity:
                    {
                        var msgGetDensity = message as MsgGetDensity;
                        message.TypeCheck(msgGetDensity);

                        msgGetDensity.Density = this.actor.Density;
                    }
                    return true;
                case MessageType.GetPhysicsBoundingBox:
                    {
                        var msgGetBox = message as MsgGetPhysicsBoundingBox;
                        message.TypeCheck(msgGetBox);

                        if (this.actor == null)
                            return false;

                        msgGetBox.Box = this.actor.GetBoundingBox();
                    }
                    return true;
                case MessageType.AddExternalForce:
                    {
                        var msgAddForce = message as MsgAddExternalForce;
                        message.TypeCheck(msgAddForce);

                        if (this.actor == null)
                            return false;

                        this.actor.AddAdditionalForce(msgAddForce.ExternalForce);
                    }
                    return true;
                case MessageType.GetIsInMotion:
                    {
                        var msgInMotion = message as MsgGetIsInMotion;
                        message.TypeCheck(msgInMotion);

                        if (this.actor == null
                         || !this.isDynamic
                         || !this.actor.IsBodyActive())
                        {
                            msgInMotion.InMotion = false;                            
                        }
                        else if (this.actor.IsBodyActive())
                        {
                            msgInMotion.InMotion = true;
                        }                        
                    }
                    return true;
                case MessageType.BodyActivated:
                    {
                        var msgBodyActive = message as MsgPhysicsBodyActivated;
                        message.TypeCheck(msgBodyActive);

                        BodyActivated();
                    }
                    return true;
                case MessageType.BodyDeactivated:
                    {
                        var msgBodyDeactivated = message as MsgPhysicsBodyDeactivated;
                        message.TypeCheck(msgBodyDeactivated);

                        BodyDeactivated();
                    }
                    return true;
                case MessageType.GetAffectedByGravity:
                    {
                        var msgGetGrav = message as MsgGetAffectedByGravity;
                        message.TypeCheck(msgGetGrav);

                        msgGetGrav.AffectedByGravity = this.affectedByGravity;                      
                    }
                    return true;
                case MessageType.SetAffectedByGravity:
                    {
                        var msgSetGrav = message as MsgSetAffectedByGravity;
                        message.TypeCheck(msgSetGrav);

                        this.affectedByGravity = msgSetGrav.AffectedByGravity;
                        this.actor.AffectedByGravity = this.affectedByGravity;
                    }
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Allows the renderer to query this component for render information.
        /// </summary>
        /// <param name="desc">Descriptor reference from the renderer</param>
        /// <remarks>This is used to render physics information. Only meant for debug use.</remarks>
        public override void QueryForChunks(ref RenderPassDesc desc)
        {
#if WINDOWS            
            if (this.parentEntity.Game.PhysicsRenderer.Enabled)
            {
                // Do not render this physics mesh's verts if it isn't in view.
                if (!desc.RenderCamera.ViewFrustum.Intersects(this.actor.GetBoundingBox()))
                    return;                
            }
#endif //WINDOWS
        }
    }
}
