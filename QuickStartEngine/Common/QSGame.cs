//
// QSGame.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

using Microsoft.Xna.Framework;

using Nuclex.UserInterface;

namespace QuickStart
{
    using Audio;
    using Compositor;    
    using Entities;
    using Graphics;
    using GUI;
    using Interfaces;
    using Physics;           

    public delegate void EngineMessageHandler(IMessage message);

    /// <summary>
    /// The QuickStart game base class.  This is the central point of control for all games built on the QuickStart engine.
    /// </summary>
    public abstract class QSGame : Game
    {        
        public const int UniqueIDEmpty = -1;
        public const int TemplateIDEmpty = -1;
        public const int TemplateIDCustom = 0; // This ID is given to any objects loaded manually rather than from a template definition.
        public const int SceneMgrRootEntityID = 0;
        public const int FirstEntityIDAbleToBeAltered = 2;    // No entity with an ID less than 2 is allowed to be deleted, unless the engine is shutting down

        private static readonly object messageLock = new object();
        private readonly List<BaseManager> managers;        

        internal List<IMessage> currentMessages;
        private bool exiting = false;        
        internal List<IMessage> queuedMessages;
        internal List<IMessage> handledMessages;

        public GameTime GameTime
        {
            get { return gameTime; }
        }
        private GameTime gameTime;

        public float PartialSecondsThisFrame
        {
            get { return partialSecondsThisFrame; }
        }
        private float partialSecondsThisFrame;

        public uint MessagesSentThisFrame
        {
            get { return messagesSentThisFrame; }
        }
        private uint messagesSentThisFrame;

        /// <summary>
        /// Should only be 'set' by <see cref="TerrainComponent"/>.
        /// </summary>
        public Int64 TerrainID
        {
            get { return terrainID; }
            set { terrainID = value; }
        }
        private Int64 terrainID = -1;        

        // @TODO: Move this to graphics system or something
        public bool DrawShadowMapTextureToScreen
        {
            get { return drawShadowMapTextureToScreen; }
            set { drawShadowMapTextureToScreen = value; }
        }
        private bool drawShadowMapTextureToScreen;        

        /// <summary>
        /// Gets the settings for the game.
        /// </summary>
        /// <remarks>
        /// Inheritors can use this property to set the settings instance.
        /// </remarks>
        public Settings Settings
        {
            get { return this.settings; }
            protected set { this.settings = value; }
        }
        private Settings settings;

        /// <summary>
        /// Retrieves the global configuration manager.
        /// </summary>
        /// <remarks>Returns null if the manager has not been initialized yet.</remarks>
        public ConfigurationManager ConfigurationManager
        {
            get
            {
                return this.Services.GetService(typeof(ConfigurationManager)) as ConfigurationManager;
            }
        }

        /// <summary>
        /// Retrieves the graphics system.
        /// </summary>
        /// <remarks>Returns null if the system has not been initialized yet.</remarks>
        public GraphicsSystem Graphics
        {
            get 
            {
                return this.Services.GetService(typeof(GraphicsSystem)) as GraphicsSystem;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="GuiManager"/>
        /// </summary>
        /// <remarks>Returns null if the manager has not been initialized yet.</remarks>
        public GuiManager Gui
        {
            get
            {
                return this.Services.GetService(typeof(GuiManager)) as GuiManager;
            }
        }

        /// <summary>
        /// Gets the <see cref="ModelLoader"/>
        /// </summary>
        /// <remarks>Returns null if the loader has not been initialized yet.</remarks>
        public ModelLoader ModelLoader
        {
            get
            {
                return this.Services.GetService(typeof(ModelLoader)) as ModelLoader;
            }
        }

        /// <summary>
        /// Gets the <see cref="SceneManager"/>
        /// </summary>
        /// <remarks>Returns null if the manager has not been initialized yet.</remarks>
        public SceneManager SceneManager
        {
            get
            {
                return this.Services.GetService(typeof(SceneManager)) as SceneManager;
            }
        }

        /// <summary>
        /// Retrieves the audio system.
        /// </summary>
        /// <remarks>Returns null if the system has not been initialized yet.</remarks>
        public AudioManager Audio
        {
            get
            {
                return this.Services.GetService(typeof(AudioManager)) as AudioManager;
            }
        }

        /// <summary>
        /// Retrieves the compositor.
        /// </summary>
        /// <remarks>Returns null if the system has not been initialized yet.</remarks>
        public ScreenCompositor Compositor
        {
            get
            {
                return this.Services.GetService(typeof(ScreenCompositor)) as ScreenCompositor;
            }
        }

        /// <summary>
        /// Retrieves the physics system.
        /// </summary>
        /// <remarks>Returns null if the system has not been initialized yet.</remarks>
        public PhysicsManager Physics
        {
            get
            {
                return this.Services.GetService(typeof(PhysicsManager)) as PhysicsManager;
            }
        }

        /// <summary>
        /// Used for drawing out physics shapes
        /// </summary>
        /// <remarks>Returns null if the manager has not been initialized yet.</remarks>
        public PhysicsRenderManager PhysicsRenderer
        {
#if WINDOWS
            get
            {
                return this.Services.GetService(typeof(PhysicsRenderManager)) as PhysicsRenderManager;
            }
#else //!WINDOWS
            get
            {
                return null;
            }
#endif

        }
      
        /// <summary>
        /// Stores whether or not the game has loaded content at least once.
        /// </summary>
        private bool contentLoadedOnce = false;
        public bool ContentLoadedOnce
        {
            get { return contentLoadedOnce; }
        }

        /// <summary>
        /// This event is raised when there are messages in the queue
        /// </summary>
        /// <remarks>
        /// The message is only raised during a update loop
        /// </remarks>
        public event EngineMessageHandler GameMessage;

        /// <summary>
        /// This event is raised when the game is exiting.
        /// </summary>
        public event EventHandler<EventArgs> GameExiting;

        /// <summary>
        /// Creates a new instance of the QuickStart game base class.
        /// </summary>
        public QSGame()
        {            
            this.Activated += this.Game_Activated;
            
            this.gameTime = new GameTime();
            
            this.Services.AddService(typeof(ConfigurationManager), new ConfigurationManager());
            this.Services.AddService(typeof(GraphicsSystem), new GraphicsSystem(this));            
            this.Services.AddService(typeof(ScreenCompositor), new ScreenCompositor(this));
            this.Services.AddService(typeof(ModelLoader), new ModelLoader(this.Content));

            // Other services loaded via Configuration.XML file, which is located in
            // ./framework/code/common (not included in the project itself, you have
            // to browse there manually)
                                
            this.currentMessages = new List<IMessage>();
            this.queuedMessages = new List<IMessage>();
            this.handledMessages = new List<IMessage>();

            this.managers = new List<BaseManager>();
        }        

        /// <summary>
        /// Cleans up the QSGame instance for deletion.
        /// </summary>
        ~QSGame()
        {
        }

        /// <summary>
        /// Handles the <see cref="Game.Activate"/> event
        /// </summary>
        /// <param name="sender">The <see cref="Game"/> sending the event</param>
        /// <param name="e">Empty event arguments</param>
        private void Game_Activated(object sender, EventArgs e)
        {
            FocusGameWindow();
        }

        private bool PreProcessMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Unknown:
                    {
                        throw new Exception("Message must be given a type");
                    }
                case MessageType.Shutdown:
                    {
                        // @TODO: Rather than simply closing the program, we should notify all systems and let
                        //          them shutdown first in case they'd like to do something like save data.
                        this.exiting = true;
                        return false;
                    }
                default:
                    break;
            }

            return true;
        }

        public void QueueMessage(IMessage message)
        {            
            if (!PreProcessMessage(message))
                return;

            if (message.Protocol == MessageProtocol.Request)
            {
                throw new Exception("Messages with 'Request' message protocols cannot be queued, as they must return results immediately");
            }

            // Lock the message queue before adding message
            lock (QSGame.messageLock)
            {
                this.queuedMessages.Add(message);
            }
        }

        /// <summary>
        /// Sends messages to the system
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> which should be send</param>
        /// <returns>Returns whether or not the message made it to the entity</returns>
        /// <remarks>Use <see cref="Message<T>"/> when sending errors</remarks>
        public bool SendMessage(IMessage message)
        {
            ++this.messagesSentThisFrame;

            if (!PreProcessMessage(message))
                return false;

            bool result = this.OnGameMessage(message);

            ProcessMessageHandled(message);

            return result;
        }

        /// <summary>
        /// Sends messages to all registered QSInterface instances only.
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> which should be sent</param>
        /// <param name="interfaceYype">The <see cref="InterfaceType"/> to send the message to</param>
        /// <remarks>Use <see cref="Message<T>"/> when sending errors</remarks>
        public void SendInterfaceMessage(IMessage message, InterfaceType interfaceType )
        {
            ++this.messagesSentThisFrame;

            SceneManager sceneMgr = this.SceneManager;
            if (sceneMgr != null)
            {
                if (!sceneMgr.ForwardInterfaceMessage(message, interfaceType))
                {
                    throw new Exception("The interface does not yet exist, hasn't been initialized with the scene manager, or didn't handle the message type it was sent.");
                }

                ProcessMessageHandled(message);
            }
        }

        /// <summary>
        /// Initializes the game instance.
        /// </summary>
        protected override void Initialize()
        {
            this.Content.RootDirectory = "Content";

            this.InitializeSettings();
            this.InitializeConfiguration();            
            this.InitializeManagers();            

            base.Initialize();
        }
        
        /// <summary>
        /// Loads all core content that will be used through-out entire lifetime of game.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            this.Graphics.LoadContent(contentLoadedOnce);
            this.Compositor.LoadContent(contentLoadedOnce);
            this.SceneManager.LoadContent(contentLoadedOnce, this.Content, false);

            contentLoadedOnce = true;
        }

        /// <summary>
        /// Unloads the content that lasts the entire lifetime of the game.
        /// </summary>
        protected override void UnloadContent()
        {
            this.SceneManager.UnloadContent();
            this.Compositor.UnloadContent();
            this.Graphics.UnloadContent();            

            base.UnloadContent();
        }

        /// <summary>
        /// Handles game activated. (Windows-only)
        /// </summary>
        /// <param name="sender">The calling Game instance</param>
        /// <param name="args">The activation arguments</param>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
        }

        /// <summary>
        /// Handles game deactivation. (Windows only)
        /// </summary>
        /// <param name="sender">The calling Game instance.</param>
        /// <param name="args">The activation arguments.</param>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
        }

        /// <summary>
        /// Updates all game and engine logic for one frame.
        /// </summary>
        /// <param name="gameTime">Time snapshot for the current update.</param>
        protected override void Update(GameTime gameTime)
        {
            this.messagesSentThisFrame = 0;

            this.gameTime = gameTime;
            this.partialSecondsThisFrame = (float)( Math.Max(gameTime.ElapsedGameTime.Ticks, 1u) * 0.0000001f );

            if (this.exiting == true)
            {
                // Clear all other messages
                this.currentMessages.Clear();
                this.queuedMessages.Clear();
                this.handledMessages.Clear();

                // Send a single shutdown message to all, bypass the pool as we are shutting down                
                this.SceneManager.Shutdown();

                // Empty out the pool
                ObjectPool.ReleaseAll();

                // Raise the exit event
                this.OnGameExiting();

                this.Exit();
                return;
            }

            this.HandleMessages();

            for (int i = this.managers.Count - 1; i >= 0; --i)
            {
                if (this.managers[i].Enabled)
                {
                    this.managers[i].Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Begins a rendering frame.
        /// </summary>
        /// <returns>True if rendering can proceed, false otherwise.</returns>
        protected override bool BeginDraw()
        {
            return base.BeginDraw();
        }

        /// <summary>
        /// Renders a single frame for the current update cycle.
        /// </summary>
        /// <param name="gameTime">Time snapshot for the current frame.</param>
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        /// <summary>
        /// Ends a rendering frame.
        /// </summary>
        protected override void EndDraw()
        {
            base.EndDraw();
        }

        /// <summary>
        /// Raises the <see cref="GameExiting"/> event
        /// </summary>
        protected virtual void OnGameExiting()
        {
            if (this.GameExiting != null)
            {
                this.GameExiting(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the <see cref="GameMessage"/> event
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> to raise</param>
        protected virtual bool OnGameMessage(IMessage message)
        {
            if (this.GameMessage == null)
                return false;

            if (message.UniqueTarget == UniqueIDEmpty)
            {
                this.GameMessage(message);                
            }
            else
            {
                BaseEntity target;

                if (this.SceneManager.Entities.TryGetValue(message.UniqueTarget, out target))
                {
                    target.ExecuteMessage(message);
                }
                else
                {
                    // Target not found
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the game settings
        /// </summary>
        protected virtual void InitializeSettings()
        {
            this.Settings = this.Content.Load<Settings>("Settings");
        }

        /// <summary>
        /// Clears the list of messages and release the aquired messages
        /// </summary>
        /// <param name="list">List of <see cref="IMessage"/> to be cleared</param>
        private static void ClearMessages(IList<IMessage> list)
        {
            while (list.Count > 0)
            {
                IMessage message = list[0];
                list.RemoveAt(0);

                ObjectPool.Release(message);
            }
        }

        /// <summary>
        /// This handles messages and raises the <see cref="QSGame.GameMessage"/> if needed
        /// </summary>
        private void HandleMessages()
        {
            if (this.GameMessage != null)
            {
                lock (messageLock)
                {
                    List<IMessage> temp = this.currentMessages;                    
                    this.currentMessages = this.queuedMessages;
                    this.queuedMessages = temp;
                    this.queuedMessages.Clear();
                }

                for (int i = 0; i < this.currentMessages.Count; ++i)
                {
                    if (this.exiting == true)
                    {
                        break;
                    }

                    // Send message to entire system
                    this.OnGameMessage(this.currentMessages[i]);
                }
            }

            ClearMessages(this.currentMessages);

            if (this.GameMessage != null)
            {
                lock (messageLock)
                {
                    List<IMessage> tempHandled = this.currentMessages;
                    this.currentMessages = this.handledMessages;
                    this.handledMessages = tempHandled;
                    this.handledMessages.Clear();
                }
            }

            ClearMessages(this.currentMessages);
        }

        /// <summary>
        /// Initializes the <see cref="ConfigurationManager"/>
        /// </summary>
        private void InitializeConfiguration()
        {
            this.ConfigurationManager.Initialize();
        }

        /// <summary>
        /// Initializes the input managers
        /// </summary>
        private void InitializeManagers()
        {
            this.managers.AddRange(this.ConfigurationManager.GetManagers(this, ConfigurationManager.SectionManagers));

#if WINDOWS
            // Load managers that exist only on the windows platform
            this.managers.AddRange(this.ConfigurationManager.GetManagers(this, ConfigurationManager.SectionWindowsOnlyManagers));
#endif //WINDOWS

            this.managers.Sort(delegate(BaseManager left, BaseManager right)
            {
                return right.UpdateOrder.CompareTo(left.UpdateOrder);
            });

            for (int i = this.managers.Count - 1; i >= 0; --i)
            {
                BaseManager manager = this.managers[i];
                manager.Initialize();
            }
        }

        public void ProcessMessageHandled(IMessage message)
        {
            if (!message.GetHandled())
            {
                lock (QSGame.messageLock)
                {
                    message.SetHandled(true);
                    this.handledMessages.Add(message);
                }
            }
            else
            {
                throw new Exception("Message was improperly sent. Did you send the same message twice without re-aquiring memory for it from the ObjectPool?");
            }
        }

        /// <summary>
        /// Removes a screen from the compositor
        /// </summary>
        /// <param name="name">Name of the screen to be removed</param>
        public void RemoveCompositorScreenByName(String name)
        {
            this.Compositor.RemoveScreen(name);
        }

        /// <summary>
        /// Sets the visbility of the mouse in the game.
        /// </summary>
        /// <param name="visible"></param>
        public void SetMouseVisibility(bool visible)
        {
            if (visible != this.IsMouseVisible)
            {
                this.IsMouseVisible = visible;

                MsgMouseCursorStateChange msgChange = ObjectPool.Aquire<MsgMouseCursorStateChange>();
                msgChange.CursorVisible = this.IsMouseVisible;
                SendMessage(msgChange);
            }
        }

        /// <summary>
        /// Toggles the visibility of the mouse cursor in the game.
        /// </summary>
        public void ToggleMouseVisiblity()
        {
            this.IsMouseVisible = !this.IsMouseVisible;

            MsgMouseCursorStateChange msgChange = ObjectPool.Aquire<MsgMouseCursorStateChange>();
            msgChange.CursorVisible = this.IsMouseVisible;
            SendMessage(msgChange);
        }        

        /// <summary>
        /// Focuses the game window within the Windows OS
        /// </summary>
        public void FocusGameWindow()
        {
            Form myForm = (Form)Form.FromHandle(this.Window.Handle);
            myForm.Activate();
        }
    }
}
