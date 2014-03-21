// MessageType.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

namespace QuickStart
{
    /// <summary>
    /// Defines the core message types
    /// </summary>
    /// <remarks>
    /// Core engine messages are in the range of 0 to 100000, please use another range for custom messages
    /// <see cref="int.MinValue"/> denotes a unknown message
    /// </remarks>
    public enum MessageType
    {
        /// <summary>
        /// Unknown message
        /// </summary>
        Unknown = -1,

        #region Core Engine Messages
        /// <summary>
        /// Shutdown message
        /// </summary>
        Shutdown = 0,

        /// <summary>
        /// Removes an entity from all scenes
        /// </summary>
        RemoveEntity,

        /// <summary>
        /// Get list of all entity IDs
        /// </summary>
        GetEntityIDList,

        /// <summary>
        /// Adds an entity's physics actor to a physics scene
        /// </summary>
        AddEntityToPhysicsScene,

        /// <summary>
        /// Removes an entity's physics actor from a physics scene
        /// </summary>
        RemoveEntityFromPhysicsScene,

        /// <summary>
        /// Returns a reference to the physics scene
        /// </summary>
        GetPhysicsScene,

        /// <summary>
        /// Begins a physics frame
        /// </summary>
        BeginPhysicsFrame,

        /// <summary>
        /// Ends a physics frame
        /// </summary>
        EndPhysicsFrame,

        /// <summary>
        /// Set gravity
        /// </summary>
        SetGravity,

        /// <summary>
        /// Get gravity
        /// </summary>
        GetGravity,

        /// <summary>
        /// Sets the physics fixed time step frequency.
        /// </summary>
        SetPhysicsTimeStep,

        /// <summary>
        /// Gets the physics actor for a given entity.
        /// </summary>
        GetPhysicsActor,

        /// <summary>
        /// Sets an actor's collision group
        /// </summary>
        SetActorToCollisionGroup,

        /// <summary>
        /// Adds a space object to the model viewer. (NOTE: This only works in Windows)
        /// </summary>
        AddPhysicsToModelViewer,

        /// <summary>
        /// Tells the engine which entity is currently the controlled entity.
        /// </summary>
        SetControlledEntity,

        /// <summary>
        /// Gets the current controlled entity.
        /// </summary>
        GetControlledEntity,

        /// <summary>
        /// Lets an entity know it is being controlled (or no longer being controlled)
        /// </summary>
        SetIsControlled,

        /// <summary>
        /// Gets an entity based on its Unique ID.
        /// </summary>
        GetEntityByID,

        /// <summary>
        /// Gets the name of an entity.
        /// </summary>
        GetName,

        /// <summary>
        /// Gets the active scene's fog settings.
        /// </summary>
        GetFogSettings,

        #endregion

        #region Keyboard Messages
        /// <summary>
        /// KeyDown message
        /// </summary>
        KeyDown,

        /// <summary>
        /// KeyPress message
        /// </summary>
        KeyHeld,

        /// <summary>
        /// KeyUp message
        /// </summary>
        KeyUp,
        #endregion

        #region Mouse Messages
        /// <summary>
        /// MouseButtonDown message
        /// </summary>
        MouseDown,

        /// <summary>
        /// MouseButtonPress message
        /// </summary>
        MouseHeld,

        /// <summary>
        /// MouseButtonUp message
        /// </summary>
        MouseUp,

        /// <summary>
        /// MouseScroll message
        /// </summary>
        MouseScroll,

        /// <summary>
        /// MouseMove message
        /// </summary>
        MouseMove,

        /// <summary>
        /// Mouse cursor's visibility state has changed
        /// </summary>
        MouseCursorStateChange,
        #endregion

        #region GamePad Messages
        /// <summary>
        /// GamePad ButtonUp message
        /// </summary>
        ButtonUp,

        /// <summary>
        /// GamePad ButtonDown message
        /// </summary>
        ButtonDown,

        /// <summary>
        /// GamePad ButtonHeld message
        /// </summary>
        ButtonHeld,

        /// <summary>
        /// GamePad Trigger message
        /// </summary>
        Trigger,

        /// <summary>
        /// GamePad Trigger release message. Sends a message saying the trigger has been re-centered.
        /// </summary>
        TriggerRelease,

        /// <summary>
        /// GamePad Thumbstick message
        /// </summary>
        Thumbstick,

        /// <summary>
        /// GamePad Thumbstick release message. Sends a message saying the thumbstick has been re-centered.
        /// </summary>
        ThumbstickRelease,
        #endregion

        #region Core Camera Messages
        /// <summary>
        /// Sets the current render camera
        /// </summary>
        CameraSetRender,
       
        /// <summary>
        /// Gets the current render camera's ID
        /// </summary>
        CameraGetRenderEntityID,

        /// <summary>
        /// CameraZoomIn message
        /// </summary>
        CameraZoomIn,

        /// <summary>
        /// CameraZoomOut message
        /// </summary>
        CameraZoomOut,

        /// <summary>
        /// CameraZoomReset message
        /// </summary>
        CameraZoomReset,            // @todo: Implement

        /// <summary>
        /// CameraSetProjection message
        /// </summary>
        CameraSetProjection,

        /// <summary>
        /// CameraSetView message
        /// </summary>
        CameraSetViewMatrix,

        /// <summary>
        /// Get this entity's View Matrix
        /// </summary>
        CameraGetViewMatrix,

        /// <summary>
        /// Get this entity's Projection Matrix
        /// </summary>
        CameraGetProjectionMatrix,

        /// <summary>
        /// Gets values associated with a camera
        /// </summary>
        GetCameraValues,

        /// <summary>
        /// Send this message to an Entity that has a sky component to attach the sky to the camera.
        /// </summary>
        SkyAttachToCamera,

        /// <summary>
        /// Send this message to the Camera Interface to get a ray from the camera's near point
        /// to the cursor's location in 3D space on the camera's far plane. This will give you
        /// a line segment which you can use to check collision for anything under the mouse.
        /// </summary>
        GetLineSegmentToCursor,
        #endregion

        #region Entity Messages
        /// <summary>
        /// Set an entity to look at a specific position.
        /// </summary>
        LookAtPosition,

        /// <summary>
        /// Set an entity to look at it's parent entity, if it has one.
        /// </summary>
        LookAtParent,

        /// <summary>
        /// Set an entity's position
        /// </summary>
        SetPosition,

        /// <summary>
        /// Get an entity's position
        /// </summary>
        GetPosition,
        
        /// <summary>
        /// Modify an entity's position from its current position
        /// </summary>
        ModifyPosition,

        /// <summary>
        /// Lets an entity and its components know that an entity's
        /// position has been changed.
        /// </summary>
        PositionChanged,

        /// <summary>
        /// Set an entity's rotation
        /// </summary>
        SetRotation,

        /// <summary>
        /// Get an entity's rotation
        /// </summary>
        GetRotation,

        /// <summary>
        /// Alter an entity's rotaiton from its current rotation
        /// </summary>
        ModifyRotation,

        /// <summary>
        /// Lets an entity and its components know that an entity's
        /// rotation has been changed.
        /// </summary>
        RotationChanged,

        /// <summary>
        /// Get an entity's forward vector
        /// </summary>
        GetVectorForward,

        /// <summary>
        /// Get an entity's up vector
        /// </summary>
        GetVectorUp,

        /// <summary>
        /// Get an entity's right vector
        /// </summary>
        GetVectorRight,

        /// <summary>
        /// Adds an entity as a child of another entity
        /// </summary>
        SetParent,

        /// <summary>
        /// Removes a child entity from an entity
        /// </summary>
        RemoveChild,

        /// <summary>
        /// Lets a child entity know the parent was removed from it
        /// </summary>
        ParentRemoved,

        /// <summary>
        /// Lets a child entity know it was given a parent
        /// </summary>
        ParentAdded,

        /// <summary>
        /// Lets a parent entity know a child was removed from it
        /// </summary>
        ChildRemoved,

        /// <summary>
        /// Lets a parent entity know a child was added to it
        /// </summary>
        ChildAdded,

        /// <summary>
        /// Gets the ID of an entity's parent
        /// </summary>
        GetParentID,

        #region Physics Messages
        /// <summary>
        /// Sets an entity's linear velocity
        /// </summary>
        SetLinearVelocity,

        /// <summary>
        /// Gets an entity's linear velocity
        /// </summary>
        GetLinearVelocity,

        /// <summary>
        /// Sets an entity's angular velocity
        /// </summary>
        SetAngularVelocity,

        /// <summary>
        /// Gets an entity's angular velocity (rate of spin)
        /// </summary>
        GetAngularVelocity,

        /// <summary>
        /// Adds a linear force to an entity
        /// </summary>
        AddLinearForce,

        /// <summary>
        /// Adds an angular force to an entity
        /// </summary>
        AddAngularForce,

        /// <summary>
        /// Sent when a collision occurs with a physics object registered for collision listening
        /// </summary>
        Collision,

        /// <summary>
        /// Sent when a collision is no longer occuring
        /// </summary>
        CollisionOff,

        /// <summary>
        /// Tells an object to listen or stop listen for collisions.
        /// </summary>
        ListenForCollision,

        /// <summary>
        /// Add a collision effect to the entity.
        /// </summary>
        AddCollisionEffect,

        /// <summary>
        /// Remove a collision effect from an entity.
        /// </summary>
        RemoveCollisionEffect,

        /// <summary>
        /// Initialize a collision effect. Do this after the effect has been added.
        /// </summary>
        InitCollisionEffect,

        /// <summary>
        /// Get whether or not an entity's physics are dynamic (movable) or not.
        /// </summary>
        GetHasDynamicPhysics,

        /// <summary>
        /// Set an entity's physics to be movable (dynamic) or non-movable.
        /// </summary>
        SetPhysicsMovableState,

        /// <summary>
        /// Gets the density of an entity's physics actor.
        /// </summary>
        GetDensity,

        /// <summary>
        /// Gets the BoundingBox of a physics actor.
        /// </summary>
        GetPhysicsBoundingBox,

        /// <summary>
        /// Adds a permanent external force to an object.
        /// </summary>
        AddExternalForce,

        /// <summary>
        /// Gets whether or not the physics body has any movement.
        /// </summary>
        GetIsInMotion,

        /// <summary>
        /// This message is sent from the <see cref="QSBody"/> to the entity
        /// whenever it becomes activated (goes from stationary to moving).
        /// </summary>
        BodyActivated,

        /// <summary>
        /// This message is sent from the <see cref="QSBody"/> to the entity
        /// whenever it becomes deactivated (goes from moving to stationary).
        /// </summary>
        BodyDeactivated,

        /// <summary>
        /// Gets whether or not the physics body is affected by gravity.
        /// </summary>
        GetAffectedByGravity,

        /// <summary>
        /// Sets whether or not the physics body is affected by gravity.
        /// </summary>
        SetAffectedByGravity,
        #endregion

        #region Character Controller Messages
        /// <summary>
        /// Base a character's movement on the camera rotation
        /// </summary>
        LockCharRotationToCamera,

        /// <summary>
        /// Gets whether or not this entity is a character.
        /// </summary>
        GetIsACharacter,

        /// <summary>
        /// Gets movement info from a character's input component.
        /// </summary>
        GetCharacterMovementInfo,
        #endregion

        #region Terrain Messages
        /// <summary>
        /// Get the height of the terrain at any X,Z coordinates
        /// </summary>
        GetTerrainHeightAtXZ,

        /// <summary>
        /// Get the entire terrain entity for use.
        /// </summary>
        GetTerrainEntity,

        /// <summary>
        /// Gets terrain properties
        /// </summary>
        GetTerrainProperties,

        /// <summary>
        /// Tells terrain to display bounding boxes.
        /// </summary>
        ToggleTerrainDisplayBoundingBoxes,
        #endregion

        #region Navmesh Messages
        /// <summary>
        /// Call this to generate a nav mesh
        /// </summary>
        GenerateNavMesh,
        #endregion

        #region Movement Component Messages
        /// <summary>
        /// Sets an entity into a constant rotation
        /// </summary>
        SetConstantRotation,

        /// <summary>
        /// Sets an entity into a constant sine wave movement
        /// </summary>
        SetConstantMovement,
        #endregion

        #region Render Messages
        /// <summary>
        /// Gets all of the vertices that make up a model
        /// </summary>
        GetModelVertices,

        /// <summary>
        /// Gets all of the indices that make up a model
        /// </summary>
        GetModelIndices,

        /// <summary>
        /// Gets the file path for the model on an entity
        /// </summary>
        GetModelPath,

        /// <summary>
        /// Gets a list of vertices and indices for a standard XNA model
        /// </summary>
        GetVerticesIndicesInfoFromModel,

        /// <summary>
        /// Sets the color of a model
        /// </summary>
        SetModelColor,

        /// <summary>
        /// Gets the color of a model
        /// </summary>
        GetModelColor,

        /// <summary>
        /// Sets the opacity of a model
        /// </summary>
        SetModelOpacity,

        /// <summary>
        /// Gets the opacity of a model
        /// </summary>
        GetModelOpacity,

        /// <summary>
        /// Broadcasts that the graphics settings have been updated/changed.
        /// </summary>
        GraphicsSettingsChanged,

        /// <summary>
        /// Gets the viewport
        /// </summary>
        GetViewport,

        /// <summary>
        /// Sets a cube map texture for the graphics system to use.
        /// </summary>
        SetCubeMapTexture,

        /// <summary>
        /// Lets the graphics system know about the water's elevation, even if the water
        /// plane is not in the view frustum this frame, that way other systems, like terrain
        /// can know about the water.
        /// </summary>
        SetRenderWaterElevation,

        GetRenderWaterElevation,
        #endregion

        #region Water Messages
        /// <summary>
        /// Sets the color of a water plane
        /// </summary>
        SetWaterColors,

        /// <summary>
        /// Sets the reflectivity of water, 0.0f being no reflection at all, 1.0f being 100% reflection (no refraction)
        /// </summary>
        SetWaterReflectivity,

        /// <summary>
        /// Gets the elevation of the water
        /// </summary>
        GetWaterElevation,
        #endregion

        #region Motion Helper Messages
        /// <summary>
        /// Yaw an entity.
        /// </summary>
        Yaw,

        /// <summary>
        /// Yaw an entity using the world Up axis.
        /// </summary>
        YawWorldUp,

        /// <summary>
        /// Pitch an entity.
        /// </summary>
        Pitch,

        /// <summary>
        /// Roll an entity.
        /// </summary>
        Roll,

        /// <summary>
        /// Strafe an entity.
        /// </summary>
        Strafe,

        /// <summary>
        /// Walk an entity.
        /// </summary>
        Walk,

        /// <summary>
        /// Jump an entity.
        /// </summary>
        Jump,
        #endregion

        #endregion

        #region Debugging Messages
        DebugText,
        #endregion
    }
}
