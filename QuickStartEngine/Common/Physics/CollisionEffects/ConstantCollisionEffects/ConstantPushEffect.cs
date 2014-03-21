//
// ConstantPushEffect.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;

using QuickStart;
using QuickStart.Entities;

namespace QuickStart.Physics.CollisionEffects
{
    /// <summary>
    /// Reverses the pull of gravity on entities it is affecting
    /// </summary>
    public class ConstantPushEffect : ConstantCollisionEffect
    {
        public Vector3 Velocity
        {
            get { return this.velocity; }
            set { this.velocity = value; }
        }
        private Vector3 velocity;

        /// <summary>
        /// Create push effect
        /// </summary>
        /// <param name="velocity">Direction * speed</param>
        public ConstantPushEffect(QSGame game, int effectID)
            : base(game, effectID)
        {
            this.game = game;
            this.type = CollisionEffectType.ConstantPushEffect;
        }

        public override void ProcessEffect(GameTime gameTime, Int64 targetID)
        {
            var msgAddForce = ObjectPool.Aquire<MsgAddLinearForce>();
            msgAddForce.LinearVelocity = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            msgAddForce.UniqueTarget = targetID;
            this.game.SendMessage(msgAddForce);
        }

        /// <summary>
        /// Setup this effect's stats
        /// </summary>
        /// <param name="direction">Direction of the effect</param>
        /// <param name="strength">Strength of the effect. Multiplied by direction.</param>
        /// <returns>Returns true if this effect handles directional effects.</returns>
        public override bool InitializeDirectionalEffect(Vector3 direction, float strength)
        {
            direction.Normalize();
            this.velocity = direction * strength;

            return true;
        }

        public override void HandleCollision(Int64 targetID)
        {
            this.collisionList.Add(targetID);
        }

        public override void HandleCollisionOff(Int64 targetID)
        {
            this.collisionList.Remove(targetID);
        }
    }
}
