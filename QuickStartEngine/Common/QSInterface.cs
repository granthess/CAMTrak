/*
 * QSInterface.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace QuickStart.Interfaces
{
    public enum InterfaceType
    {
        Invalid = 0,
        SceneManager,
        Camera,
        Physics,
        Graphics,
        Input,
    }

    /// <summary>
    /// Generic interface from which all interfaces are derived.
    /// </summary>
    /// <remarks>@todo: A more robust definition should be placed here later. - N.Foster</remarks>
    public abstract class QSInterface
    {
        protected QSGame game;

        public InterfaceType InterfaceID
        {
            get { return interfaceID; }
        }
        private InterfaceType interfaceID;

        /// <summary>
        /// Called from a derived class to construct an interface.
        /// </summary>
        public QSInterface(QSGame game, InterfaceType interfaceID)
        {
            this.game = game;

            this.interfaceID = interfaceID;
        }

        public abstract void Shutdown();

        /// <summary>
        /// Updates interface.
        /// </summary>
        /// <param name="gameTime">Contains timer information</param>
        public virtual void Update(GameTime gameTime)
        {
            // This is only reached for interfaces without update methods.
        }

        /// <summary>
        /// Sends a message directly to an interface.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <returns>Whether or not the message was handled by the interface</returns>
        public virtual bool ExecuteMessage(IMessage message)
        {
            // This is only reached for interfaces without update methods.
            return false;
        }
    }
}
