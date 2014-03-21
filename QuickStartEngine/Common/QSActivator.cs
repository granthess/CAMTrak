//
// QSActivator.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Reflection;

namespace QuickStart
{
    /// <summary>
    /// Custom System.Activator-like class to provide missing functionality on Xbox platform.
    /// </summary>
    public static class QSActivator
    {
        /// <summary>
        /// Create instance of a type.
        /// </summary>
        /// <remarks>
        /// The type must contain a constructor of the form: Type(QSGame)
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> to instantiate.</param>
        /// <param name="game">The <see cref="QSGame"/> instance to pass to the class constructor.</param>
        /// <returns>An instance of the specified <see cref="Type"/> or null if it could not be instantiated</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> or <paramref name="=game"/> are null.</exception>
        public static object CreateInstance(Type type, QSGame game)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            ConstructorInfo info = type.GetConstructor(new Type[] { typeof(QSGame) });

            if(info == null)
            {
                return null;
            }

            object instance;
            try
            {
                instance = info.Invoke(new object[] { game });
            }
            catch (Exception exception)
            {
                if (exception is SystemException)
                {
                    throw;
                }

                instance = null;
            }

            return instance;
        }
    }
}
