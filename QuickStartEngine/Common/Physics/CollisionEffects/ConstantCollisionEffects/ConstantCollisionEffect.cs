//
// ConstantCollisionEffect.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace QuickStart.Physics.CollisionEffects
{
    public abstract class ConstantCollisionEffect : CollisionEffect
    {
        protected List<Int64> collisionList;

        public ConstantCollisionEffect(QSGame game, int effectID)
            : base(game, effectID)
        {
            collisionList = new List<Int64>();
        }

        public override void UpdateEffect(GameTime gameTime)
        {
            for (int i = 0; i < collisionList.Count; ++i)
            {
                ProcessEffect(gameTime, collisionList[i]);
            }
        }

        public abstract void ProcessEffect(GameTime gameTime, Int64 targetID);
    }
}
