// IPoolItem.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

namespace QuickStart
{
    /// <summary>
    /// Interface for poolable item
    /// </summary>
    public interface IPoolItem
    {
        /// <summary>
        /// This releases a item freeing up all allocated resources
        /// </summary>
        void Release();

        /// <summary>
        /// This reassings the item, this method should be used to reinitialize the item
        /// </summary>
        void Aquire();

        bool GetHandled();

        void SetHandled(bool handledState);
    }
}
