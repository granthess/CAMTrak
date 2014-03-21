//
// StatsScreen.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;

using QuickStart.Entities;
using QuickStart.Interfaces;

namespace QuickStart.Compositor
{
    /// <summary>
    /// GUI for the StatsScreen
    /// </summary>
    public class StatsScreenGUI : MinimizableWindowControl
    {
        public LabelControl Label
        {
            get { return this.label; }
        }
        private LabelControl label;

        private ButtonControl closeButton;

        public StatsScreenGUI(int xPos, int yPos)
            : base()
        {
            Initialize(xPos, yPos);
        }

        private void Initialize(int xPos, int yPos)
        {
            this.Title = "Stats - Double-click to Hide";

            this.EnableDragging = true;
            this.MinimizeWidth = 310;
            this.MinimizeHeight = 25;

            this.label = new LabelControl();
            this.label.Bounds = new UniRectangle(10.0f, 15.0f, 110.0f, 30.0f);

            this.closeButton = new ButtonControl();
            this.closeButton.Text = "Close";
            this.closeButton.Bounds = new UniRectangle(new UniScalar(1.0f, -90.0f),
                                                        new UniScalar(1.0f, -40.0f),
                                                        80, 24);
            this.closeButton.Pressed += new EventHandler(closeButton_Pressed);

            this.Bounds = new UniRectangle(xPos, yPos, 320, 370);

            this.Children.Add(this.label);
            this.Children.Add(this.closeButton);

            this.Minimize();
        }

        void closeButton_Pressed(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnWindowMinimized()
        {
            this.Title = "Stats - Double-click to Open";
        }

        protected override void OnWindowRestored()
        {
            this.Title = "Stats - Double-click to Hide";
        }
    }

    /// <summary>
    /// Compositor screen for a simple debug output.
    /// </summary>
    public class StatsScreen : IScreen
    {
        /// <summary>
        /// Gets/sets the horizontal position.
        /// </summary>
        public int X
        {
            get { return this.x; }
            set { this.x = value; }
        }

        /// <summary>
        /// Gets/sets the vertical position.
        /// </summary>
        public int Y
        {
            get { return this.y; }
            set { this.y = value; }
        }

        /// <summary>
        /// The <see cref="StatsScreen"/> never needs the previous output as texture
        /// </summary>
        /// <remarks>
        /// This always returns false
        /// </remarks>
        public bool NeedBackgroundAsTexture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the name of the <see cref="StatsScreen"/>
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        private int x;
        private int y;
        private ContentManager content;
        private SpriteFont font;
        private QSGame game;
        private readonly List<Keys> keys = new List<Keys>();        

        private const string name = "Debug Information";

        private StatsScreenGUI guiScreen;

        /// <summary>
        /// A screen to show performance and rendering statistics.
        /// </summary>
        /// <param name="xPos">The horizontal position</param>
        /// <param name="yPos">The vertical position</param>
        public StatsScreen(int xPos, int yPos, QSGame game)
        {
            this.game = game;
            this.game.GameMessage += this.Game_GameMessage;
            this.x = xPos;
            this.y = yPos;

            this.guiScreen = new StatsScreenGUI(xPos, yPos);

            // Add the screen the to GuiManager's screen
            this.game.Gui.Screen.Desktop.Children.Add(this.guiScreen);
        }
        
        /// <summary>
        /// Handles game messages
        /// </summary>
        /// <param name="message">The <see cref="IMessage"/> sent</param>
        private void Game_GameMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.KeyDown:
                    {
                        MsgKeyPressed keyMessage = message as MsgKeyPressed;
                        message.TypeCheck(keyMessage);

                        this.keys.Add(keyMessage.Key);
                    }
                    break;

                case MessageType.KeyUp:
                    {
                        MsgKeyReleased keyMessage = message as MsgKeyReleased;
                        message.TypeCheck(keyMessage);

                        this.keys.Remove(keyMessage.Key);
                    }
                    break;
            }
        }

        /// <summary>
        /// Loads all content needed
        /// </summary>
        /// <param name="contentManager">The <see cref="ContentManager"/> instance to use for all content loading.</param>
        public void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor)
        {
            this.content = contentManager;
            this.content.RootDirectory = "Content";

            this.font = this.content.Load<SpriteFont>("Textures/Fonts/Tahoma8PtBold");
        }

        /// <summary>
        /// Unloads all previously loaded content.
        /// </summary>
        public void UnloadContent()
        {
            this.content.Unload();
        }

        /// <summary>
        /// Draws the debug scene
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> to use for 2D drawing.</param>
        /// <param name="background">The previous screen's output as a texture, if NeedBackgroundAsTexture is true.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawScreen(SpriteBatch batch, Texture2D background, GameTime gameTime)
        {
            // No need to create the string for a closed window
            if (!this.guiScreen.IsOpen)
            {
                return;
            }

            // We don't want the window to drag if the cursor is invisible.
            this.guiScreen.EnableDragging = this.game.IsMouseVisible;

            if (!this.game.IsMouseVisible)
            {
                this.guiScreen.Title = "Stats - F3 to enable cursor";
            }
            else
            {
                if (this.guiScreen.Minimized)
                {
                    this.guiScreen.Title = "Stats - Double-click to Open";
                }
                else
                {
                    this.guiScreen.Title = "Stats - Double-click to Hide";
                }
            }

            // Get the current camera's position
            MsgGetPosition msgGetPos = ObjectPool.Aquire<MsgGetPosition>();
            msgGetPos.UniqueTarget = QSGame.SceneMgrRootEntityID;
            this.game.SendMessage(msgGetPos);

            MsgGetRotation msgGetRot = ObjectPool.Aquire<MsgGetRotation>();
            msgGetRot.UniqueTarget = QSGame.SceneMgrRootEntityID;
            this.game.SendMessage(msgGetRot);

            MsgGetName msgName = ObjectPool.Aquire<MsgGetName>();
            msgName.UniqueTarget = QSGame.SceneMgrRootEntityID;
            this.game.SendMessage(msgName);            

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-- On-screen --");
            sb.AppendLine("Triangles: " + this.game.Graphics.TrianglesProcessedLastFrame +
                          " -- Geometry chunks: " + this.game.Graphics.GeometryChunksRenderedLastFrame);            
            sb.AppendLine("Particles: " + this.game.Graphics.ParticlesRenderedLastFrame);
            sb.AppendLine();            

            sb.AppendLine("Object Pool: ");
            sb.Append("Types: " + ObjectPool.free.Count);
            int count = 0;
            foreach (List<IPoolItem> list in ObjectPool.free.Values)
            {
                count += list.Count;
            }
            sb.Append(", free: " + count);            

            count = 0;
            foreach (List<IPoolItem> list in ObjectPool.locked.Values)
            {
                count += list.Count;
            }
            sb.AppendLine(", locked: " + count);
            sb.AppendLine();

            sb.AppendLine("Messages sent last frame: " + this.game.MessagesSentThisFrame);
            sb.AppendLine("Number of current messages: " + this.game.currentMessages.Count);
            sb.AppendLine("Number of queued messages: " + this.game.queuedMessages.Count);
            sb.AppendLine("  Queued messages:");
            for (int i = this.game.queuedMessages.Count - 1; i >= 0; --i)
            {
                IMessage message = this.game.queuedMessages[i];
                sb.AppendLine(string.Format("  {0} - {1}", message.Type, message.GetType().Name));
            }
            sb.AppendLine();

            sb.AppendLine(string.Format("Entities, total: {0}, active: {1}",
                          this.game.SceneManager.Entities.Count,
                          this.game.SceneManager.NumberOfActiveEntities));
            sb.AppendLine();
            sb.AppendLine("Entity acting as camera: " + msgName.Name);

            Vector3 camPos = msgGetPos.Position;
            sb.AppendLine(string.Format("Cam Pos - X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}", camPos.X, camPos.Y, camPos.Z));

            Vector3 camFwd = msgGetRot.Rotation.Forward;
            sb.AppendLine(string.Format("Cam Fwd - X: {0:0.00}, Y: {1:0.00}, Z: {2:0.00}", camFwd.X, camFwd.Y, camFwd.Z));

            MsgGetControlledEntity msgControlled = ObjectPool.Aquire<MsgGetControlledEntity>();
            this.game.SendInterfaceMessage(msgControlled, InterfaceType.SceneManager);
            if (msgControlled.ControlledEntityID != QSGame.UniqueIDEmpty)
            {
                MsgGetName msgGetName = ObjectPool.Aquire<MsgGetName>();
                msgGetName.UniqueTarget = msgControlled.ControlledEntityID;
                this.game.SendMessage(msgGetName);

                sb.AppendLine("Camera attached to: " + msgGetName.Name);                
            }
            sb.AppendLine();

            sb.AppendLine("Keys pressed: ");
            for (int i = this.keys.Count - 1; i >= 0; --i)
            {
                sb.Append(this.keys[i].ToString() + " ");
            }
            sb.AppendLine();

            this.guiScreen.Label.Text = sb.ToString();
        }
    }
}
