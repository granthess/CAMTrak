using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;

using QuickStart.Entities.Heightfield;
using QuickStart.Interfaces;
using QuickStart.Geometry;
using QuickStart.Physics;
using QuickStart.Physics.CollisionEffects;
using QuickStart.EnvironmentalSettings;

namespace QuickStart.Entities
{
    #region Basic Messages
    public class MsgGetEntityIDList : Message<Int64[]>
    {
        public Int64[] EntityIDList
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetEntityIDList;
        }
    }

    public class MsgGetName : Message<String>
    {
        public String Name
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetName;
        }
    }

    public class MsgRemoveEntity : Message<Int64>
    {
        public Int64 EntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.RemoveEntity;
            this.data = QSGame.UniqueIDEmpty;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetParent : Message<BaseEntity>
    {
        public BaseEntity ParentEntity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetParent;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetParentID : Message<Int64>
    {
        public Int64 ParentEntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetParentID;
            this.ParentEntityID = QSGame.UniqueIDEmpty;
        }
    }

    public class MsgGetPosition : Message<Vector3>
    {
        public Vector3 Position
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetPosition;
        }
    }

    public class MsgModifyPosition : Message<Vector3>
    {
        public Vector3 Position
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ModifyPosition;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetPosition : Message<Vector3>
    {
        public Vector3 Position
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.SetPosition;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgPositionChanged : Message<Vector3>
    {
        public Vector3 Position
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.PositionChanged;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetRotation : Message<Matrix>
    {
        public Matrix Rotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.SetRotation;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgModifyRotation : Message<Matrix>
    {
        public Matrix Rotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ModifyRotation;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgRotationChanged : Message<Matrix>
    {
        public Matrix Rotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.RotationChanged;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetRotation : Message<Matrix>
    {
        public Matrix Rotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetRotation;
        }
    }

    public class MsgGetVectorForward : Message<Vector3>
    {
        public Vector3 Forward
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetVectorForward;
        }
    }

    public class MsgGetVectorUp : Message<Vector3>
    {
        public Vector3 Up
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetVectorUp;
        }
    }

    public class MsgGetVectorRight : Message<Vector3>
    {
        public Vector3 Right
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.GetVectorRight;
        }
    }

    public class MsgLookAtPosition : Message<Vector3>
    {
        public Vector3 Position
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.LookAtPosition;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgModifyPitch : Message<float>
    {
        public float PitchAmount
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.Pitch;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgModifyYaw : Message<float>
    {
        public float YawAmount
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.Yaw;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgModifyYawWorldUp : Message<float>
    {
        public float YawAmount
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.YawWorldUp;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgRemoveChild : Message<BaseEntity>
    {
        public BaseEntity Child
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.RemoveChild;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgParentRemoved : Message<BaseEntity>
    {
        public BaseEntity Parent
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ParentRemoved;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgParentAdded : Message<BaseEntity>
    {
        public BaseEntity Parent
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ParentAdded;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgChildRemoved : Message<BaseEntity>
    {
        public BaseEntity Child
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ChildRemoved;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgChildAdded : Message<BaseEntity>
    {
        public BaseEntity Child
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.ChildAdded;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetConstantRotation : Message<Vector3>
    {
        /// <summary>
        /// Rotation is represented as a Vector3, in which each value represents the amount of radians
        /// per second around each axis. (e.g. (1, 0, 0), would represent 1 radian around the X axis, per second)
        /// </summary>
        public Vector3 Rotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.SetConstantRotation;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public struct MovementInfo
    {
        public Vector3 movementVector;
        public float frequency;
        public float amplitude;
    }

    public class MsgSetConstantMovement : Message<MovementInfo>
    {
        /// <summary>
        /// Movement will be along this vector
        /// </summary>
        public Vector3 MovementVector
        {
            get { return this.data.movementVector; }
            set { this.data.movementVector = value; }
        }

        public float Frequency
        {
            get { return this.data.frequency; }
            set { this.data.frequency = value; }
        }

        public float Amplitude
        {
            get { return this.data.amplitude; }
            set { this.data.amplitude = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Type = MessageType.SetConstantMovement;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetControlledEntity : Message<Int64>
    {
        public Int64 ControlledEntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetControlledEntity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetControlledEntity : Message<Int64>
    {
        public Int64 ControlledEntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetControlledEntity;
        }
    }

    public class MsgSetIsControlled : Message<bool>
    {
        public bool Controlled
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetIsControlled;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public struct GetEntityInfo
    {        
        public Int64 EntityID;

        public BaseEntity OutEntity;
    }

    public class MsgGetEntityByID : Message<GetEntityInfo>
    {
        public BaseEntity Entity
        {
            get { return this.data.OutEntity; }
            set { this.data.OutEntity = value; }
        }

        public Int64 EntityID
        {
            get { return this.data.EntityID; }
            set { this.data.EntityID = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetEntityByID;
        }
    }

    public class MsgGetFogSettings : Message<FogSettings>
    {
        public FogSettings Settings
        {
            get { return this.data; }
            set { this.data = value; }
        }

        public Vector4 FogColor
        {
            get { return this.data.FogColor; }
            set { this.data.FogColor = value; }
        }

        public float FogNearDistance
        {
            get { return this.data.FogNear; }
            set { this.data.FogNear = value; }
        }

        public float FogFarDistance
        {
            get { return this.data.FogFar; }
            set { this.data.FogFar = value; }
        }

        public float FogThinning
        {
            get { return this.data.FogThinning; }
            set { this.data.FogThinning = value; }
        }

        public float FogAltitude
        {
            get { return this.data.FogAltitude; }
            set { this.data.FogAltitude = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetFogSettings;
        }
    }
    
    #endregion

    #region Keyboard Messages
    public class MsgKeyPressed : Message<Keys>
    {
        public Keys Key
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.KeyDown;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgKeyReleased : Message<Keys>
    {
        public Keys Key
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.KeyUp;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgKeyHeld : Message<Keys>
    {
        public Keys Key
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.KeyHeld;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Gamepad Messages
    public class MsgGamePadTrigger : Message<GamePadTrigger>
    {
        public GamePadInputSide GamePadInputSide
        {
            get { return this.data.TriggerType; }
            set { this.data.TriggerType = value; }
        }

        public float TriggerValue
        {
            get { return this.data.TriggerValue; }
            set { this.data.TriggerValue = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.Trigger;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadTriggerReleased : Message<GamePadTrigger>
    {
        public GamePadInputSide GamePadInputSide
        {
            get { return this.data.TriggerType; }
            set { this.data.TriggerType = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.TriggerRelease;
            this.data.TriggerValue = 0.0f;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadThumbstick : Message<GamePadThumbStick>
    {
        public GamePadInputSide StickType
        {
            get { return this.data.StickType; }
            set { this.data.StickType = value; }
        }

        public Vector2 StickValues
        {
            get { return this.data.StickValues; }
            set { this.data.StickValues = value; }
        }

        public float StickValueX
        {
            get { return this.data.StickValues.X; }
            set { this.data.StickValues.X = value; }
        }

        public float StickValueY
        {
            get { return this.data.StickValues.Y; }
            set { this.data.StickValues.Y = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.Thumbstick;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadThumbstickReleased : Message<GamePadThumbStick>
    {
        public GamePadInputSide StickType
        {
            get { return this.data.StickType; }
            set { this.data.StickType = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ThumbstickRelease;
            this.data.StickValues = Vector2.Zero;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadButtonPressed : Message<GamePadButton>
    {
        public Buttons Button
        {
            get { return this.data.Button; }
            set { this.data.Button = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ButtonDown;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadButtonReleased : Message<GamePadButton>
    {
        public Buttons Button
        {
            get { return this.data.Button; }
            set { this.data.Button = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ButtonUp;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGamePadButtonHeld : Message<GamePadButton>
    {
        public Buttons Button
        {
            get { return this.data.Button; }
            set { this.data.Button = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.data.Player; }
            set { this.data.Player = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ButtonHeld;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Mouse Messages
    public class MsgMouseButtonPressed : Message<MouseButton>
    {
        public MouseButton Button
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseDown;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgMouseButtonReleased : Message<MouseButton>
    {
        public MouseButton Button
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseUp;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgMouseButtonHeld : Message<MouseButton>
    {
        public MouseButton Button
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseHeld;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgMouseScroll : Message<int>
    {
        public int ScrollWheelDelta
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseScroll;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgMouseMove : Message<Vector2>
    {
        public Vector2 PositionDelta
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseMove;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgMouseCursorStateChange : Message<bool>
    {
        public bool CursorVisible
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.MouseCursorStateChange;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Camera Messages
    public class MsgCameraZoomIn : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraZoomIn;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgCameraZoomOut : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraZoomOut;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public struct CameraInfo
    {
        public float nearPlane;
        public float farPlane;
        public float fov;
        public float aspectRatio;
        public Matrix viewMatrix;
    }

    public class MsgCameraGetValues : Message<CameraInfo>
    {
        public float NearPlane
        {
            get { return this.data.nearPlane; }
            set { this.data.nearPlane = value; }
        }

        public float FarPlane
        {
            get { return this.data.farPlane; }
            set { this.data.farPlane = value; }
        }

        public float FOV
        {
            get { return this.data.fov; }
            set { this.data.fov = value; }
        }

        public float AspectRatio
        {
            get { return this.data.aspectRatio; }
            set { this.data.aspectRatio = value; }
        }

        public Matrix ViewMatrix
        {
            get { return this.data.viewMatrix; }
            set { this.data.viewMatrix = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetCameraValues;
        }
    } 

    public class MsgSetRenderEntity : Message<BaseEntity>
    {
        public BaseEntity Entity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraSetRender;
            this.protocol = MessageProtocol.Broadcast;
        }
    } 

    public class MsgGetRenderEntity : Message<Int64>
    {
        public Int64 EntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraGetRenderEntityID;
        }
    }

    public class MsgGetLineSegmentToCursor : Message<LineSegment>
    {
        public LineSegment lineSegment
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetLineSegmentToCursor;
            this.protocol = MessageProtocol.Request;
        }
    }

    public class MsgGetViewMatrix : Message<Matrix>
    {
        public Matrix ViewMatrix
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraGetViewMatrix;
        }
    }

    public class MsgGetProjectionMatrix : Message<Matrix>
    {
        public Matrix ProjectionMatrix
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CameraGetProjectionMatrix;
        }
    }

    public class MsgSetGraphicsCubeMap : Message<Texture2D>
    {
        public Texture2D CubeMapTexture
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetCubeMapTexture;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetGraphicsWaterElevation : Message<float>
    {
        public float Elevation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetRenderWaterElevation;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetGraphicsWaterElevation : Message<float>
    {
        public float Elevation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetRenderWaterElevation;
            this.protocol = MessageProtocol.Request;
        }
    }

    public class MsgAttachSkyToCamera : Message<Int64>
    {
        public Int64 CameraEntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SkyAttachToCamera;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Terrain Messages
    public struct TerrainHeightMessage
    {
        /// <summary>
        /// X coordinate of the location you want terrain height at
        /// </summary>
        public float    XPos;

        /// <summary>
        /// Z coordinate of the location you want terrain height at
        /// </summary>
        public float    ZPos;

        /// <summary>
        /// Output Y coordinate, terrain height
        /// </summary>
        public float    OutHeight;

        /// <summary>
        /// Output, true if the X, Z coordinates were within the bounds of the heightfield
        /// </summary>
        public bool     PositionAboveTerrain;
    }

    public class MsgGetTerrainHeight : Message<TerrainHeightMessage>
    {
        public float XPos
        {
            get { return this.data.XPos; }
            set { this.data.XPos = value; }
        }

        public float ZPos
        {
            get { return this.data.ZPos; }
            set { this.data.ZPos = value; }
        }

        public float OutHeight
        {
            get { return this.data.OutHeight; }
            set { this.data.OutHeight = value; }
        }

        /// <summary>
        /// Whether or not this position's X and Z values are in the same region as a
        /// chunk of terrain. This will only be false if the camera is not above or below
        /// a chunk of terrain.
        /// </summary>
        public bool PositionAboveTerrain
        {
            get { return this.data.PositionAboveTerrain; }
            set { this.data.PositionAboveTerrain = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetTerrainHeightAtXZ;
        }
    }

    public class MsgGetTerrainEntity : Message<BaseEntity>
    {
        public BaseEntity TerrainEntity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetTerrainEntity;
        }
    }

    public struct TerrainProperties
    {
        public Texture2D HeightMap;
        public float ElevationStrength;
        public int TerrainScale;
        public int Size;
        public float MinHeight;
        public float MaxHeight;
    }

    public class MsgGetTerrainProperties : Message<TerrainProperties>
    {
        public Texture2D HeightMap
        {
            get { return this.data.HeightMap; }
            set { this.data.HeightMap = value; }
        }

        public float ElevationStrength
        {
            get { return this.data.ElevationStrength; }
            set { this.data.ElevationStrength = value; }
        }

        public int TerrainScale
        {
            get { return this.data.TerrainScale; }
            set { this.data.TerrainScale = value; }
        }

        public int Size
        {
            get { return this.data.Size; }
            set { this.data.Size = value; }
        }

        public float MinHeight
        {
            get { return this.data.MinHeight; }
            set { this.data.MinHeight = value; }
        }

        public float MaxHeight
        {
            get { return this.data.MaxHeight; }
            set { this.data.MaxHeight = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetTerrainProperties;
        }
    }

    public class MsgToggleDisplayBoundingBoxes : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ToggleTerrainDisplayBoundingBoxes;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Collision Effect Messages
    public struct CollisionEffectInfo
    {
        public int      effectID;
        public CollisionEffectType effectType;

        /// <summary>
        /// Only needed for RemoveCollisionEffect message, otherwise set to false. If this is set to true, all <see cref="CollisionEffectType"> of 
        /// 'effectType' are removed from the <see cref="BaseEntity"/>
        /// </summary>
        public bool     removeFromAll;
    }

    public class MsgAddCollisionEffect : Message<CollisionEffectInfo>
    {
        public int EffectID
        {
            get { return this.data.effectID; }
            set { this.data.effectID = value; }
        }

        public CollisionEffectType EffectType
        {
            get { return this.data.effectType; }
            set { this.data.effectType = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddCollisionEffect;
            this.data.removeFromAll = false;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgRemoveCollisionEffect : Message<CollisionEffectInfo>
    {
        public int EffectID
        {
            get { return this.data.effectID; }
            set { this.data.effectID = value; }
        }

        public CollisionEffectType EffectType
        {
            get { return this.data.effectType; }
            set { this.data.effectType = value; }
        }

        public bool RemoveAll
        {
            get { return this.data.removeFromAll; }
            set { this.data.removeFromAll = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.RemoveCollisionEffect;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public struct InitCollisionEffectInfo
    {
        /// <summary>
        /// effectID is unique per entity, but not unique across all entities.
        /// </summary>
        public int      effectID;

        /// <summary>
        /// Is this effect directional?
        /// </summary>
        public bool     directionalEffect;

        /// <summary>
        /// Only needed for directional-type collision effects
        /// </summary>
        public Vector3  direction;

        /// <summary>
        /// How strong effect is.
        /// </summary>
        public float    strength;
    }

    public class MsgInitCollisionEffect : Message<InitCollisionEffectInfo>
    {
        public int EffectID
        {
            get { return this.data.effectID; }
            set { this.data.effectID = value; }
        }

        public bool DirectionalEffect
        {
            get { return this.data.directionalEffect; }
            set { this.data.directionalEffect = value; }
        }

        public Vector3 Direction
        {
            get { return this.data.direction; }
            set { this.data.direction = value; }
        }

        public float Strength
        {
            get { return this.data.strength; }
            set { this.data.strength = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.InitCollisionEffect;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Character Controller Messages
    public struct CharacterMovementInfo
    {
        /// <summary>
        /// Forward/backward movement amount 
        /// </summary>
        public float    forward;

        /// <summary>
        /// Left/right movement amount
        /// </summary>
        public float    right;

        /// <summary>
        /// Rotation movement amount
        /// </summary>
        public float    clockwise;

        /// <summary>
        /// If character wants to jump this frame (based on input)
        /// </summary>
        public bool     wantsJump;

        /// <summary>
        /// If character is crouching
        /// </summary>
        public bool     crouching;

        /// <summary>
        /// Whether or not the character's movement is based on camera rotation
        /// </summary>
        public bool     moveBasedOnCamera;
    }

    public class MsgGetCharacterMovement : Message<CharacterMovementInfo>
    {
        public float ForwardAmount
        {
            get { return this.data.forward; }
            set { this.data.forward = value; }
        }

        public float RightAmount
        {
            get { return this.data.right; }
            set { this.data.right = value; }
        }

        public float ClockwiseAmount
        {
            get { return this.data.clockwise; }
            set { this.data.clockwise = value; }
        }

        public bool WantsJump
        {
            get { return this.data.wantsJump; }
            set { this.data.wantsJump = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetCharacterMovementInfo;
        }
    }

    public class MsgLockCharacterRotationToCamera : Message<bool>
    {
        public bool LockRotation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.LockCharRotationToCamera;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetIsACharacter : Message<bool>
    {
        public bool IsCharacter
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetIsACharacter;
        }
    }
    
    #endregion

    #region Physics Messages
    public class MsgSetGravity : Message<Vector3>
    {
        public Vector3 Gravity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetGravity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetGravity : Message<Vector3>
    {
        public Vector3 Gravity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetGravity;
        }
    }

    public class MsgGetPhysicsScene : Message<IPhysicsScene>
    {
        public IPhysicsScene PhysicsScene
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetPhysicsScene;
        }
    }

    public class MsgAddEntityToPhysicsScene : Message<AddEntityPhysicsActor>
    {
        public IPhysicsActor Actor
        {
            get { return this.data.Actor; }
            set { this.data.Actor = value; }
        }

        public Int64 EntityID
        {
            get { return this.data.EntityUniqueID; }
            set { this.data.EntityUniqueID = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddEntityToPhysicsScene;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgRemoveEntityFromPhysicsScene : Message<Int64>
    {
        public Int64 EntityID
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.RemoveEntityFromPhysicsScene;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public struct PhysicsFrameInfo
    {
        // Fill this in when sending the message
        public GameTime timeDelta;

        // This information goes back to the message sender
        public bool frameBegan;
    }

    public class MsgBeginPhysicsFrame : Message<PhysicsFrameInfo>
    {  
        /// <summary>
        /// Fill this in when sending the message
        /// </summary>
        public GameTime GameTime
        {
            get { return this.data.timeDelta; }
            set { this.data.timeDelta = value; }
        }

        /// <summary>
        /// This information goes back to the message sender
        /// </summary>
        public bool FrameBegan
        {
            get { return this.data.frameBegan; }
            set { this.data.frameBegan = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.BeginPhysicsFrame;

            // We want anything listening for this message to get it, but we also
            // need a result back on whether or not the physics frame began this game frame
            this.protocol = MessageProtocol.Accumulate;
        }
    }

    public class MsgEndPhysicsFrame : Message<bool>
    {
        public bool FrameEnded
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.EndPhysicsFrame;

            // We want anything listening for this message to get it, but we also
            // need a result back on whether or not the physics frame ended this game frame
            this.protocol = MessageProtocol.Accumulate;
        }
    }

    public class MsgSetPhysicsTimeStep : Message<int>
    {
        public int TimeStep
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetPhysicsTimeStep;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetPhysicsActor : Message<IPhysicsActor>
    {
        public IPhysicsActor Actor
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetPhysicsActor;
            this.protocol = MessageProtocol.Request;
        }
    }

    public class MsgSetActorToCollisionGroup : Message<CollisionGroupInfo>
    {
        public IPhysicsActor Actor
        {
            get { return this.data.actor; }
            set { this.data.actor = value; }
        }

        public CollisionGroups GroupType
        {
            get { return this.data.groupType; }
            set { this.data.groupType = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetActorToCollisionGroup;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgAddPhysicsToModelViewer : Message<ISpaceObject>
    {
        public ISpaceObject SpaceObject
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddPhysicsToModelViewer;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgListenForCollision : Message<bool>
    {
        public bool ListenForCollisions
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.ListenForCollision;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetHasDynamicPhysics : Message<bool>
    {
        public bool HasDynamicPhysics
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetHasDynamicPhysics;
        }
    }

    public class MsgSetPhysicsMovableState : Message<bool>
    {
        public bool Movable
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetPhysicsMovableState;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetDensity : Message<float>
    {
        public float Density
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetDensity;
            this.protocol = MessageProtocol.Request;
        }
    }

    public class MsgGetPhysicsBoundingBox : Message<BoundingBox>
    {
        public BoundingBox Box
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetPhysicsBoundingBox;
            this.protocol = MessageProtocol.Request;
        }
    }
    
    public class MsgSetLinearVelocity : Message<Vector3>
    {
        public Vector3 LinearVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetLinearVelocity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetLinearVelocity : Message<Vector3>
    {
        public Vector3 LinearVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetLinearVelocity;
        }
    }

    public class MsgAddLinearForce : Message<Vector3>
    {
        public Vector3 LinearVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddLinearForce;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetAngularVelocity : Message<Vector3>
    {
        public Vector3 AngularVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetAngularVelocity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetAngularVelocity : Message<Vector3>
    {
        public Vector3 AngularVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetAngularVelocity;
        }
    }

    public class MsgAddAngularForce : Message<Vector3>
    {
        public Vector3 AngularVelocity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddAngularForce;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgAddExternalForce : Message<Vector3>
    {
        public Vector3 ExternalForce
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.AddExternalForce;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetIsInMotion : Message<bool>
    {
        public bool InMotion
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetIsInMotion;
            this.protocol = MessageProtocol.Request;
        }
    }    

    public class MsgPhysicsBodyActivated : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.BodyActivated;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgPhysicsBodyDeactivated : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.BodyDeactivated;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgOnCollision : Message<EntityCollisionInfo>
    {
        public Int64 EntityID
        {
            get { return this.data.EntityID; }
            set { this.data.EntityID = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.Collision;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgOffCollision : Message<EntityCollisionInfo>
    {
        public Int64 EntityID
        {
            get { return this.data.EntityID; }
            set { this.data.EntityID = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.CollisionOff;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetAffectedByGravity : Message<bool>
    {
        public bool AffectedByGravity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetAffectedByGravity;
            this.protocol = MessageProtocol.Request;
        }
    }

    public class MsgSetAffectedByGravity : Message<bool>
    {
        public bool AffectedByGravity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetAffectedByGravity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Render Messages
    public class MsgGetModelVertices : Message<List<Vector3>>
    {
        public List<Vector3> Vertices
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetModelVertices;
        }
    }

    public class MsgGetModelIndices : Message<List<int>>
    {
        public List<int> Indices
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetModelIndices;
        }
    }

    public class MsgSetModelColor : Message<Color>
    {
        public Color Color
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetModelColor;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetModelColor : Message<Color>
    {
        public Color Color
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetModelColor;
        }
    }

    public class MsgSetModelOpacity : Message<float>
    {
        public float Opacity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetModelOpacity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetModelOpacity : Message<float>
    {
        public float Opacity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetModelOpacity;
        }
    }

    public class MsgGraphicsSettingsChanged : Message<bool>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GraphicsSettingsChanged;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetViewport : Message<Viewport>
    {
        public Viewport Viewport
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetViewport;
            this.protocol = MessageProtocol.Request;
        }
    }
    #endregion

    #region NavMesh Messages
    public class MsgGenerateNavMesh : Message<char>
    {
        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GenerateNavMesh;
            this.protocol = MessageProtocol.Broadcast;
        }
    }
    #endregion

    #region Water Messages
    public struct WaterColorInfo
    {
        public Color lightColor;
        public Color darkColor;
    }

    public class MsgSetWaterColors : Message<WaterColorInfo>
    {
        public Color WaterColorLight
        {
            get { return this.data.lightColor; }
            set { this.data.lightColor = value; }
        }

        public Color WaterColorDark
        {
            get { return this.data.darkColor; }
            set { this.data.darkColor = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetWaterColors;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgSetWaterReflectivity : Message<float>
    {
        public float Reflectivity
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.SetWaterReflectivity;
            this.protocol = MessageProtocol.Broadcast;
        }
    }

    public class MsgGetWaterElevation : Message<float>
    {
        public float Elevation
        {
            get { return this.data; }
            set { this.data = value; }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.type = MessageType.GetWaterElevation;
            this.protocol = MessageProtocol.Request;
        }
    }
    #endregion
}
