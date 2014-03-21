//
// CollisionEffect.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;

using QuickStart.Components;

namespace QuickStart.Physics.CollisionEffects
{
    public enum CollisionEffectType
    {
        Invalid = -1,
        ConstantPushEffect
    }

    public abstract class CollisionEffect
    {
        protected QSGame game;

        public CollisionEffectType Type
        {
            get { return this.type; }
        }
        protected CollisionEffectType type;

        public int EffectID
        {
            get { return this.effectID; }
        }
        protected int effectID;

        /// <summary>
        /// Create push effect
        /// </summary>
        /// <param name="velocity">Direction * speed</param>
        public CollisionEffect(QSGame game, int effectID)
        {
            this.game = game;
            this.effectID = effectID;
        }

        ~CollisionEffect()
        {
        }

        public abstract void HandleCollision(Int64 targetID);

        public abstract void HandleCollisionOff(Int64 targetID);

        public virtual void UpdateEffect(GameTime gameTime)
        {
        }

        public void InitializeEffect(CollisionEffectDefinition effectDefinition)
        {
            if (effectDefinition.IsDirectional)
            {
                if (!InitializeDirectionalEffect(effectDefinition.Direction, effectDefinition.Strength))
                {
                    throw new Exception("This collision effect doesn't handle directional effects");
                }
            }
            else
            {
                if (!InitializeNonDirectionalEffect(effectDefinition.Strength))
                {
                    throw new Exception("This collision effect doesn't handle non-directional effects");

                }
            }
        }

        /// <summary>
        /// Setup this effect's stats
        /// </summary>
        /// <param name="direction">Direction of the effect</param>
        /// <param name="strength">Strength of the effect. Multiplied by direction.</param>
        public virtual bool InitializeDirectionalEffect(Vector3 direction, float strength)
        {
            return false;
        }

        /// <summary>
        /// Setup this effect's stats
        /// </summary>
        /// <param name="strength"></param>
        public virtual bool InitializeNonDirectionalEffect(float strength)
        {
            return false;
        }
    }
}
