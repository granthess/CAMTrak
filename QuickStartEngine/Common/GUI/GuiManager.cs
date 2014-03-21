using System;
using System.Collections.Generic;
using System.Linq;
// GuiManager.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using QuickStart;
using QuickStart.Graphics;

namespace QuickStart.GUI
{
    public class GuiManager : BaseManager 
    {        
        private QSGame game;

        private Nuclex.UserInterface.GuiManager gui;
        private Nuclex.Input.InputManager input;

        public Nuclex.UserInterface.Screen Screen
        {
            get { return this.gui.Screen; }
            set { this.gui.Screen = value; }
        }

        public GuiManager(QSGame game)
            : base(game)
        {
            this.game = game;

            // Add this to the game service list
            this.game.Services.AddService(typeof(GuiManager), this);

            this.gui = new Nuclex.UserInterface.GuiManager(this.game.Services);
            this.input = new Nuclex.Input.InputManager(this.game.Services, game.Window.Handle);
            
            // The Nuclex UserInterface requires its own input handling setup. For now
            // this engine will use Nuclex's input handling for GUI, but it may be replaced
            // in the future if needed.
            this.game.Services.AddService(typeof(Nuclex.Input.InputManager), this.input);
        }

        protected override void InitializeCore()
        {            
            this.gui.Initialize();

            Viewport viewport = this.game.GraphicsDevice.Viewport;
            Screen mainScreen = new Screen(viewport.Width, viewport.Height);
            this.gui.Screen = mainScreen;

            mainScreen.Desktop.Bounds = new UniRectangle(
                new UniScalar(0.0f, 0.0f), new UniScalar(0.0f, 0.0f), // x and y
                new UniScalar(1.0f, 0.0f), new UniScalar(1.0f, 0.0f) // width and height
            );
        }

        protected override void UpdateCore(GameTime gameTime)
        {            
            this.input.Update();
            this.gui.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            this.gui.Draw(gameTime);
        }       
    }
}
