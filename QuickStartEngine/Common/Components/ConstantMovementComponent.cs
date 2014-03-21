//
// ConstantMovementComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart;
using QuickStart.Entities;

namespace QuickStart.Components
{
    public class ConstantMovementComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.ConstantMovementComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            ConstantMovementComponentDefinition compDef = content.Load<ConstantMovementComponentDefinition>(definitionPath);

            ConstantMovementComponent newComponent = new ConstantMovementComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Direction of movement (units per second).        
        /// </summary>
        private Vector3 MovementVector;

        /// <summary>
        /// Frequency of movement (crests per second). Movement is in a sine wave pattern.
        /// Frequency is 1 / period. Where the period is defined as the length between crests in the wave.
        /// </summary>
        private float Frequency;

        /// <summary>
        /// Amplitude of movement. Movement is in a sine wave pattern.
        /// Amplitude is the height of the wave.
        /// </summary>
        private float Amplitude;

        /// <summary>
        /// Stores how far from the origin the position change was last frame. Only needed for
        /// sine-wave motion patterns.
        /// </summary>
        private float distanceFromOriginLastFrame;

        public ConstantMovementComponent(BaseEntity parent)
            : base(parent)
        {
            ActivateComponent();
        }

        public ConstantMovementComponent(BaseEntity parent, ConstantMovementComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            this.MovementVector = compDef.MovementVector;
            this.Frequency = compDef.Frequency;
            this.Amplitude = compDef.Amplitude;
        }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void Update(GameTime gameTime)
        {
            // If we have a sine-wave motion
            if (this.Frequency > 0.0f && this.Amplitude > 0.0f)
            {
                float distance = (float)Math.Sin((double)((MathHelper.TwoPi * gameTime.TotalGameTime.TotalMilliseconds * 0.001f) / Frequency)) * Amplitude;

                float diffFromLastFrame = distance - distanceFromOriginLastFrame;

                if (diffFromLastFrame != 0.0f)
                {
                    MsgModifyPosition modPosMsg = ObjectPool.Aquire<MsgModifyPosition>();
                    modPosMsg.Position = this.MovementVector * diffFromLastFrame;
                    modPosMsg.UniqueTarget = this.parentEntity.UniqueID;
                    this.parentEntity.Game.SendMessage(modPosMsg);

                    distanceFromOriginLastFrame = distance;
                }
            }
            else  // Linear motion
            {
                MsgModifyPosition modPosMsg = ObjectPool.Aquire<MsgModifyPosition>();
                modPosMsg.Position = this.MovementVector * ((float)gameTime.TotalGameTime.TotalMilliseconds * 0.001f);
                modPosMsg.UniqueTarget = this.parentEntity.UniqueID;
                this.parentEntity.Game.SendMessage(modPosMsg);
            }
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        public override void Initialize()
        {
            this.parentEntity.AddComponent(this);
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            if (message.UniqueTarget != this.parentEntity.UniqueID)
                return false;

            switch (message.Type)
            {
                case MessageType.SetConstantMovement:
                    {                        
                        MsgSetConstantMovement setConstMoveMsg = message as MsgSetConstantMovement;
                        message.TypeCheck(setConstMoveMsg);

                        this.MovementVector = setConstMoveMsg.MovementVector;
                        this.Frequency = setConstMoveMsg.Frequency;
                        this.Amplitude = setConstMoveMsg.Amplitude;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
