// BaseEntity.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Xna.Framework;

using QuickStart.Graphics;
using QuickStart.Components;

namespace QuickStart.Entities
{
    /// <summary>
    /// Create an entity.
    /// </summary>
    public class BaseEntity
    {
        public Int64 UniqueID
        {
            get { return this.uniqueID; }
        }
        private Int64 uniqueID = QSGame.UniqueIDEmpty;
        static Int64 totalIDsUsed = 0;

        public String Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        private String name = "Unnamed";

        public int TemplateID
        {
            get { return this.templateID; }
            set { this.templateID = value; }
        }
        private int templateID = QSGame.TemplateIDEmpty;

        /// <summary>
        /// List of all the components attached to this entity.
        /// </summary>
        private Dictionary<ComponentType, BaseComponent> componentList;
        public Dictionary<ComponentType, BaseComponent> Components
        {
            get { return componentList; }
        }

        /// <summary>
        /// List of all components that are active on this entity.
        /// </summary>
        private List<BaseComponent> activeComponents;

        /// <summary>
        /// List of all the child entities of this entity.
        /// </summary>
        private Dictionary<Int64, BaseEntity> children;

        /// <summary>
        /// Reference to this entity's parent entity (if it has one).
        /// </summary>
        public BaseEntity ParentEntity
        {
            get { return this.parentEntity; }
        }
        private BaseEntity parentEntity;

        /// <summary>
        /// Returns whether or not this entity has any components that actively
        /// update.
        /// </summary>
        public bool IsActivated
        {
            get { return (this.activeComponents.Count > 0); }
        }

        /// <summary>
        /// Gets the <see cref="QSGame"/> for the entity
        /// </summary>
        public QSGame Game
        {
            get { return this.game; }
        }
        private readonly QSGame game;

        /// <summary>
        /// Position in the scene
        /// </summary>        
        public Vector3 Position
        {
            get { return this.position; }
            set
            {
                if (this.position != value)
                {
                    this.position = value;

                    MsgPositionChanged msgPosChanged = ObjectPool.Aquire<MsgPositionChanged>();
                    msgPosChanged.Position = value;
                    msgPosChanged.UniqueTarget = this.uniqueID;
                    this.game.SendMessage(msgPosChanged);
                }
            }
        }
        private Vector3 position = Vector3.Zero;

        /// <summary>
        /// Rotation matrix, which stores its right, up, and forward vectors.
        /// </summary>
        public Matrix Rotation
        {
            get { return this.rotation; }
            set
            {
                if (this.rotation != value)
                {
                    this.rotation = value;

                    MsgRotationChanged msgRotChanged = ObjectPool.Aquire<MsgRotationChanged>();
                    msgRotChanged.Rotation = value;
                    msgRotChanged.UniqueTarget = this.uniqueID;
                    this.game.SendMessage(msgRotChanged);
                }
            }
        }
        private Matrix rotation = Matrix.Identity;

        /// <summary>
        /// Entity's scale
        /// </summary>
        public float Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }
        private float scale = 1.0f;

        /// <summary>
        /// This event is raised when a property is set
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Create an entity
        /// </summary>
        /// <param name="game">Reference to the main game object.</param>
        public BaseEntity(QSGame game)
            : this(game, QSGame.TemplateIDCustom)
        {            
        }

        /// <summary>
        /// Create an entity
        /// </summary>
        /// <param name="game">Reference to the main game object.</param>
        /// <param name="position">Position of entity</param>
        /// <param name="rotation">Rotation of entity</param>
        /// <param name="scale">Scale of entity</param>
        public BaseEntity(QSGame game, Vector3 position, Matrix rotation, float scale)
            : this(game, QSGame.TemplateIDCustom)
        {            
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        /// <summary>
        /// Create an entity
        /// </summary>
        /// <param name="game">Reference to the main game object.</param>
        /// <param name="TemplateID">Template ID used to create this object.</param>
        public BaseEntity(QSGame game, int TemplateID)
        {
            this.game = game;
            this.templateID = TemplateID;

            this.componentList = new Dictionary<ComponentType, BaseComponent>();
            this.activeComponents = new List<BaseComponent>();
            this.children = new Dictionary<Int64, BaseEntity>();

            this.Game.GameMessage += this.Game_GameMessage;

            this.uniqueID = totalIDsUsed;
            ++totalIDsUsed;
        }

        /// <summary>
        /// Adds a component to the active list, allowing it to have Update() called on it each frame.
        /// </summary>
        /// <param name="component">Component to add to active list</param>
        public void ActivateComponent(BaseComponent component)
        {
            BaseComponent dummy = null;
            if (this.componentList.TryGetValue(component.GetComponentType(), out dummy))
            {
                if (!this.activeComponents.Contains(component) )
                {
                    this.activeComponents.Add(component);

                    if (this.activeComponents.Count == 1)
                    {
                        this.game.SceneManager.EntityActivated(this);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a component from the active list, preventing it from being updated (Update()) each frame.
        /// </summary>
        /// <param name="component">Component to remove from active list</param>
        public void DeactivateComponent(BaseComponent component)
        {
            if ( this.activeComponents.Contains(component) )
            {
                this.activeComponents.Remove(component);

                if (this.activeComponents.Count == 0)
                {
                    this.game.SceneManager.EntityDeactivated(this);
                }
            }
        }

        public BaseComponent GetComponentByType(ComponentType type)
        {
            BaseComponent returnVal = null;
            this.componentList.TryGetValue(type, out returnVal);
            
            return returnVal;
        }

        /// <summary>
        /// BaseEntity update methods
        /// </summary>
        /// <param name="gameTime">Contains timer information</param>
        public void Update(GameTime gameTime)
        {
            for (int i = this.activeComponents.Count - 1; i >= 0; --i)
            {
                this.activeComponents[i].Update(gameTime);
            }
        }

        /// <summary>
        /// This update method occurs at a fixed rate. It always occurs immediately after
        /// a physics update occurs. NOTE: This update will not occur at a fixed rate if
        /// the physics simulation is not fixed (it IS fixed by default).
        /// </summary>        
        /// <param name="gameTime"></param>
        public void FixedUpdate(GameTime gameTime)
        {
            for (int i = this.activeComponents.Count - 1; i >= 0; --i)
            {
                this.activeComponents[i].FixedUpdate(gameTime);
            }
        }

        /// <summary>
        /// Gives each component a change to give render information.
        /// Generally only the <see cref="RenderComponent"/> does this, but occasionally we may want others to
        /// have the chance to do it, just like the PhysicsComponent can when it is displaying physics meshes.
        /// </summary>
        /// <param name="desc">Descriptor reference from the renderer</param>
        public virtual void QueryForRenderChunks(ref RenderPassDesc desc)
        {
            foreach (KeyValuePair<ComponentType, BaseComponent> pair in this.componentList)                        
            {
                (pair.Value).QueryForChunks(ref desc);
            }
        }

        /// <summary>
        /// This method is invoked when a property on the entity is changed
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed</param>
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Attaches a component to this entity.
        /// </summary>
        /// <param name="component">Component to attach</param>
        public void AddComponent(BaseComponent component)
        {
            BaseComponent dummy = null;
            if (!this.componentList.TryGetValue(component.GetComponentType(), out dummy))
            {
                this.componentList.Add(component.GetComponentType(), component);
            }
        }

        /// <summary>
        /// Removes a component from this entity.
        /// </summary>
        /// <param name="component">Component to remove</param>
        public void RemoveComponent(BaseComponent component)
        {
            BaseComponent dummy = null;
            if (this.componentList.TryGetValue(component.GetComponentType(), out dummy))
            {            
                component.Shutdown();

                this.componentList.Remove(component.GetComponentType());
                this.activeComponents.Remove(component);
            }
        }

        /// <summary>
        /// Attaches a child entity to this entity.
        /// </summary>
        /// <param name="childEntity">Entity to be the child</param>
        private void AddChild(BaseEntity childEntity)
        {
            if (childEntity != null && childEntity != this)
            {
                MsgParentAdded msgParentAdd = ObjectPool.Aquire<MsgParentAdded>();                
                msgParentAdd.UniqueTarget = childEntity.uniqueID;
                this.game.SendMessage(msgParentAdd);

                children.Add(childEntity.UniqueID, childEntity);                
            }
        }

        private void RemoveAllChildren()
        {
            foreach (Int64 child in this.children.Keys)
            {
                MsgParentRemoved msgParentRem = ObjectPool.Aquire<MsgParentRemoved>();                
                msgParentRem.UniqueTarget = child;
                this.game.SendMessage(msgParentRem);
            }

            this.children.Clear();
        }        

        private void RemoveChild(BaseEntity childEntity)
        {
            if (childEntity != null)
            {
                MsgParentRemoved msgParentRem = ObjectPool.Aquire<MsgParentRemoved>();                
                msgParentRem.UniqueTarget = childEntity.uniqueID;
                this.game.SendMessage(msgParentRem);

                children.Remove(childEntity.UniqueID);
            }
        }

        /// <summary>
        /// Sets a new parent for this entity.
        /// </summary>
        /// <param name="parent">Parent entity</param>
        private void SetParent(BaseEntity parent)
        {
            if (this.parentEntity != null)
            {
                MsgChildRemoved msgChildRem = ObjectPool.Aquire<MsgChildRemoved>();                
                msgChildRem.UniqueTarget = this.parentEntity.uniqueID;
                this.game.SendMessage(msgChildRem);
            }

            if (parent == null)
            {
                parent = this.game.SceneManager.SceneMgrRootEntity;
            }

            MsgChildAdded msgChildAdd = ObjectPool.Aquire<MsgChildAdded>();            
            msgChildAdd.UniqueTarget = parent.uniqueID;
            msgChildAdd.Child = this;
            this.game.SendMessage(msgChildAdd);

            this.parentEntity = parent;
        }

        public bool HasParent
        {
            get { return (this.parentEntity != null); }
        }

        public void Shutdown()
        {
            // First we call shutdown on all components, giving them one last
            // chance to send any messages they need to send.
            foreach (KeyValuePair<ComponentType, BaseComponent> pair in this.componentList)                                                
            {
                (pair.Value).Shutdown();                
            }

            // Now we detach the entity from parent and children entities it may have
            SetParent(null);
            RemoveAllChildren();

            // Now we get rid of the components entirely.
            componentList.Clear();
        }

        /// <summary>
        /// Receive and update for this entity's position, rotation, and velocities from the physics system.
        /// </summary>
        /// <param name="position">Entity's position</param>
        /// <param name="rotation">Entity's rotation</param>
        public void UpdateFromPhysics(Vector3 position, Matrix rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        /// Points camera in direction of any position.
        /// </summary>
        /// <param name="targetPos">Target position for camera to face.</param>
        public void LookAt(Vector3 targetPos)
        {
            Vector3 newForward = targetPos - this.position;
            newForward.Normalize();

            Matrix rot = Matrix.Identity;
            rot.Forward = newForward;

            Vector3 referenceVector = Vector3.UnitY;

            // On the slim chance that the camera is pointer perfectly parallel with the Y Axis, we cannot
            // use cross product with a parallel axis, so we change the reference vector to the forward axis (Z).
            if (rot.Forward.Y == referenceVector.Y || rot.Forward.Y == -referenceVector.Y)
            {
                referenceVector = Vector3.UnitZ;
            }

            rot.Right = Vector3.Cross(rot.Forward, referenceVector);
            rot.Up = Vector3.Cross(rot.Right, rot.Forward);

            MsgSetRotation setRotMsg = ObjectPool.Aquire<MsgSetRotation>();
            setRotMsg.Rotation = rot;
            setRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(setRotMsg);
        }

        /// <summary>
        /// Points camera in direction of any position.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public void LookAt(float x, float y, float z)
        {
            Vector3 newForward = new Vector3(x - this.position.X, y - this.position.Y, z - this.position.Z);
            newForward.Normalize();

            Matrix rot = Matrix.Identity;
            rot.Forward = newForward;

            Vector3 referenceVector = Vector3.UnitY;

            // On the slim chance that the camera is pointer perfectly parallel with the Y Axis, we cannot
            // use cross product with a parallel axis, so we change the reference vector to the forward axis (Z).
            if (rot.Forward.Y == referenceVector.Y || rot.Forward.Y == -referenceVector.Y)
            {
                referenceVector = Vector3.UnitZ;
            }

            rot.Right = Vector3.Cross(rot.Forward, referenceVector);
            rot.Up = Vector3.Cross(rot.Right, rot.Forward);

            MsgSetRotation setRotMsg = ObjectPool.Aquire<MsgSetRotation>();            
            setRotMsg.Rotation = rot;
            setRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(setRotMsg);
        }

        /// <summary>
        /// Points camera in direction of its <see cref="attachedEntity"/>
        /// </summary>
        /// <returns>Returns false is camera has no attached <seealso cref="BaseEntity"/></returns>
        public bool LookAtAttached()
        {
            // Look at this entity's parent entity, if it has one
            if (this.parentEntity != null)
            {
                MsgSetRotation setRotMsg = ObjectPool.Aquire<MsgSetRotation>();                
                Matrix rot = Matrix.Identity;
                rot.Forward = this.parentEntity.ParentEntity.position - this.parentEntity.position;

                Vector3 referenceVector = Vector3.UnitY;

                // On the slim chance that the camera is pointer perfectly parallel with the Y Axis, we cannot
                // use cross product with a parallel axis, so we change the reference vector to the forward axis (Z).
                if (rot.Forward.Y == referenceVector.Y || rot.Forward.Y == -referenceVector.Y)
                {
                    referenceVector = Vector3.UnitZ;
                }

                rot.Right = Vector3.Cross(rot.Forward, referenceVector);
                rot.Up = Vector3.Cross(rot.Right, rot.Forward);

                setRotMsg.Rotation = rot;                
                setRotMsg.UniqueTarget = this.uniqueID;
                this.game.SendMessage(setRotMsg);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Yaw this Entity around its own Up vector.
        /// </summary>
        /// <param name="yawAmount">Amount of radians to yaw.</param>
        public void Yaw(float yawAmount)
        {
            MsgModifyRotation modRotMsg = ObjectPool.Aquire<MsgModifyRotation>();            
            modRotMsg.Rotation = Matrix.CreateFromAxisAngle(this.rotation.Up, yawAmount);
            modRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(modRotMsg);

        }

        /// <summary>
        /// Yaw this Entity around the world Up Vector (default is Y-axis).
        /// </summary>
        /// <param name="amount">Amount of radians to yaw.</param>
        public void YawAroundWorldUp(float amount)
        {
            MsgModifyRotation modRotMsg = ObjectPool.Aquire<MsgModifyRotation>();            
            modRotMsg.Rotation = Matrix.CreateRotationY(amount);
            modRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(modRotMsg);
        }

        /// <summary>
        /// Pitch this Entity around its own Right vector.
        /// </summary>
        /// <param name="amount">Amount of radians to pitch.</param>
        public void Pitch(float amount)
        {
            MsgModifyRotation modRotMsg = ObjectPool.Aquire<MsgModifyRotation>();
            modRotMsg.Rotation = Matrix.CreateFromAxisAngle(this.rotation.Right, amount);
            modRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(modRotMsg);
        }

        /// <summary>
        /// Roll this Entity around its own Forward vector.
        /// </summary>
        /// <param name="amount">Amount of radians to roll.</param>
        public void Roll(float amount)
        {
            MsgModifyRotation modRotMsg = ObjectPool.Aquire<MsgModifyRotation>();            
            modRotMsg.Rotation = Matrix.CreateFromAxisAngle(this.rotation.Forward, amount);
            modRotMsg.UniqueTarget = this.uniqueID;
            this.game.SendMessage(modRotMsg);
        }

        /// <summary>
        /// Strafe this Entity. Strafing moves the entity left or right by moving it along its Right vector.
        /// </summary>
        /// <param name="amount">Strafe distance, in world units.</param>
        public void Strafe(float amount)
        {
            MsgModifyPosition setPositionMsg = ObjectPool.Aquire<MsgModifyPosition>();            
            setPositionMsg.Position = this.rotation.Right * amount;
            setPositionMsg.UniqueTarget = this.uniqueID;
            this.Game.SendMessage(setPositionMsg);
        }

        /// <summary>
        /// Walk this Entity. Walking moves the entity forward or backward by moving it along its Forward vector.
        /// </summary>
        /// <param name="amount">Walking distance, in world units.</param>
        public void Walk(float amount)
        {
            MsgModifyPosition setPositionMsg = ObjectPool.Aquire<MsgModifyPosition>();            
            setPositionMsg.Position = this.rotation.Forward * amount;
            setPositionMsg.UniqueTarget = this.uniqueID;
            this.Game.SendMessage(setPositionMsg);
        }

        /// <summary>
        /// Jump this Entity. Jumping moves the entity up or down (relative to itself) by moving it along its Up vector.
        /// </summary>
        /// <param name="amount">Strafe distance, in world units.</param>
        public void Jump(float amount)
        {
            MsgModifyPosition setPositionMsg = ObjectPool.Aquire<MsgModifyPosition>();            
            setPositionMsg.Position = this.rotation.Up * amount;
            setPositionMsg.UniqueTarget = this.uniqueID;
            this.Game.SendMessage(setPositionMsg);
        }

        /// <summary>
        /// Raise this Entity. Raising moves the entity up or down (relative to the world) by moving it along the world Up vector.
        /// </summary>
        /// <param name="amount">Rise distance, in world units.</param>
        public void Rise(float amount)
        {
            MsgModifyPosition setPositionMsg = ObjectPool.Aquire<MsgModifyPosition>();            
            setPositionMsg.Position = Vector3.Up * amount;
            setPositionMsg.UniqueTarget = this.uniqueID;
            this.Game.SendMessage(setPositionMsg);
        }

        /// <summary>
        /// Sent to an entity when it's added to a scene
        /// </summary>
        /// <param name="manager">The <see cref="SceneManager"/> that added the entity to the scene.</param>
        public void AddedToScene(SceneManager manager)
        {
            // Let each component know about this
            foreach (KeyValuePair<ComponentType, BaseComponent> pair in this.componentList)
            {
                (pair.Value).AddedToScene(manager);
            }
        }

        /// <summary>
        /// Message listener for all entities
        /// </summary>
        /// <param name="message">Incoming message</param>
        protected virtual void Game_GameMessage(IMessage message)
        {
            ExecuteMessage(message);
        }

        /// <summary>
        /// Send a message directly to an Entity through this method.
        /// This function MUST be called only from QSGame, otherwise it will never mark the message properly for release
        /// and it will leak memory.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Returns true if message was handled</returns>
        public virtual bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.GetName:
                    {
                        MsgGetName msgGetName = message as MsgGetName;
                        message.TypeCheck(msgGetName);

                        msgGetName.Name = this.name;
                    }
                    break;
                case MessageType.GetParentID:
                    {
                        MsgGetParentID msgGetID = message as MsgGetParentID;
                        message.TypeCheck(msgGetID);

                        if (this.parentEntity != null)
                        {
                            msgGetID.ParentEntityID = this.parentEntity.UniqueID;
                        }
                    }
                    break;
                case MessageType.SetPosition:
                    {
                        MsgSetPosition setPositionMsg = message as MsgSetPosition;
                        message.TypeCheck(setPositionMsg);

                        this.Position = setPositionMsg.Position;                        
                    }
                    break;
                case MessageType.ModifyPosition:
                    {
                        MsgModifyPosition modPosMsg = message as MsgModifyPosition;
                        message.TypeCheck(modPosMsg);

                        this.Position += modPosMsg.Position;                        
                    }
                    break;
                case MessageType.GetPosition:
                    {
                        MsgGetPosition getPosMsg = message as MsgGetPosition;
                        message.TypeCheck(getPosMsg);

                        getPosMsg.Position = this.position;
                    }
                    break;
                case MessageType.SetRotation:
                    {
                        MsgSetRotation setRotMsg = message as MsgSetRotation;
                        message.TypeCheck(setRotMsg);

                        this.Rotation = setRotMsg.Rotation;                        
                    }
                    break;
                case MessageType.ModifyRotation:
                    {
                        MsgModifyRotation modRotMsg = message as MsgModifyRotation;
                        message.TypeCheck(modRotMsg);

                        this.Rotation *= modRotMsg.Rotation;                        
                    }
                    break;
                case MessageType.GetRotation:
                    {
                        MsgGetRotation getRotMsg = message as MsgGetRotation;
                        message.TypeCheck(getRotMsg);

                        getRotMsg.Rotation = this.rotation;
                    }
                    break;
                case MessageType.GetVectorForward:
                    {
                        MsgGetVectorForward getVectorMsg = message as MsgGetVectorForward;
                        message.TypeCheck(getVectorMsg);

                        getVectorMsg.Forward = this.rotation.Forward;
                    }
                    break;
                case MessageType.GetVectorUp:
                    {
                        MsgGetVectorUp getVectorMsg = message as MsgGetVectorUp;
                        message.TypeCheck(getVectorMsg);

                        getVectorMsg.Up = this.rotation.Up;
                    }
                    break;
                case MessageType.GetVectorRight:
                    {
                        MsgGetVectorRight getVectorMsg = message as MsgGetVectorRight;
                        message.TypeCheck(getVectorMsg);

                        getVectorMsg.Right = this.rotation.Right;
                    }
                    break;
                case MessageType.LookAtPosition:
                    {
                        MsgLookAtPosition setPositionMsg = message as MsgLookAtPosition;
                        message.TypeCheck(setPositionMsg);

                        this.LookAt(setPositionMsg.Position);
                    }
                    break;
                case MessageType.Pitch:
                    {
                        MsgModifyPitch camPitchMsg = message as MsgModifyPitch;
                        message.TypeCheck(camPitchMsg);

                        Pitch(camPitchMsg.PitchAmount);
                    }
                    break;
                case MessageType.Yaw:
                    {
                        MsgModifyYaw camYawMsg = message as MsgModifyYaw;
                        message.TypeCheck(camYawMsg);

                        Yaw(camYawMsg.YawAmount);
                    }
                    break;
                case MessageType.YawWorldUp:
                    {
                        MsgModifyYawWorldUp camYawMsg = message as MsgModifyYawWorldUp;
                        message.TypeCheck(camYawMsg);

                        YawAroundWorldUp(camYawMsg.YawAmount);
                    }
                    break;                
                case MessageType.SetParent:
                    {
                        MsgSetParent msgSetParent = message as MsgSetParent;
                        message.TypeCheck(msgSetParent);

                        SetParent(msgSetParent.ParentEntity);
                    }
                    break;
                case MessageType.RemoveChild:
                    {
                        MsgRemoveChild msgRemChild = message as MsgRemoveChild;
                        message.TypeCheck(msgRemChild);

                        RemoveChild(msgRemChild.Child);
                    }
                    break;
                case MessageType.ParentRemoved:
                    {
                        MsgParentRemoved msgParentRem = message as MsgParentRemoved;
                        message.TypeCheck(msgParentRem);

                        if (parentEntity == msgParentRem.Parent)
                        {
                            SetParent(null);
                        }
                    }
                    break;
                case MessageType.ParentAdded:
                    {
                        MsgParentAdded msgParentAdded = message as MsgParentAdded;
                        message.TypeCheck(msgParentAdded);

                        SetParent(msgParentAdded.Parent);
                    }
                    break;
                case MessageType.ChildRemoved:
                    {
                        MsgChildRemoved msgChildRem = message as MsgChildRemoved;
                        message.TypeCheck(msgChildRem);

                        if (msgChildRem.Child != null)
                        {
                            this.children.Remove(msgChildRem.Data.UniqueID);
                        }
                    }
                    break;
                case MessageType.ChildAdded:
                    {
                        MsgChildAdded msgChildAdd = message as MsgChildAdded;
                        message.TypeCheck(msgChildAdd);

                        if (msgChildAdd.Child != null && msgChildAdd.Child != this)
                        {
                            this.children.Add(msgChildAdd.Data.UniqueID, msgChildAdd.Child);
                        }
                    }
                    break;
                default:
                    return SendMessageThroughComponents(message);
            }

            // For messages handled directly by the Entity but aren't Request Protocol,
            // we still pass them to the Entity's components in case those components care.            
            if (message.Protocol != MessageProtocol.Request)
            {
                return SendMessageThroughComponents(message);
            }

            return true;
        }

        /// <summary>
        /// Sends a message through each component until it is handled. If the message does not
        /// have a request protocol then the message continues sending to all components.
        /// </summary>
        /// <param name="message">Message to distribute</param>
        private bool SendMessageThroughComponents(IMessage message)
        {
            bool handled = false;

            foreach (KeyValuePair<ComponentType, BaseComponent> pair in this.componentList)
            {            
                // If message is handled, quit sending message through components
                if ((pair.Value).ExecuteMessage(message))
                {
                    handled = true;

                    if (message.Protocol == MessageProtocol.Request)
                    {
                        break;
                    }
                }
            }

            return handled;
        }
    }
}
