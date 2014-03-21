// PhysicsActor.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using BEPUphysics;
using BEPUphysics.Character;
using BEPUphysics.Entities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.Entities;

namespace QuickStart.Physics
{
    /// <summary>
    /// Lets a PhysicsComponent know which type of shape is referenced.
    /// </summary>
    public enum ShapeType
    {
        Invalid = -1,
        Box = 0,
        Sphere,
        Capsule,
        Cylinder,
        Cone,
        Heightfield,
        TriangleMesh,
    }

    public interface IEntityTag
    {
        Int64 EntityID { get; }
    }

    public class EntityTag : IEntityTag
    {
        public EntityTag(Int64 entityID)
        {
            this.entityID = entityID;
        }

        public Int64 EntityID
        {
            get { return this.entityID; }
        }
        private Int64 entityID;
    }

    public class CharacterTag : CharacterSynchronizer, IEntityTag
    {
        public CharacterTag( Int64 entityID, Entity body ) : base(body)
        {
            Debug.Assert(body != null, "You cannot create a character tag without a valid physic body.");

            this.entityID = entityID;
        }

        public Int64 EntityID
        {
            get { return this.entityID; }
        }
        private Int64 entityID;
    }

    /// <summary>
    /// Interface for all physics actors.
    /// </summary>
    public interface IPhysicsActor : IDisposable
    {
        Int64 OwnerEntityID { get; }

        /// <summary>
        /// Gets/sets the density of the actor.
        /// </summary>
        float Density { get; set; }

        /// <summary>
        /// Gets/sets the mass of the actor.
        /// </summary>
        float Mass { get; set; }

        /// <summary>
        /// Gets/sets the position of the actor.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets/set the orientation of the actor.
        /// </summary>
        Matrix Orientation { get; set; }

        Vector3 LinearVelocity { get; set; }

        Vector3 AngularVelocity { get; set; }

        bool AffectedByGravity { get; set; }

        /// <summary>
        /// Gets the list of shapes composing the actor.
        /// </summary>
        List<ShapeDesc> Shapes { get; }
        ISpaceObject SpaceObject { get; }
        ActorType ActorType { get; }

        void EnableCollisionListening();
        void DisableCollisionListening();

        void AddForceFromOutsideSimulation(Vector3 force);

        void UpdateCollisions();

        void AddAdditionalForce(Vector3 force);
        bool HasAdditionalForce();
        void GetAdditionalForce(out Vector3 force);

        void SetMovable(bool movable);

        void ApplyLocalTransform(Vector3 position, Matrix rotation);

        BoundingBox GetBoundingBox();

        bool IsBodyActive();

        /// <summary>
        /// Called whenever this actor's <see cref="QSBody"/> is set to 'Active'
        /// </summary>
        void BodyActivated();

        /// <summary>
        /// Called whenever this actor's <see cref="QSBody"/> is set to 'Inactive'
        /// </summary>
        void BodyDeactivated();

        void ProcessMovementFromInput( CharacterMovementInfo movementData );
    }
}
