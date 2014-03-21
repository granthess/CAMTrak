//
// LightEmitterComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart.Entities;
using QuickStart.Graphics;
using QuickStart.EnvironmentalSettings;

namespace QuickStart.Components
{
    /// <summary>
    /// Different types of lights.
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// Directional lights are near-infinite distance light sources with a single direction. 
        /// This is like the Sun is to the Earth.
        /// </summary>
        Directional = 0,

        /// <summary>
        /// Currently NOT supported.
        /// This is a light with a single direction, but at a non-infinite distance. This is like a traditional spot light.
        /// Spot lights also have a focal length which determines how wide of an angle it covers
        /// </summary>
        Spot,

        /// <summary>
        /// Currently NOT supported.
        /// This light has no direction, it emits light in all directions around it for a specific distance.
        /// </summary>
        Point
    }

    /// <summary>
    /// Create a light emitter
    /// </summary>
    public class LightEmitterComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.LightEmitterComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            LightComponentDefinition compDef = content.Load<LightComponentDefinition>(definitionPath);

            LightEmitterComponent newComponent = new LightEmitterComponent(parent, compDef);
            return newComponent;
        }

        /// <summary>
        /// Type of light this component is using
        /// </summary>
        public LightType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        private LightType type = LightType.Directional;

        /// <summary>
        /// Direction this light is facing (forward vector)
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Vector3 Direction
        {
            get { return this.direction; }
        }
        public Vector3 direction = Vector3.Zero;

        /// <summary>
        /// Ambient light color from this light
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Vector4 AmbientColor
        {
            set
            {
                this.ambientColor = value;
            }
        }
        public Vector4 ambientColor = Vector4.Zero;

        /// <summary>
        /// Specular power of this light.
        /// </summary>
        public float SpecularPower
        {
            get { return this.specularPower; }
            set { this.specularPower = value; }
        }
        private float specularPower = 1.0f;

        /// <summary>
        /// Specular color of this light.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Vector4 SpecularColor
        {
            get { return this.specularColor; }
        }
        public Vector4 specularColor = Vector4.Zero;

        /// <summary>
        /// Diffuse color of this light.
        /// </summary>
        /// <remarks>Public for performance reasons</remarks>
        public Vector4 DiffuseColor
        {
            get { return this.diffuseColor; }
        }
        public Vector4 diffuseColor = Vector4.Zero;

        /// <summary>
        /// Is this light currently emitting?
        /// </summary>
        public bool IsLightOn
        {
            get { return isLightOn; }
        }
        public bool isLightOn = true;

        /// <summary>
        /// Create a light emitter
        /// </summary>
        /// <param name="parent">Entity this light belongs to.</param>
        /// <remarks>Not meant to be called by anything but the ComponentLoader</remarks>
        public LightEmitterComponent(BaseEntity parent, LightComponentDefinition settings)
            : base(parent)
        {
            this.ambientColor = settings.AmbientColor;
            this.specularColor = settings.SpecularColor;
            this.SpecularPower = settings.SpecularPower;
            this.diffuseColor = settings.DiffuseColor;            

            this.direction = settings.LightDirection;
        }

        /// <summary>
        /// Sent to a component when its parent entity is added to a scene
        /// </summary>
        /// <param name="manager">The <see cref="SceneManager"/> that added the entity to the scene.</param>
        public override void AddedToScene(SceneManager manager)
        {
            InitializeLightDirection();
        }

        /// <summary>
        /// Create a light emitter
        /// </summary>
        /// <param name="parent">Entity this light belongs to.</param>
        public LightEmitterComponent(BaseEntity parent)
            : base(parent)
        {
        }

        /// <summary>
        /// This must be done after a light is added to a scene.
        /// </summary>
        public void InitializeLightDirection()
        {
            this.parentEntity.LookAt(this.direction);
        }

        /// <summary>
        /// Turn on light emitter
        /// </summary>
        public void TurnLightOn()
        {
            isLightOn = true;
        }

        /// <summary>
        /// Turn off light emitter
        /// </summary>
        public void TurnLightOff()
        {
            isLightOn = false;
        }

        /// <summary>
        /// Turn on/off light emitter
        /// </summary>
        /// <param name="lightOnState">On/off state</param>
        public void SetLightState(bool lightOnState)
        {
            isLightOn = lightOnState;
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            return false;
        }

        /// <summary>
        /// Allows the renderer to query this component for light information
        /// </summary>
        /// <param name="desc">Descriptor reference from the renderer</param>
        public override void QueryForChunks(ref RenderPassDesc desc)
        {
            if (desc.GeometryChunksOnlyThisPass)
                return;

            LightChunk chunk;
            chunk = this.parentEntity.Game.Graphics.AllocateLightChunk();
            chunk.position = this.parentEntity.Position;
            chunk.direction = this.parentEntity.Rotation.Forward;
            chunk.directional = (this.type == LightType.Directional);
            chunk.ambientColor = this.ambientColor;
            chunk.diffuseColor = this.diffuseColor;
            chunk.specularColor = this.specularColor;
            chunk.specularPower = this.specularPower;
        }
    }
}
