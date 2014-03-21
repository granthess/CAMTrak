//
// ConstantRotationComponent.cs
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
    public class ConstantRotationComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.ConstantRotationComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            ConstantRotationComponentDefinition compDef = content.Load<ConstantRotationComponentDefinition>(definitionPath);

            ConstantRotationComponent newComponent = new ConstantRotationComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// This is the amount of rotation around the X axis that will be added to this object every second.
        /// </summary>
        private float amountXAxisPerSecond = 0.0f;
        
        /// <summary>
        /// This is the amount of rotation around the Y axis that will be added to this object every second.
        /// </summary>
        private float amountYAxisPerSecond = 0.0f;

        /// <summary>
        /// This is the amount of rotation around the Z axis that will be added to this object every second. 
        /// </summary>
        private float amountZAxisPerSecond = 0.0f;

        public ConstantRotationComponent(BaseEntity parent)
            : base(parent)
        {            
        }

        public ConstantRotationComponent(BaseEntity parent, ConstantRotationComponentDefinition compDef)
            : base(parent)
        {
            this.amountXAxisPerSecond = compDef.AmountXAxisPerSecond;
            this.amountYAxisPerSecond = compDef.AmountYAxisPerSecond;
            this.amountZAxisPerSecond = compDef.AmountZAxisPerSecond;

            if ( this.amountXAxisPerSecond != 0.0f
              || this.amountYAxisPerSecond != 0.0f
              || this.amountZAxisPerSecond != 0.0f )
            {
                ActivateComponent();
            }
        }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public override void Update(GameTime gameTime)
        {
            // We factor time into our constant rotations. After all, they wouldn't be "constant" if they were frame-rate dependant.
            float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
            timeDelta = MathHelper.Max(timeDelta, 1u);   // We make sure timedelta is never less than 1ms for movement.
            timeDelta *= 0.001f;

            Matrix constRot = Matrix.CreateFromYawPitchRoll(amountYAxisPerSecond * timeDelta, amountXAxisPerSecond * timeDelta, amountZAxisPerSecond * timeDelta);

            MsgModifyRotation modRotMsg = ObjectPool.Aquire<MsgModifyRotation>();
            modRotMsg.Rotation = constRot;
            modRotMsg.UniqueTarget = this.parentEntity.UniqueID;            
            this.parentEntity.Game.SendMessage(modRotMsg);
        }

        /// <summary>
        /// Initializes this component.
        /// </summary>
        public override void Initialize()
        {
            this.parentEntity.AddComponent(this);
        }

        /// <summary>
        /// Shutdown this component
        /// </summary>
        public override void Shutdown()
        {
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
                case MessageType.SetConstantRotation:
                    {
                        // We interpret each float in the Vector3 within this message as the amount of rotation (in radians)
                        // around that axis per second. So if this messages Vector3s X value is 3.0, that is 3 radians per second
                        // around the X axis per second.
                        MsgSetConstantRotation setConstRotMsg = message as MsgSetConstantRotation;
                        message.TypeCheck(setConstRotMsg);

                        Vector3 Rotation = setConstRotMsg.Rotation;
                        this.amountXAxisPerSecond = Rotation.X;
                        this.amountYAxisPerSecond = Rotation.Y;
                        this.amountZAxisPerSecond = Rotation.Z;

                        if ( this.amountXAxisPerSecond != 0.0f
                          || this.amountYAxisPerSecond != 0.0f
                          || this.amountZAxisPerSecond != 0.0f )
                        {
                            ActivateComponent();
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
