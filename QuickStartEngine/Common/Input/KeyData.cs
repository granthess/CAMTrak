// KeyData.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using Microsoft.Xna.Framework.Input;

namespace QuickStart
{
    /// <summary>
    /// Holds information about the current key event
    /// </summary>
    public class KeyData : IPoolItem
    {
        private Keys key;

        /// <summary>
        /// The <see cref="Keys"/> which raised the event
        /// </summary>
        public Keys Key
        {
            get { return this.key; }
            set { this.key = value; }
        }

        /// <summary>
        /// Explicit implementation for releasing
        /// </summary>
        void IPoolItem.Release()
        {
            this.Key = Keys.None;
        }

        /// <summary>
        /// Explicit implementation for aquiring
        /// </summary>
        void IPoolItem.Aquire()
        {
        }

        void IPoolItem.SetHandled(bool handledState)
        {
        }

        bool IPoolItem.GetHandled()
        {
            return true;
        }
    }
}
