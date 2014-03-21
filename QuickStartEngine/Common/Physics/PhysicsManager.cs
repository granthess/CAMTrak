// PhysicsSystem.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

namespace QuickStart.Physics
{
    /// <summary>
    /// Interface to the physics simulation system.
    /// </summary>
    public abstract class PhysicsManager : BaseManager, IDisposable
    {
        /// <summary>
        /// Constructs a new <see cref="PhysicsSystem"/> instance.
        /// </summary>
        /// <param name="game"></param>
        public PhysicsManager(QSGame game)
            : base(game)
        {
            this.Game.Services.AddService(typeof(PhysicsManager), this);
        }

        /// <summary>
        /// Creates a new physics scene.
        /// </summary>
        /// <returns>Instance of the new physics scene.</returns>
        public abstract IPhysicsScene CreateScene();

        /// <summary>
        /// Disposes the physics manager.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
