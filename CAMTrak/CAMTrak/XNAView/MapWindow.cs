using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using theprof.CAMTrak.XNAControl;
using Microsoft.Xna.Framework;

namespace theprof.CAMTrak.XNAView
{
    class MapWindow : XNAControlGame
    {

        public MapWindow(IntPtr windowHandle, string contentRoot)
            : base(windowHandle, contentRoot)
        {
        }


        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Orange);
            base.Draw(gameTime);
        }
    }
}