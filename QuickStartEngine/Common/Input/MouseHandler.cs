// MouseHandler.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;

namespace QuickStart
{
    /// <summary>
    /// This class handles keyboard input
    /// </summary>
    public class MouseHandler : InputHandler
    {       
        private GameTime gameTime;

        /// <summary>
        /// List of <see cref="MouseButton"/> and their last <see cref="ButtonState"/>
        /// </summary>
        private readonly Dictionary<MouseButton, ButtonState> lastState = new Dictionary<MouseButton, ButtonState>(6, new MouseButtonComparer());

        /// <summary>
        /// Stores info about buttons that were in a held state last frame. 
        /// </summary>
        private Dictionary<MouseButton, bool> previousButtonsHeld = new Dictionary<MouseButton, bool>(6);

        /// <summary>
        /// We want releative values not cumulative
        /// </summary>
        private int lastScrollValue;

        /// <summary>
        /// X position of the mouse cursor last frame (in screen space)
        /// </summary>
        private int lastPositionX;

        /// <summary>
        /// Y position of the mouse cursor last frame (in screen space)
        /// </summary>
        private int lastPositionY;

        /// <summary>
        /// Last visible X position of the mouse cursor (in screen space)
        /// </summary>
        private int lastVisiblePositionX;

        /// <summary>
        /// Last visible X position of the mouse cursor (in screen space)
        /// </summary>
        private int lastVisiblePositionY;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The <see cref="QSGame"/> instance for the game</param>
        public MouseHandler(QSGame game)
            : base(game)
        {
            MouseState state = Mouse.GetState();
            this.lastState[MouseButton.Left] = state.LeftButton;
            this.lastState[MouseButton.Middle] = state.MiddleButton;
            this.lastState[MouseButton.Right] = state.RightButton;
            this.lastState[MouseButton.XButton1] = state.XButton1;
            this.lastState[MouseButton.XButton2] = state.XButton2;

            this.lastScrollValue = state.ScrollWheelValue;

            lastPositionX = lastPositionY = 100;
            Mouse.SetPosition(lastPositionX, lastPositionY);

            this.Game.Activated += this.Game_Activated;
            this.Game.Deactivated += this.Game_Deactivated;
            this.Game.GameMessage += this.Game_GameMessage;
        }

        /// <summary>
        /// Updates the mouse state, retriving the current state and sending the needed <see cref="Message<???>"/>
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        protected override void UpdateCore(GameTime gameTime)
        {
            this.gameTime = gameTime;

            MouseState state = Mouse.GetState();
            this.ValidateButtons(state);

            this.ValidateScroll(state);

            this.ValidatePosition(state);

            this.gameTime = null;
        }

        /// <summary>
        /// Handles the <see cref="Game.Deactivate"/> event
        /// </summary>
        /// <param name="sender">The <see cref="Game"/> sending the event</param>
        /// <param name="e">Empty event arguments</param>
        private void Game_Deactivated(object sender, EventArgs e)
        {
            this.Game.SetMouseVisibility(true);
        }

        /// <summary>
        /// Handles the <see cref="Game.Activate"/> event
        /// </summary>
        /// <param name="sender">The <see cref="Game"/> sending the event</param>
        /// <param name="e">Empty event arguments</param>
        private void Game_Activated(object sender, EventArgs e)
        {
            if (!this.Game.IsMouseVisible)
            {
                Mouse.SetPosition(100, 100);
            }            
        }

        /// <summary>
        /// Validates the mouse scroll value and sends a <see cref="Message<int>"/> if changed
        /// </summary>
        /// <param name="state">Current <see cref="MouseState"/></param>
        private void ValidateScroll(MouseState state)
        {
            if (state.ScrollWheelValue != this.lastScrollValue)
            {
                MsgMouseScroll scrollMessage = ObjectPool.Aquire<MsgMouseScroll>();
                scrollMessage.ScrollWheelDelta = state.ScrollWheelValue - this.lastScrollValue;
                scrollMessage.Time = this.gameTime;
                scrollMessage.Type = MessageType.MouseScroll;                
                this.Game.SendMessage(scrollMessage);

                this.lastScrollValue = state.ScrollWheelValue;
            }
        }

        /// <summary>
        /// Validates the mouse position and sends a <see cref="Message<Vector2>"/> if changed
        /// </summary>
        /// <param name="state">Current <see cref="MouseState"/></param>
        private void ValidatePosition(MouseState state)
        {
            // If the mouse hasn't moved and hasn't switched visibility states then just return
            if (state.X == lastPositionX && state.Y == lastPositionY)
            {
                return;
            }

            int newPosX = state.X;
            int newPosY = state.Y;

            lastPositionX = newPosX;
            lastPositionY = newPosY;

            if (!this.Game.IsMouseVisible)
            {
                // Keep the mouse position at 100, 100 while it is not visible
                lastPositionX = lastPositionY = 100;
                Mouse.SetPosition(lastPositionX, lastPositionY);                           
            }

            MsgMouseMove moveMessage = ObjectPool.Aquire<MsgMouseMove>();            
            moveMessage.Time = this.gameTime;
            moveMessage.PositionDelta = new Vector2((float)(newPosX - lastPositionX) / QSConstants.MouseSensitivity, (float)(newPosY - lastPositionY) / QSConstants.MouseSensitivity);
            this.Game.SendMessage(moveMessage);
        }

        /// <summary>
        /// Validates the mouse button states and sends a <see cref="Message<MouseButton>"/> if changed
        /// </summary>
        /// <param name="state">Current <see cref="MouseState"/></param>
        private void ValidateButtons(MouseState state)
        {
            this.ValidateButtonState(MouseButton.Left, state.LeftButton);
            this.ValidateButtonState(MouseButton.Middle, state.MiddleButton);
            this.ValidateButtonState(MouseButton.Right, state.RightButton);
            this.ValidateButtonState(MouseButton.XButton1, state.XButton1);
            this.ValidateButtonState(MouseButton.XButton2, state.XButton2);
        }

        /// <summary>
        /// Validates the state of a specific mouse button
        /// </summary>
        /// <param name="currentButton">The <see cref="MouseButton"/> which should be tested</param>
        /// <param name="currentState">The current <see cref="ButtonState"/> of the button</param>
        private void ValidateButtonState(MouseButton currentButton, ButtonState currentState)
        {     
            // If there is a change in the state the button has just been pressed or released
            if (currentState != this.lastState[currentButton])
            {
                // Determine the state of the button
                switch (currentState)
                {
                    case ButtonState.Pressed:
                        {
                            MsgMouseButtonPressed buttonMessage = ObjectPool.Aquire<MsgMouseButtonPressed>();
                            buttonMessage.Time = this.gameTime;
                            buttonMessage.Button = currentButton;
                            this.Game.SendMessage(buttonMessage);
                        }
                        break;

                    case ButtonState.Released:
                        {
                            MsgMouseButtonReleased buttonMessage = ObjectPool.Aquire<MsgMouseButtonReleased>();
                            buttonMessage.Time = this.gameTime;
                            buttonMessage.Button = currentButton;
                            this.Game.SendMessage(buttonMessage);
                        }
                        break;
                }

                this.lastState[currentButton] = currentState;                

                previousButtonsHeld[currentButton] = false;
            }
            else
            {
                // If the state here is pressed then send the MouseHeld message
                if (currentState == ButtonState.Pressed)
                {
                    bool wasHeld = false;

                    previousButtonsHeld.TryGetValue(currentButton, out wasHeld);

                    if (wasHeld == false)
                    {
                        previousButtonsHeld[currentButton] = true;

                        MsgMouseButtonHeld buttonMessage = ObjectPool.Aquire<MsgMouseButtonHeld>();
                        buttonMessage.Time = this.gameTime;                        
                        buttonMessage.Button = currentButton;
                        this.Game.SendMessage(buttonMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Message handler for the input polling handler.
        /// </summary>
        /// <param name="message">Incoming message</param>
        private void Game_GameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.MouseCursorStateChange:
                    MsgMouseCursorStateChange msgCursorChange = message as MsgMouseCursorStateChange;
                    message.TypeCheck(msgCursorChange);

                    if (!msgCursorChange.CursorVisible)
                    {
                        MouseState currentState = Mouse.GetState();
                        lastVisiblePositionX = currentState.X;
                        lastVisiblePositionY = currentState.Y;

                        // Set to a safe position to hold while invisible
                        lastPositionX = lastPositionY = 100;
                        Mouse.SetPosition(lastPositionX, lastPositionY);
                    }
                    else
                    {
                        // Return mouse to position it was last in when visible
                        lastPositionX = lastVisiblePositionX;
                        lastPositionY = lastVisiblePositionY;
                        Mouse.SetPosition(lastVisiblePositionX, lastVisiblePositionY);
                    }
                    break;
            }
        }
    }
}
