// InputPollHandler.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;

namespace QuickStart.Input
{
    public class InputPollingHandler
    {
        private QSGame game;

        /// <summary>
        /// Stores all <see cref="Buttons"/> that are specifically listened for.
        /// </summary>
        private Dictionary<Buttons, InputButton>[] gamepadButtons;

        /// <summary>
        /// Stores all <see cref="Keys"/> that are specifically listened for.
        /// </summary>
        private Dictionary<Keys, InputButton> keys;

        /// <summary>
        /// Stores all <see cref="MouseButton"/> that are specifically listened for.
        /// </summary>
        private Dictionary<MouseButton, InputButton> mouseButtons;

        /// <summary>
        /// Stores information about the positions of both thumbsticks, for all controllers.
        /// </summary>
        private Vector2[,] thumbsticks;

        /// <summary>
        /// Stores information about the previous positions of both thumbsticks, for all controllers.
        /// </summary>
        private Vector2[,] previousThumbsticks;

        /// <summary>
        /// Stores information about the positions of both triggers, for all controllers.
        /// </summary>
        private float[,] triggers;

        /// <summary>
        /// Stores information about the previous positions of both triggers, for all controllers.
        /// </summary>
        private float[,] previousTriggers;

        /// <summary>
        /// Create an instance of an input polling handler
        /// </summary>
        /// <param name="Game"></param>
        public InputPollingHandler(QSGame Game)
        {
            this.game = Game;

            this.gamepadButtons = new Dictionary<Buttons, InputButton>[4];
            for (int i = 0; i < gamepadButtons.Length; ++i)
            {
                gamepadButtons[i] = new Dictionary<Buttons, InputButton>();
            }

            this.keys = new Dictionary<Keys, InputButton>();
            this.mouseButtons = new Dictionary<MouseButton, InputButton>();

            this.thumbsticks = new Vector2[4, 2];       // 4 players, 2 thumbsticks
            this.previousThumbsticks = new Vector2[4, 2];
            this.triggers = new float[4, 2];
            this.previousTriggers = new float[4, 2];

            this.game.GameMessage += this.Game_GameMessage;
        }

        /// <summary>
        /// Add an input listener for a gamepad button.
        /// </summary>
        /// <param name="buttonType">Button to listen for</param>
        /// <param name="pIndex">Player controller belongs to</param>
        public void AddInputListener(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton newButton = new InputButton();
            this.gamepadButtons[(int)pIndex].Add(buttonType, newButton);
        }

        /// <summary>
        /// Add an input listener for a keyboard key.
        /// </summary>
        /// <param name="keyType">Key to listen for</param>
        public void AddInputListener(Keys keyType)
        {
            InputButton newButton = new InputButton();
            this.keys.Add(keyType, newButton);
        }

        /// <summary>
        /// Add an input listener for a mouse button.
        /// </summary>
        /// <param name="mouseButtonType">Mouse button to listen for</param>
        public void AddInputListener(MouseButton mouseButtonType)
        {
            InputButton newButton = new InputButton();
            this.mouseButtons.Add(mouseButtonType, newButton);
        }

        /// <summary>
        /// Acquire a gamepad button
        /// </summary>
        /// <param name="buttonType">Gamepad button to acquire</param>
        /// <param name="pIndex">Player index of controller</param>
        /// <param name="buttonRequested">Returns the <see cref="InputButton"/> requested</param>
        /// <returns>True if that button was registered for listening</returns>
        private bool ButtonFromType(Buttons buttonType, PlayerIndex pIndex, out InputButton buttonRequested)
        {
            return this.gamepadButtons[(int)pIndex].TryGetValue(buttonType, out buttonRequested);
        }

        /// <summary>
        /// Acquire a keyboard key
        /// </summary>
        /// <param name="keyType">Key to acquire</param>
        /// <param name="buttonRequested">Returns the <see cref="InputButton"/> requested</param>
        /// <returns>True if that button was registered for listening</returns>
        private bool ButtonFromType(Keys keyType, out InputButton buttonRequested)
        {
            return this.keys.TryGetValue(keyType, out buttonRequested);
        }

        /// <summary>
        /// Acquire a mouse button
        /// </summary>
        /// <param name="keyType">Mouse button to acquire</param>
        /// <param name="buttonRequested">Returns the <see cref="InputButton"/> requested</param>
        /// <returns>True if that button was registered for listening</returns>
        private bool ButtonFromType(MouseButton mouseButtonType, out InputButton buttonRequested)
        {
            return this.mouseButtons.TryGetValue(mouseButtonType, out buttonRequested);
        }

        /// <summary>
        /// Check if a gamepad button is currently being held
        /// </summary>
        /// <param name="buttonType">Gamepad button to check</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <returns>True if button is being held</returns>
        public bool IsHeld(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                return buttonRequested.IsHeld;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a keyboard key is currently being held
        /// </summary>
        /// <param name="keyType">Key to check</param>
        /// <returns>True if button is being held</returns>
        public bool IsHeld(Keys keyType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                return buttonRequested.IsHeld;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This key does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a mouse button is currently being held
        /// </summary>
        /// <param name="mouseButtonType">Mouse button to check</param>
        /// <returns>True if button is being held</returns>
        public bool IsHeld(MouseButton mouseButtonType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                return buttonRequested.IsHeld;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This mouse-button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a gamepad button is in the down state (was just pressed down).
        /// </summary>
        /// <param name="buttonType">Gamepad button to check</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <returns>True if button has just been pressed down</returns>
        public bool IsDown(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                return buttonRequested.IsDown;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a keyboard key is in the down state (was just pressed down).
        /// </summary>
        /// <param name="keyType">Keyboard key to check</param>
        /// <returns>True if key has just been pressed down</returns>
        public bool IsDown(Keys keyType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                return buttonRequested.IsDown;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This key does not have a listener. It must have a listener before it can be used.");
            }
        }

        /// <summary>
        /// Check if a mouse button is in the down state (was just pressed down).
        /// </summary>
        /// <param name="mouseButtonType">Mouse button to check</param>
        /// <returns>True if mouse button has just been pressed down</returns>
        public bool IsDown(MouseButton mouseButtonType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                return buttonRequested.IsDown;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This mouse button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a gamepad button is in the up state (not pressed down or held).
        /// </summary>
        /// <param name="buttonType">Gamepad button to check</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <returns>True if button is up</returns>
        public bool IsUp(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                return buttonRequested.IsUp;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a keyboard key is in the up state (not pressed down or held).
        /// </summary>
        /// <param name="keyType">Keyboard key to check</param>
        /// <returns>True if button is up</returns>
        public bool IsUp(Keys keyType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                return buttonRequested.IsUp;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This key does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Check if a mouse button is in the up state (not pressed down or held).
        /// </summary>
        /// <param name="mouseButtonType"><see cref="MouseButton"/> to check</param>
        /// <returns>True is button is up</returns>
        public bool IsUp(MouseButton mouseButtonType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                return buttonRequested.IsUp;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This mouse button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Press down a gamepad button in the polling handler (not the actual button).
        /// </summary>
        /// <param name="buttonType">Gamepad button to press</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <returns>True if button has been registered with a listener</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Press(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                buttonRequested.Press();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Press down a keyboard key in the polling handler (not the actual key).
        /// </summary>
        /// <param name="keyType">Key to press</param>
        /// <returns>True if key has been registered with a listener</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Press(Keys keyType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                buttonRequested.Press();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Press down a mouse button in the polling handler (not the actual button).
        /// </summary>
        /// <param name="mouseButtonType">Mouse button to press</param>
        /// <returns>True if button has been registered with a listener.</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Press(MouseButton mouseButtonType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                buttonRequested.Press();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Release a Gamepad button in the polling handler (not the actual gamepad button).
        /// </summary>
        /// <param name="buttonType">Gamepad button to release</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <returns>True if button has been registered with a listener.</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Release(Buttons buttonType, PlayerIndex pIndex)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                buttonRequested.Release();
                buttonRequested.SetHeld(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Release a keyboard key in the polling handler (not the actual gamepad button).
        /// </summary>
        /// <param name="keyType">Keyboard key to release</param>
        /// <returns>True if key has been registered with a listener.</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Release(Keys keyType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                buttonRequested.Release();
                buttonRequested.SetHeld(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Release a mouse button in the polling handler (not the actual gamepad button).
        /// </summary>
        /// <param name="mouseButtonType">Mouse button to release</param>
        /// <returns>True if button has been registered with a listener.</returns>
        /// <remarks>Private because only the message system should use this function</remarks>
        private bool Release(MouseButton mouseButtonType)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                buttonRequested.Release();
                buttonRequested.SetHeld(false);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Release thumbstick stored in the polling handler. This occurs whenever a thumbstick
        /// is literally released by a player.
        /// </summary>
        /// <param name="thumbstick"><see cref="GamePadThumbStick"/> to release, holds stick type and player index</param>
        private void ReleaseThumbstick(GamePadThumbStick thumbstick)
        {
            this.thumbsticks[(int)thumbstick.Player, (int)thumbstick.StickType] = Vector2.Zero;
        }

        /// <summary>
        /// Release trigger stored in the polling handler. This occurs whenever a trigger is
        /// literally released by a player.
        /// </summary>
        /// <param name="trigger"><see cref="GamePadTrigger"/> to release, holds trigger type and player index</param>
        private void ReleaseTrigger(GamePadTrigger trigger)
        {
            this.triggers[(int)trigger.Player, (int)trigger.TriggerType] = 0.0f;
        }

        /// <summary>
        /// Set the held state of this button in the polling handler. This occurs whenever a button is being held.
        /// </summary>
        /// <param name="buttonType">Gamepad Button to hold</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <param name="heldState">True for 'held', false to 'unhold'</param>
        /// <returns>True if button has been registered with a listener</returns>
        private bool SetHeld(Buttons buttonType, PlayerIndex pIndex, bool heldState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                buttonRequested.SetHeld(heldState);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the held state of this keyboard key in the polling handler. This occurs whenever a key is being held.
        /// </summary>
        /// <param name="keyType">Keyboard key to hold</param>
        /// <param name="heldState">True for 'held', false to 'unhold'</param>
        /// <returns>True if key has been registered with a listener</returns>
        private bool SetHeld(Keys keyType, bool heldState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                buttonRequested.SetHeld(heldState);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the held state of this <see cref="MouseButton"/> in the polling handler. This occurs whenever a button is being held.
        /// </summary>
        /// <param name="keyType"><see cref="MouseButton"/> to hold</param>
        /// <param name="heldState">True for 'held', false to 'unhold'</param>
        /// <returns>True if <see cref="MouseButton"/> has been registered with a listener</returns>
        private bool SetHeld(MouseButton mouseButtonType, bool heldState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                buttonRequested.SetHeld(heldState);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the lockable state of this Gamepad button in the polling handler. Locked keys do not repeat or report as 'held'.
        /// </summary>
        /// <param name="buttonType">Gamepad button for which to set lockable state</param>
        /// <param name="pIndex">Index of player's controller (1-4)</param>
        /// <param name="lockableState">'true' will set this button to 'lockable'</param>
        /// <returns>True if this button has been registered with a listener</returns>
        public bool SetLockable(Buttons buttonType, PlayerIndex pIndex, bool lockableState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(buttonType, pIndex, out buttonRequested))
            {
                buttonRequested.SetLockable(lockableState);
                return true;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Set the lockable state of this keyboard key in the polling handler. Locked keys do not repeat or report as 'held'.
        /// </summary>
        /// <param name="keyType">Keyboard key for which to set lockable state</param>
        /// <param name="lockableState">'true' will set this key to 'lockable'</param>
        /// <returns>True if this key has been registered with a listener</returns>
        public bool SetLockable(Keys keyType, bool lockableState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(keyType, out buttonRequested))
            {
                buttonRequested.SetLockable(lockableState);
                return true;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This key does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Set the lockable state of this mouse button in the polling handler. Locked keys do not repeat or report as 'held'.
        /// </summary>
        /// <param name="mouseButtonType"><see cref="MouseButton"/> for which to set lockable state</param>
        /// <param name="lockableState">'true' will set this button to 'lockable'</param>
        /// <returns>True if this button has been registered with a listener</returns>
        public bool SetLockable(MouseButton mouseButtonType, bool lockableState)
        {
            InputButton buttonRequested;
            if (ButtonFromType(mouseButtonType, out buttonRequested))
            {
                buttonRequested.SetLockable(lockableState);
                return true;
            }
            else
            {
                // This should be converted to an error that doesn't break like an exception does.
                throw new Exception("This mouse button does not have a listener. It must have a listener before it can be used.");
                //return false;
            }
        }

        /// <summary>
        /// Gets value of a given thumbstick for a given player's controller (by index).
        /// </summary>
        /// <param name="pIndex">Index of player's controller</param>
        /// <param name="side">Which thumbstick side"/></param>
        /// <param name="stickValue">Current value of thumbstick</param>
        /// <returns>False if the thumbstick hasn't moved since its last update</returns>
        public bool Thumbstick(PlayerIndex pIndex, GamePadInputSide side, out Vector2 stickValue)
        {
            stickValue = this.thumbsticks[(int)pIndex, (int)side];

            if (stickValue != this.previousThumbsticks[(int)pIndex, (int)side])
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets value of a given trigger for a given player's controller (by index).
        /// </summary>
        /// <param name="pIndex">Index of player's controller</param>
        /// <param name="side">Which trigger side"/></param>
        /// <param name="triggerValue">Current value of trigger</param>
        /// <returns>False if the trigger hasn't moved since its last update</returns>
        public bool Trigger(PlayerIndex pIndex, GamePadInputSide side, out float triggerValue)
        {
            triggerValue = this.triggers[(int)pIndex, (int)side];

            if (triggerValue != this.previousTriggers[(int)pIndex, (int)side])
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Message handler for the input polling handler.
        /// </summary>
        /// <param name="message">Incoming message</param>
        private void Game_GameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.KeyDown:
                    MsgKeyPressed keyDownMessage = message as MsgKeyPressed;
                    message.TypeCheck(keyDownMessage);

                    Press(keyDownMessage.Key);
                    break;

                case MessageType.KeyUp:
                    MsgKeyReleased keyUpMessage = message as MsgKeyReleased;
                    message.TypeCheck(keyUpMessage);

                    Release(keyUpMessage.Key);
                    break;

                case MessageType.KeyHeld:
                    MsgKeyHeld keyPressMessage = message as MsgKeyHeld;
                    message.TypeCheck(keyPressMessage);

                    SetHeld(keyPressMessage.Key, true);
                    break;

                case MessageType.MouseDown:
                    MsgMouseButtonPressed mouseDownMessage = message as MsgMouseButtonPressed;
                    message.TypeCheck(mouseDownMessage);

                    Press(mouseDownMessage.Button);
                    break;

                case MessageType.MouseUp:
                    MsgMouseButtonReleased mouseUpMessage = message as MsgMouseButtonReleased;
                    message.TypeCheck(mouseUpMessage);

                    Release(mouseUpMessage.Button);
                    break;

                case MessageType.MouseHeld:
                    MsgMouseButtonHeld mousePressMessage = message as MsgMouseButtonHeld;
                    message.TypeCheck(mousePressMessage);

                    SetHeld(mousePressMessage.Button, true);
                    break;

                case MessageType.Thumbstick:
                    MsgGamePadThumbstick thumbstickMessage = message as MsgGamePadThumbstick;
                    message.TypeCheck(thumbstickMessage);

                    // Store previous state
                    this.previousThumbsticks[(int)thumbstickMessage.PlayerIndex, (int)thumbstickMessage.StickType] =
                        this.thumbsticks[(int)thumbstickMessage.PlayerIndex, (int)thumbstickMessage.StickType];

                    this.thumbsticks[(int)thumbstickMessage.PlayerIndex, (int)thumbstickMessage.StickType] =
                        thumbstickMessage.StickValues;
                    break;

                case MessageType.ThumbstickRelease:
                    MsgGamePadThumbstickReleased thumbstickReleaseMessage = message as MsgGamePadThumbstickReleased;
                    message.TypeCheck(thumbstickReleaseMessage);

                    // Store previous state
                    this.previousThumbsticks[(int)thumbstickReleaseMessage.PlayerIndex, (int)thumbstickReleaseMessage.StickType] =
                        this.thumbsticks[(int)thumbstickReleaseMessage.PlayerIndex, (int)thumbstickReleaseMessage.StickType];

                    ReleaseThumbstick(thumbstickReleaseMessage.Data);
                    break;

                case MessageType.Trigger:
                    MsgGamePadTrigger triggerMessage = message as MsgGamePadTrigger;
                    message.TypeCheck(triggerMessage);

                    // Store previous state
                    this.previousTriggers[(int)triggerMessage.PlayerIndex, (int)triggerMessage.GamePadInputSide] =
                        this.triggers[(int)triggerMessage.PlayerIndex, (int)triggerMessage.GamePadInputSide];

                    this.triggers[(int)triggerMessage.PlayerIndex, (int)triggerMessage.GamePadInputSide] =
                        triggerMessage.TriggerValue;
                    break;

                case MessageType.TriggerRelease:
                    MsgGamePadTriggerReleased triggerReleaseMessage = message as MsgGamePadTriggerReleased;
                    message.TypeCheck(triggerReleaseMessage);

                    // Store previous state
                    previousTriggers[(int)triggerReleaseMessage.PlayerIndex, (int)triggerReleaseMessage.GamePadInputSide] =
                        triggers[(int)triggerReleaseMessage.PlayerIndex, (int)triggerReleaseMessage.GamePadInputSide];

                    ReleaseTrigger(triggerReleaseMessage.Data);
                    break;

                case MessageType.ButtonDown:
                    MsgGamePadButtonPressed buttonDownMessage = message as MsgGamePadButtonPressed;
                    message.TypeCheck(buttonDownMessage);

                    Press(buttonDownMessage.Button, buttonDownMessage.PlayerIndex);
                    break;

                case MessageType.ButtonUp:
                    MsgGamePadButtonReleased buttonUpMessage = message as MsgGamePadButtonReleased;
                    message.TypeCheck(buttonUpMessage);

                    Release(buttonUpMessage.Button, buttonUpMessage.PlayerIndex);
                    break;

                case MessageType.ButtonHeld:
                    MsgGamePadButtonHeld buttonHeldMessage = message as MsgGamePadButtonHeld;
                    message.TypeCheck(buttonHeldMessage);

                    SetHeld(buttonHeldMessage.Button, buttonHeldMessage.PlayerIndex, true);
                    break;
            }
        }
    }
}

