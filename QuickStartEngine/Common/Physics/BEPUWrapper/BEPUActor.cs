//
// JigLibXActor.cs
//
// This file is part of the QuickStart Engine's Wrapper to JigLibX. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.MathExtensions;

using QuickStart;
using QuickStart.Physics;
using QuickStart.Entities;

namespace QuickStart.Physics.BEPU
{
    /// <summary>
    /// An implementation of the <see cref="IPhysicsActor"/> interface using BEPU.
    /// </summary>
    public abstract class BEPUActor : IPhysicsActor
    {
        protected QSBody body;

        protected ISpaceObject spaceObject;
        public ISpaceObject SpaceObject
        {
            get { return this.spaceObject; }
            set { this.spaceObject = value; }
        }

        private ActorType actorType;
        public ActorType ActorType
        {
            get { return this.actorType; }
        }
        
        /// <summary>
        /// Gets the list of shapes that make up the actor.
        /// </summary>
        public List<ShapeDesc> Shapes
        {
            get { return shapes; }
        }
        protected List<ShapeDesc> shapes;

        /// <summary>
        /// EntityID attached to this physics actor
        /// </summary>
        protected Int64 ownerEntityID;
        public Int64 OwnerEntityID
        {
            get { return this.ownerEntityID; }
        }

        /// <summary>
        /// Reference to the game.
        /// </summary>
        protected QSGame game;
        public QSGame Game
        {
            get { return this.game; }
        }

        /// <summary>
        /// Constructs a new physics actor.
        /// </summary>
        /// <param name="desc">Descriptor for the actor.</param>
        internal BEPUActor( ActorDesc desc )
        {
            this.actorType = desc.Type;
            this.shapes = desc.Shapes;           
            this.ownerEntityID = desc.EntityID;
            this.game = desc.Game;

            this.body = new QSBody(this);
        }

        /// <summary>
        /// Releases all unmanaged resources for the actor.
        /// </summary>
        public void Dispose()
        {
        }

        public abstract float Density { get; set; }
        public abstract float Mass { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract Matrix Orientation { get; set; }
        public abstract Vector3 LinearVelocity { get; set; }
        public abstract Vector3 AngularVelocity { get; set; }
        public abstract bool AffectedByGravity { get; set; }
        public abstract void EnableCollisionListening();
        public abstract void DisableCollisionListening();
        public abstract void AddForceFromOutsideSimulation( Vector3 force );
        public abstract void UpdateCollisions();
        public abstract void AddAdditionalForce( Vector3 force );
        public abstract bool HasAdditionalForce();
        public abstract void GetAdditionalForce( out Vector3 force );
        public abstract void SetMovable( bool movable );
        public abstract void ApplyLocalTransform( Vector3 position, Matrix rotation );
        public abstract BoundingBox GetBoundingBox();
        public abstract bool IsBodyActive();
        public abstract void BodyActivated();
        public abstract void BodyDeactivated();
        public abstract void ProcessMovementFromInput( CharacterMovementInfo movementData );
    }
}
