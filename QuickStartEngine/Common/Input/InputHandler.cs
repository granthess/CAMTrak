// InputHandler.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace QuickStart
{
    /// <summary>
    /// Base class for all input handlers
    /// </summary>
    public abstract class InputHandler : IUpdateable
    {
        private bool enabled;
        private int updateOrder;
        private QSGame game;

        /// <summary>
        /// Gets/sets the enable state
        /// </summary>
        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = true;
                this.OnEnabledChanged();
                this.OnPropertyChanged(PropertyName.Enabled);
            }
        }

        /// <summary>
        /// Gets the <see cref="QSGame"/> instance
        /// </summary>
        public QSGame Game
        {
            get { return this.game; }
        }

        /// <summary>
        /// Gets/Sets the update order
        /// </summary>
        public int UpdateOrder
        {
            get { return this.updateOrder; }
            set
            {
                this.updateOrder = value;
                this.OnUpdateOrderChanged();
                this.OnPropertyChanged(PropertyName.UpdateOrder);
            }
        }

        /// <summary>
        /// This event is raised when the <see cref="BaseManager.Enabled"/> property changes
        /// </summary>
        public event EventHandler<EventArgs> EnabledChanged;

        /// <summary>
        /// This event is raised when the <see cref="BaseManager.UpdateOrder"/> property changes
        /// </summary>
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>
        /// This event is raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="game">The <see cref="QSGame"/> instance for the game</param>
        public InputHandler(QSGame game)
        {
            this.enabled = false;
            this.updateOrder = -1;
            this.game = game;
        }

        /// <summary>
        /// Initializes the handler
        /// </summary>
        public void Initialize()
        {
            this.InitializeCore();
        }

        /// <summary>
        /// This message is invoked during initialize 
        /// </summary>
        protected virtual void InitializeCore()
        {
        }

        /// <summary>
        /// Updates the handler
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        public void Update(GameTime gameTime)
        {
            this.UpdateCore(gameTime);
        }

        /// <summary>
        /// This method is invoked during update
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        protected virtual void UpdateCore(GameTime gameTime)
        {
        }

        /// <summary>
        /// Invoked the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed</param>
        /// <remarks>Use the <see cref="PropertyName"/> class for retrieving core property names</remarks>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Invoked the <see cref="EnabledChanged"/> event
        /// </summary>
        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Invoked the <see cref="UpdateOrderChanged"/> event
        /// </summary>
        protected virtual void OnUpdateOrderChanged()
        {
            if (this.UpdateOrderChanged != null)
            {
                this.UpdateOrderChanged(this, EventArgs.Empty);
            }
        }
    }
}
