//
// BaseComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using Microsoft.Xna.Framework;

using QuickStart.Entities;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    public enum ComponentType
    {
        Invalid = -1,                
        CameraComponent,
        ConstantMovementComponent,
        ConstantRotationComponent,
        LightEmitterComponent,        
        WaterComponent,
        ArcBallCameraInputComponent,
        CharacterInputComponent,
        FreeCameraInputComponent,
        ParticleEmitterComponent,        
        PhysicsComponent,
        CharacterPhysicsComponent,
        PhantomPhysicsComponent,
        WaterVolumePhysicsComponent,
        RenderComponent,
        SkyDomeComponent,        
        CollisionTriggerComponent,
        TerrainComponent,
        TerrainPhysicsComponent,
    }

    /// <summary>
    /// Base component class from which all components are derived
    /// </summary>
    public abstract class BaseComponent
    {
        /// <summary>
        /// Entity this component belongs to.
        /// </summary>
        protected BaseEntity parentEntity;

        /// <summary>
        /// What type this component is
        /// </summary>        
        public abstract ComponentType GetComponentType();

        /// <summary>
        /// Whether or not this component is active. When active this component will have Update()
        /// called on it each frame.
        /// </summary>
        public bool Active
        {
            get { return active; }
        }
        private bool active;

        /// <summary>
        /// Create a component
        /// </summary>
        /// <param name="parent">Entity this component belongs to.</param>
        public BaseComponent(BaseEntity parent)
        {
            this.parentEntity = parent;

            Initialize();
        }

        /// <summary>
        /// Initialize the component, which will attach it to the entity
        /// </summary>
        public virtual void Initialize()
        {
            this.parentEntity.AddComponent(this);
        }

        /// <summary>
        /// Shutdown the component and remove it from the entity and scene (if needed)
        /// </summary>
        public virtual void Shutdown() { }

        /// <summary>
        /// Send a message directly to components through this method.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Returns true if message was handled</returns>
        public abstract bool ExecuteMessage(IMessage message);

        /// <summary>
        /// Loads all needed ContentManager content.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Unloads all ContentManager content.
        /// </summary>
        public virtual void UnloadContent() { }

        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Update the component, at a fixed rate in sync with the physics world
        /// </summary>
        /// <param name="gameTime">XNA Timing snapshot</param>
        public virtual void FixedUpdate(GameTime gameTime) { }

        /// <summary>
        /// Allows the renderer to query this component for render information
        /// </summary>
        /// <param name="desc">Descriptor reference from the renderer</param>
        public virtual void QueryForChunks(ref RenderPassDesc desc) { }

        /// <summary>
        /// Sent to a component when its parent entity is added to a scene
        /// </summary>
        /// <param name="manager">The <see cref="SceneManager"/> that added the entity to the scene.</param>
        public virtual void AddedToScene(SceneManager manager) { }

        /// <summary>
        /// Activates a component, which allows it to receive Update() calls each frame. Only
        /// do this if you need a component be Updated each frame.
        /// </summary>
        protected void ActivateComponent()
        {
            if (!this.active)
            {
                this.active = true;

                this.parentEntity.ActivateComponent(this);
            }
        }

        /// <summary>
        /// Deactivates a component, which prevents it from getting Update() calls each frame. This
        /// is more efficient than leaving a component activated, deactivate a component whenever possible.
        /// </summary>
        protected void DeactivateComponent()
        {
            if (this.active)
            {
                this.active = false;

                this.parentEntity.DeactivateComponent(this);
            }
        }
    }
}
