// RenderPassDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using QuickStart.Components;
using QuickStart.Entities;

namespace QuickStart.Graphics
{
    public enum RenderPassType
    {
        Normal = 0,
        OpaqueOnly,
        SemiTransparentOnly,
        WaterReflection,        
        WaterRefraction,
        SkyOnly,
        ShadowMapCreate,
        GUIOnly,
    }

    /// <summary>
    /// Descriptor for one rendering pass of a graphics frame.
    /// </summary>
    public struct RenderPassDesc
    {
        /// <summary>
        /// The <see cref="BaseEntity"/> defining the view to render.
        /// </summary>
        public CameraComponent RenderCamera;

        /// <summary>
        /// Describes what kind of rendering pass this is.
        /// </summary>
        public RenderPassType Type;

        /// <summary>
        /// Holds details about the view frustum during this render pass.
        /// </summary>
        public BoundingFrustum ViewFrustum;

        /// <summary>
        /// Is non-null when a clipping plane exists.
        /// </summary>
        public Plane ClippingPlane;

        /// <summary>
        /// LOD that this render pass prefers.
        /// </summary>
        public LOD RequestedLOD;

        /// <summary>
        /// Contains the elevation of the water, if there is any.
        /// </summary>
        public float WaterElevation;

        /// <summary>
        /// Whether or not we just want geometry chunks during this pass.
        /// </summary>
        public bool GeometryChunksOnlyThisPass;

        /// <summary>
        /// Whether or not we exclude geometry chunks during this pass.
        /// </summary>
        public bool GeometryChunksExcludedThisPass;
    }
}
