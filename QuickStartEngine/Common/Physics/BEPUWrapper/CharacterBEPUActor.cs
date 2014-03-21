using System;
using System.Collections.Generic;
using System.Diagnostics;

using BEPUphysics.Character;
using BEPUphysics.Collidables;
using BEPUphysics.Collidables.Events;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;
using BEPUphysics.NarrowPhaseSystems.Pairs;

using Microsoft.Xna.Framework;

using QuickStart.Entities;
using QuickStart.Interfaces;

namespace QuickStart.Physics.BEPU
{
    public class CharacterBEPUActor : BEPUActor
    {
        private CharacterController character;
        private Cylinder characterBody;

        private List<Int64> newCollisions;
        private List<Int64> endedCollisions;
        private InitialCollisionDetectedEventHandler<EntityCollidable> listener;
        private CollisionEndedEventHandler<EntityCollidable> endListener;

        /// <summary>
        /// Gets/sets the actor's position.
        /// </summary>
        public override Vector3 Position
        {
            get { return this.characterBody.Position; }
            set { this.characterBody.Position = value; }
        }

        /// <summary>
        /// Gets/sets the actor's orientation.
        /// </summary>
        public override Matrix Orientation
        {
            get { return Matrix3X3.ToMatrix4X4(this.characterBody.OrientationMatrix); }
            set { this.characterBody.OrientationMatrix = Matrix3X3.CreateFromMatrix(value); }
        }

        public override Vector3 LinearVelocity
        {
            get { return this.characterBody.LinearVelocity; }
            set { this.characterBody.LinearVelocity = value; }
        }

        public override Vector3 AngularVelocity
        {
            get { return this.characterBody.AngularVelocity; }
            set { this.characterBody.AngularVelocity = value; }
        }

        public override float Density
        {
            get { return (this.characterBody.Mass / this.characterBody.Volume); }
            set { Debug.Assert(false, "Density must be altered by changing Mass or Volume."); }
        }

        public override float Mass 
        {
            get { return this.characterBody.Mass; }
            set { this.characterBody.Mass = value; }
        }

        public override bool AffectedByGravity 
        {
            get { return this.characterBody.IsAffectedByGravity; }
            set { this.characterBody.IsAffectedByGravity = value; }
        }

        /// <summary>
        /// Constructs a new physics actor.
        /// </summary>
        /// <param name="desc">Descriptor for the actor.</param>
        public CharacterBEPUActor( ActorDesc desc ) : base(desc)
        {
            character = new CharacterController(desc.Position, 17f, 17f * 0.6f, 6.0f, 4500.0f);
            characterBody = character.Body;

            // Tag the physics with data needed by the engine
            var tag = new CharacterTag(this.ownerEntityID, characterBody);
            this.characterBody.Tag = tag;
            this.characterBody.CollisionInformation.Tag = tag;

            this.Position = desc.Position;
            this.Orientation = desc.Orientation;

            Debug.Assert(this.characterBody != null, "A physics entity was not properly created.");

            this.spaceObject = this.character;
            
            this.characterBody.IsAffectedByGravity = desc.AffectedByGravity;

#if WINDOWS
            // We add the character's cylinder body to the physics viewer.
            var msgAddPhys = ObjectPool.Aquire<MsgAddPhysicsToModelViewer>();
            msgAddPhys.SpaceObject = this.characterBody;
            this.game.SendInterfaceMessage(msgAddPhys, InterfaceType.Physics);
#endif //WINDOW
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
            this.characterBody.CollisionInformation.Events.InitialCollisionDetected += this.listener;
            this.characterBody.CollisionInformation.Events.CollisionEnded += this.endListener;
        }

        public override void DisableCollisionListening()
        {
            // If listener is enabled
            if (this.listener == null)
            {
                return;
            }

            this.characterBody.CollisionInformation.Events.InitialCollisionDetected -= this.listener;
            this.characterBody.CollisionInformation.Events.CollisionEnded -= this.endListener;

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
            Vector3 momentum = this.characterBody.LinearMomentum;
            momentum.X += force.X;
            momentum.Y += force.Y;
            momentum.Z += force.Z;
            this.characterBody.LinearMomentum = momentum;
        }

        public void RemoveFromSimulation()
        {
            this.characterBody.Space.Remove(this.characterBody);
        }

        public override void SetMovable( bool movable )
        {
            if (!movable)
            {
                this.characterBody.BecomeKinematic();
            }
            else
            {
                this.characterBody.BecomeDynamic(this.characterBody.Mass);
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
            if (this.characterBody != null)
            {
                return this.characterBody.CollisionInformation.BoundingBox;
            }
            else
            {
                return new BoundingBox();
            }
        }

        public override bool IsBodyActive()
        {
            return ( this.characterBody.Space != null );
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

        public override void ProcessMovementFromInput( CharacterMovementInfo movementData ) 
        {
            Vector3 totalMovement = Vector3.Zero;
            Vector3 movementDir;
            float movementSpeed = 1.0f;

            // Grab the current camera's ID
            var camMsg = ObjectPool.Aquire<MsgGetRenderEntity>();
            this.game.SendInterfaceMessage(camMsg, InterfaceType.Camera);

            // Get the current camera's rotation
            var msgGetRot = ObjectPool.Aquire<MsgGetRotation>();
            msgGetRot.UniqueTarget = camMsg.EntityID;
            this.game.SendMessage(msgGetRot);

            if (movementData.forward > 0.0f)
            {
                movementDir = msgGetRot.Rotation.Forward;
                totalMovement += movementDir;
            }
            else if (movementData.forward < 0.0f)
            {
                movementDir = msgGetRot.Rotation.Forward;
                totalMovement -= movementDir;
            }

            if (movementData.right < 0.0f)
            {
                movementDir = msgGetRot.Rotation.Left;
                totalMovement += movementDir;
            }
            else if (movementData.right > 0.0f)
            {
                movementDir = msgGetRot.Rotation.Right;
                totalMovement += movementDir;
            }

            if (totalMovement == Vector3.Zero)
            {
                character.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
            }
            else
            {
                totalMovement.Y = 0.0f;
                totalMovement.Normalize();

                Matrix rot = msgGetRot.Rotation;
                rot.Forward = totalMovement;
                rot.Up = Vector3.Up;
                rot.Right = Vector3.Cross(rot.Forward, rot.Up);

                var msgSetRot = ObjectPool.Aquire<MsgSetRotation>();
                msgSetRot.Rotation = rot;
                msgSetRot.UniqueTarget = this.ownerEntityID;
                this.game.SendMessage(msgSetRot);

                character.HorizontalMotionConstraint.MovementDirection = new Vector2(totalMovement.X, totalMovement.Z);

                var msgGetVel = ObjectPool.Aquire<MsgGetLinearVelocity>();
                msgGetVel.UniqueTarget = this.ownerEntityID;
                this.game.SendMessage(msgGetVel);

                if (msgGetVel.LinearVelocity.LengthSquared() <= 400.0f)
                {
                    var msgSetVel = ObjectPool.Aquire<MsgAddLinearForce>();
                    msgSetVel.LinearVelocity = totalMovement * movementSpeed;
                    msgSetVel.UniqueTarget = this.ownerEntityID;
                    this.game.SendMessage(msgSetVel);
                }
            }

            character.StanceManager.DesiredStance = movementData.crouching ? Stance.Crouching : Stance.Standing;

            // Jumping
            if (movementData.wantsJump)
            {
                character.Jump();
            }
        }
    }
}
