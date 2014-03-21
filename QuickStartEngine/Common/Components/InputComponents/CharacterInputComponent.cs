//
// CharacterInputComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Input;

namespace QuickStart.Components
{
    public class CharacterInputComponent : BaseInputComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.CharacterInputComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            CharacterInputComponentDefinition compDef = content.Load<CharacterInputComponentDefinition>(definitionPath);

            CharacterInputComponent newComponent = new CharacterInputComponent(parent, compDef);
            return newComponent;
        }

        private float forwardMovement = 0.0f;      // Movement forward/backward this frame
        private float rightMovement = 0.0f;        // Movement right/left this frame
        private float clockwiseRotation = 0.0f;    // Rotation around Y axis this frame
        private bool wantsJump = false;

        private float movementSpeed = 50.0f;
        private float rotationSpeed = MathHelper.PiOver2;

        public CharacterInputComponent(BaseEntity parent)
            : base(parent)
        {            
        }

        public CharacterInputComponent(BaseEntity parent, CharacterInputComponentDefinition compDef)
            : base(parent)
        {
            this.movementSpeed = compDef.MovementSpeed;
            this.rotationSpeed = compDef.RotationSpeed;

            this.LeftThumbStickModifier = compDef.LeftThumbStickModifier;
            this.RightThumbStickModifier = compDef.RightThumbStickModifier;
        }

        /// <summary>
        /// Initializes this component. Also initializes the input poller's input listeners.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            InitButtonListeners();
        }

        /// <summary>
        /// Initialize button listeners. This determines which buttons the input poller will listen for
        /// </summary>
        public void InitButtonListeners()
        {
            this.inputs = new InputPollingHandler(this.parentEntity.Game);

#if !XBOX
            // Listen for these keyboard keys
            this.inputs.AddInputListener(Keys.E);
            this.inputs.AddInputListener(Keys.Q);
            this.inputs.AddInputListener(Keys.W);
            this.inputs.AddInputListener(Keys.S);
            this.inputs.AddInputListener(Keys.A);
            this.inputs.AddInputListener(Keys.D);
            this.inputs.AddInputListener(Keys.Up);
            this.inputs.AddInputListener(Keys.Down);
            this.inputs.AddInputListener(Keys.Right);
            this.inputs.AddInputListener(Keys.Left);
            this.inputs.AddInputListener(Keys.Space);
#endif //!XBOX

            this.inputs.AddInputListener(Buttons.A, PlayerIndex.One);
        }

        /// <summary>
        /// This is the component's Update loop, it takes care of updating the input that the character
        /// physics will use this frame.
        /// </summary>
        /// <param name="gameTime">XNA's built-in timer, provides a snapshot of the game's time.</param>
        public override void FixedUpdate(GameTime gameTime)
        {
            ResetInputValuesFromLastFrame();

            // Check for any input this frame
            ProcessInput();
        }

        /// <summary>
        /// Clear out any input values from last frame
        /// </summary>
        private void ResetInputValuesFromLastFrame()
        {
            forwardMovement = rightMovement = clockwiseRotation = 0.0f;
        }

        /// <summary>
        /// Process camera movements based on input
        /// </summary>
        /// <param name="gameTime">Snapshot of the timer</param>
        private void ProcessInput()
        {
            bool forward = (this.inputs.IsDown(Keys.W) || this.inputs.IsDown(Keys.Up));
            bool backward = (this.inputs.IsDown(Keys.S) || this.inputs.IsDown(Keys.Down));
            bool left = (this.inputs.IsDown(Keys.A) || this.inputs.IsDown(Keys.Left));
            bool right = (this.inputs.IsDown(Keys.D) || this.inputs.IsDown(Keys.Right));
            bool clockwise = this.inputs.IsDown(Keys.E);
            bool counterclockwise = this.inputs.IsDown(Keys.Q);
            this.wantsJump = this.inputs.IsDown(Buttons.A, PlayerIndex.One);
#if WINDOWS
            if (!this.wantsJump)
            {
                this.wantsJump = this.inputs.IsDown(Keys.Space);
            }

            ProcessMovementFromInput(forward, backward, left, right, clockwise, counterclockwise);
#endif //WINDOWS

            ProcessMovementFromGamepadInput();
        }

        private void ProcessMovementFromGamepadInput()
        {
            Vector2 LeftStick;
            this.inputs.Thumbstick(PlayerIndex.One, GamePadInputSide.Left, out LeftStick);

            if (LeftStick.Y != 0.0f && this.forwardMovement == 0.0f)
            {
                this.forwardMovement += LeftStick.Y * this.movementSpeed;
            }

            if (LeftStick.X != 0.0f && this.rightMovement == 0.0f)
            {
                this.rightMovement += LeftStick.X * this.movementSpeed;
            }
        }

#if WINDOWS
        private void ProcessMovementFromInput( bool forward, bool backward, bool left, bool right, bool clockwise, 
                                               bool counterclockwise)
        {
            float timeDelta = this.parentEntity.Game.PartialSecondsThisFrame;

            // If user is pressing both forward and backward, then negate both.
            if (forward && backward)
            {
                forward = backward = false;
            }

            if (left && right)
            {
                left = right = false;
            }

            if ( clockwise && counterclockwise )
            {
                clockwise = counterclockwise = false;
            }

            // Check if movement is already given (from gamepad)
            if (this.forwardMovement == 0.0f)
            {
                if (forward)
                {
                    this.forwardMovement += this.movementSpeed;
                }
                else if (backward)
                {
                    this.forwardMovement -= this.movementSpeed;
                }
            }

            // Check if movement is already given (from gamepad)
            if (this.rightMovement == 0.0f)
            {
                if (left)
                {
                    this.rightMovement -= this.movementSpeed;
                }
                else if (right)
                {
                    this.rightMovement += this.movementSpeed;
                }
            }

            if (clockwise)
            {
                this.clockwiseRotation -= this.rotationSpeed * timeDelta;
            }
            else if (counterclockwise)
            {
                this.clockwiseRotation += this.rotationSpeed * timeDelta;
            }
        }
#endif //WINDOWS

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.GetCharacterMovementInfo:
                    {
                        MsgGetCharacterMovement msgGetMovement = message as MsgGetCharacterMovement;
                        message.TypeCheck(msgGetMovement);

                        msgGetMovement.ForwardAmount = this.forwardMovement;
                        msgGetMovement.RightAmount = this.rightMovement;
                        msgGetMovement.ClockwiseAmount = this.clockwiseRotation;
                        msgGetMovement.WantsJump = this.wantsJump;
                    }
                    return true;

                case MessageType.SetIsControlled:
                    {
                        MsgSetIsControlled msgSetControlled = message as MsgSetIsControlled;
                        message.TypeCheck(msgSetControlled);

                        // We only enable controls when the character is being controlled, this prevents things like
                        // jumping and moving while something else is being controlled.
                        if (msgSetControlled.Controlled)
                        {
                            ActivateComponent();
                        }
                        else
                        {
                            DeactivateComponent();
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
