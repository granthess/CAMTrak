/*
 * CompiledMaterialProcessor.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace QuickStart.ContentPipeline.Material
{
    /// <summary>
    /// Content pipeline processor for QuickStart compiled materials.
    /// </summary>
    [ContentProcessor(DisplayName="QuickStart Material Processor")]
    public class CompiledMaterialProcessor : ContentProcessor<CompiledMaterial, CompiledMaterial>
    {
        /// <summary>
        /// Processes a compiled material and prepares it for disk writing.
        /// </summary>
        /// <param name="input">The CompiledMaterial instance to process.</param>
        /// <param name="context">The current content processor context.</param>
        /// <returns></returns>
        public override CompiledMaterial Process(CompiledMaterial input, ContentProcessorContext context)
        {
            input.compiledEffect = context.BuildAsset<EffectContent, CompiledEffectContent>(new ExternalReference<EffectContent>(string.Format("{0}{1}.fx", Path.GetFullPath("Effects/"), input.effect)), "EffectProcessor");

            // TODO: ExternalReference to textures, or leave management to run-time renderer?

            return input;
        }
    }
}
