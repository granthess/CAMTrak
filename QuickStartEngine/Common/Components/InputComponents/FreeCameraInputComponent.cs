//
// FreeCameraInputComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using QuickStart;
using QuickStart.Entities;
using QuickStart.Input;

namespace QuickStart.Components
{
    /// <summary>
    /// Create an input component to handle camera movement in such a way that it will act like an
    /// Free moving camera.
    /// </summary>
    public class FreeCameraInputComponent : BaseInputComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.FreeCameraInputComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            FreeCameraInputComponentDefinition compDef = content.Load<FreeCameraInputComponentDefinition>(definitionPath);

            FreeCameraInputComponent newComponent = new FreeCameraInputComponent(parent, compDef);
            return newComponent;

        }

        /// <summary>
        /// Speed modifier to determine how much movement is sent to the parent entity.
        /// </summary>
        private float speed = 100.0f;

        /// <summary>
        /// Speed is multiplied by this amount when the player holds down the left mouse button.
        /// </summary>
        private int turboSpeedModifier = 4;

        /// <summary>
        /// Creates an input controller to handle a camera in such a way that is behaves as an
        /// Free-moving Camera.
        /// </summary>
        /// <param name="parent">Entity this component is attached to</param>
        public FreeCameraInputComponent(BaseEntity parent)
            : base(parent)
        {
            ActivateComponent();
        }

        /// <summary>
        /// Creates an input controller to handle a camera in such a way that is behaves as an
        /// Free-moving Camera.
        /// </summary>
        /// <param name="parent">Entity this component is attached to</param>
        public FreeCameraInputComponent(BaseEntity parent, FreeCameraInputComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            this.inverted = compDef.Inverted;
            this.speed = compDef.Speed;
            this.turboSpeedModifier = compDef.TurboSpeedModifier;
            this.leftThumbStickModifier = compDef.LeftThumbStickModifier;
            this.rightThumbStickModifier = compDef.RightThumbStickModifier;
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
            this.inputs.AddInputListener(Keys.Up);
            this.inputs.AddInputListener(Keys.Down);
            this.inputs.AddInputListener(Keys.Left);
            this.inputs.AddInputListener(Keys.Right);
            this.inputs.AddInputListener(Keys.W);
            this.inputs.AddInputListener(Keys.S);
            this.inputs.AddInputListener(Keys.A);
            this.inputs.AddInputListener(Keys.D);
            this.inputs.AddInputListener(Keys.Q);
            this.inputs.AddInputListener(Keys.E);
#endif //!XBOX
        }

        /// <summary>
        /// This is the component's Update loop, it takes care of placing and moving the camera each frame.
        /// </summary>
        /// <param name="gameTime">XNA's built-in timer, provides a snapshot of the game's time.</param>
        public override void Update(GameTime gameTime)
        {
            if (!this.parentEntity.Game.IsMouseVisible)
            {
                ProcessInput(gameTime);
            }
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
#if WINDOWS
            switch (message.Type)
            {
                case MessageType.MouseDown:
                    {
                        MsgMouseButtonPressed mouseDownMessage = message as MsgMouseButtonPressed;
                        message.TypeCheck(mouseDownMessage);

                        if ( mouseDownMessage.Button == MouseButton.Left )
                            this.speed *= this.turboSpeedModifier;
                    }
                    return true;

                case MessageType.MouseUp:
                    {
                        MsgMouseButtonReleased mouseUpMessage = message as MsgMouseButtonReleased;
                        message.TypeCheck(mouseUpMessage);

                        if (mouseUpMessage.Button == MouseButton.Left)
                            this.speed *= (1.0f / this.turboSpeedModifier);
                    }
                    return true;

                case MessageType.MouseScroll:
                    {
                        MsgMouseScroll scrollWheelMessage = message as MsgMouseScroll;
                        message.TypeCheck(scrollWheelMessage);

                        HandleMouseScroll(scrollWheelMessage.ScrollWheelDelta);
                    }
                    return true;

                case MessageType.MouseMove:
                    {
                        MsgMouseMove moveMessage = message as MsgMouseMove;
                        message.TypeCheck(moveMessage);

                        HandleMouseMove(moveMessage.PositionDelta);
                    }
                    return true;

                default:
                    return false;
            }
#else //!WINDOWS
            return false;
#endif //!WINDOWS
        }

        /// <summary>
        /// Handles mouse scroll wheel. This camera zooms based on mouse scrolling
        /// </summary>
        /// <param name="distance">Amount mouse scroll wheel has moved</param>
        private void HandleMouseScroll(int distance)
        {
            if (distance > 0)
            {
                MsgCameraZoomIn camZoomMsg = ObjectPool.Aquire<MsgCameraZoomIn>();
                camZoomMsg.UniqueTarget = this.parentEntity.UniqueID;
                this.parentEntity.Game.SendMessage(camZoomMsg);
            }
            else
            {
                MsgCameraZoomOut camZoomMsg = ObjectPool.Aquire<MsgCameraZoomOut>();
                camZoomMsg.UniqueTarget = this.parentEntity.UniqueID;
                this.parentEntity.Game.SendMessage(camZoomMsg);
            }
        }

        /// <summary>
        /// Handles mouse movement
        /// </summary>
        /// <param name="distance">Distance mouse has moved, in each direction (X and Y).</param>
        private void HandleMouseMove(Vector2 distance)
        {
            int invertMod = (this.inverted) ? 1 : -1;

            this.parentEntity.Pitch(distance.Y * invertMod);
            
            // If the camera is not upside down
            if (this.parentEntity.Rotation.Up.Y > 0.0f)
            {
                this.parentEntity.YawAroundWorldUp(-distance.X);
            }
            else // If the camera is upside down reverse the rotation direction
            {
                this.parentEntity.YawAroundWorldUp(distance.X);
            }
        }

        /// <summary>
        /// Process camera movements based on input
        /// </summary>
        /// <param name="gameTime">Snapshot of the timers</param>
        private void ProcessInput(GameTime gameTime)
        {           
#if !XBOX
            float timeDelta = this.parentEntity.Game.PartialSecondsThisFrame;

            if (this.inputs.IsDown(Keys.Up))
            {
                this.parentEntity.Pitch(timeDelta);
            }

            if (this.inputs.IsDown(Keys.Down))
            {
                this.parentEntity.Pitch(-timeDelta);
            }

            if (this.inputs.IsDown(Keys.Left))
            {
                this.parentEntity.YawAroundWorldUp(timeDelta);
            }

            if (this.inputs.IsDown(Keys.Right))
            {
                this.parentEntity.YawAroundWorldUp(-timeDelta);
            }

            if (this.inputs.IsDown(Keys.W))
            {
                this.parentEntity.Walk(timeDelta * this.speed);
            }

            if (this.inputs.IsDown(Keys.S))
            {
                this.parentEntity.Walk(timeDelta * -this.speed);
            }

            if (this.inputs.IsDown(Keys.A))
            {
                this.parentEntity.Strafe(timeDelta * -this.speed);
            }

            if (this.inputs.IsDown(Keys.D))
            {
                this.parentEntity.Strafe(timeDelta * this.speed);
            }

            if (this.inputs.IsDown(Keys.Q))
            {
                this.parentEntity.Roll(-timeDelta);
            }

            if (this.inputs.IsDown(Keys.E))
            {
                this.parentEntity.Roll(timeDelta);
            }
#endif //!XBOX

            // ===== Thumbstick input =====
            // Left thumbstick moves the camera's position
            Vector2 LeftStick;
            Vector2 RightStick;
            this.inputs.Thumbstick(PlayerIndex.One, GamePadInputSide.Left, out LeftStick);
            this.inputs.Thumbstick(PlayerIndex.One, GamePadInputSide.Right, out RightStick);

            if (Math.Abs(LeftStick.Y) > 0.0f)
            {
                this.parentEntity.Walk(LeftStick.Y * this.leftThumbStickModifier);
            }

            if (Math.Abs(LeftStick.X) > 0.0f)
            {
                this.parentEntity.Strafe(LeftStick.X * this.leftThumbStickModifier);
            }

            if (Math.Abs(RightStick.Y) > 0.0f)
            {
                int invertMod = (this.inverted) ? 1 : -1;
                this.parentEntity.Pitch(RightStick.Y * this.rightThumbStickModifier * -invertMod);
            }

            if (Math.Abs(RightStick.X) > 0.0f)
            {
                this.parentEntity.YawAroundWorldUp(RightStick.X * this.rightThumbStickModifier);
            }
        }
    }
}
