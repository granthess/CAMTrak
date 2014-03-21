using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Input;

namespace QuickStart
{
    public class MouseButtonComparer : IEqualityComparer<MouseButton>
    {
        #region IEqualityComparer<MouseButton> Members

        public bool Equals(MouseButton x, MouseButton y)
        {
            return x == y;
        }

        public int GetHashCode(MouseButton obj)
        {
            return 0;
        }

        #endregion
    }

    public class KeyboardKeyComparer : IEqualityComparer<Keys>
    {
        #region IEqualityComparer<Keys> Members

        public bool Equals(Keys x, Keys y)
        {
            return x == y;
        }

        public int GetHashCode(Keys obj)
        {
            return 1;
        }

        #endregion
    }

    public class GamepadButtonComparer : IEqualityComparer<Buttons>
    {
        #region IEqualityComparer<Buttons> Members

        public bool Equals(Buttons x, Buttons y)
        {
            return x == y;
        }

        public int GetHashCode(Buttons obj)
        {
            return 2;
        }

        #endregion
    }
}
