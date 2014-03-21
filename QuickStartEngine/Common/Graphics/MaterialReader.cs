// MaterialReader.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Graphics
{
    /// <summary>
    /// Content type reader for materials.
    /// </summary>
    public class MaterialReader : ContentTypeReader<Material>
    {
        /// <summary>
        /// Creates a <see cref="Material"/> instance from an XNB data file.
        /// </summary>
        /// <param name="input"><see cref="ContentReader"/> instance for the data file.</param>
        /// <param name="existingInstance">Reference to an existing <see cref="Material"/> instance.</param>
        /// <returns>The <see cref="Material"/> instance read from the data file.</returns>
        protected override Material Read(ContentReader input, Material existingInstance)
        {
            Material material = new Material();
            material.LoadFromContent(input);

            return material;
        }
    }
}
