// StaticModelWriter.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace QuickStart.ContentPipeline.Model
{
    /// <summary>
    /// Writes a StaticModelContent instance to a content pipeline data file.
    /// </summary>
    [ContentTypeWriter]
    public class StaticModelWriter : ContentTypeWriter<StaticModelContent>
    {
        /// <summary>
        /// Writes the StaticModelContent instance to a content pipeline data file.
        /// </summary>
        /// <param name="output">The ContentWriter instance for the data file.</param>
        /// <param name="value">The StaticModelContent instance to write.</param>
        protected override void Write(ContentWriter output, StaticModelContent value)
        {
            value.Write(output);
        }

        /// <summary>
        /// Retrieves the type string for the content reader responsible for reading static model content files.
        /// </summary>
        /// <param name="targetPlatform">The target platform for this static model.</param>
        /// <returns>The type string for the type reader.</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "QuickStart.Graphics.StaticModelReader, QuickStart";
        }
    }
}
