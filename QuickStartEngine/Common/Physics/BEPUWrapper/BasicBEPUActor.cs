using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Collidables.Events;
using BEPUphysics.CollisionShapes;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities;

namespace QuickStart.Physics.BEPU
{
    public class BasicBEPUActor : BEPUActor
    {
        private List<Int64> newCollisions;
        private List<Int64> endedCollisions;
        private InitialCollisionDetectedEventHandler<EntityCollidable> listener;
        private CollisionEndedEventHandler<EntityCollidable> endListener;

        /// <summary>
        /// Gets/sets the actor's position.
        /// </summary>
        public override Vector3 Position
        {
            get { return this.body.PhysicsEntity.Position; }
            set { this.body.PhysicsEntity.Position = value; }
        }

        /// <summary>
        /// Gets/sets the actor's orientation.
        /// </summary>
        public override Matrix Orientation
        {
            get { return Matrix3X3.ToMatrix4X4(this.body.PhysicsEntity.OrientationMatrix); }
            set { this.body.PhysicsEntity.OrientationMatrix = Matrix3X3.CreateFromMatrix(value); }
        }

        public override Vector3 LinearVelocity
        {
            get { return this.body.PhysicsEntity.LinearVelocity; }
            set { this.body.PhysicsEntity.LinearVelocity = value; }
        }

        public override Vector3 AngularVelocity
        {
            get { return this.body.PhysicsEntity.AngularVelocity; }
            set { this.body.PhysicsEntity.AngularVelocity = value; }
        }

        public override float Density
        {
            get { return (this.body.PhysicsEntity.Mass / this.body.PhysicsEntity.Volume); }
            set { Debug.Assert(false, "Density must be altered by changing Mass or Volume."); }
        }

        public override float Mass 
        {
            get { return this.body.PhysicsEntity.Mass; }
            set { this.body.PhysicsEntity.Mass = value; }
        }

        public override bool AffectedByGravity 
        {
            get { return this.body.PhysicsEntity.IsAffectedByGravity; }
            set { this.body.PhysicsEntity.IsAffectedByGravity = value; }
        }

        /// <summary>
        /// Constructs a new physics actor.
        /// </summary>
        /// <param name="desc">Descriptor for the actor.</param>
        public BasicBEPUActor( ActorDesc desc ) : base(desc)
        {
            // Build all shapes that make up the actor.
            for(int i = 0; i < desc.Shapes.Count; ++i)
            {
                ShapeDesc shapeDesc = desc.Shapes[i];

                if(shapeDesc is BoxShapeDesc)
                {
                    var boxDesc = shapeDesc as BoxShapeDesc;
                    this.body.PhysicsEntity = new Box(desc.Position, boxDesc.Extents.X, boxDesc.Extents.Y, boxDesc.Extents.Z, desc.Mass);                 
                }
                else if(shapeDesc is SphereShapeDesc)
                {
                    var sphereDesc = shapeDesc as SphereShapeDesc;
                    this.body.PhysicsEntity = new Sphere(desc.Position, sphereDesc.Radius, desc.Mass);
                }
                else if (shapeDesc is CapsuleShapeDesc)
                {
                    var capDesc = shapeDesc as CapsuleShapeDesc;
                    this.body.PhysicsEntity = new Capsule(desc.Position, capDesc.Length, capDesc.Radius, desc.Mass);
                }
                else if (shapeDesc is CylinderShapeDesc)
                {
                    var cylDesc = shapeDesc as CylinderShapeDesc;
                    this.body.PhysicsEntity = new Cylinder(desc.Position, cylDesc.Height, cylDesc.Radius, desc.Mass);
                }
                else if (shapeDesc is ConeShapeDesc)
                {
                    var coneDesc = shapeDesc as ConeShapeDesc;
                    this.body.PhysicsEntity = new Cone(desc.Position, coneDesc.Height, coneDesc.Radius, desc.Mass);
                    this.body.PhysicsEntity.CollisionInformation.LocalPosition = new Vector3(0.0f, coneDesc.Height * -0.25f, 0.0f);
                }
                else if (shapeDesc is TriangleMeshShapeDesc)
                {
                    var triDesc = shapeDesc as TriangleMeshShapeDesc;

                    if (triDesc.Vertices.Count > 100)
                    {
                        throw new ArgumentException("For TriangleMeshShapes with over 100 vertices you must use a StaticBEPUActor. It takes too long to generate a convex hull with more vertices than that.");
                    }

                    this.body.PhysicsEntity = new ConvexHull(triDesc.Vertices, desc.Mass);
                }
                else if (shapeDesc is HeightFieldShapeDesc)
                {
                    throw new Exception("To load terrain physics you must use a TerrainBEPUActor.");
                }
                else
                {
                    throw new Exception("Bad shape.");
                }

                // Tag the physics with data needed by the engine
                var tag = new EntityTag(this.ownerEntityID);                
                this.body.PhysicsEntity.Tag = tag;
                this.body.PhysicsEntity.CollisionInformation.Tag = tag;
            }

            Debug.Assert(this.body.PhysicsEntity != null, "A physics entity was not properly created.");

            this.spaceObject = this.body.PhysicsEntity;

            SetMovable(desc.Dynamic);

            this.body.PhysicsEntity.IsAffectedByGravity = desc.AffectedByGravity;
        }

        public override void ApplyLocalTransform( Vector3 position, Matrix rotation )
        {
            this.Position = position;
            this.Orientation = rotation;
        }

        public override void EnableCollisionListening()
        {
            // If listener isn't already enabled
            if (this.listener != null)
            {
                return;
            }

            this.newCollisions = new List<Int64>();
            this.endedCollisions = new List<Int64>();

            this.listener = new InitialCollisionDetectedEventHandler<EntityCollidable>(CollisionHandler);
            this.endListener = new CollisionEndedEventHandler<EntityCollidable>(CollisionEndHandler);
            this.body.PhysicsEntity.CollisionInformation.Events.InitialCollisionDetected += this.listener;
            this.body.PhysicsEntity.CollisionInformation.Events.CollisionEnded += this.endListener;
        }

        public override void DisableCollisionListening()
        {
            // If listener is enabled
            if (this.listener == null)
            {
                return;
            }

            this.body.PhysicsEntity.CollisionInformation.Events.InitialCollisionDetected -= this.listener;
            this.body.PhysicsEntity.CollisionInformation.Events.CollisionEnded -= this.endListener;

            this.listener = null;

            BroadcastCollisions();

            this.newCollisions.Clear();
            this.newCollisions = null;

            this.endedCollisions.Clear();
            this.endedCollisions = null;
        }

        public void CollisionHandler( EntityCollidable sender, Collidable other, CollidablePairHandler collisionPair )
        {
            var otherTag = other.Tag as IEntityTag;
            if (null == otherTag)
            {
                return;
            }

            if (!this.newCollisions.Contains(otherTag.EntityID))
            {
                this.newCollisions.Add(otherTag.EntityID);
            }
        }

        public void CollisionEndHandler( EntityCollidable sender, Collidable other, CollidablePairHandler collisionPair )
        {
            var otherTag = other.Tag as IEntityTag;
            if (null == otherTag)
            {
                return;
            }

            if (!this.endedCollisions.Contains(otherTag.EntityID))
            {
                this.endedCollisions.Add(otherTag.EntityID);
            }
        }

        private void BroadcastCollisions()
        {
            foreach (Int64 entityID in this.newCollisions)
            {
                // If there is a collision that wasn't there last frame, send out a message
                var msgCollData = ObjectPool.Aquire<MsgOnCollision>();
                msgCollData.EntityID = entityID;
                msgCollData.UniqueTarget = this.OwnerEntityID;
                this.game.SendMessage(msgCollData);
            }
            this.newCollisions.Clear();

            foreach (Int64 entityID in this.endedCollisions)
            {
                // If there is no longer a collision that was there last frame, send out a message.
                var msgCollData = ObjectPool.Aquire<MsgOffCollision>();
                msgCollData.EntityID = entityID;
                msgCollData.UniqueTarget = this.OwnerEntityID;
                this.game.SendMessage(msgCollData);
            }
            this.endedCollisions.Clear();
        }

        public override void UpdateCollisions()
        {
            if (listener == null)
            {
                return;
            }

            if (this.newCollisions.Count == 0 && this.endedCollisions.Count == 0)
            {
                return;
            }

            BroadcastCollisions();
        }

        public override void AddForceFromOutsideSimulation( Vector3 force )
        {
            Vector3 momentum = this.body.PhysicsEntity.LinearMomentum;
            momentum.X += force.X;
            momentum.Y += force.Y;
            momentum.Z += force.Z;
            this.body.PhysicsEntity.LinearMomentum = momentum;
        }

        public void RemoveFromSimulation()
        {
            this.body.PhysicsEntity.Space.Remove(this.body.PhysicsEntity);
        }

        public override void SetMovable( bool movable )
        {
            if (!movable)
            {
                this.body.PhysicsEntity.BecomeKinematic();
            }
            else
            {
                this.body.PhysicsEntity.BecomeDynamic(this.body.PhysicsEntity.Mass);
            }
        }

        public override void AddAdditionalForce( Vector3 force )
        {
            this.body.AddAdditionalForce(ref force);
        }

        public override bool HasAdditionalForce()
        {
            return ( this.body.AdditionalForce != Vector3.Zero );
        }

        public override void GetAdditionalForce( out Vector3 force )
        {
            force = this.body.AdditionalForce;
        }

        public override BoundingBox GetBoundingBox()
        {
            if (this.body.PhysicsEntity != null)
            {
                return this.body.PhysicsEntity.CollisionInformation.BoundingBox;
            }
            else
            {
                return new BoundingBox();
            }
        }

        public override bool IsBodyActive()
        {
            return (this.body.PhysicsEntity.Space != null);
        }

        /// <summary>
        /// Called whenever this actor's <see cref="QSBody"/> is set to 'Active'
        /// </summary>
        public override void BodyActivated()
        {
            var msgBodyActive = ObjectPool.Aquire<MsgPhysicsBodyActivated>();
            msgBodyActive.UniqueTarget = this.ownerEntityID;
            this.game.SendMessage(msgBodyActive);
        }

        /// <summary>
        /// Called whenever this actor's <see cref="QSBody"/> is set to 'Inactive'
        /// </summary>
        public override void BodyDeactivated()
        {
            var msgBodyDeactivated = ObjectPool.Aquire<MsgPhysicsBodyDeactivated>();
            msgBodyDeactivated.UniqueTarget = this.ownerEntityID;
            this.game.SendMessage(msgBodyDeactivated);
        }

        public override void ProcessMovementFromInput( CharacterMovementInfo movementData ) {}
    }
}
