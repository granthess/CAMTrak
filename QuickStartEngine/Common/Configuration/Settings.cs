//
// Settings.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.IO;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;

namespace QuickStart
{
    /// <summary>
    /// Setting for the game
    /// </summary>
    /// <remarks>
    /// The settings are stored in the "Settings.xml" file.
    /// </remarks>
    public class Settings
    {
        // Name of the default settings file
        private const string Filename = "Settings.xml";

        private GraphicsLevel graphicsLevel;
        private bool isFixedTimeStep;
        private bool isFullscreen;
        private bool isMultiSampling;
        private bool isVerticalSynchronized;
        private Point resolution;
        private bool isShadowMapping;
        private int physicsStepsPerSecond;

        private bool modified = false;

        // Entities directory must contain Entities.xml, which contains the definitions for all BaseEntity objects.
        private string entitiesDirectory;

        // EntityDefinitions directory must contain all entity definitions referred to by the Entities.xml file.
        private string entityDefinitionsDirectory;

        // ComponentDefinitions directory must contain all component definitions referred to by any entity definitions.
        private string componentDefinitionsDirectory;

        /// <summary>
        /// Gets or sets the <see cref="GraphicsLevel"/> the game should run in
        /// </summary>
        public GraphicsLevel GraphicsLevel
        {
            get { return this.graphicsLevel; }
            set
            {
                this.graphicsLevel = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Gets or set if the game should use fixed time step
        /// </summary>
        public bool IsFixedTimeStep
        {
            get { return this.isFixedTimeStep; }
            set
            {
                this.isFixedTimeStep = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// GEts or sets if the game should run in full screen
        /// </summary>
        public bool IsFullscreen
        {
            get { return this.isFullscreen; }
            set
            {
                this.isFullscreen = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Gets or sets if the game should use MultiSampling
        /// </summary>
        public bool IsMultiSampling
        {
            get { return this.isMultiSampling; }
            set
            {
                this.isMultiSampling = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Gets or sets if the game should run in vertical sync mode
        /// </summary>
        /// <remarks>
        /// Enabling this fixes the framerate to values under 60 frames per second (60hz). This
        /// is because most games do not need to run above this speed as it just puts more strain
        /// </remarks>
        public bool IsVerticalSynchronized
        {
            get { return this.isVerticalSynchronized; }
            set
            {
                this.isVerticalSynchronized = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// The resolution fo the game screen
        /// </summary>
        /// <remarks>
        /// You should maintain a 4:3, 16:9, or 16:10 ratio in whatever
        /// you choose. 4:3 for standard monitor or SDTV, 16:9 for HDTV,
        /// or 16:10 for widescreen LCD monitors. 4:3 will be compatible
        /// with the most devices.
        /// 1024 / 768 = 1.3333 or 4/3 (4:3)
        /// </remarks>
        public Point Resolution
        {
            get { return this.resolution; }
            set
            {
                this.resolution = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Whether or not shadow mapping is enabled
        /// </summary>
        public bool IsShadowMapping
        {
            get { return this.isShadowMapping; }
            set
            {
                this.isShadowMapping = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Amount of times the physics simulation will step, per second
        /// </summary>
        public int PhysicsStepsPerSecond
        {
            get { return this.physicsStepsPerSecond; }
            set
            {
                this.physicsStepsPerSecond = value;
                this.modified = true;
            }
        }

        public string EntitiesDirectory
        {
            get { return this.entitiesDirectory; }
            set
            {
                this.entitiesDirectory = value;
                this.modified = true;
            }
        }

        public string EntityDefinitionsDirectory
        {
            get { return this.entityDefinitionsDirectory; }
            set
            {
                this.entityDefinitionsDirectory = value;
                this.modified = true;
            }
        }

        public string ComponentDefinitionsDirectory
        {
            get { return this.componentDefinitionsDirectory; }
            set
            {
                this.componentDefinitionsDirectory = value;
                this.modified = true;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>
        /// The default settings are
        /// Resolution:                 1024 x 768
        /// Fullscreen:                 False
        /// VertSync:                   True
        /// GraphicLevel:               Low
        /// Fixed Time step:            True
        /// MultiSampling:              False
        /// Shadow Mapping:             False
        /// Physics Steps Per Second:   30
        /// 
        /// To unlock framerate, set 'isVerticalSynchronized' and 'isFixedTimeStep' both to false.
        /// </remarks>
        protected Settings()
        {
            this.resolution = new Point(1024, 768);
            this.isFullscreen = false;
            this.isVerticalSynchronized = false;
            this.graphicsLevel = GraphicsLevel.Low;     // Currently not used
            this.isFixedTimeStep = false;
            this.isMultiSampling = false;
            this.isShadowMapping = false;               // Default is false because this is not yet fully supported.
            this.physicsStepsPerSecond = 60;
            this.entitiesDirectory = "Content/Entities";
            this.entityDefinitionsDirectory = "Content/Entities/EntityDefinitions";
            this.componentDefinitionsDirectory = "Content/Entities/EntityDefinitions";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Instance of the current <see cref="QSGame"/></param>
        public Settings(QSGame game) : this()
        {
            game.GameExiting += Game_Exiting;
        }

        ///// <summary>
        ///// Loads the Game settings from disk
        ///// </summary>
        ///// <returns>A new instance of <see cref="Settings"/></returns>
        //public static Settings Load(QSGame game)
        //{
        //    if (File.Exists(Filename) == false)
        //    {
        //        return new Settings(game);
        //    }

        //    Settings settings;
        //    using (FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None))
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof (Settings));
        //        settings = serializer.Deserialize(fs) as Settings;
        //    }

        //    if (settings == null)
        //    {
        //        throw new InvalidOperationException("Settings file was not in the expected format");
        //    }

        //    game.GameExiting += settings.Game_Exiting;

        //    return settings;
        //}

        /// <summary>
        /// Saves the the <see cref="Settings"/> to disk
        /// </summary>
        /// <param name="settings">The <see cref="Settings"/> to save</param>
        /// <remarks>
        /// The settings will only be saved if there has been a change
        /// </remarks>
        public static void Save(Settings settings)
        {
            if (settings.modified == false)
            {
                return;
            }

            using (FileStream fs = new FileStream(Filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(fs, settings);
            }
        }

        /// <summary>
        /// Handles game exit
        /// </summary>
        /// <param name="sender">The current <see cref="QSGame"/> instance</param>
        /// <param name="e">Event arguments</param>
        private void Game_Exiting(object sender, EventArgs e)
        {
            Save(this);
        }
    }
}