using System;

using Microsoft.Xna.Framework;

namespace QuickStart
{
    class QSMotionHelper
    {
        static public void Yaw(ref Matrix rotation, float amount)
        {
            rotation *= Matrix.CreateFromAxisAngle(rotation.Up, amount);
        }

        static public void YawAroundWorldUp(ref Matrix rotation, float amount)
        {
            rotation *= Matrix.CreateRotationY(amount);
        }

        static public void Pitch(ref Matrix rotation, float amount)
        {
            rotation *= Matrix.CreateFromAxisAngle(rotation.Right, amount);
        }

        static public void Roll(ref Matrix rotation, float amount)
        {
            rotation *= Matrix.CreateFromAxisAngle(rotation.Forward, amount);
        }

        static public void Strafe(ref Vector3 position, ref Matrix rotation, float amount)
        {
            position += rotation.Right * amount;
        }

        static public void Walk(ref Vector3 position, ref Matrix rotation, float amount)
        {
            position += rotation.Forward * amount;
        }

        static public void Jump(ref Vector3 position, ref Matrix rotation, float amount)
        {
            position += rotation.Up * amount;
        }

        static public void Rise(ref Vector3 position, float amount)
        {
            position += Vector3.Up * amount;
        }
    }
}
