//
// QSConstants.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

namespace QuickStart
{
    using System;

    using Microsoft.Xna.Framework;

    /// <summary>
    /// Used to set levels of graphics detail
    /// </summary>
    public enum GraphicsLevel
    {
        Low = 0,
        Med = 1,
        High = 2,
        Highest = 3
    }

    /// <summary>
    /// A class to hold any global constants. Good for holding anything
    /// that is considered a "magic number". Magic numbers are what you notice
    /// if you look through someone's code and notice numbers defined for values,
    /// but you have no idea why that number was chosen. Also, having a single
    /// class for global constants creates an easy place to look when you know
    /// you're looking for a constant.
    /// </summary>
    public static class QSConstants
    {
        // ============================================================================================
        // XNA constants
        // ============================================================================================

        /// <summary>
        /// XNA 4.0's imposed limit of primitives per draw call.
        /// </summary>
        public const int MaxPrimitivesPerDrawCall = 1045575;

        // ============================================================================================
        // Graphics constants
        // ============================================================================================
        
        // ============================================================================================
        // Camera constants
        // ============================================================================================

        /// <summary>
        /// Default camera far plane distance.
        /// </summary>
        public const float DefaultFarPlane = 1000.0f;

        /// <summary>
        /// Default camera near plane distance.
        /// </summary>
        public const float DefaultNearPlane = 0.1f;

        /// <summary>
        /// Default camera field-of-view (in degrees).
        /// </summary>
        public const float DefaultFOV = 60.0f;

        /// <summary>
        /// Maximum camera far plane distance.
        /// </summary>
        public const float MaxFarPlane = 10000.0f;

        /// <summary>
        /// Minimum camera near plane distance.
        /// </summary>
        public const float MinNearPlane = 0.01f;

        /// <summary>
        /// Minimum degrees allowable for a field-of-view.
        /// </summary>
        public const float MinFOV = 1.0f;

        /// <summary>
        /// Maximum degrees allowable for a field-of-view.
        /// </summary>
        public const float MaxFOV = 175.0f;

        /// <summary>
        /// Maximum zoom/magnification level (32x)
        /// </summary>
        public const int MaxZoomLevel = 32;

        // ============================================================================================
        // World constants
        // ============================================================================================

        // ============================================================================================
        // Terrain constants
        // ============================================================================================
        
        public const int MaxTerrainScale = 4;
        public const int DefaultTerrainSmoothing = 10;

        /// <summary>
        /// Default elevation strength of Terrain.
        /// </summary>
        public const float DefaultTerrainElevStr = 6.0f;

        /// <summary>
        /// Must be a power of two value (i.e. 2, 4, 32, 128, etc..).
        /// Should probably never be lower than 32, especially for large terrains.
        /// </summary>
        public const int DefaultQuadTreeWidth = 128;

        /// <summary>
        /// Maximum width of quad-tree sections. Video cards which only support 16-bit index buffers are
        /// limited to 2^16 number of indices, or 65536 indices. This just happens to be 256x256 as well. To be safe
        /// we limit the size of the nodes to 128x128, because using 256x256 would be the EXACT limit for the card.
        /// If the user has a card that supports 32-bit we limit the size to 512x512, anything larger would not benefit
        /// from a quad-tree much anyway.
        /// </summary>
        public static int MinQuadTreeWidth = 512;
        public static float MinQuadTreeCubeCenterDistance = (float)Math.Sqrt(MinQuadTreeWidth * MinQuadTreeWidth * 3);

        // ============================================================================================
        // Input constants
        // ============================================================================================

        // Will comment these with XML once they're back in use... N.Foster
        public const float MinControlSensitivity = 0.1f;
        public const float MaxControlSensitivity = 100f;
        public const float MouseSensitivity = 200.0f;
        public const bool MouseDefaultVisible = false;

        public const int NumberGamepadButtons = 24;
        public const int NumberMouseButtons = 5;

        // ============================================================================================
        // HUD constants
        // ============================================================================================

        // ============================================================================================
        // Physics constants
        // ============================================================================================

        /// <summary>
        /// Default gravity velocity
        /// </summary>
        public readonly static Vector3 DefaultGravity = new Vector3(0.0f, -9.8f, 0.0f);

        /// <summary>
        /// This is the minimum required linear velocity a physics entity can have
        /// </summary>
        public const float MinLinearVelocityRequiredForDeactivation = 1.0f;

        /// <summary>
        /// This is the minimum required angular velocity a physics entity can have
        /// </summary>
        public const float MinAngularVelocityRequiredForDeactivation = 30.0f;

        /// <summary>
        /// This is the required amount of time that a physics entity must be below both the minimum
        /// linear and minimum angular velocities to stop moving and deactivate.
        /// </summary>
        public const float TimeRequiredForDeactivation = 1.0f;

        // ============================================================================================
        // Object pool constants
        // ============================================================================================

        /// <summary>
        /// The allocation size of the free items
        /// </summary>
        /// <remarks>This is the number of new items which will be created if no more are available</remarks>
        public const int PoolAllocationSize = 5;

        /// <summary>
        /// Incremental size of the lists used by the pool.
        /// </summary>
        public const int PoolIncrements = 20;

        // ============================================================================================
        // NavMesh constants
        // ============================================================================================

        /// <summary>
        /// The steepest a slope can be and still be traversable (upward)
        /// </summary>
        public const float SlopeSteepnessThreshold = 1.3f;
    }
}
