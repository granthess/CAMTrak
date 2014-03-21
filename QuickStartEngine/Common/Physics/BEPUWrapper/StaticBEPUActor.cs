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
    /// <summary>
    /// A physics actor that cannot be moved once it is created. This is suitable for static triggers or more complex static mesh shapes, and is
    /// more efficient than a <see cref="BasicBEPUActor"/> in most cases.
    /// </summary>
    public class StaticBEPUActor : BEPUActor
    {
        private StaticCollidable collidable;

        // We're using 'Contains' with these a lot, which has a complexity of O(n), however
        // the number of collisions between objects is probably not a very large number, so this
        // should be cheaper than a HashSet. Also, by making this a list the order of collisions
        // will match the order we send out collision messages.
        private List<Int64> newCollisions;
        private List<Int64> endedCollisions;

        private InitialCollisionDetectedEventHandler<EntityCollidable> listener;
        private CollisionEndedEventHandler<EntityCollidable> endListener;

        /// <summary>
        /// Gets/sets the actor's position.
        /// </summary>
        public override Vector3 Position
        {
            get 
            {
                if (this.body.PhysicsEntity != null)
                {
                    return this.body.PhysicsEntity.Position;
                }

                return Vector3.Zero;
            }
            set 
            {
                // Intentionally left blank
            }
        }

        /// <summary>
        /// Gets/sets the actor's orientation.
        /// </summary>
        public override Matrix Orientation
        {
            get
            {
                if (this.body.PhysicsEntity != null)
                {
                    return Matrix3X3.ToMatrix4X4(this.body.PhysicsEntity.OrientationMatrix);
                }

                return Matrix.Identity;
            }
            set
            {
                // Intentionally left blank
            }
        }

        public override Vector3 LinearVelocity
        {
            get { return Vector3.Zero; }
            set
            {
                // Intentionally left blank
            }
        }

        public override Vector3 AngularVelocity
        {
            get { return Vector3.Zero; }
            set
            {
                // Intentionally left blank
            }
        }

        public override float Density
        {
            get { return 0.0f; }
            set
            {
                // Intentionally left blank
            }
        }

        public override float Mass 
        {
            get { return 0.0f; }
            set
            {
                // Intentionally left blank
            }
        }

        public override bool AffectedByGravity
        {
            get { return false; }
            set
            {
                // Intentionally left blank
            }
        }

        /// <summary>
        /// Constructs a new physics actor.
        /// </summary>
        /// <param name="desc">Descriptor for the actor.</param>
        public StaticBEPUActor( ActorDesc desc ) : base(desc)
        {
            // Build all shapes that make up the actor.
            for(int i = 0; i < desc.Shapes.Count; ++i)
            {
                ShapeDesc shapeDesc = desc.Shapes[i];

                var tag = new EntityTag(this.ownerEntityID);                

                if(shapeDesc is BoxShapeDesc)
                {
                    var boxDesc = shapeDesc as BoxShapeDesc;
                    this.body.PhysicsEntity = new Box(desc.Position, boxDesc.Extents.X, boxDesc.Extents.Y, boxDesc.Extents.Z);                   
                }
                else if(shapeDesc is SphereShapeDesc)
                {
                    var sphereDesc = shapeDesc as SphereShapeDesc;
                    this.body.PhysicsEntity = new Sphere(desc.Position, sphereDesc.Radius);
                }
                else if (shapeDesc is CapsuleShapeDesc)
                {
                    var capDesc = shapeDesc as CapsuleShapeDesc;
                    this.body.PhysicsEntity = new Capsule(desc.Position, capDesc.Length, capDesc.Radius);
                }
                else if (shapeDesc is CylinderShapeDesc)
                {
                    var cylDesc = shapeDesc as CylinderShapeDesc;
                    this.body.PhysicsEntity = new Cylinder(desc.Position, cylDesc.Height, cylDesc.Radius);
                }
                else if (shapeDesc is TriangleMeshShapeDesc)
                {
                    var triDesc = shapeDesc as TriangleMeshShapeDesc;

                    this.collidable = new StaticMesh(triDesc.Vertices.ToArray(), 
                                                     triDesc.Indices.ToArray(),
                                                     new AffineTransform(Quaternion.CreateFromRotationMatrix(desc.Orientation), desc.Position));

                    this.collidable.Tag = tag;
                    this.spaceObject = this.collidable;
                }
                else if (shapeDesc is HeightFieldShapeDesc)
                {
                    throw new Exception("To load terrain physics you must use a TerrainBEPUActor.");
                }
                else
                {
                    throw new Exception("Bad shape.");
                }

                if (null != this.body.PhysicsEntity)
                {
                    SetMovable(false);
                    this.body.PhysicsEntity.Tag = tag;
                    this.body.PhysicsEntity.CollisionInformation.Tag = tag;
                    this.spaceObject = this.body.PhysicsEntity;
                }
            }

            if (this.body.PhysicsEntity != null)
            {
                this.spaceObject = this.body.PhysicsEntity;
                this.body.PhysicsEntity.BecomeKinematic();
                this.body.PhysicsEntity.IsAffectedByGravity = desc.Dynamic;
            }
            
            Debug.Assert(this.spaceObject != null, "A physics entity was not properly created.");
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
            if ( null == otherTag )
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
            // Intentionally left blank 
        }

        public void RemoveFromSimulation()
        {
            if (this.body.PhysicsEntity != null)
            {
                this.body.PhysicsEntity.Space.Remove(this.body.PhysicsEntity);
            }

            if (this.collidable != null)
            {
                this.collidable.Space.Remove(this.collidable);
            }
        }

        public override void SetMovable( bool movable )
        {
            // Intentionally left blank
        }

        public override void AddAdditionalForce( Vector3 force )
        {
            // Intentionally left blank
        }

        public override bool HasAdditionalForce()
        {
            return false;
        }

        public override void GetAdditionalForce( out Vector3 force )
        {
            force = Vector3.Zero;
        }

        public override BoundingBox GetBoundingBox()
        {
            if (null != this.body.PhysicsEntity)
            {
                return this.body.PhysicsEntity.CollisionInformation.BoundingBox;
            }

            if (null != this.collidable)
            {
                return this.collidable.BoundingBox;
            }

            return new BoundingBox();
        }

        public override bool IsBodyActive()
        {
            if (null != this.body.PhysicsEntity)
            {
                return ( this.body.PhysicsEntity.Space != null );
            }

            if (null != this.collidable)
            {
                return this.collidable.Space != null;
            }

            return false;
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
