//
// CameraComponent.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart;
using QuickStart.Entities;

namespace QuickStart.Components
{
    /// <summary>
    /// Super class for all Cameras. This class is abstract, which means you cannot just create a
    /// Camera instance, you must create a type of camera, like <see cref="FreeCamera"/>. To render something in the 
    /// QuickStart Engine you must have a <see cref="Camera"/>.
    /// </summary>
    public class CameraComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.CameraComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            CameraComponentDefinition compDef = content.Load<CameraComponentDefinition>(definitionPath);

            CameraComponent newComponent = new CameraComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Camera's aspect ratio (used for <see cref="projectionMatrix"/>)
        /// </summary>
        public float AspectRatio
        {
            get { return this.aspectRatio; }
            set { this.aspectRatio = value; }
        }
        private float aspectRatio = 1.33333f;

        private bool lockAspectRatioToViewport = true;

        /// <summary>
        /// Current zoom level, 1 would be (1x), 2 would be (2x), etc...
        /// </summary>
        private int zoomLevel = 1;

        /// <summary>
        /// Holds the default field of view. Storing the default FOV allows you to make changes to the camera's
        /// FOV and return back to your original values.
        /// </summary>
        private float defaultFOV = QSConstants.DefaultFOV;

        /// <summary>
        /// When true the camera will force itself above the terrain
        /// </summary>
        public bool ForceFrustumAboveTerrain
        {
            set { forceFrustumAboveTerrain = value; }
        }
        private bool forceFrustumAboveTerrain = true;

        /// <summary>
        /// Holds projection-matrix information.
        /// </summary>        
        public Matrix ProjectionMatrix
        {
            get { return this.projectionMatrix; }
        }
        public Matrix projectionMatrix = Matrix.Identity;

        /// <summary>
        /// Holds view-matrix information.
        /// </summary>        
        public Matrix ViewMatrix
        {
            get { return this.viewMatrix; }
        }
        public Matrix viewMatrix = Matrix.Identity;

        /// <summary>
        /// Holds a view-frustum.
        /// </summary>
        public BoundingFrustum ViewFrustum
        {
            get { return this.viewFrustum; }
            set { this.viewFrustum = value; }
        }
        private BoundingFrustum viewFrustum;

        /// <summary>
        /// Camera's Field-of-view (Degrees). Field of view determines the angle of the camera's
        /// vision. Most FPS style games use between 70-110 degrees.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a <see cref="FOV"/> value is too high or less than or
        /// equal to the float.Epsilon."/></exception>
        public float FOV
        {
            protected get { return this.fov; }
            set
            {
                if (value < float.Epsilon)
                {
                    throw new ArgumentOutOfRangeException("FOV (field of view) cannot be zero or a negative value");
                }

                if (value > QSConstants.MaxFOV)
                {
                    throw new ArgumentOutOfRangeException("FOV (field of view) cannot be greater than " + QSConstants.MaxFOV + "degrees");
                }

                this.fov = value;
                // Set hasChanged flag to true (if we use a bool)

                // Update projection matrix
            }
        }
        private float fov = QSConstants.DefaultFOV;

        /// <summary>
        /// Camera's near plane distance. The camera will only draw things between the near and far planes.
        /// The camera's bounding frustum near-plane is based on this value as well.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a <see cref="NearPlane"/> value is too low or greater than or
        /// equal to the far-plane distance."/></exception>
        protected float NearPlane
        {
            get { return this.nearPlane; }
            set
            {
                if (value >= this.farPlane)
                {
                    throw new ArgumentOutOfRangeException("Near-plane distance cannot be greater than or equal to the far-plane distance");
                }

                if (value < QSConstants.MinNearPlane)
                {
                    throw new ArgumentOutOfRangeException("Near-plane distance cannot be less than " + QSConstants.MinNearPlane);
                }

                this.nearPlane = value;
                // Set hasChanged flag to true (if we use a bool)

                // Update projection matrix
            }
        }
        private float nearPlane = QSConstants.DefaultNearPlane;

        /// <summary>
        /// Camera's far plane distance. The camera will only draw things between the near and far planes.
        /// The camera's bounding frustum far-plane is based on this value as well.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a <see cref="FarPlane"/> value is too great or less than or
        /// equal to the near-plane distance."/></exception>
        public float FarPlane
        {
            protected get { return this.farPlane; }
            set
            {
                if (value <= this.nearPlane)
                {
                    throw new ArgumentOutOfRangeException("Far-plane distance cannot be less than or equal to the near-plane distance");
                }

                if (value > QSConstants.MaxFarPlane)
                {
                    throw new ArgumentOutOfRangeException("Far-plane distance cannot be greater than " + QSConstants.MaxFarPlane);
                }

                this.farPlane = value;
                // Set hasChanged flag to true (if we use a bool)

                // Update projection matrix
            }
        }
        private float farPlane = QSConstants.DefaultFarPlane;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CameraComponent(BaseEntity parent)
            : base(parent)
        {
            this.aspectRatio = this.parentEntity.Game.GraphicsDevice.Viewport.AspectRatio;

            if (this.aspectRatio < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException("aspectRatio cannot be zero or a negative value");
            }

            ActivateComponent();
        }

        public CameraComponent(BaseEntity parent, CameraComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();            

            // If the definition file specifies something greater than zero for the aspect ratio then we
            // ignore it and let the default take over.
            if (compDef.AspectRatio > 0.0f)
            {
                this.lockAspectRatioToViewport = false;
                this.aspectRatio = compDef.AspectRatio;
            }
            else
            {
                this.lockAspectRatioToViewport = true;
                this.aspectRatio = this.parentEntity.Game.GraphicsDevice.Viewport.AspectRatio;
            }

            if (this.aspectRatio < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException("aspectRatio cannot be zero or a negative value");
            }

            if (compDef.DefaultFOV > 0.0f)
            {
                this.defaultFOV = compDef.DefaultFOV;
            }

            if (compDef.StartingFOV > 0.0f)
            {
                this.FOV = compDef.StartingFOV;
            }

            this.zoomLevel = compDef.StartingZoomLevel;

            if (compDef.NearPlane > 0.0f)
            {
                this.NearPlane = compDef.NearPlane;
            }

            if (compDef.FarPlane > 0.0f)
            {
                this.FarPlane = compDef.FarPlane;
            }

            this.forceFrustumAboveTerrain = compDef.ForceFrustumAboveTerrain;

            SetFrustum(FOV, aspectRatio, nearPlane, farPlane);
            UpdateProjectionMatrix();

            // @todo: Set hasChanged flag to true (if we use a bool)
        }

        /// <summary>
        /// Creates a camera.
        /// </summary>
        /// <param name="FOV">Field-of-view, in degrees</param>
        /// <param name="aspectRatio">Aspect Ratio is the screen's width (resolution in pixels) divided by the 
        /// screen's height (resolution in pixels). Most standard monitors and televisions are 4:3, or 1.333r. 
        /// Most widescreen monitors are 16:9, or 1.777r. So something like 1024x768 would be 4:3 because 1024/768 is 1.333r.</param>
        /// <param name="nearPlane">Near plane distance</param>
        /// <param name="farPlane">Far plane distance</param>
        /// <param name="game">The <see cref="QSGame"/> the camera belongs to</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a <see cref="aspectRatio"/> value is near-zero or negative."/></exception>
        public CameraComponent(BaseEntity parent, float FOV, float aspectRatio, float nearPlane, float farPlane)
            : base(parent)
        {
            ActivateComponent();

            if (aspectRatio < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException("aspectRatio cannot be zero or a negative value");
            }

            SetFrustum(FOV, aspectRatio, nearPlane, farPlane);
            UpdateProjectionMatrix();

            // @todo: Set hasChanged flag to true (if we use a bool)
        }

        /// <summary>
        /// Creates a camera.
        /// </summary>
        /// <param name="FOV">Field-of-view, in degrees</param>
        /// <param name="screenWidth">Screen's width (resolution in pixels).</param>
        /// <param name="screenHeight">Screen's height (resolution in pixels).</param>
        /// <param name="nearPlane">Near plane distance</param>
        /// <param name="farPlane">Far plane distance</param>
        /// <param name="game">The <see cref="QSGame"/> the camera belongs to</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a screen either dimension value near-zero is negative."/></exception>
        public CameraComponent(BaseEntity parent, float FOV, float screenWidth, float screenHeight, float nearPlane, float farPlane)
            : base(parent)
        {
            ActivateComponent();

            if (screenHeight < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException("screenHeight cannot be zero or a negative value");
            }

            if (screenWidth < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException("screenWidth cannot be zero or a negative value");
            }

            SetFrustum(FOV, screenWidth / screenHeight, nearPlane, farPlane);
            UpdateProjectionMatrix();
        }

        /// <summary>
        /// Sets up a new view frustum. This should only be called when there are changes to <see cref="FOV"/>,
        /// <see cref="aspectRatio"/>, the <see cref="nearPlane"/>, or the <see cref="farPlane"/>.
        /// </summary>
        /// <param name="FOV">Field-of-view, in degrees</param>
        /// <param name="aspectRatio">Aspect Ratio is the screen's width (resolution in pixels) divided by the 
        /// screen's height (resolution in pixels). Most standard monitors and televisions are 4:3, or 1.333r. 
        /// Most widescreen monitors are 16:9, or 1.777r. So something like 1024x768 would be 4:3 because 1024/768 is 1.333r.</param>
        /// <param name="nearPlane">Near plane distance</param>
        /// <param name="farPlane">Far plane distance</param>
        public void SetFrustum(float FOV, float aspectRatio, float nearPlane, float farPlane)
        {
            this.FOV = FOV;
            this.NearPlane = nearPlane;
            this.FarPlane = farPlane;

            this.viewFrustum = new BoundingFrustum(this.viewMatrix * this.projectionMatrix);
        }

        /// <summary>
        /// This is the component's Update loop, it takes care of placing and moving the camera each frame.
        /// </summary>
        /// <param name="gameTime">XNA's built-in timer, provides a snapshot of the game's time.</param>
        public override void Update(GameTime gameTime)
        {
            // Compute view matrix
            this.viewMatrix = Matrix.CreateLookAt(this.parentEntity.Position,
                                                  this.parentEntity.Position + this.parentEntity.Rotation.Forward, 
                                                  this.parentEntity.Rotation.Up);

            UpdateFrustum();

            ProcessFrustumTerrainCollision();
        }

        /// <summary>
        /// Updates the camera's frustum based on its current view and projection matrices.
        /// </summary>
        protected void UpdateFrustum()
        {
            this.viewFrustum.Matrix = this.viewMatrix * this.projectionMatrix;
        }

        /// <summary>
        /// Updates the projection matrix based on current camera settings.
        /// </summary>
        protected void UpdateProjectionMatrix()
        {
            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), this.aspectRatio,
                                                                        this.nearPlane, this.farPlane);
        }        

        /// <summary>
        /// Checks the bottom corners of the view frustum against the terrain to make sure the camera doesn't see below the terrain.
        /// </summary>
        private void ProcessFrustumTerrainCollision()
        {
            if (forceFrustumAboveTerrain && (this.parentEntity.Game.TerrainID != QSGame.UniqueIDEmpty) )
            {
                // Find lower frustum corners
                Vector3[] corners = this.viewFrustum.GetCorners();
                Vector3 bottomRight = corners[2];   // Corner at index 2 is the bottom right corner for BoundingFrustums
                Vector3 bottomLeft = corners[3];    // Corner at index 3 is the bottom left corner for BoundingFrustums

                // Get terrain height at the camera's bottom right corner
                var getHeightMsg = ObjectPool.Aquire<MsgGetTerrainHeight>();
                getHeightMsg.XPos = bottomRight.X;
                getHeightMsg.ZPos = bottomRight.Z;
                getHeightMsg.UniqueTarget = this.parentEntity.Game.TerrainID;
                this.parentEntity.Game.SendMessage(getHeightMsg);

                bool hitTerrain = false;
                if (getHeightMsg.PositionAboveTerrain)
                {
                    // Bump up the bottom right if it is less than 1.0 above the terrain
                    if (bottomRight.Y < (getHeightMsg.OutHeight + 1.0f))
                    {
                        hitTerrain = true;
                        bottomRight.Y = (getHeightMsg.OutHeight + 1.0f);
                    }
                }

                // Get terrain height at the camera's bottom left corner

                // You can re-use a message that has already been sent, but you MUST call aquire on it, or you'll get a memory leak.
                getHeightMsg = ObjectPool.Aquire<MsgGetTerrainHeight>();
                getHeightMsg.XPos = bottomLeft.X;
                getHeightMsg.ZPos = bottomLeft.Z;
                getHeightMsg.PositionAboveTerrain = false;
                getHeightMsg.UniqueTarget = this.parentEntity.Game.TerrainID;
                this.parentEntity.Game.SendMessage(getHeightMsg);

                if (getHeightMsg.PositionAboveTerrain)
                {
                    // Bump up the bottom right if it is less than 1.0 above the terrain
                    if (bottomLeft.Y < (getHeightMsg.OutHeight + 1.0f))
                    {
                        hitTerrain = true;
                        bottomLeft.Y = (getHeightMsg.OutHeight + 1.0f);
                    }
                }

                if (hitTerrain)
                {
                    float lowestPoint = Math.Min(bottomRight.Y, bottomLeft.Y);

                    if (lowestPoint > this.parentEntity.Position.Y)
                    {
                        var setPositionMsg = ObjectPool.Aquire<MsgSetPosition>();
                        setPositionMsg.Position = new Vector3(this.parentEntity.Position.X, lowestPoint, this.parentEntity.Position.Z);
                        setPositionMsg.UniqueTarget = this.parentEntity.UniqueID;
                        this.parentEntity.Game.SendMessage(setPositionMsg);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="Ray"/> from the camera's position in the direction the camera is facing.
        /// </summary>
        /// <returns>Ray in direction camera faces</returns>
        public Ray CreateRayFromForward()
        {
            Ray cameraRay = new Ray(this.parentEntity.Position, this.parentEntity.Rotation.Forward);

            return cameraRay;
        }

        /// <summary>
        /// Create a <see cref="Ray"/> from the camera's position through any other specified position.        
        /// </summary>
        /// <param name="destinationPos">Point to cast a ray through</param>
        /// <returns>Ray to specified position from the camera</returns>
        public Ray CreateRayFromEye(Vector3 destinationPos)
        {
            Vector3 direction = destinationPos - this.parentEntity.Position;

            Ray cameraRay = new Ray(this.parentEntity.Position, direction);

            return cameraRay;
        }

        /// <summary>
        /// Zooms in on current view by 2x the current value (by restricting the <see cref="FOV"/>).
        /// Returns back to default zoom when zoomed past the <see cref="FreeCamera.MaxZoomLevel"/>
        /// </summary>
        public void ZoomPerspectiveIn()
        {
            if (this.zoomLevel < QSConstants.MaxZoomLevel)
            {
                this.zoomLevel *= 2;
            }

            this.FOV = this.defaultFOV / this.zoomLevel;

            this.UpdateProjectionMatrix();
        }

        /// <summary>
        /// Zooms out on current view (by widening the <see cref="FOV"/>)
        /// </summary>
        public void ZoomPerspectiveOut()
        {
            if (this.zoomLevel > 1)
            {
                this.zoomLevel /= 2;

                this.FOV = this.defaultFOV / this.zoomLevel;

                this.UpdateProjectionMatrix();
            }
        }

        public bool IsAboveElevation( float elevation )
        {
            return this.parentEntity.Position.Y > elevation;
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
                case MessageType.CameraZoomIn:
                    {
                        MsgCameraZoomIn zoomMsg = message as MsgCameraZoomIn;
                        message.TypeCheck(zoomMsg);

                        ZoomPerspectiveIn();
                    }
                    return true;
                case MessageType.CameraZoomOut:
                    {
                        MsgCameraZoomOut zoomMsg = message as MsgCameraZoomOut;
                        message.TypeCheck(zoomMsg);

                        ZoomPerspectiveOut();
                    }
                    return true;
                case MessageType.GetCameraValues:
                    {
                        MsgCameraGetValues msgCamValues = message as MsgCameraGetValues;
                        message.TypeCheck(msgCamValues);

                        msgCamValues.AspectRatio = this.aspectRatio;
                        msgCamValues.FOV = this.fov;
                        msgCamValues.NearPlane = this.nearPlane;
                        msgCamValues.FarPlane = this.farPlane;
                        msgCamValues.ViewMatrix = this.viewMatrix;
                    }
                    return true;
                case MessageType.CameraGetViewMatrix:
                    {
                        MsgGetViewMatrix getViewMsg = message as MsgGetViewMatrix;
                        message.TypeCheck(getViewMsg);

                        getViewMsg.ViewMatrix = this.viewMatrix;
                    }
                    return true;
                case MessageType.CameraGetProjectionMatrix:
                    {
                        MsgGetProjectionMatrix getProjMsg = message as MsgGetProjectionMatrix;
                        message.TypeCheck(getProjMsg);

                        getProjMsg.ProjectionMatrix = this.projectionMatrix;
                    }
                    return true;
                case MessageType.GraphicsSettingsChanged:
                    {
                        if (this.lockAspectRatioToViewport)
                        {
                            this.aspectRatio = this.parentEntity.Game.GraphicsDevice.Viewport.AspectRatio;

                            if (this.aspectRatio < float.Epsilon)
                            {
                                throw new ArgumentOutOfRangeException("aspectRatio cannot be zero or a negative value");
                            }

                            this.UpdateProjectionMatrix();
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
