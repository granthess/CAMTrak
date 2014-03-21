//
// FPSScreen.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.using System;
//

using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Compositor
{
    /// <summary>
    /// Compositor screen for a simple frames-per-second counter.
    /// </summary>
    public class FPSScreen : IScreen
    {
        /// <summary>
        /// Retrieves the horizontal position of the FPS counter.
        /// </summary>
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Retrieves the vertical position of the FPS counter.
        /// </summary>
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// Retrieves the name of the FPS counter.  Currently, "FPS Counter"
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private int x;
        private int y;
        private ContentManager content;
        private SpriteFont font;
        private int frameCount;
        private string fpsString;
        private Stopwatch timer;
        private QSGame game;

        private const string name = "FPS Counter";

        /// <summary>
        /// Constructs a new FPS counter.
        /// </summary>
        /// <param name="xPos">The horizontal position of the FPS counter.</param>
        /// <param name="yPos">The vertical position of the FPS counter.</param>
        public FPSScreen(int xPos, int yPos, QSGame game)
        {
            this.game = game;
            this.fpsString = "";
            this.frameCount = 0;

            this.x = xPos;
            this.y = yPos;

            timer = new Stopwatch();
            timer.Start();
        }

        /// <summary>
        /// Loads all content needed by the FPS counter.
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> instance to use for all content loading.</param>
        public void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor)
        {
            this.content = contentManager;
            this.content.RootDirectory = "Content";

            this.font = this.content.Load<SpriteFont>("Textures/Fonts/CourierNew18PtBold");
        }

        /// <summary>
        /// Unloads all previously loaded content.
        /// </summary>
        public void UnloadContent()
        {
            this.content.Unload();
        }

        /// <summary>
        /// Draws the FPS counter.
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> to use for 2D drawing.</param>
        /// <param name="background">The previous screen's output as a texture, if NeedBackgroundAsTexture is true.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawScreen(SpriteBatch batch, Texture2D background, GameTime gameTime)
        {
            ++frameCount;

            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // Update four times per second
            if (timer.ElapsedMilliseconds > 250.0)
            {
                StringBuilder sb = new StringBuilder();

                long fps = frameCount * 1000 / timer.ElapsedMilliseconds;

                sb.AppendLine("FPS: " + fps);                

                frameCount = 0;

                timer.Reset();
                timer.Start();

                fpsString = sb.ToString();
            }

            batch.DrawString(font, fpsString, new Vector2(this.x, this.y), Color.White);

            batch.End();
        }
    }
}
