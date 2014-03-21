/*
 * AudioSystem.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace QuickStart.Audio
{
    /// <summary>
    /// A subsystem that manages the playing of audio.
    /// </summary>
    public class AudioManager : BaseManager
    {
         /// <summary>
        /// Sound stuff for XACT
        /// </summary>
        AudioEngine audioEngine;

        /// <summary>
        /// Wave bank
        /// </summary>
        WaveBank waveBank;

        /// <summary>
        /// Sound bank
        /// </summary>
        SoundBank soundBank;

        /// <summary>
        /// A list of all non-3D cues being played through the AudioSubSystem.
        /// This is so they are not collected by the garbage collecter.
        /// </summary>
        List<Cue> activeCues;

        /// <summary>
        /// A list of all cue3Ds that are currently being played, and thus need
        /// their 3D settings updated each frame.
        /// </summary>
        List<Cue3D> activeCue3Ds;

        /// <summary>
        /// A stack that keeps inactive Cue3D instances, so a new instance does not
        /// need to be created each time a sound is played.
        /// </summary>
        Queue<Cue3D> inactiveCue3Ds;

        /// <summary>
        /// Sound catagories
        /// </summary>
        Dictionary<string, AudioCategory> categories;

        /// <summary>
        /// The volumes of each catagory, before being scaled by the global volume.
        /// </summary>
        Dictionary<AudioCategory, float> categoryVolumes;

        /// <summary>
        /// The global sound volume.
        /// </summary>
        /// <remarks>
        /// Categories that have not been loaded into the 
        /// AudioSystem will not have their volume automatically adjusted.
        /// </remarks>
        public float GlobalVolume
        {
            get { return globalVolume; }
            set
            {
                globalVolume = value; // MathHelper.Clamp(value, 0, 1);
                ApplyGlobalVolume();
            }
        }
        float globalVolume = 1;

        /// <summary>
        /// A list of all AudioListeners for 3D sound.
        /// </summary>
        public List<AudioListener> Listeners
        {
            get { return listeners; }
        }
        List<AudioListener> listeners;

        /// <summary>
        /// Returns whether the sound bank or wave bank is currently in use.
        /// </summary>
        public bool IsInUse
        {
            get { return soundBank.IsInUse || waveBank.IsInUse; }
        }

        /// <summary>
        /// States if the audio system has been initialized or not.
        /// </summary>
        public bool IsInitialized
        {
            get { return isInitialized; }
        }
        bool isInitialized;        

        /// <summary>
        /// Creates a new AudioSystem instance.
        /// </summary>
        public AudioManager(QSGame game)
            : base(game)
        {
            this.Game.Services.AddService(typeof(AudioManager), this);

            listeners = new List<AudioListener>();
            activeCues = new List<Cue>();
            activeCue3Ds = new List<Cue3D>();
            inactiveCue3Ds = new Queue<Cue3D>();

            categories = new Dictionary<string, AudioCategory>();
            categoryVolumes = new Dictionary<AudioCategory, float>();

            // Set default volume.
            globalVolume = 50f;
        }

        protected override void InitializeCore()
        {
            //InitBanksAndEngine(settingsFilePath, waveBankFilePath, soundBankFilePath);

            base.InitializeCore();
        }

        private void InitBanksAndEngine(string settingsFilePath, string waveBankFilePath, string soundBankFilePath)
        {


            // @TODO: Read these from XML
            audioEngine = new AudioEngine(settingsFilePath);
            waveBank = new WaveBank(audioEngine, waveBankFilePath);
            soundBank = new SoundBank(audioEngine, soundBankFilePath);

            // Attempt to load some default AudioCatagories
            try
            {
                LoadCategories("Music", "Effects", "Ambience");
            }
            catch { }

            isInitialized = true;
        }

        /// <summary>
        /// Updates the AudioSystem.
        /// </summary>
        protected override void UpdateCore(GameTime gameTime)
        {
            if (!isInitialized)
                return;

            // Remove cues from the activeCues list which have stopped.
            for (int i = activeCues.Count - 1; i >= 0; i--)
            {
                if (activeCues[i].IsStopped)
                    activeCues.RemoveAt(i);
            }

            // Update 3D cues.
            for (int i = activeCue3Ds.Count - 1; i >= 0; i--)
            {
                Cue3D cue3D = activeCue3Ds[i];

                if (cue3D.Cues[0].IsStopped)
                {
                    // If the cue has stopped playing, dispose it.
                    cue3D.DisposeCues();

                    // Store the Cue3D instance for future reuse.
                    inactiveCue3Ds.Enqueue(cue3D);

                    // Remove it from the active list.
                    activeCue3Ds.RemoveAt(i);
                }
                else
                {
                    // If the cue is still playing, update its 3D settings.
                    Update3DSettings(cue3D);
                }
            }

            // Update the audio engine.
            audioEngine.Update();
        }

        /// <summary>
        /// Applies 3D settings to a cue.
        /// </summary>
        void Update3DSettings(Cue3D cue3D)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                cue3D.Cues[i].Apply3D(listeners[i], cue3D.Emitter);
            }
        }

        /// <summary>
        /// Scales the volume of each AudioCatagory loaded into the AudioSystem.
        /// </summary>
        private void ApplyGlobalVolume()
        {
            foreach (KeyValuePair<string, AudioCategory> category in categories)
            {
                try
                {
                    category.Value.SetVolume(categoryVolumes[category.Value] * globalVolume);
                }
                catch { }
            }
        }

        /// <summary>
        /// Sets the value of a global variable.
        /// </summary>
        public void SetGlobalVariable(string variableName, float value)
        {
            audioEngine.SetGlobalVariable(variableName, value);
        }

        /// <summary>
        /// Gets the value of a global variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public float GetGlobalVariable(string variableName)
        {
            return audioEngine.GetGlobalVariable(variableName);
        }

        /// <summary>
        /// Pauses all cues currently being played.
        /// </summary>
        public void PauseAll()
        {
            for (int i = 0; i < activeCues.Count; i++)
            {
                activeCues[i].Pause();
            }

            for (int i = 0; i < activeCue3Ds.Count; i++)
            {
                activeCue3Ds[i].Pause();
            }
        }

        /// <summary>
        /// Resumes all paused cues played through the AudioSystem.
        /// </summary>
        public void Resume()
        {
            for (int i = 0; i < activeCues.Count; i++)
            {
                activeCues[i].Resume();
            }

            for (int i = 0; i < activeCue3Ds.Count; i++)
            {
                activeCue3Ds[i].Resume();
            }
        }

        /// <summary>
        /// Stops all cues played through the AudioSystem.
        /// Stopped cues cannot be resumed.
        /// </summary>
        /// <param name="stopOptions">Controls how the cues should stop.</param>
        public void Stop(AudioStopOptions stopOptions)
        {
            for (int i = 0; i < activeCues.Count; i++)
            {
                activeCues[i].Stop(stopOptions);
            }

            for (int i = 0; i < activeCue3Ds.Count; i++)
            {
                activeCue3Ds[i].Stop(stopOptions);
            }
        }

        /// <summary>
        /// Loads the categories into the AudioSystem.
        /// This allows the system to adjust the volume of the categories with the GolbalVolume.
        /// </summary>
        /// <param name="categoryNames"></param>
        public void LoadCategories(params string[] categoryNames)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            // Load each category.
            for (int i = 0; i < categoryNames.Length; i++)
            {
                GetCategory(categoryNames[i]);
            }

            // Force the system to set each category volume.
            ApplyGlobalVolume();
        }

        /// <summary>
        /// Plays a cue
        /// </summary>
        /// <param name="cueName">Name of the cue</param>
        /// <param name="variables">Variables to be applied to the cue.</param>
        public void Play(string cueName, params CueVariable[] variables)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            Cue cue = soundBank.GetCue(cueName);

            for (int i = 0; i < variables.Length; i++)
                variables[i].Apply(cue);

            cue.Play();
            activeCues.Add(cue);
        }

        /// <summary>
        /// Plays a cue. 
        /// This method prevents the cue being collected by the garbage collecter while the cue is playing,
        /// so references to the cue outside of the AudioSubSystem can be safely lost.
        /// </summary>
        /// <param name="cue">The cue to be played</param>
        /// <param name="variables">Variables to be applied to the cue.</param>
        public void Play(Cue cue, params CueVariable[] variables)
        {
            for (int i = 0; i < variables.Length; i++)
                variables[i].Apply(cue);

            cue.Play();
            activeCues.Add(cue);
        }

        /// <summary>
        /// Plays a sound in 3D to all the AudioListeners listed in Listeners.
        /// </summary>
        /// <param name="cueName">Name of the sound to be played.</param>
        /// <param name="emitter">The emitter of the sound.</param>
        /// <remarks>If there are no listeners, then the sound is just played normally.</remarks>
        /// <param name="variables">Variables to be applied to the cue.</param>
        public void Play(string cueName, AudioEmitter emitter, params CueVariable[] variables)
        {
            // Play the cue normally if there are no listeners for 3D sound.
            if (listeners.Count == 0)
            {
                Play(cueName);
                return;
            }

            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            // Generate a cue instance for each listener
            List<Cue> cues = new List<Cue>(listeners.Count);
            for (int i = 0; i < listeners.Count; i++)
            {
                Cue cue = soundBank.GetCue(cueName);

                for (int v = 0; v < variables.Length; v++)
                    variables[v].Apply(cue);

                cues.Add(soundBank.GetCue(cueName));
            }

            // Get an inactive instance if available
            Cue3D cue3D;
            if (inactiveCue3Ds.Count > 0)
            {
                cue3D = inactiveCue3Ds.Dequeue();
                cue3D.Cues = cues;
                cue3D.Emitter = emitter;
            }
            else
            {
                // we need to create a new one
                cue3D = new Cue3D(cues, emitter);
            }

            // apply the 3D settings for this listener
            Update3DSettings(cue3D);

            // play the cue
            cue3D.Play();

            // add to the activeCue3Ds list.
            activeCue3Ds.Add(cue3D);
        }

        /// <summary>
        /// Gets a cue for later playing.
        /// </summary>
        /// <param name="cueName">Name of the cue</param>
        public Cue GetCue(string cueName)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            return soundBank.GetCue(cueName);
        }

        /// <summary>
        /// Gets a sound catagory from the audio engine.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        public AudioCategory GetCategory(string categoryName)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            // Categories are remembered as they are got,
            // so their volume can be set when globalVolume changes.
            if (categories.ContainsKey(categoryName))
                return categories[categoryName];
            else
            {
                AudioCategory category = audioEngine.GetCategory(categoryName);
                categories.Add(categoryName, category);
                categoryVolumes.Add(category, 100f);
                return category;
            }
        }

        /// <summary>
        /// Stops a catagory
        /// </summary>
        public void StopCategory(string category, AudioStopOptions stopOptions)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            GetCategory(category).Stop(stopOptions);
        }

        /// <summary>
        /// Pauses a catagory
        /// </summary>
        public void PauseCategory(string category)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            GetCategory(category).Pause();
        }

        /// <summary>
        /// Resumes a catagory
        /// </summary>
        public void ResumeCategory(string category)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            GetCategory(category).Resume();
        }

        /// <summary>
        /// Sets the volume of a catagory
        /// </summary>
        /// <param name="volume">Between 0 and 1, where 0 is mute.</param>
        public void SetVolume(string categoryName, float volume)
        {
            if (!isInitialized)
                throw new Exception("Audio system must be initialized.");

            //volume = MathHelper.Clamp(volume, 0, 1);
            AudioCategory category = GetCategory(categoryName);

            categoryVolumes[category] = volume;
            category.SetVolume(volume * globalVolume);
        }
    }
}
