// ExitMessage.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
using System;

namespace QuickStart
{
    /// <summary>
    /// Special message for exiting the game
    /// </summary>
    public sealed class ExitMessage : Message<object>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExitMessage()
        {
            this.Type = MessageType.Shutdown;
        }
    }
}
