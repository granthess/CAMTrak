//
// JigLibXPhysicsManager.cs
//
// This file is part of the QuickStart Engine's Wrapper to JigLibX. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;

namespace QuickStart.Physics.BEPU
{
    /// <summary>
    /// An implementation of the <see cref="PhysicsManager"/> base class using BEPU Physics.
    /// </summary>
    public class BEPUPhysicsManager : PhysicsManager
    {
        /// <summary>
        /// Constructs a new physics manager.
        /// </summary>
        /// <param name="game">A reference to the QuickStart QSGame instance.</param>
        public BEPUPhysicsManager( QSGame game )
            : base(game)
        {
        }

        /// <summary>
        /// Creates a new physics scene.
        /// </summary>
        /// <returns>The new physics scene instance.</returns>
        public override IPhysicsScene CreateScene()
        {
            return new BEPUScene();
        }

        /// <summary>
        /// Releases all unmanaged resources held by the manager.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
