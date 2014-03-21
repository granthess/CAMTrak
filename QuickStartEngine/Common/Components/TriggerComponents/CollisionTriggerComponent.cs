//
// CollisionTriggerComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart.Entities;
using QuickStart.Physics;
using QuickStart.Physics.CollisionEffects;

namespace QuickStart.Components
{
    public struct CollisionEffectDefinition
    {
        [ContentSerializer]
        public int EffectUniqueID;

        [ContentSerializer]
        public CollisionEffectType CollisionEffectType;

        [ContentSerializer]
        public bool IsDirectional;

        /// <summary>
        /// Direction of the effect
        /// </summary>
        /// <remarks>This is only required if this effect is directional</remarks>
        [ContentSerializer]
        public Vector3 Direction;      

        /// <summary>
        /// Strength of the effect
        /// </summary>
        /// <remarks>Some effects may not need a strength</remarks>
        [ContentSerializer]
        public float Strength;
    }

    public class CollisionTriggerComponent : BaseComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.CollisionTriggerComponent; }

        public static BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            CollisionTriggerComponentDefinition compDef = content.Load<CollisionTriggerComponentDefinition>(definitionPath);

            CollisionTriggerComponent newComponent = new CollisionTriggerComponent(parent, compDef);
            return newComponent;
        }

        Dictionary<int, CollisionEffect> collisionEffects;

        public CollisionTriggerComponent(BaseEntity parent)
            : base(parent)
        {
            ActivateComponent();

            collisionEffects = new Dictionary<int, CollisionEffect>();
        }

        public CollisionTriggerComponent(BaseEntity parent, CollisionTriggerComponentDefinition compDef)
            : base(parent)
        {
            ActivateComponent();

            collisionEffects = new Dictionary<int, CollisionEffect>();

            // If the CollisionTriggerComponentDefinition has a definition file path for
            // collision effects then we load that up and parse through, add, and initialize
            // add of those effects.
            if (compDef.EffectsDefinitionXML.Length > 0)
            {
                CollisionEffectDefinition[] effects = this.parentEntity.Game.Content.Load<CollisionEffectDefinition[]>(compDef.EffectsDefinitionXML);

                foreach (CollisionEffectDefinition effect in effects)
                {
                    AddEffect(effect.CollisionEffectType, effect.EffectUniqueID);
                    InitializeCollisionEffect(effect);
                }
            }
        }

        public override void Shutdown()
        {
            collisionEffects.Clear();
            collisionEffects = null;
        }

        public void AddEffect(CollisionEffectType type, int effectID)
        {
            switch (type)
            {
                case CollisionEffectType.ConstantPushEffect:
                    {
                        ConstantPushEffect newEffect = new ConstantPushEffect(this.parentEntity.Game, effectID);

                        CollisionEffect effect;
                        if ( !collisionEffects.TryGetValue(effectID, out effect) )
                        {
                            collisionEffects.Add(effectID, newEffect);
                        }
                    }
                    break;
                case CollisionEffectType.Invalid:
                    break;
                default:
                    break;
            }
        }

        public void RemoveEffect( int effectID )
        {
            CollisionEffect effect;
            if (collisionEffects.TryGetValue(effectID, out effect))
            {
                collisionEffects.Remove(effectID);
            }
        }

        public void RemoveAllEffectsOfType(CollisionEffectType type)
        {
            for (int i = collisionEffects.Count - 1; i >= 0; --i)
            {
                if (collisionEffects[i].Type == type)
                {
                    collisionEffects.Remove(collisionEffects[i].EffectID);
                }
            }
        }

        public void NofityEffectsOfCollision(Int64 EntityID)
        {
            foreach ( CollisionEffect effect in collisionEffects.Values )
            {
                effect.HandleCollision(EntityID);
            }
        }

        public void NofityEffectsOfCollisionOff(Int64 EntityID)
        {
            foreach (CollisionEffect effect in collisionEffects.Values)
            {
                effect.HandleCollisionOff(EntityID);
            }
        }

        public CollisionEffect GetEffect(int effectID)
        {
            CollisionEffect effect = null;
            collisionEffects.TryGetValue(effectID, out effect);
            
            return effect;
        }
     
        public override void Update(GameTime gameTime)
        {
            foreach (CollisionEffect effect in collisionEffects.Values)
            {
                effect.UpdateEffect(gameTime);
            }            
        }      
  
        public void InitializeCollisionEffect(CollisionEffectDefinition effectDef)
        {
            CollisionEffect effect = GetEffect(effectDef.EffectUniqueID);
            effect.InitializeEffect(effectDef);
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.AddCollisionEffect:
                    {
                        MsgAddCollisionEffect msgAddEffect = message as MsgAddCollisionEffect;
                        message.TypeCheck(msgAddEffect);

                        AddEffect(msgAddEffect.EffectType, msgAddEffect.EffectID);
                    }
                    return true;
                case MessageType.RemoveCollisionEffect:
                    {
                        MsgRemoveCollisionEffect msgRemEffect = message as MsgRemoveCollisionEffect;
                        message.TypeCheck(msgRemEffect);

                        if (msgRemEffect.RemoveAll)
                        {
                            RemoveAllEffectsOfType(msgRemEffect.EffectType);
                        }
                        else
                        {
                            RemoveEffect(msgRemEffect.EffectID);
                        }
                    }
                    return true;                
                case MessageType.InitCollisionEffect:
                    {
                        MsgInitCollisionEffect msgInitEffect = message as MsgInitCollisionEffect;
                        message.TypeCheck(msgInitEffect);

                        CollisionEffect effect = GetEffect(msgInitEffect.EffectID);

                        if (effect != null)
                        {
                            if (msgInitEffect.DirectionalEffect)
                            {
                                if (!effect.InitializeDirectionalEffect(msgInitEffect.Direction, msgInitEffect.Strength))
                                {
                                    throw new Exception("This collision effect doesn't handle directional effects");
                                }
                            }
                            else
                            {
                                if (!effect.InitializeNonDirectionalEffect(msgInitEffect.Strength))
                                {
                                    throw new Exception("This collision effect doesn't handle non-directional effects");

                                }
                            }
                        }
                    }
                    return true;
                case MessageType.Collision:
                    {
                        MsgOnCollision msgCollData = message as MsgOnCollision;
                        message.TypeCheck(msgCollData);

                        NofityEffectsOfCollision(msgCollData.EntityID);
                    }
                    return true;
                case MessageType.CollisionOff:
                    {
                        MsgOffCollision msgCollData = message as MsgOffCollision;
                        message.TypeCheck(msgCollData);

                        NofityEffectsOfCollisionOff(msgCollData.EntityID);
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
