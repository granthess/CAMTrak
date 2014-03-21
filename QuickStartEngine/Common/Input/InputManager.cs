// InputManager.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace QuickStart
{
    /// <summary>
    /// This class manages all input
    /// </summary>
    public class InputManager : BaseManager
    {
        /// <summary>
        /// Gets the list of <see cref="InputHandler"/>'s
        /// </summary>
        public List<InputHandler> Handlers
        {
            get { return handlers; }
        }
        private readonly List<InputHandler> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The <see cref="QSGame"/> instance for the game</param>
        public InputManager(QSGame game)
            : base(game)
        {
            this.UpdateOrder = Int32.MaxValue;
            this.handlers = new List<InputHandler>();
        }

        /// <summary>
        /// Initializes the manager and adds initializing for contained managers
        /// </summary>
        protected override void InitializeCore()
        {
            this.handlers.AddRange(this.Game.ConfigurationManager.GetInputHandlers(this.Game));
            for (int i = this.handlers.Count - 1; i >= 0; i--)
            {
                InputHandler handler = this.handlers[i];
                handler.Initialize();
            }
        }

        /// <summary>
        /// Updates the manager and also updates all managers
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        protected override void UpdateCore(GameTime gameTime)
        {
            // Do not update the any handlers if the game does not have focus
            if (this.Game.IsActive == false)
            {
                return;
            }

            for (int i = this.handlers.Count - 1; i >= 0; i--)
            {
                InputHandler handler = this.handlers[i];
                handler.Update(gameTime);
            }
        }
    }
}
