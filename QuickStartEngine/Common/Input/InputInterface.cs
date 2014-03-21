//
// CameraInterface.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;

namespace QuickStart.Interfaces
{
    public struct CursorInfo
    {
        public Vector2 Position;
        public bool IsVisible;
    }

    /// <summary>
    /// All camera messages will be handled by the camera interface. It will allow
    /// anyone in the code that has access to this interface to request camera information.
    /// </summary>
    public class InputInterface : QSInterface
    {
        /// <summary>
        /// Create a <see cref="CameraInterface"/>.
        /// </summary>
        public InputInterface(QSGame game)
            : base(game, InterfaceType.Input)
        {
            this.game.GameMessage += this.Game_GameMessage;
        }

        public override void Shutdown()
        {
        }

        public bool IsGamepadConnected(PlayerIndex index)
        {
            GamePadState state = GamePad.GetState(index);
            return state.IsConnected;
        }

        public CursorInfo GetCursorInfo()
        {
            MouseState state = Mouse.GetState();

            CursorInfo info;
            info.Position = new Vector2(state.X, state.Y);
            info.IsVisible = this.game.IsMouseVisible;
            
            return info;
        }

        /// <summary>
        /// Message listener for messages that are not directed at any particular Entity or Interface.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        protected virtual void Game_GameMessage(IMessage message)
        {
            ExecuteMessage(message);
        }

        /// <summary>
        /// Message handler for all incoming messages.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            return false;
        }
    }
}
