using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;

using Microsoft.Xna.Framework;

using QuickStart.Entities;

namespace QuickStart.Physics.BEPU
{
    class TerrainBEPUActor : BEPUActor
    {
        private StaticCollidable collidable;
        public StaticCollidable Collidable
        {
            get { return this.collidable; }
        }

        /// <summary>
        /// Gets/sets the actor's position.
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                return (this.collidable.BoundingBox.Min + this.collidable.BoundingBox.Max) * 0.5f;
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
            get { return Matrix.Identity; }
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
            get { return 1.0f; }
            set
            {
                throw new Exception("Density must be altered by changing Mass or Volume.");
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

        public override bool HasAdditionalForce()
        {
            return false;
        }

        public override bool IsBodyActive()
        {
            return false;
        }

        public override void GetAdditionalForce( out Vector3 force )
        {
            force = Vector3.Zero;
        }

        public override BoundingBox GetBoundingBox()
        {
            return this.collidable.BoundingBox;
        }

        public override void EnableCollisionListening() {}
        public override void DisableCollisionListening() {}
        public override void AddForceFromOutsideSimulation( Vector3 force ) {}
        public override void UpdateCollisions() {}
        public override void AddAdditionalForce( Vector3 force ) {}
        public override void SetMovable( bool movable ) {}
        public override void ApplyLocalTransform( Vector3 position, Matrix rotation ) {}
        public override void BodyActivated() {}
        public override void BodyDeactivated() {}
        public override void ProcessMovementFromInput( CharacterMovementInfo movementData ) {}

        /// <summary>
        /// Constructs a new static physics actor.
        /// </summary>
        /// <param name="desc">Descriptor for the actor.</param>
        public TerrainBEPUActor( ActorDesc desc ) : base(desc)
        {
            if ( desc.Shapes.Count != 1 )
            {
                throw new Exception("Terrain actors can only consist of one shape");
            }

            ShapeDesc shapeDesc = desc.Shapes[0];

            if (shapeDesc is HeightFieldShapeDesc)
            {
                // For height fields, we need to copy the data into an Array2D.
                var heightFieldDesc = shapeDesc as HeightFieldShapeDesc;

                float spacing = heightFieldDesc.SizeX / heightFieldDesc.HeightField.GetLength(0);

                this.collidable = new Terrain(heightFieldDesc.HeightField, new AffineTransform( 
                                              new Vector3(spacing, 1, spacing),
                                              Quaternion.Identity,
                                              Vector3.Zero));
            }
            else
            {
                throw new Exception("Shape description for a Terrain actor must be HeightFieldShapeDesc.");
            }

            // Tag the physics with data needed by the engine
            var tag = new EntityTag(this.ownerEntityID);            
            this.collidable.Tag = tag;

            this.spaceObject = this.collidable;
        }
    }
}
