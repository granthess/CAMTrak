/*
 * Cue3D.cs
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
    /// A class used internally be the AudioSystem to hold information about a 3D cue.
    /// </summary>
    class Cue3D
    {
        /// <summary>
        /// A list of cues, one for each listener, for that listeners 3D settings.
        /// </summary>
        public List<Cue> Cues;

        /// <summary>
        /// The emitter of the sound.
        /// </summary>
        public AudioEmitter Emitter;

        /// <summary>
        /// Creates a new Cue3D.
        /// </summary>
        /// <param name="cues">A list of cues. One for each AuidoListener.</param>
        /// <param name="emitter">The emitter of the sound.</param>
        public Cue3D(List<Cue> cues, AudioEmitter emitter)
        {
            Cues = cues;
            Emitter = emitter;
        }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        public void Play()
        {
            for (int i = 0; i < Cues.Count; i++)
            {
                Cues[i].Play();
            }
        }

        /// <summary>
        /// Pauses the sound.
        /// </summary>
        public void Pause()
        {
            for (int i = 0; i < Cues.Count; i++)
            {
                Cues[i].Pause();
            }
        }

        /// <summary>
        /// Resumes the sound.
        /// </summary>
        public void Resume()
        {
            for (int i = 0; i < Cues.Count; i++)
            {
                Cues[i].Resume();
            }
        }

        /// <summary>
        /// Stops the sound.
        /// </summary>
        /// <param name="stopOptions"></param>
        public void Stop(AudioStopOptions stopOptions)
        {
            for (int i = 0; i < Cues.Count; i++)
            {
                Cues[i].Stop(stopOptions);
            }
        }

        /// <summary>
        /// Disposes all cues.
        /// </summary>
        public void DisposeCues()
        {
            for (int i = 0; i < Cues.Count; i++)
            {
                Cues[i].Dispose();
            }
        }
    }
}
