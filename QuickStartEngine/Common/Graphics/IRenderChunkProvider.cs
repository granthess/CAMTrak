//
// IRenderChunkProvider.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Interface for systems that provide render chunks for the graphics system.
    /// </summary>
    public interface IRenderChunkProvider
    {
        /// <summary>
        /// Query the system for potentially visible renderable chunks.
        /// </summary>
        /// <param name="pass">A descriptor for the rendering pass.</param>
        void QueryForChunks(ref RenderPassDesc pass);
    }
}
