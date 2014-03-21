//
// Scene.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using QuickStart.EnvironmentalSettings;

namespace QuickStart
{
    /// <summary>
    /// A scene is a container that holds all the information for a given area, such as entities and
    /// terrain information. Entities includes things like weather generator, and sky. Scene can also
    /// hold information like gravity and wind directions and strengths.
    /// </summary>
    public class Scene
    {
        protected QSGame game;

        /// <summary>
        /// Scene's gravity, direction and speed.
        /// </summary>
        protected Vector3 Gravity = QSConstants.DefaultGravity;

        /// <summary>
        /// Fog settings for this scene
        /// </summary>
        public FogSettings FogSettings
        {
            get { return this.fogSettings; }
            set
            {
                this.fogSettings = value;
                this.fogSettings.SceneFogChanged = true;
            }
        }
        protected FogSettings fogSettings;

        /// <summary>
        /// True when scene is active, otherwise false. A scene will not be processed when it is not active.
        /// </summary>
        public bool Active
        {
            get { return this.active; }
        }
        private bool active = false;        

        /// <summary>
        /// Default constructor
        /// </summary>
        public Scene(QSGame game)
        {
            this.game = game;
            this.fogSettings = new FogSettings();
        }

        /// <summary>
        /// Load fog for this scene
        /// </summary>
        /// <param name="fogPath">Path to fog environmental settings</param>
        public void LoadFog(string fogPath)
        {
            if ( fogPath.Length > 0 )
            {
                fogSettings = this.game.Content.Load<FogSettings>(fogPath);
            }
        }

        /// <summary>
        /// Cleans up the Scene instance for deletion.
        /// </summary>
        ~Scene()
        {

        }

        /// <summary>
        /// Activates a scene, letting the scene manager know it is ready
        /// to be run. This method should only be called by the scene manager.
        /// </summary>
        public virtual void Activate()
        {
            this.active = true;            
        }

        /// <summary>
        /// Deactivates a scene, which stops it from running/updating.
        /// </summary>
        public virtual void Deactiviate()
        {
            this.active = false;
        }
    }
}
