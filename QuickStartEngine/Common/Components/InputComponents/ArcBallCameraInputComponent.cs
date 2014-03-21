//
// ArcBallCameraInputComponent.cs
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
    /// <summary>
    /// Create an input component to handle camera movement in such a way that it will act like an
    /// Arc-Ball camera.
    /// </summary>
    public class ArcBallCameraInputComponent : BaseInputComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.ArcBallCameraInputComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            ArcBallCameraInputComponentDefinition compDef = content.Load<ArcBallCameraInputComponentDefinition>(definitionPath);

            ArcBallCameraInputComponent newComponent = new ArcBallCameraInputComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Amount of radians the camera is rotated around the UP axis from the world forward (+Z).
        /// </summary>
        public float HorizontalAngle
        {
            get { return horizontalAngle; }
            set { horizontalAngle = value; }
        }
        private float horizontalAngle = 0.0f;

        /// <summary>
        /// Amount of radians the camera is rotated around the world right axis (X).
        /// </summary>
        public float VerticalAngle
        {
            get { return verticalAngle; }
            set { verticalAngle = value; }
        }
        private float verticalAngle = MathHelper.PiOver2;

        /// <summary>
        /// Lowest value that <see cref="verticalAngle"/> is allowed to go.
        /// </summary>
        private float verticalAngleMin = 0.01f;

        /// <summary>
        /// Highest value that <see cref="verticalAngle"/> is allowed to go.
        /// </summary>
        private float verticalAngleMax = MathHelper.Pi - 0.01f;

        /// <summary>
        /// Lowest value that <see cref="zoom"/> is allowed to go.
        /// </summary>
        public float ZoomMin
        {
            get { return zoomMin; }
            set
            {
                // Zoom can never reach zero, distance must always be positive
                value = MathHelper.Max(0.01f, value);
                zoomMin = value;
                // Re-calculate zoom range whenever the range changes
                zoomRange = zoomMax - zoomMin;
            }
        }
        private float zoomMin = 0.01f;

        /// <summary>
        /// Highest value that <see cref="zoom"/> is allowed to go.
        /// </summary>
        public float ZoomMax
        {
            get { return zoomMax; }
            set
            {
                zoomMin = value;
                // Re-calculate zoom range whenever the range changes
                zoomRange = zoomMax - zoomMin;
            }
        }
        private float zoomMax = 50.0f;

        /// <summary>
        /// Camera's zoom value.
        /// </summary>
        public float Zoom
        {
            get { return zoom; }
            set
            {
                // Make sure zoom is being set within the min-max range
                value = MathHelper.Clamp(value, zoomMin, zoomMax);
                zoom = value;
            }
        }
        private float zoom = 30.0f;

        /// <summary>
        /// Difference between the zoomMax and zoomMin
        /// </summary>
        private float zoomRange = 49.99f;

        /// <summary>
        /// Position of parent is stored here for ease of use.
        /// </summary>
        public Vector3 TargetPosition
        {
            set { targetPosition = value; }
        }
        private Vector3 targetPosition = Vector3.Zero;

        /// <summary>
        /// Modifier for trigger sensitivity for this specific camera.
        /// </summary>
        private float triggerModifier = 0.05f;

        /// <summary>
        /// Locks the camera so that it is facing the same direction as its target, along the X-Z plane.
        /// Camera's Y (vertical) rotation remains unlocked.
        /// </summary>
        public bool LockRotationToTargetRotation
        {
            set { lockRotationToTargetRotation = value; }
        }
        private bool lockRotationToTargetRotation = false;

        /// <summary>
        /// Distance that the camera will look ahead of its target, along the camera's forward vector
        /// (flattened) to the X-Z plane.
        /// </summary>
        public float LookAheadDistance
        {
            set { lookAheadDistance = value; }
        }
        private float lookAheadDistance = 0.0f;

        /// <summary>
        /// Distance that the camera will look above its target, along the world UP axis.
        /// </summary>
        public float LookAboveDistance
        {
            set { lookAboveDistance = value; }
        }
        private float lookAboveDistance = 0.0f;

        /// <summary>
        /// Creates an input controller to handle a camera in such a way that is behaves as an
        /// Arc-Ball Camera.
        /// </summary>
        /// <param name="parent">Entity this component is attached to</param>
        public ArcBallCameraInputComponent(BaseEntity parent, ArcBallCameraInputComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            // Setting these through their accessors will update 'zoomRange'.
            this.ZoomMax = compDef.ZoomMax;
            this.ZoomMin = compDef.ZoomMin;

            this.RightThumbStickModifier = compDef.RightThumbStickModifier;

            this.HorizontalAngle = compDef.HorizontalAngle;
            this.VerticalAngle = compDef.VerticalAngle;
            this.verticalAngleMin = compDef.VerticalAngleMin;
            this.verticalAngleMax = compDef.VerticalAngleMax;
            this.Zoom = compDef.StartingZoom;
            this.triggerModifier = compDef.TriggerModifier;
            this.LookAheadDistance = compDef.LookAheadDistance;
            this.LookAboveDistance = compDef.LookAboveDistance;

            this.LeftThumbStickModifier = compDef.LeftThumbStickModifier;
            this.RightThumbStickModifier = compDef.RightThumbStickModifier;
        }

        /// <summary>
        /// Creates an input controller to handle a camera in such a way that is behaves as an
        /// Arc-Ball Camera.
        /// </summary>
        /// <param name="parent">Entity this component is attached to</param>
        public ArcBallCameraInputComponent(BaseEntity parent)
            : base(parent)
        {
            ActivateComponent();

            // Set some defaults. Setting these through their accessors will update 'zoomRange'.
            this.ZoomMax = 50.0f;
            this.ZoomMin = 0.01f;

            this.rightThumbStickModifier = 0.05f;            
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

            // Listen for these mouse buttons, also only for example, this camera uses event-based input for mouse buttons
            this.inputs.AddInputListener(MouseButton.Left);
            this.inputs.AddInputListener(MouseButton.Right);
        }

        /// <summary>
        /// This is the component's Update loop, it takes care of placing and moving the camera each frame.
        /// </summary>
        /// <param name="gameTime">XNA's built-in timer, provides a snapshot of the game's time.</param>
        public override void Update(GameTime gameTime)
        {
            // Check for any input this frame
            ProcessInput();

            // Keep camera's angles and zoom within range
            ProcessAngleRestriction();
            ProcessZoomRestriction();

            // Start with an initial offset
            Vector3 cameraPosition = new Vector3(0.0f, this.zoom, 0.0f);
                        
            // Rotate camera from its offset to its desired position
            ProcessRotation(ref cameraPosition);

            // Update camera's target's position
            ProcessTargetUpdate();

            // Move the camera to world coordinates, based on target
            MsgSetPosition setPositionMsg = ObjectPool.Aquire<MsgSetPosition>();
            setPositionMsg.Position = (cameraPosition + this.targetPosition);
            setPositionMsg.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendMessage(setPositionMsg);            

            // Offset the view to look ahead and/or look up if needed
            ProcessLookOffsets();

            // Look at the updated target position
            this.parentEntity.LookAt(this.targetPosition);
        }        

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
        }

        /// <summary>
        /// Handles mouse scroll wheel. This camera zooms based on mouse scrolling
        /// </summary>
        /// <param name="distance">Amount mouse scroll wheel has moved</param>
        private void HandleMouseScroll(int distance)
        {
            if (distance > 0)
            {
                this.zoom -= (this.zoomMax - this.zoomMin) * 0.1f;
            }
            else
            {
                this.zoom += (this.zoomMax - this.zoomMin) * 0.1f;
            }
        }

        /// <summary>
        /// Handles mouse movement
        /// </summary>
        /// <param name="distance">Distance mouse has moved, in each direction (X and Y).</param>
        private void HandleMouseMove(Vector2 distance)
        {
            // Only allow mouse movement to control camera if the left mouse button is being held
            if (this.inputs.IsHeld(MouseButton.Left))
            {
                int invertMod = (this.inverted) ? 1 : -1;

                this.verticalAngle += distance.Y * invertMod;

                if ( lockRotationToTargetRotation == false )
                {
                    this.horizontalAngle -= distance.X;
                }
            }
        }

        private void HandleThumbstickMove(Vector2 amount)
        {
            int invertMod = (this.inverted) ? -1 : 1;

            amount *= this.parentEntity.Game.PartialSecondsThisFrame;

            this.verticalAngle += amount.Y * invertMod;

            if (lockRotationToTargetRotation == false)
            {
                this.horizontalAngle -= amount.X;
            }
        }

        /// <summary>
        /// Process camera movements based on input
        /// </summary>
        /// <param name="gameTime">Snapshot of the timer</param>
        private void ProcessInput()
        {
            float timeDelta = this.parentEntity.Game.PartialSecondsThisFrame;

            this.lockRotationToTargetRotation = false;

            if (this.inputs.IsHeld(MouseButton.Right))
            {
                this.lockRotationToTargetRotation = true;
            }

            // ===== Thumbstick input =====
            // Left thumbstick moves the camera's rotation
            Vector2 RightStick;
            this.inputs.Thumbstick(PlayerIndex.One, GamePadInputSide.Right, out RightStick);

            float LeftTrigger;
            float RightTrigger;
            this.inputs.Trigger(PlayerIndex.One, GamePadInputSide.Left, out LeftTrigger);
            this.inputs.Trigger(PlayerIndex.One, GamePadInputSide.Right, out RightTrigger);

            if (Math.Abs(RightStick.Y) > 0.0f)
            {
                int invertMod = (this.inverted) ? -1 : 1;
                this.verticalAngle -= RightStick.Y * RightThumbStickModifier * invertMod;
            }

            if (Math.Abs(RightStick.X) > 0.0f)
            {
                this.horizontalAngle -= RightStick.X * RightThumbStickModifier;
            }

            if (Math.Abs(LeftTrigger) > 0.0f)
            {
                this.zoom += LeftTrigger * triggerModifier;
            }

            if (Math.Abs(RightTrigger) > 0.0f)
            {
                this.zoom -= RightTrigger * triggerModifier;
            }
        }

        /// <summary>
        /// Process camera's angles to make sure they're always within acceptable limits.
        /// </summary>
        private void ProcessAngleRestriction()
        {
            // Keep vertical angle within tolerances
            verticalAngle = MathHelper.Clamp(verticalAngle, verticalAngleMin, verticalAngleMax);

            // Keep vertical angle within PI
            if (horizontalAngle > MathHelper.TwoPi)
            {
                horizontalAngle -= MathHelper.TwoPi;
            }
            else if (horizontalAngle < 0.0f)
            {
                horizontalAngle += MathHelper.TwoPi;
            }
        }

        /// <summary>
        /// Process camera's zoom to make sure it is always within acceptable range.
        /// </summary>
        private void ProcessZoomRestriction()
        {
            zoom = MathHelper.Clamp(zoom, zoomMin, zoomMax);
        }

        /// <summary>
        /// Processes the target's position. If there is a target, the camera's <seealso cref="targetPosition"/>
        /// is updated
        /// </summary>
        private void ProcessTargetUpdate()
        {
            // If this camera is attached, use that as target.
            if (this.parentEntity.HasParent)
            {
                this.targetPosition = this.parentEntity.ParentEntity.Position;
            }
        }

        /// <summary>
        /// This processes any look-ahead or look-above distances that the camera may use.
        /// </summary>
        private void ProcessLookOffsets()
        {
            if ( lookAheadDistance > 0.0f )
            {
                Vector3 currentForward = this.targetPosition - this.parentEntity.Position;
                currentForward.Y = 0.0f;
                currentForward.Normalize();

                this.targetPosition += currentForward * (lookAheadDistance * (zoom / zoomRange));

                // lookAboveDistance must always be greater than lookAheadDistance
                if ( lookAboveDistance > 0.0f && lookAboveDistance > lookAheadDistance)
                {
                    Vector3 currentUp = Vector3.Cross(this.parentEntity.Rotation.Right, currentForward);

                    this.targetPosition += currentUp * lookAboveDistance;
                }
            }
        }

        /// <summary>
        /// Process camera rotation. Each frame the camera actually starts above the target, and then rotates
        /// properly into place.
        /// </summary>
        /// <param name="cameraPosition">Camera's current position</param>
        private void ProcessRotation(ref Vector3 cameraPosition)
        {
            // Rotate vertically
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationX(verticalAngle));

            if (this.parentEntity.HasParent)
            {
                if (this.lockRotationToTargetRotation)
                {
                    float targetAngle = (float)(Math.Atan2(this.parentEntity.ParentEntity.Rotation.Forward.X, 
                                                           -this.parentEntity.ParentEntity.Rotation.Forward.Z));

                    cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(-targetAngle));

                    return;
                } 
            }
            
            // Rotate horizontally
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateRotationY(horizontalAngle));
        }
    }
}
