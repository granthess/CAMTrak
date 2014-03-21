//
// IScreen.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Compositor
{
    /// <summary>
    /// Interface definition for a screen that is part of the compositor chain.
    /// </summary>
    public interface IScreen
    {
#if false
        /// <summary>
        /// Retrieves the width of the screen.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Retrieves the height of the screen.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Retrives the horizontal (X) position of the screen relative to the top-left corner.
        /// </summary>
        int X { get; }

        /// <summary>
        /// Retrieves the vertical (Y) position of the screen relative to the top-left corner.
        /// </summary>
        int Y { get; }
#endif
        /// <summary>
        /// Retrieves the name of the screen.  This name must be unique across all screens.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Loads all content required by the screen.
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> instance for loading all content for this screen.</param>
        void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor);

        /// <summary>
        /// Unloads all content previously loaded by the screen.
        /// </summary>
        void UnloadContent();

        /// <summary>
        /// Draws the screen to the currently bound render target.
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> instance to use for 2D drawing.</param>
        /// <param name="previousScreen">The <see cref="Texture2D"/> for the previous screen's render target, if NeedBackgroundAsTexture is true.  null, otherwise.  NOTE: Currently unimplemented!</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        void DrawScreen(SpriteBatch batch, Texture2D previousScreen, GameTime gameTime);
    }
}
