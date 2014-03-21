//
// Compositor.cs
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
    /// Screen compositor used to generate the final per-frame back buffer from various 2D and 3D sources.  This class controls the over and composition of per-frame rendering.
    /// </summary>
    public class ScreenCompositor
    {
        private SpriteBatch batch;
        private List<IScreen> screens;
        private QSGame game;
        private bool afterLoadContent;

        /// <summary>
        /// Constructs a new instance of a compositor.
        /// </summary>
        public ScreenCompositor(QSGame gameInstance)
        {
            this.screens = new List<IScreen>();
            this.game = gameInstance;

            this.afterLoadContent = false;
        }

        /// <summary>
        /// Inserts a screen at the head or end of the compositor chain.
        /// </summary>
        /// <param name="screen">The screen to insert.</param>
        /// <param name="head">True if screen should be inserted at the head of the compositor chain, false if screen should be inserted at end.</param>
        public void InsertScreen(IScreen screen, bool head)
        {
            if(head)
            {
                this.screens.Insert(0, screen);
            }
            else
            {
                this.screens.Add(screen);
            }

            if(this.afterLoadContent)
            {
                screen.LoadContent(false, new ContentManager(this.game.Services), true);
            }
        }

        /// <summary>
        /// Inserts a screen after the screen referenced by <paramref name="previous"/> in the compositor chain.
        /// </summary>
        /// <param name="screen">The screen to insert.</param>
        /// <param name="previous">The reference to the screen previous to the new screen.  If null, the new screen will be inserted at the head of the composition chain.</param>
        public void InsertScreen(IScreen screen, IScreen previous)
        {
            int idx = this.screens.IndexOf(previous);

            if(idx == -1)
            {
                // @todo: Proper error logging.
                // Insert screen at end of chain, because previous was not found.
                this.screens.Add(screen);
            }
            else
            {
                this.screens.Insert(idx + 1, screen);
            }


            if(this.afterLoadContent)
            {
                screen.LoadContent(false, new ContentManager(this.game.Services), true);
            }
        }

        /// <summary>
        /// Inserts a screen after the screen whose name is given by <paramref name="previous"/> in the compositor chain.
        /// </summary>
        /// <param name="screen">The screen to insert.</param>
        /// <param name="previous">The name of the screen previous to the new screen.</param>
        public void InsertScreen(IScreen screen, string previous)
        {
            int i;

            for(i = 0; i < this.screens.Count; ++i)
            {
                if(this.screens[i].Name == previous)
                {
                    this.screens.Insert(i + 1, screen);
                    break;
                }
            }

            if(i == this.screens.Count)
            {
                // @todo: Proper error logging.
                // Insert screen at end of chain, because previous was not found.
                this.screens.Add(screen);
            }

            if(this.afterLoadContent)
            {
                screen.LoadContent(false, new ContentManager(this.game.Services), true);
            }
        }

        /// <summary>
        /// Removes the given screen from the compositor chain.
        /// </summary>
        /// <param name="screen">The screen to remove.</param>
        public void RemoveScreen(IScreen screen)
        {
            int idx = screens.IndexOf(screen);

            if(idx != -1)
            {
                screens[idx].UnloadContent();
                screens.RemoveAt(idx);
            }
        }

        /// <summary>
        /// Remove the screen with the given name.
        /// </summary>
        /// <param name="name">The screen to remove.</param>
        public void RemoveScreen(string name)
        {
            int idx;

            for(idx = 0; idx < screens.Count; ++idx)
            {
                if(screens[idx].Name == name)
                {
                    break;
                }
            }

            if(idx != screens.Count)
            {
                screens[idx].UnloadContent();
                screens.RemoveAt(idx);
            }
        }

        /// <summary>
        /// Loads all content required by the compositor.
        /// </summary>
        public void LoadContent(bool isReload)
        {
            this.batch = new SpriteBatch(this.game.GraphicsDevice);

            for(int i = 0; i < screens.Count; ++i)
            {
                screens[i].LoadContent(isReload, new ContentManager(this.game.Services), true);
            }

            this.afterLoadContent = true;
        }

        /// <summary>
        /// Unloads all previously loaded content.
        /// </summary>
        public void UnloadContent()
        {
            this.afterLoadContent = false;

            for(int i = 0; i < screens.Count; ++i)
            {
                screens[i].UnloadContent();
            }
        }

        /// <summary>
        /// Draws the entire compositor chain in order, screen by screen.
        /// </summary>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawCompositorChain(GameTime gameTime)
        {
            this.game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            for(int i = 0; i < this.screens.Count; ++i)
            {
                this.screens[i].DrawScreen(batch, null, gameTime);
            }
        }
    }
}
