// GuiManager.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart.Graphics;

namespace QuickStart
{
    /// <summary>
    /// A helper class used to load models of different types
    /// </summary>
    public sealed class ModelLoader
    {
        private ContentManager content;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> for the game</param>
        public ModelLoader(ContentManager contentManager)
        {
            this.content = contentManager;
        }

        /// <summary>
        /// Loads a model of the type StaticModel.
        /// </summary>
        /// <param name="modelPath">File path of model to load</param>
        /// <returns>A loaded StaticModel</returns>
        public StaticModel LoadStaticModel(string modelPath)
        {
            StaticModel newModel = content.Load<StaticModel>(modelPath);
            
            return newModel;
        }
    }
}
