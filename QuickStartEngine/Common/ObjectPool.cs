// ObjectPool.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

namespace QuickStart
{
    /// <summary>
    /// Provides managed resources which are not disposed in order to prevent GC
    /// </summary>
    public static class ObjectPool
    {
        internal static readonly Dictionary<Type, List<IPoolItem>> free = new Dictionary<Type, List<IPoolItem>>(QSConstants.PoolIncrements);
        internal static readonly Dictionary<Type, List<IPoolItem>> locked = new Dictionary<Type, List<IPoolItem>>(QSConstants.PoolIncrements);
        private static readonly object aquireLock = new object();

        /// <summary>
        /// Aquires a new item from the pool
        /// </summary>
        /// <typeparam name="T">Type of the item to retrieve</typeparam>
        /// <returns>An instance of <typeparamref name="T"/>, which might be already instantiated</returns>
        public static T Aquire<T>() where T : class, IPoolItem, new()
        {
            lock (aquireLock)
            {
                if (free.ContainsKey(typeof(T)) == false)
                {
                    free.Add(typeof(T), new List<IPoolItem>(QSConstants.PoolIncrements));
                }

                if (locked.ContainsKey(typeof(T)) == false)
                {
                    locked.Add(typeof(T), new List<IPoolItem>(QSConstants.PoolIncrements));
                }

                List<IPoolItem> items = free[typeof(T)];

                if (items.Count == 0)
                {
                    for (int i = 0; i < QSConstants.PoolAllocationSize; ++i)
                    {
                        items.Add(new T());
                    }
                }

                T item = items[0] as T;

                // @TODO: Find out why this is happening, it shouldn't be
                if (item == null)
                {
                    throw new Exception("Bad data in items container");
                }

                items.RemoveAt(0);

                locked[typeof(T)].Add(item);    

                item.Aquire();
                item.SetHandled(false);
                return item;
            }
        }

        /// <summary>
        /// Releases a item and puts it back in the available stack
        /// </summary>
        /// <typeparam name="T">Type of the item to release</typeparam>
        /// <param name="item">The instance of <typeparamref name="T"/> which should be released</param>
        /// <exception cref="ArgumentException">Is thrown if the <paramref name="item"/> was not created using <see cref="ObjectPool.Aquire"/></exception>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="item"/> is null</exception>
        public static void Release(IPoolItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (locked[item.GetType()].Contains(item) == false)
            {
                // @TODO: Find out why this is even necessary
                if (locked[item.GetType()].Count == 0)
                {
                    locked.Remove(item.GetType());
                }

                //throw new ArgumentException("Release item has not been aquired");
            }
            else
            {
                locked[item.GetType()].Remove(item);
            }

            item.Release();

            free[item.GetType()].Add(item);
        }

        /// <summary>
        /// Releases all items freeing all queues
        /// </summary>
        public static void ReleaseAll()
        {
            // Release and clear all locked items first
            foreach (KeyValuePair<Type, List<IPoolItem>> lockedItem in locked)
            {
                foreach (IPoolItem element in lockedItem.Value)
                {
                    element.Release();
                }
                lockedItem.Value.Clear();
            }

            // Then release and clear all free items 
            foreach (KeyValuePair<Type, List<IPoolItem>> freeItem in free)
            {
                freeItem.Value.Clear();
            }
        }
    }
}
