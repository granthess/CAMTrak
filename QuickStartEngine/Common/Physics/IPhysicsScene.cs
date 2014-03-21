// PhysicsScene.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using Microsoft.Xna.Framework;
using System;

using BEPUphysics;

namespace QuickStart.Physics
{
    /// <summary>
    /// Interface for all physics scenes.
    /// </summary>
    public interface IPhysicsScene : IDisposable
    {
        /// <summary>
        /// Gets/sets the gravity vector.
        /// </summary>
        Vector3 Gravity { get; set; }

        Space PhysicsSpace { get; }
        
        /// <summary>
        /// Creates a new physics actor.
        /// </summary>
        /// <param name="desc">The <see cref="ActorDesc"/> describing the actor.</param>
        /// <returns>Instance of new physics actor.</returns>
        IPhysicsActor CreateActor(ActorDesc desc);

        /// <summary>
        /// Adds a physics actor to the simulation.
        /// </summary>
        /// <param name="actor">Actor to add to simulation.</param>
        void AddActor( IPhysicsActor actor );

        /// <summary>
        /// Sets the number of times per second the physics simulation is updated.
        /// </summary>
        /// <param name="stepsPerSecond">Number of times per second to update.</param>
        void SetPhysicsTimeStep( int stepsPerSecond );

        /// <summary>
        /// Schedules an actor to be deleted and removed from the physics scene before the next simulation step.
        /// </summary>
        /// <param name="actor"><see cref="IPhysicsActor"/> to remove.</param>
        void ScheduleForDeletion(IPhysicsActor actor);

        /// <summary>
        /// Begins processing a physics simulation frame.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last update.</param>
        bool BeginFrame(GameTime gameTime);

        /// <summary>
        /// Blocks until the currently processing physics frame is complete.
        /// </summary>
        bool EndFrame();

        /// <summary>
        /// Kills the physics processing thread.
        /// </summary>
        void KillProcessingThread();


        SegmentIntersectInfo PerformSegmentIntersectQuery( Vector3 rayOrigin, Vector3 direction );
    }
}
