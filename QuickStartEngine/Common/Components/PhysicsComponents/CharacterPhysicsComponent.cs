//
// CharacterPhysicsComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using BEPUphysics.Character;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    public class CharacterPhysicsComponent : PhysicsComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.CharacterPhysicsComponent; }

        public static new BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            CharacterPhysicsComponentDefinition compDef = content.Load<CharacterPhysicsComponentDefinition>(definitionPath);

            CharacterPhysicsComponent newComponent = new CharacterPhysicsComponent(parent, compDef);
            return newComponent;
        }

        bool MoveBasedOnCameraOrientation = false;

        private CharacterController character;

        public CharacterPhysicsComponent(BaseEntity parent, ShapeType type, float density)
            : base(parent, type, density, true)
        {
            ActivateComponent();
        }

        public CharacterPhysicsComponent(BaseEntity parent, CharacterPhysicsComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            if (compDef.PhysicsModelPath.Length > 0)
            {
                this.physMesh = this.parentEntity.Game.ModelLoader.LoadStaticModel(compDef.PhysicsModelPath);
            }

            this.affectedByGravity = compDef.AffectedByGravity;
            this.shapeType = ShapeType.Cylinder;
            this.mass = compDef.Mass;
            this.collisionGroupType = CollisionGroups.RigidBody;

            this.height = compDef.Height;
            this.width = compDef.Width;
            this.depth = compDef.Depth;
            this.diameter = compDef.Diameter;

            InitializeActor();
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        protected override void CreateActor(IPhysicsScene PhysicsScene)
        {
            // Character physics have custom physics bodies that have gravity and other
            // forces manually applied, but they're still considered dynamic so they can
            // collide with other objects
            this.isDynamic = true;

            ActorDesc desc = new ActorDesc();
            desc.Orientation = this.parentEntity.Rotation;
            desc.Mass = this.mass;
            desc.Dynamic = this.isDynamic;
            desc.AffectedByGravity = this.affectedByGravity;
            desc.Position = this.parentEntity.Position;            
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Game = this.parentEntity.Game;
            desc.Type = ActorType.Character;

            this.actor = PhysicsScene.CreateActor(desc);            

            if (this.actor != null)
            {
                this.actor.EnableCollisionListening();
            }
        }

        protected override void BodyActivated()
        {
            // Intentionally left empty, character physics is always active because it checks for
            // input from the character
        }

        protected override void BodyDeactivated()
        {
            // Intentionally left empty, character physics is always active because it checks for
            // input from the character
        }

        public override void FixedUpdate(GameTime gameTime)
        {
            base.FixedUpdate(gameTime);

            var msgGetMovement = ObjectPool.Aquire<MsgGetCharacterMovement>();            
            msgGetMovement.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendMessage(msgGetMovement);
            
            this.actor.ProcessMovementFromInput( msgGetMovement.Data ); 
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
                case MessageType.LockCharRotationToCamera:
                    {
                        var orientMsg = message as MsgLockCharacterRotationToCamera;
                        message.TypeCheck(orientMsg);

                        MoveBasedOnCameraOrientation = orientMsg.LockRotation;
                    }
                    return true;
                case MessageType.GetIsACharacter:
                    {
                        var msgIsChar = message as MsgGetIsACharacter;
                        message.TypeCheck(msgIsChar);

                        msgIsChar.IsCharacter = true;
                    }
                    return true;
                default:
                    return base.ExecuteMessage(message);
            }
        }
    }
}
