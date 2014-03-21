/*
 * SceneManager.cs
 * 
 * This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
 * for license details.
 */

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using QuickStart.Camera;
using QuickStart.Components;
using QuickStart.Compositor;
using QuickStart.Entities;
using QuickStart.Graphics;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.EnvironmentalSettings;
using QuickStart.Utils;

namespace QuickStart
{
    /// <summary>
    /// The scene manager will eventually be the "brains" of the game. It will update and run
    /// each of the interfaces and store some core information (TBD).
    /// </summary>
    public class SceneManager : BaseManager, IRenderChunkProvider, IScreen
    {
        private class InterfaceTypeComparer : IEqualityComparer<InterfaceType>
        {
            #region IEqualityComparer<InterfaceType> Members

            public bool Equals(InterfaceType lhs, InterfaceType rhs)    { return lhs == rhs; }
            public int GetHashCode(InterfaceType obj)   { return 0; }

            #endregion
        }

        /// <summary>
        /// Exposes the <see cref="ContentManager"/>.
        /// </summary>
        private ContentManager content;
        
        /// <summary>
        /// All interfaces controlled by the <see cref="SceneManager"/> are run through this list
        /// </summary>
        private Dictionary<InterfaceType, QSInterface> interfaces;

        /// <summary>
        /// Holds all of the scenes in memory. Scenes may be used to stream data in and out of the
        /// scene manager as scenes load and unload.
        /// </summary>
        public List<Scene> Scenes
        {
            get { return scenes; }
        }
        private List<Scene> scenes;

        public Scene ActiveScene
        {
            get { return activeScene; }
            set
            {
                if (value != this.activeScene)
                {
                    if (this.activeScene != null)
                    {
                        // Set the current active scene to inactive
                        this.activeScene.Deactiviate();
                    }
                   
                    this.activeScene = value;
                    this.activeScene.Activate();
                }
            }
        }
        private Scene activeScene;

        /// <summary>
        /// A list of all the entities in this scene.
        /// </summary>
        public Dictionary<Int64, BaseEntity> Entities
        {
            get { return entities; }
        }
        private Dictionary<Int64, BaseEntity> entities;
        private List<BaseEntity> entitiesToRemove;

        public int NumberOfActiveEntities
        {
            get { return this.activeEntities.Count; }
        }
        private HashSet<BaseEntity> activeEntities;
        private List<BaseEntity> activeEntitiesToAdd;
        private List<BaseEntity> activeEntitiesToRemove;      

        private CameraInterface cameraInterface;        

        public EntityLoader EntityLoader
        {
            get { return entityLoader; }
        }
        private EntityLoader entityLoader; 

        /// <summary>
        /// A root entity for the scene manager to contain things like cameras.
        /// </summary>
        /// <remarks>@todo: This may be TEMPORARY. Still testing out the idea of a root entity for the camera system.</remarks>
        public BaseEntity SceneMgrRootEntity
        {
            get { return sceneMgrRootEntity; }
        }
        private BaseEntity sceneMgrRootEntity;

        public bool NeedBackgroundAsTexture
        {
            get { return false; }
        }

        /// <summary>
        /// Name of this manager
        /// </summary>
        public string Name
        {
            get { return name; }
        }
        private const string name = "SceneManager";

        BaseInputComponent currentCameraInputType;

        public Int64 ControlledEntity
        {
            get { return this.controlledEntity; }
        }
        private Int64 controlledEntity = -1;

        public GameTime GameTimeData
        {
            get { return this.gameTimeData; }
        }
        private GameTime gameTimeData;

        /// <summary>
        /// Creates and initializes a scene manager.
        /// </summary>
        /// <param name="game">Base engine game class</param>
        public SceneManager(QSGame game) :
            base(game)
        {            
            this.Game.Services.AddService(typeof(SceneManager), this);
        }

        /// <summary>
        /// Cleans up the SceneManager instance for deletion.
        /// </summary>
        ~SceneManager()
        {
            foreach (QSInterface qsInterface in this.interfaces.Values)
            {
                qsInterface.Shutdown();
            }
        }

        /// <summary>
        /// Shuts down all interfaces
        /// </summary>
        public void Shutdown()
        {
            foreach (QSInterface qsInterface in this.interfaces.Values)
            {
                qsInterface.Shutdown();
            }
            this.interfaces.Clear();
        }

        protected override void InitializeCore()
        {
            this.interfaces = new Dictionary<InterfaceType, QSInterface>();
            this.scenes = new List<Scene>();
            this.entities = new Dictionary<Int64, BaseEntity>();
            this.entitiesToRemove = new List<BaseEntity>();
            this.activeEntities = new HashSet<BaseEntity>();
            this.activeEntitiesToAdd = new List<BaseEntity>();
            this.activeEntitiesToRemove = new List<BaseEntity>();
        }

        /// <summary>
        /// Updates all entities and interfaces that are currently running
        /// </summary>
        /// <param name="gameTime">Contains timer information</param>
        protected override void UpdateCore(GameTime gameTime)
        {
            ProcessEntityQueues();

            gameTimeData = gameTime;

            // Update all interfaces
            foreach ( QSInterface qsInterface in this.interfaces.Values )
            {
                qsInterface.Update(gameTime);
            }

            // End the current physics frame
            MsgEndPhysicsFrame msgEndFrame = ObjectPool.Aquire<MsgEndPhysicsFrame>();
            this.Game.SendInterfaceMessage(msgEndFrame, InterfaceType.Physics);

            foreach (BaseEntity entity in this.activeEntities)
            {
                entity.Update(gameTime);
            }

            if (msgEndFrame.FrameEnded)
            {
                foreach (BaseEntity entity in this.activeEntities)
                {
                    entity.FixedUpdate(gameTime);
                }
            }

            // Begin the next physics frame
            MsgBeginPhysicsFrame msgBeginFrame = ObjectPool.Aquire<MsgBeginPhysicsFrame>();
            msgBeginFrame.GameTime = gameTime;
            this.Game.SendInterfaceMessage(msgBeginFrame, InterfaceType.Physics);
        }

        private void ProcessEntityQueues()
        {
            for (int i = 0; i < this.entitiesToRemove.Count; ++i)            
            {
                this.entities.Remove(this.entitiesToRemove[i].UniqueID);                
            }
            this.entitiesToRemove.Clear();

            foreach (BaseEntity entity in this.activeEntitiesToAdd)
            {
                this.activeEntities.Add(entity);
            }
            this.activeEntitiesToAdd.Clear();

            foreach (BaseEntity entity in this.activeEntitiesToRemove)
            {
                this.activeEntities.Remove(entity);
            }
            this.activeEntitiesToRemove.Clear();
        }

        /// <summary>
        /// Queries for all potentially visible RenderChunk instances given a RenderPassDesc descriptor.
        /// </summary>
        /// <param name="desc">A descriptor for the current rendering pass.</param>
        public void QueryForChunks(ref RenderPassDesc desc)
        {
            foreach (BaseEntity entity in this.entities.Values)
            {
                entity.QueryForRenderChunks(ref desc);
            }
        }

        /// <summary>
        /// Draws the scene for the compositor.
        /// </summary>
        /// <param name="batch">The <see cref="SpriteBatch"/> instance to use for 2D rendering.</param>
        /// <param name="background">The previous screen's output as a texture, if NeedBackgroundAsTexture is true.  Null, otherwise.</param>
        /// <param name="gameTime">The <see cref="GameTime"/> structure for the current Game.Draw() cycle.</param>
        public void DrawScreen(SpriteBatch batch, Texture2D background, GameTime gameTime)
        {
            if (this.cameraInterface.RenderCamera != null)
            {
                this.Game.Graphics.DrawFrame(this.cameraInterface.RenderCamera, gameTime);
            }
        }

        /// <summary>
        /// Adds a <see cref="QSInterface"/> to the <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="inQSInterface"><see cref="QSInterface"/> to add.</param>
        private void AddInterface(QSInterface inQSInterface)
        {
            QSInterface qsInterface;
            if ( interfaces.TryGetValue(inQSInterface.InterfaceID, out qsInterface) )
            {
                throw new Exception("An interface by this ID already exists");
            }

            this.interfaces.Add(inQSInterface.InterfaceID, inQSInterface);
        }

        public QSInterface GetInterface(InterfaceType type)
        {
            QSInterface foundInterface;
            interfaces.TryGetValue(type, out foundInterface);

            return foundInterface;
        }

        /// <summary>
        /// Adds a <see cref="Scene"/> to the <see cref="SceneManager"/>.
        /// </summary>
        /// <param name="inScene">Scene to add</param>
        private void AddScene(Scene inScene)
        {
            // @todo: Should we check here if scene is finished loading?
            this.scenes.Add(inScene);
        }

        /// <summary>
        /// Buffer and load a <see cref="Scene"/> into memory.
        /// </summary>
        /// <param name="targetScene"><see cref="Scene"/> to load</param>
        /// <param name="autoBegin">Begin a <see cref="Scene"/> once loaded</param>
        public void LoadScene( Scene targetScene, bool autoBegin )
        {
            // We need a scene loader, which should probably be its own class.
            // Scene loading should be buffered, preferably in its own thread.
            
            // If a scene is to be buffered in, would we ever want to begin before all of it
            // is loaded?

            // *** TEMP ***
            // Currently this method doesn't buffer anything, it simply adds a given scene
            // to a list. Eventually the scene should be loaded from an XML file. XML file can contain
            // all pertainant info about the scene, like which models to load, where to place them, what
            // kind of sky dome, what strength of gravity, etc. The loader will create a scene out of this
            // and then start it if needed.

            AddScene(targetScene);

            if (autoBegin)
            {
                BeginScene(targetScene);
            }
        }

        /// <summary>
        /// Unload a <see cref="Scene"/> from memory.
        /// </summary>
        /// <param name="targetScene">Scene to unload</param>
        public void UnloadScene(Scene targetScene)
        {
            // We need a scene unload, which should probably be in a separate class.
            // Could be combined with the scene loader. Scene unloading doesn't need
            // to be buffered, but should still be done in its own scene.
            
            // Scene should not be able to unload unless stopped first.

            // Any other conditions required for unloading?
            StopScene(targetScene);

            this.scenes.Remove(targetScene);
        }

        /// <summary>
        /// Start running a new <see cref="Scene"/>. After beginning a scene it will continue to run
        /// until stopped. Scene must be loaded before it can begin.
        /// </summary>
        /// <param name="targetScene"><see cref="Scene"/> to run.</param>
        public void BeginScene(Scene targetScene)
        {
            targetScene.Activate();
        }

        /// <summary>
        /// Stops <see cref="Scene"/> from being processes. This step must be done before unloading a scene.
        /// </summary>
        /// <param name="targetScene"><see cref="Scene"/> to stop.</param>
        public void StopScene(Scene targetScene)
        {
            targetScene.Deactiviate();
        }

        /// <summary>
        /// Loads all needed ContentManager content.
        /// </summary>
        public void LoadContent(bool isReload, ContentManager contentManager, bool fromCompositor)
        {
            this.content = contentManager;

            if (!fromCompositor && !isReload)
            {
                ////////////////////////////////////////////////////////////////////////////
                // This must always remain the first entity created
                this.sceneMgrRootEntity = new BaseEntity(this.Game);
                this.sceneMgrRootEntity.Name = "SceneMgr Root";

                this.AddEntity(sceneMgrRootEntity);
                ////////////////////////////////////////////////////////////////////////////

                this.cameraInterface = new CameraInterface(this.Game);
                AddInterface(this.cameraInterface);
                
                AddInterface(new PhysicsInterface(this.Game));

                AddInterface(new InputInterface(this.Game));

                this.entityLoader = new EntityLoader(this.Game);

                AddInterface(this.Game.Graphics);

                SetupBaseCamera();

                MsgSetGravity msgSetGrav = ObjectPool.Aquire<MsgSetGravity>();                
                msgSetGrav.Gravity = new Vector3(0.0f, -60.00f, 0.0f);
                this.Game.SendInterfaceMessage(msgSetGrav, InterfaceType.Physics);

                // Start the first physics frame.
                MsgBeginPhysicsFrame msgBeginFrame = ObjectPool.Aquire<MsgBeginPhysicsFrame>();
                msgBeginFrame.GameTime = new GameTime();
                this.Game.SendInterfaceMessage(msgBeginFrame, InterfaceType.Physics);
            }
        }

        /// <summary>
        /// Unloads all ContentManager content.
        /// </summary>
        public void UnloadContent()
        {     
        }

        /// <summary>
        /// Adds an entity to the overall scene.
        /// </summary>
        /// <param name="inEntity">Entity to add to scene</param>        
        public void AddEntity(BaseEntity entity)
        {
            this.entities.Add(entity.UniqueID, entity);            

            entity.AddedToScene(this);

            if (entity.IsActivated)
            {
                this.activeEntitiesToAdd.Add(entity);
            }
        }

        /// <summary>
        /// Removes an entity from the overall scene on the next Update() loop.
        /// </summary>
        /// <param name="targetEntity">Entity to remove from scene</param>        
        private void AddEntityToDeleteQueue(BaseEntity entity)
        {
            this.entitiesToRemove.Add(entity);
            this.activeEntitiesToAdd.Remove(entity);
            this.activeEntitiesToRemove.Add(entity);
        }

        public void EntityActivated(BaseEntity entity)
        {
            if (this.entities.ContainsKey(entity.UniqueID))
            {
                this.activeEntitiesToAdd.Add(entity);
                this.activeEntitiesToRemove.Remove(entity);
            }
        }

        public void EntityDeactivated(BaseEntity entity)
        {
            if (this.entities.ContainsKey(entity.UniqueID))
            {
                this.activeEntitiesToRemove.Add(entity);
                this.activeEntitiesToAdd.Remove(entity);
            }
        }

        private void ShutdownEntity(BaseEntity entity)
        {
            // If entity is in a scene
            if (entity != null)
            {
                // Send a shutdown to the entity
                entity.Shutdown();

                // Shutdown should be complete, now pull it from the scene
                AddEntityToDeleteQueue(entity);                
            }
        }

        /// <summary>     
        /// Uses the EntityLoader to create an entity and its components, based
        /// on a TemplateID. After entity is loaded it is added to the scene.
        /// </summary>
        /// <param name="TemplateID">ID of the the entity template from the definition XML file</param>
        /// <returns>The loaded entity</returns>
        public BaseEntity CreateAndAddEntityByTemplateID(int TemplateID)
        {
            BaseEntity newEntity = this.entityLoader.LoadEntity(TemplateID);
            AddEntity(newEntity);

            return newEntity;
        }

        /// <summary>
        /// Uses the EntityLoader to load components onto an existing entity, based
        /// on a TemplateID. After components are loaded onto the entity it is added
        /// to the scene.
        /// </summary>
        /// <param name="entity">The entity that will be added to the scene</param>
        /// <param name="TemplateID">ID of the the entity template from the definition XML file</param>        
        public void AddEntityByTemplateID(BaseEntity entity, int TemplateID)
        {
            this.entityLoader.LoadEntity(entity, TemplateID);
            AddEntity(entity);
        }

        /// <summary>
        /// Sets up the base camera to start off the scene manager.
        /// </summary>
        /// <remarks>This will likely be temporary until we are reading in a camera from the editor.</remarks>
        public void SetupBaseCamera()
        {
            CameraComponent sceneMgrCamera = EntityLoader.LoadComponent(this.sceneMgrRootEntity, 
                                                        ComponentType.CameraComponent, this.content,
                                                        "Entities/ComponentDefinitions/Camera/StandardCamera")
                                                        as CameraComponent;

            // Lets create a free-cam
            CreateAndStartFreeMovingCamera();

            // Set our camera's position
            this.sceneMgrRootEntity.Position = new Vector3(800, 220, 780); 

            // Set our camera's look-at point            
            this.sceneMgrRootEntity.LookAt(new Vector3(900, 220, 700));
        }

        private void CreateAndStartFreeMovingCamera()
        {
            if (currentCameraInputType != null)
            {
                if (currentCameraInputType is FreeCameraInputComponent)
                    return;

                this.sceneMgrRootEntity.RemoveComponent(currentCameraInputType);

                currentCameraInputType = null;
            }

            currentCameraInputType = new FreeCameraInputComponent(this.sceneMgrRootEntity);

            // Set a new render camera
            MsgSetRenderEntity camSetRenderMsg = ObjectPool.Aquire<MsgSetRenderEntity>();
            camSetRenderMsg.Entity = this.sceneMgrRootEntity;
            this.Game.SendInterfaceMessage(camSetRenderMsg, InterfaceType.Camera);
        }

        private void CreateAndStartArcBallCamera()
        {
            BaseEntity entity;
            if (entities.TryGetValue(this.controlledEntity, out entity))
            {
                MsgSetParent msgSetParent = ObjectPool.Aquire<MsgSetParent>();
                msgSetParent.ParentEntity = entity;
                msgSetParent.UniqueTarget = this.sceneMgrRootEntity.UniqueID;
                this.Game.SendMessage(msgSetParent);
            }

            if (currentCameraInputType != null)
            {
                if (currentCameraInputType is ArcBallCameraInputComponent)
                    return;

                this.sceneMgrRootEntity.RemoveComponent(currentCameraInputType);
            }

            ArcBallCameraInputComponent arcBallCam = new ArcBallCameraInputComponent(this.sceneMgrRootEntity);
            currentCameraInputType = arcBallCam;

            arcBallCam.TargetPosition = new Vector3(950, 400, 950);
            arcBallCam.LookAheadDistance = 10.0f;
            arcBallCam.LookAboveDistance = 5.0f;
            arcBallCam.HorizontalAngle = -1.3f;
            arcBallCam.VerticalAngle = 1.2f;
        }

        public void GetEntityReflectionInfo( Int64 EntityID, out List<TypeReflectionInfo> output )
        {
            output = new List<TypeReflectionInfo>();

            BaseEntity entity;
            if (entities.TryGetValue(EntityID, out entity))
            {
                // First we'll get the Entity info                
                TypeReflectionInfo entityInfo = QSUtils.GetReflectionInfo(entity, entity.GetType());
                output.Add(entityInfo);

                // Now we'll get the Components' info                                
                foreach (KeyValuePair<ComponentType, BaseComponent> entry in entity.Components)
                {                    
                    TypeReflectionInfo compInfo = QSUtils.GetReflectionInfo(entry.Value, entry.Value.GetType());
                    output.Add(compInfo);
                }
            }
        }

        /// <summary>
        /// Forwards a message from the message system to the appropriate <see cref="QSInterface"/>.
        /// This function MUST be called only from QSGame, otherwise it will never mark the message properly for release
        /// and it will leak memory.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <param name="interfaceType">Interface type to send message to</param>
        /// <returns>Returns false if interface was not found</returns>
        public bool ForwardInterfaceMessage(IMessage message, InterfaceType interfaceType)
        {
            if (interfaceType == InterfaceType.SceneManager)
            {
                return this.ExecuteMessage(message);
            }
            
            QSInterface qsInterface;
            if ( !this.interfaces.TryGetValue(interfaceType, out qsInterface) )
                return false;

            return qsInterface.ExecuteMessage(message);
        }

        /// <summary>
        /// Message handler for messages meant only for the scene manager.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.RemoveEntity:
                    {
                        MsgRemoveEntity remEntityMsg = message as MsgRemoveEntity;
                        message.TypeCheck(remEntityMsg);

                        if (remEntityMsg.EntityID != QSGame.UniqueIDEmpty)
                        {
                            BaseEntity entity;
                            if (this.entities.TryGetValue(remEntityMsg.EntityID, out entity))
                            {
                                ShutdownEntity(entity);
                            }
                        }
                    }
                    return true;
                case MessageType.GetEntityIDList:
                    {
                        MsgGetEntityIDList msgGetEntityIDs = message as MsgGetEntityIDList;
                        message.TypeCheck(msgGetEntityIDs);

                        msgGetEntityIDs.EntityIDList = new Int64[this.entities.Count];
                        this.entities.Keys.CopyTo(msgGetEntityIDs.EntityIDList, 0);
                    }
                    return true;
                case MessageType.SetControlledEntity:
                    {
                        MsgSetControlledEntity msgSetControlled = message as MsgSetControlledEntity;
                        message.TypeCheck(msgSetControlled);

                        Int64 oldControlledEntity = this.controlledEntity;

                        this.controlledEntity = msgSetControlled.ControlledEntityID;

                        // Was there a change
                        if (oldControlledEntity != this.controlledEntity)
                        {
                            if (oldControlledEntity != QSGame.UniqueIDEmpty)
                            {
                                MsgRemoveChild msgRemoveChild = ObjectPool.Aquire<MsgRemoveChild>();
                                msgRemoveChild.Child = this.sceneMgrRootEntity;
                                msgRemoveChild.UniqueTarget = oldControlledEntity;
                                this.Game.SendMessage(msgRemoveChild);

                                MsgSetIsControlled msgControlled = ObjectPool.Aquire<MsgSetIsControlled>();
                                msgControlled.Controlled = false;
                                msgControlled.UniqueTarget = oldControlledEntity;
                                this.Game.SendMessage(msgControlled);
                            }

                            // If the new ID is empty, there is no more controlled entity
                            if (this.controlledEntity == QSGame.UniqueIDEmpty)
                            {
                                CreateAndStartFreeMovingCamera();
                            }
                            else
                            {
                                MsgSetIsControlled msgControlled = ObjectPool.Aquire<MsgSetIsControlled>();
                                msgControlled.Controlled = true;
                                msgControlled.UniqueTarget = this.controlledEntity;
                                this.Game.SendMessage(msgControlled);

                                BaseEntity controlledEntity;
                                if (entities.TryGetValue(this.controlledEntity, out controlledEntity))
                                {
                                    CreateAndStartArcBallCamera();
                                }
                            }
                        }
                    }
                    return true;
                case MessageType.GetControlledEntity:
                    {
                        MsgGetControlledEntity msgGetControlled = message as MsgGetControlledEntity;
                        message.TypeCheck(msgGetControlled);

                        msgGetControlled.ControlledEntityID = this.controlledEntity;
                    }
                    return true;
                case MessageType.GetEntityByID:
                    {
                        MsgGetEntityByID msgGetEntity = message as MsgGetEntityByID;
                        message.TypeCheck(msgGetEntity);

                        BaseEntity entity;
                        if (this.entities.TryGetValue(msgGetEntity.EntityID, out entity))
                        {
                            msgGetEntity.Entity = entity;
                        }
                    }
                    return true;
                case MessageType.GetFogSettings:
                    {
                        if (this.activeScene != null)
                        {
                            MsgGetFogSettings msgGetFog = message as MsgGetFogSettings;
                            message.TypeCheck(msgGetFog);

                            msgGetFog.Settings = this.activeScene.FogSettings;
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
