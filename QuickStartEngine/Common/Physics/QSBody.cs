//
// QSBody.cs
//
// This file is part of the QuickStart Engine's Wrapper to JigLibX. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using BEPUphysics.Entities;

using Microsoft.Xna.Framework;

namespace QuickStart.Physics
{
    public class QSBody
    {
        private IPhysicsActor actor;
        public IPhysicsActor Actor
        {
            get { return this.actor; }
        }

        private Entity physicsEntity;
        public Entity PhysicsEntity
        {
            get { return this.physicsEntity; }
            set 
            {
                if (this.physicsEntity != null)
                {
                    throw new Exception("You cannot overwrite an existing entity.");
                }

                this.physicsEntity = value; 
            }
        }

        public bool IsPhantom
        {
            get { return this.isPhantom; }
            protected set { this.isPhantom = value; }
        }
        protected bool isPhantom;

        /// <summary>
        /// A combination of all additional forces being placed on this body.
        /// </summary>
        public Vector3 AdditionalForce
        {
            get { return additionalForce; }
        }
        protected Vector3 additionalForce;

        public QSBody(IPhysicsActor actor)
            : base()
        {
            this.actor = actor;
            this.additionalForce = Vector3.Zero;
        }        

        public void AddAdditionalForce(ref Vector3 force)
        {
            this.additionalForce += force;
        }

        public virtual void AddExternalForces(float dt)
        {
            //ClearForces();

            this.actor.LinearVelocity += (this.additionalForce * dt);

            //AddGravityToExternalForce();
        }

        /// <summary>
        /// Called whenever this <see cref="QSBody"/> is set to 'Active'
        /// </summary>
        public void Activated()
        {
            this.actor.BodyActivated();
        }

        /// <summary>
        /// Called whenever this <see cref="QSBody"/> is set to 'Inactive'
        /// </summary>
        public void Deactivated()
        {
            this.actor.BodyDeactivated();
        }

        /// <summary>
        /// Checks if this body is at least partially made up of a specific shape type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasShapeDesc(Type type)
        {
            List<ShapeDesc> shapes = this.actor.Shapes;
            foreach (ShapeDesc shape in shapes)
            {                                
                if (shape.GetType() == type)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
