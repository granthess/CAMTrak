using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using theprof.CAMTrak.XNAControl;
using Microsoft.Xna.Framework;

namespace theprof.CAMTrak.XNAView
{
    class EditorWindow : XNAControlGame
{

        public EditorWindow(IntPtr windowHandle, string contentRoot)
            : base(windowHandle, contentRoot)
        {
        }


        protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Red);
            base.Draw(gameTime);
        }
    }
}
