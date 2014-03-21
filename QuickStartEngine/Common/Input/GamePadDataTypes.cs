//
// GamePadDataTypes.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using Microsoft.Xna.Framework;          // Needed for Vector2 and PlayerIndex
using Microsoft.Xna.Framework.Input;

namespace QuickStart
{
    /// <summary>
    /// Stores info about a gamepad button for gamepad messages.
    /// </summary>
    public struct GamePadButton
    {
        /// <summary>
        /// Button for which information is being stored.
        /// </summary>
        public Buttons Button;

        /// <summary>
        /// Player who this button info pertains to.
        /// </summary>
        public PlayerIndex Player;
    }

    /// <summary>
    /// Stores info about a gamepad trigger for gamepad messages.
    /// </summary>
    public struct GamePadTrigger
    {
        /// <summary>
        /// Value of this trigger. (0.0f to 1.0f, 0 being nothing, 1 being all the way down).
        /// </summary>
        public float TriggerValue;

        /// <summary>
        /// Type of trigger being referred to in this message (left or right).
        /// </summary>
        public GamePadInputSide TriggerType;

        /// <summary>
        /// Player who this trigger info pertains to.
        /// </summary>
        public PlayerIndex Player;
    }

    /// <summary>
    /// Which input side is being used. e.g. "Left" thumbstick, or "Right" trigger
    /// </summary>
    public enum GamePadInputSide
    {
        /// <summary>
        /// Left side of the gamepad
        /// </summary>
        Left = 0,

        /// <summary>
        /// Right side of the gamepad
        /// </summary>
        Right = 1
    }

    /// <summary>
    /// Stores info about a gamepad thumbstick for gamepad messages.
    /// </summary>
    public struct GamePadThumbStick
    {
        /// <summary>
        /// Holds values for the X and Y axis movements of the thumbstick.
        /// </summary>
        public Vector2 StickValues;

        /// <summary>
        /// Type of thumbstick being referred to in this message (left or right).
        /// </summary>
        public GamePadInputSide StickType;

        /// <summary>
        /// Player who this thumbstick info pertains to.
        /// </summary>
        public PlayerIndex Player;
    }
}
