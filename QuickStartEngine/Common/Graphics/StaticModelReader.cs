// StaticModelReader.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Creates a StaticModel instance from a content pipeline data file.
    /// </summary>
    internal class StaticModelReader : ContentTypeReader<StaticModel>
    {
        /// <summary>
        /// Creates a <see cref="StaticModel"/> instance from a content pipeline data file.
        /// </summary>
        /// <param name="input">The <see cref="ContentReader"/> instance for the data file.</param>
        /// <param name="existingInstance">The existing <see cref="StaticModel"/> instance to replace.</param>
        /// <returns>The <see cref="StaticModel"/> instance read from the data file.</returns>
        protected override StaticModel Read(ContentReader input, StaticModel existingInstance)
        {
            return new StaticModel(input);
        }
    }
}
