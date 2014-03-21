// InputButton.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

namespace QuickStart
{
    /// <summary>
    /// An input button holds button information for use with the input polling handler.
    /// </summary>
    public class InputButton
    {
        /// <summary>
        /// Up/down state of this button. True means down.
        /// </summary>
        private bool pressed;

        /// <summary>
        /// Held state of this button.
        /// </summary>
        private bool held;

        /// <summary>
        /// Locked/unlocked state of this button. True means locked.
        /// </summary>
        private bool locked;

        /// <summary>
        /// True means this button will lock when pressed.
        /// </summary>
        private bool lockable;

        /// <summary>
        /// Create an <see cref="InputButton"/>
        /// </summary>
        public InputButton()
        {
            pressed = false;
            held = false;
            locked = false;
            lockable = false;
        }

        /// <summary>
        /// Create an <see cref="InputButton"/>
        /// </summary>
        /// <param name="lockableState">True means this button will lock when pressed</param>
        public InputButton(bool lockableState)
        {
            pressed = false;
            held = false;
            locked = false;
            lockable = false;

            SetLockable(lockableState);
        }

        /// <summary>
        /// Press this button down
        /// </summary>
        public void Press()
        {
            pressed = true;
        }

        /// <summary>
        /// Release this button
        /// </summary>
        public void Release()
        {
            pressed = false;
            locked = false;
            held = false;
        }

        /// <summary>
        /// Set this button's held state
        /// </summary>
        /// <param name="heldState">True will set this button to 'held'</param>
        public void SetHeld(bool heldState)
        {
            held = heldState;
        }

        /// <summary>
        /// Set this button's lockable state
        /// </summary>
        /// <param name="lockableState">True will allow this button to lock</param>
        public void SetLockable(bool lockableState)
        {
            lockable = lockableState;
        }

        /// <summary>
        /// Returns true if button is up
        /// </summary>
        public bool IsUp
        {
            get
            {
                if (lockable)
                {
                    if (pressed)
                    {
                        if (locked == false)
                        {
                            locked = true;
                            return false;
                        }

                        return true;
                    }
                }

                return (pressed == false);
            }
        }

        /// <summary>
        /// Returns true if button is down
        /// </summary>
        public bool IsDown
        {
            get
            {
                if (lockable)
                {
                    if (pressed)
                    {
                        if (locked == false)
                        {
                            locked = true;
                            return true;
                        }

                        return false;
                    }
                }

                return pressed;
            }
        }

        /// <summary>
        /// Returns true if button is in held state
        /// </summary>
        public bool IsHeld
        {
            get { return held; }
        }
    }
}
