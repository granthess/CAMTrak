//
// PhantomPhysicsComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    public class PhantomPhysicsComponent : PhysicsComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.PhantomPhysicsComponent; }

        public static new BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            var compDef = content.Load<PhysicsComponentDefinition>(definitionPath);
            compDef.IsDynamic = false;  // Phantoms cannot have dynamic physics
            compDef.CollisionGroupType = CollisionGroups.Phantom;

            var newComponent = new PhantomPhysicsComponent(parent, compDef);
            return newComponent;
        }

        public PhantomPhysicsComponent(BaseEntity parent)
            : base(parent)
        {
            this.collisionGroupType = CollisionGroups.Phantom;

            ActivateComponent();
        }

        public PhantomPhysicsComponent(BaseEntity parent, PhysicsComponentDefinition compDef)
            : base(parent, compDef)
        {
            ActivateComponent();
        }

        public PhantomPhysicsComponent(BaseEntity parent, ShapeType type, float density)
            : base(parent, type, density, false)
        {
            this.collisionGroupType = CollisionGroups.Phantom;

            ActivateComponent();
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        protected override void CreateActor(IPhysicsScene PhysicsScene)
        {
            ShapeDesc newShape = CreateShapeFromType(shapeType);

            // Phantoms cannot currently have dynamic physics
            this.isDynamic = false;

            var desc = new ActorDesc();
            desc.Orientation = this.parentEntity.Rotation;
            desc.Mass = this.mass;
            desc.Dynamic = this.isDynamic;
            desc.AffectedByGravity = this.affectedByGravity;
            desc.Position = this.parentEntity.Position;
            desc.Shapes.Add(newShape);
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Game = this.parentEntity.Game;
            desc.Type = ActorType.Static;

            this.actor = PhysicsScene.CreateActor(desc);

            if (this.actor != null)
            {
                this.actor.EnableCollisionListening();
            }
        }

        protected override void BodyActivated()
        {
            // Intentionally left empty, phantoms don't deactivate phantoms because each FixedUpdate() they
            // have to do collision checks, so we don't need to activate them here either.
        }

        protected override void BodyDeactivated()
        {
            // Intentionally left empty, we don't deactivate phantoms because each FixedUpdate() they
            // have to do collision checks.
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            // ALL components derived from non-abstract parents need to pass the message to their parent
            // to let the parent class have a chance to use it.
            return base.ExecuteMessage(message);
        }
    }
}
