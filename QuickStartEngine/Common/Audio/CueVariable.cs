/*
 * CueVariable.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace QuickStart.Audio
{
    /// <summary>
    /// Holds information about a cue variable.
    /// </summary>
    public struct CueVariable
    {
        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the variable.
        /// </summary>
        public float Value;

        /// <summary>
        /// Applies the value of the variable to the specified cue instance.
        /// </summary>
        /// <param name="cue">Cue that contains the variable to be set.</param>
        public void Apply(Cue cue)
        {
            cue.SetVariable(Name, Value);
        }
    }
}
