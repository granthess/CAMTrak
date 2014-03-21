//
// GamepadHandler.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;

namespace QuickStart
{
    /// <summary>
    /// This class handles gamepad input and broadcasts all input messages.
    /// </summary>
    public class GamePadHandler : InputHandler
    {
        /// <summary>
        /// Stores info about each player's current gamepad state.
        /// </summary>
        private readonly GamePadState[] currentGamePadStates;

        /// <summary>
        /// Stores info about each player's previous gamepad state (last update loop's gamepad state).
        /// </summary>
        private readonly GamePadState[] previousGamePadStates;

        /// <summary>
        /// Stores info about buttons that were in a held state last frame. 
        /// </summary>
        private readonly Dictionary<int, bool>[] previousButtonsHeld;

        /// <summary>
        /// Maximum number of supported gamepads.
        /// </summary>
        private const int MaxNumGamepads = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The <see cref="QSGame"/> instance for the game</param>
        public GamePadHandler(QSGame game)
            : base(game)
        {
            this.currentGamePadStates = new GamePadState[MaxNumGamepads];
            this.previousGamePadStates = new GamePadState[MaxNumGamepads];

            previousButtonsHeld = new Dictionary<int, bool>[MaxNumGamepads];
            for (int i = 0; i < previousButtonsHeld.Length; i++)
            {
                previousButtonsHeld[i] = new Dictionary<int, bool>();
            }
        }

        /// <summary>
        /// Updates the gamepad state, retriving the current state
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        protected override void UpdateCore(GameTime gameTime)
        {
            // Check each player's controller (up to 4 controls)
            for (int i = 0; i < MaxNumGamepads; i++)
            {
                // If this player's control is connected, process its functions.
                if (GamePad.GetState((PlayerIndex)i).IsConnected)
                {
                    ProcessGamePad(i, gameTime);
                }
            }
        }

        /// <summary>
        /// Setup gamepad states.
        /// </summary>
        /// <param name="pIndex">Int index of the <see cref="PlayerIndex"/></param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void ProcessGamePad(int pIndex, GameTime gameTime)
        {
            this.currentGamePadStates[pIndex] = GamePad.GetState((PlayerIndex)pIndex);

            // Check all functions of the controller for currentPlayer
            ProcessButtons(pIndex, gameTime);
            ProcessTriggers(pIndex, gameTime);
            ProcessThumbsticks(pIndex, gameTime);

            // Update previous gamepad state before input processing finishes
            this.previousGamePadStates[pIndex] = this.currentGamePadStates[pIndex];
        }

        /// <summary>
        /// Process all <see cref="Gamepadbuttons"/>
        /// </summary>
        /// <param name="pIndex">Int index of the <see cref="PlayerIndex"/></param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void ProcessButtons(int pIndex, GameTime gameTime)
        {
            PlayerIndex player = (PlayerIndex)pIndex;
            GamePadState currentState = this.currentGamePadStates[pIndex];
            GamePadState previousState = this.previousGamePadStates[pIndex];

            ProcessButton(currentState.Buttons.A, previousState.Buttons.A, Buttons.A, pIndex, gameTime);
            ProcessButton(currentState.Buttons.B, previousState.Buttons.B, Buttons.B, pIndex, gameTime);
            ProcessButton(currentState.Buttons.X, previousState.Buttons.X, Buttons.X, pIndex, gameTime);
            ProcessButton(currentState.Buttons.Y, previousState.Buttons.Y, Buttons.Y, pIndex, gameTime);
            ProcessButton(currentState.Buttons.Back, previousState.Buttons.Back, Buttons.Back, pIndex, gameTime);
            ProcessButton(currentState.Buttons.Start, previousState.Buttons.Start, Buttons.Start, pIndex, gameTime);
            ProcessButton(currentState.Buttons.LeftShoulder, previousState.Buttons.LeftShoulder, Buttons.LeftShoulder, pIndex, gameTime);
            ProcessButton(currentState.Buttons.RightShoulder, previousState.Buttons.RightShoulder, Buttons.RightShoulder, pIndex, gameTime);
            ProcessButton(currentState.Buttons.LeftStick, previousState.Buttons.LeftStick, Buttons.LeftStick, pIndex, gameTime);
            ProcessButton(currentState.Buttons.RightStick, previousState.Buttons.RightStick, Buttons.RightStick, pIndex, gameTime);
            ProcessButton(currentState.DPad.Up, previousState.DPad.Up, Buttons.DPadUp, pIndex, gameTime);
            ProcessButton(currentState.DPad.Down, previousState.DPad.Down, Buttons.DPadDown, pIndex, gameTime);
            ProcessButton(currentState.DPad.Left, previousState.DPad.Left, Buttons.DPadLeft, pIndex, gameTime);
            ProcessButton(currentState.DPad.Right, previousState.DPad.Right, Buttons.DPadRight, pIndex, gameTime);
        }

        /// <summary>
        /// Process a gamepad button.
        /// </summary>
        /// <param name="currentButtonState">Current state of the button</param>
        /// <param name="previousButtonState">Previous state of the button</param>
        /// <param name="button">A gamepad button</param>
        /// <param name="pIndex">Player index for which player is pressing this button</param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void ProcessButton(ButtonState currentButtonState, ButtonState previousButtonState, Buttons button,
                                   int pIndex, GameTime gameTime)
        {
            PlayerIndex player = (PlayerIndex)pIndex;
            MessageType messageType = MessageType.Unknown;

            // If button has changed over the last frame.
            if (currentButtonState != previousButtonState)
            {
                if (currentGamePadStates[pIndex].IsButtonUp(button))
                {
                    messageType = MessageType.ButtonUp;
                }
                else
                {
                    messageType = MessageType.ButtonDown;
                }

                this.previousButtonsHeld[pIndex][(int)button] = false;
            }
            else
            {
                if (currentGamePadStates[pIndex].IsButtonDown(button))
                {
                    bool wasHeld = false;

                    this.previousButtonsHeld[pIndex].TryGetValue((int)button, out wasHeld);

                    if (wasHeld == false)
                    {
                        this.previousButtonsHeld[pIndex][(int)button] = true;
                        messageType = MessageType.ButtonHeld;
                    }
                }
            }

            if (messageType != MessageType.Unknown)
            {
                SendButtonMessage(messageType, button, player, gameTime);
            }
        }

        /// <summary>
        /// Process all <see cref="GamePadTriggers"/>
        /// </summary>
        /// <param name="pIndex">Int index of the <see cref="PlayerIndex"/></param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void ProcessTriggers(int pIndex, GameTime gameTime)
        {
            PlayerIndex player = (PlayerIndex)pIndex;
            GamePadState currentState = this.currentGamePadStates[pIndex];
            GamePadState previousState = this.previousGamePadStates[pIndex];

            // If left trigger has changed since the last update
            if (currentState.Triggers.Left != previousState.Triggers.Left)
            {
                // If trigger is in use
                if (currentState.Triggers.Left > 0.0f)
                {
                    SendTriggerMessage(GamePadInputSide.Left, currentState.Triggers.Left, player, gameTime);
                }
                else   // If not in use, then trigger has been released
                {
                    SendTriggerReleaseMessage(GamePadInputSide.Left, player, gameTime);
                }
            }

            // If right trigger has changed since the last update
            if (currentState.Triggers.Right != previousState.Triggers.Right)
            {
                if (currentState.Triggers.Right > 0.0f)
                {
                    SendTriggerMessage(GamePadInputSide.Right, currentState.Triggers.Right, player, gameTime);
                }
                else   // If not in use, then trigger has been released
                {
                    SendTriggerReleaseMessage(GamePadInputSide.Right, player, gameTime);
                }
            }
        }

        /// <summary>
        /// Process all <see cref="GamePadThumbsticks"/>
        /// </summary>
        /// <param name="pIndex">Int index of the <see cref="PlayerIndex"/></param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void ProcessThumbsticks(int pIndex, GameTime gameTime)
        {
            PlayerIndex player = (PlayerIndex)pIndex;
            GamePadState currentState = this.currentGamePadStates[pIndex];
            GamePadState previousState = this.previousGamePadStates[pIndex];

            // If left thumbstick has changed since the last update
            if (currentState.ThumbSticks.Left != previousState.ThumbSticks.Left)
            {
                // If thumbstick is in use
                if ((Math.Abs(currentState.ThumbSticks.Left.X) > 0.0f) || (Math.Abs(currentState.ThumbSticks.Left.Y) > 0.0f))
                {
                    SendThumbstickMessage(GamePadInputSide.Left, currentState.ThumbSticks.Left.X, currentState.ThumbSticks.Left.Y,
                                          player, gameTime);
                }
                else   // If not in use, then thumbstick has been released
                {
                    SendThumbstickReleaseMessage(GamePadInputSide.Left, player, gameTime);
                }
            }

            // If right thumbstick has changed since the last update
            if (currentState.ThumbSticks.Right != previousState.ThumbSticks.Right)
            {
                // If thumbstick is in use
                if ((Math.Abs(currentState.ThumbSticks.Right.X) > 0.0f) || (Math.Abs(currentState.ThumbSticks.Right.Y) > 0.0f))
                {
                    SendThumbstickMessage(GamePadInputSide.Right, -currentState.ThumbSticks.Right.X, currentState.ThumbSticks.Right.Y,
                                          player, gameTime);
                }
                else   // If not in use, then thumbstick has been released
                {
                    SendThumbstickReleaseMessage(GamePadInputSide.Right, player, gameTime);
                }
            }
        }

        /// <summary>
        /// Sends a message containing information about a specific button.
        /// </summary>
        /// <param name="buttonState"></param>
        /// <param name="button"></param>
        /// <param name="player">Player whose controller this message pertains to</param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void SendButtonMessage(MessageType buttonState, Buttons button, PlayerIndex player, GameTime gameTime)
        {
            switch (buttonState)
            {
                case MessageType.ButtonDown:
                    {
                        MsgGamePadButtonPressed buttonMessage = ObjectPool.Aquire<MsgGamePadButtonPressed>();
                        buttonMessage.Button = button;
                        buttonMessage.PlayerIndex = player;
                        buttonMessage.Time = gameTime;
                        this.Game.SendMessage(buttonMessage);
                    }
                    break;
                case MessageType.ButtonUp:
                    {
                        MsgGamePadButtonReleased buttonMessage = ObjectPool.Aquire<MsgGamePadButtonReleased>();
                        buttonMessage.Button = button;
                        buttonMessage.PlayerIndex = player;
                        buttonMessage.Time = gameTime;
                        this.Game.SendMessage(buttonMessage);
                    }
                    break;
                case MessageType.ButtonHeld:
                    {
                        MsgGamePadButtonHeld buttonMessage = ObjectPool.Aquire<MsgGamePadButtonHeld>();
                        buttonMessage.Button = button;
                        buttonMessage.PlayerIndex = player;
                        buttonMessage.Time = gameTime;
                        this.Game.SendMessage(buttonMessage);
                    }
                    break;
                default:
                    break;
            }   
        }

        /// <summary>
        /// Sends a message containing information about a specific trigger.
        /// </summary>
        /// <param name="triggerType">Type of trigger</param>
        /// <param name="triggerValue">Value of trigger (0.0f to 1.0f)</param>
        /// <param name="player">Player whose controller this message pertains to</param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void SendTriggerMessage(GamePadInputSide triggerType, float triggerValue, PlayerIndex player, GameTime gameTime)
        {
            MsgGamePadTrigger triggerMessage = ObjectPool.Aquire<MsgGamePadTrigger>();
            triggerMessage.GamePadInputSide = triggerType;
            triggerMessage.TriggerValue = triggerValue;
            triggerMessage.PlayerIndex = player;
            triggerMessage.Time = gameTime;
            this.Game.SendMessage(triggerMessage);
        }

        /// <summary>
        /// Sends a message meant to tell listeners to that a trigger has been recently released.
        /// </summary>
        /// <param name="triggerType">Type of trigger</param>
        /// <param name="player">Index of player's controller (1-4)</param>
        /// <param name="gameTime">Snapshot of game's timing state</param>
        private void SendTriggerReleaseMessage(GamePadInputSide triggerType, PlayerIndex player, GameTime gameTime)
        {
            MsgGamePadTriggerReleased triggerMessage = ObjectPool.Aquire<MsgGamePadTriggerReleased>();
            triggerMessage.GamePadInputSide = triggerType;
            triggerMessage.PlayerIndex = player;
            triggerMessage.Time = gameTime;
            this.Game.SendMessage(triggerMessage);
        }

        /// <summary>
        /// Sends a message containing information about a specific trigger
        /// </summary>
        /// <param name="stickType">Type of thumbstick (left or right)</param>
        /// <param name="XValue">Value of X-axis of thumbstick (-1.0f to 1.0f)</param>
        /// <param name="YValue">Value of Y-axis of thumbstick (-1.0f to 1.0f)</param>
        /// <param name="player">Player whose controller this message pertains to</param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void SendThumbstickMessage(GamePadInputSide stickType, float XValue, float YValue, PlayerIndex player, GameTime gameTime)
        {
            MsgGamePadThumbstick thumbstickMessage = ObjectPool.Aquire<MsgGamePadThumbstick>();
            thumbstickMessage.StickType = stickType;
            thumbstickMessage.StickValueX = XValue;
            thumbstickMessage.StickValueY = YValue;
            thumbstickMessage.PlayerIndex = player;
            thumbstickMessage.Time = gameTime;
            this.Game.QueueMessage(thumbstickMessage);
        }

        /// <summary>
        /// Sends a message to listeners that a thumbstick has recently been released.
        /// </summary>
        /// <param name="stickType">Type of thumbstick (left or right)</param>
        /// <param name="player">Player whose controller this message pertains to</param>
        /// <param name="gameTime">Snapshot of the game's timing state</param>
        private void SendThumbstickReleaseMessage(GamePadInputSide stickType, PlayerIndex player, GameTime gameTime)
        {
            MsgGamePadThumbstickReleased thumbstickMessage = ObjectPool.Aquire<MsgGamePadThumbstickReleased>();
            thumbstickMessage.StickType = stickType;
            thumbstickMessage.PlayerIndex = player;
            thumbstickMessage.Time = gameTime;
            this.Game.QueueMessage(thumbstickMessage);
        }
    }
}
