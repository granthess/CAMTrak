//
// Message.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Xna.Framework;


namespace QuickStart
{
    public enum MessageProtocol
    {
        // The first component to return true during handling of a message will stop the
        // message from sending to any other components. This is the most efficient way
        // to send a message. Generally you use this method when you're sending a message
        // expecting data in return from a specific component.
        Request = 0,

        // This message type will continue to distribute to components even if a component
        // has already handled it. This method is less efficient than the 'Request' type.
        // Generally you use this method if you're sending a message that each component
        // may want to know about and you don't expect any data in return after sending this
        // message. This protocol should be treated as read-only, there is no guarantee on
        // the order a component will get this message, if another Component or the BaseEntity
        // have altered the content of the message it will affect what any other components
        // see. If you want to alter the message you should probably be using Request type (for
        // a single source handling the message), or Accumulate (for multiple sources handling
        // and potentially altering the message).
        Broadcast = 1,

        // This message type is identical to 'Broadcast' in behavior. But this type denotes
        // to the programmer that it is expecting each component that handles the message
        // to possibly contribute to the data in the message, and that the sender of the
        // message will use the data returned in the message. Where 'Broadcast' type is considered
        // read-only, this type is the opposite, and expects that handlers of the message
        // may alter it.
        Accumulate = 2,
    }

    /// <summary>
    /// Message class
    /// </summary>
    /// <typeparam name="T">Type of the contained data</typeparam>
    public partial class Message<T> : IMessage
    {
        protected MessageType type;
        protected Int64 uniqueTarget;
        protected T data;
        protected GameTime gameTime;
        protected bool handled;
        protected MessageProtocol protocol;

        public Message()
        {
            this.data = default(T);
            this.gameTime = null;
            this.type = MessageType.Unknown;
            this.handled = false;
            this.uniqueTarget = -1;
            this.protocol = MessageProtocol.Request;
        }

        /// <summary>
        /// Gets/Sets the <see cref="GameTime"/> for this message
        /// </summary>
        public GameTime Time
        {
            get { return this.gameTime; }
            set { this.gameTime = value; }
        }

        /// <summary>
        /// Gets/Sets the data for this message
        /// </summary>
        public T Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        /// <summary>
        /// Gets/Sets the type of the message
        /// </summary>
        public MessageType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public Int64 UniqueTarget
        {
            get { return uniqueTarget; }
            set { uniqueTarget = value; }
        }

        public MessageProtocol Protocol
        {
            get { return this.protocol; }
            set { this.protocol = value; }
        }

        public bool GetHandled()
        {
            return this.handled;
        }

        public void SetHandled(bool handledState)
        {
            this.handled = handledState;
        }
    
        /// <summary>
        /// Explicit implementation of <see cref="IPoolItem.Release"/>
        /// </summary>
        void IPoolItem.Release()
        {
            this.ReleaseCore();
        }

        /// <summary>
        /// If the message passed in is null, we display an error.
        /// </summary>
        /// <param name="message"></param>
        public void TypeCheck(Object message)
        {
            Debug.Assert(message != null, "Passed message was not actually of the type: " + this.ToString());
        }

        /// <summary>
        /// Releases the current message
        /// </summary>
        protected virtual void ReleaseCore()
        {
            this.data = default(T);
            this.gameTime = null;
            this.type = MessageType.Unknown;
            this.uniqueTarget = -1;
        }

        /// <summary>
        /// Explicit implementation of <see cref="IPoolItem.Assign"/>
        /// </summary>
        void IPoolItem.Aquire()
        {
            AssignCore();

            Initialize();
        }

        /// <summary>
        /// Initialize the message. This is only needed for custom message objects.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Assign the current message
        /// </summary>
        protected virtual void AssignCore()
        {
        }        
    }
}
