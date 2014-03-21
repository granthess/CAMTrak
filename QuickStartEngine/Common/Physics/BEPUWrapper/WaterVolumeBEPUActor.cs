using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using BEPUphysics.UpdateableSystems;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Physics.BEPU;

namespace QuickStart.Physics
{
    public class WaterVolumeBEPUActor : BEPUActor
    {
        private FluidVolume fluidVolume;
        public FluidVolume FluidVolume
        {
            get { return this.fluidVolume; }
        }

        private Vector3 volumeExtents = Vector3.Zero;
        
        /// <summary>
        /// Gets/sets the actor's position.
        /// </summary>
        public override Vector3 Position
        {
            get { return this.position; }
            set
            {
                this.position = value;

                var tris = new List<Vector3[]>();
                float basinWidth = this.volumeExtents.X;
                float basinLength = this.volumeExtents.Z;
                float waterHeight = value.Y;                

                this.fluidVolume.SurfaceTriangles[0] = new[]
                             {
                                 value, new Vector3(basinWidth, 0, 0) + value,
                                 new Vector3(0, 0, basinLength) + value
                             };
                this.fluidVolume.SurfaceTriangles[1] = new[]
                             {
                                 new Vector3(0, 0, basinLength) + value, new Vector3(basinWidth, 0, 0) + value,
                                 new Vector3(basinWidth, 0, basinLength) + value
                             };

                this.fluidVolume.MaxDepth = value.Y;
            }
        }
        private Vector3 position = Vector3.Zero;

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
            get { return this.fluidVolume.Density; }
            set { this.fluidVolume.Density = value; }
        }

        public override float Mass
        {
            get 
            { 
                BoundingBox box = this.fluidVolume.BoundingBox;
                Vector3 extents = ( box.Max - box.Min );
                return ( extents.X * extents.Y * extents.Z * this.fluidVolume.Density );
            }
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
            return this.fluidVolume.BoundingBox;
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
        public WaterVolumeBEPUActor( ActorDesc desc ) : base(desc)
        {
            if ( desc.Shapes.Count != 1 )
            {
                throw new Exception("Water volume actors can only consist of one shape");
            }

            ShapeDesc shapeDesc = desc.Shapes[0];

            if (shapeDesc is BoxShapeDesc)
            {
                var boxDesc = shapeDesc as BoxShapeDesc;

                this.volumeExtents = boxDesc.Extents;

                var tris = new List<Vector3[]>();
                float basinWidth = boxDesc.Extents.X;
                float basinLength = boxDesc.Extents.Z;
                float waterHeight = desc.Position.Y;
                this.position = desc.Position;

                //Remember, the triangles composing the surface need to be coplanar with the surface.  In this case, this means they have the same height.
                tris.Add(new[]
                             {
                                 desc.Position, new Vector3(basinWidth, 0, 0) + desc.Position,
                                 new Vector3(0, 0, basinLength) + desc.Position
                             });
                tris.Add(new[]
                             {
                                 new Vector3(0, 0, basinLength) + desc.Position, new Vector3(basinWidth, 0, 0) + desc.Position,
                                 new Vector3(basinWidth, 0, basinLength) + desc.Position
                             });

                var msgGetScene = ObjectPool.Aquire<MsgGetPhysicsScene>();
                this.game.SendInterfaceMessage(msgGetScene, Interfaces.InterfaceType.Physics);

                this.fluidVolume = new FluidVolume(Vector3.Up, msgGetScene.PhysicsScene.Gravity.Y, tris, waterHeight, 1.0f, 0.8f, 0.7f, msgGetScene.PhysicsScene.PhysicsSpace.BroadPhase.QueryAccelerator, msgGetScene.PhysicsScene.PhysicsSpace.ThreadManager); 
            }
            else
            {
                throw new Exception("Shape description for a Terrain actor must be HeightFieldShapeDesc.");
            }

            // Tag the physics with data needed by the engine
            var tag = new EntityTag(this.ownerEntityID);            
            this.fluidVolume.Tag = tag;

            this.spaceObject = this.fluidVolume;
        }
    }
}