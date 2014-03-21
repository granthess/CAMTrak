// KeyMessage.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace QuickStart
{
    /// <summary>
    /// Message class for keyboard messages
    /// </summary>
    public class KeyMessage : Message<KeyData>
    {
        /// <summary>
        /// Releases the key message and the attached <see cref="KeyData"/>
        /// </summary>
        protected override void ReleaseCore()
        {
            ObjectPool.Release(this.Data);
            this.Data = null;

            base.ReleaseCore();
        }

        /// <summary>
        /// This message is invoked when a KeyMessage is aquired
        /// </summary>
        /// <remarks>
        /// This will aquire a new KeyData and assign it to <see cref="KeyMessage.KeyData"/>
        /// </remarks>
        protected override void AssignCore()
        {
            this.Data = ObjectPool.Aquire<KeyData>();
        }
    }
}
