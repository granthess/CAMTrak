//
// JigLibXScene.cs
//
// This file is part of the QuickStart Engine's Wrapper to JigLibX. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;

using QuickStart.Physics;
using QuickStart.Physics.BEPU;

using BEPUphysics;
using BEPUphysics.Entities;

namespace QuickStart.Physics.BEPU
{
    /// <summary>
    /// An implementation of <see cref="IPhysicsScene"/> using JigLibX.
    /// </summary>
    public class BEPUScene : IPhysicsScene
    {
        private float fixedStepFrequency = 1 / 30.0f;

        private Space physicsSpace;
        public Space PhysicsSpace
        {
            get { return this.physicsSpace; }
        }

        private float totalDt = 0.0f;
        private bool bFixedTimeMet = false;
        private Thread processingThread;

        private ManualResetEvent startFrame;
        private ManualResetEvent endFrame;
        private ManualResetEvent endThread;

        private List<BEPUActor> deletionList;

        /// <summary>
        /// Gets/sets the gravity vector.
        /// </summary>
        public Vector3 Gravity
        {
            get
            {
                return physicsSpace.ForceUpdater.Gravity;
            }
            set
            {
                physicsSpace.ForceUpdater.Gravity = value;
            }
        }        

        /// <summary>
        /// Constructs a new physics scene.
        /// </summary>
        /// <param name="game">A reference to the current <see cref="QSGame"/> instance.</param>
        internal BEPUScene()
        {
            // Create JigLibX scene with standard settings.
            // TODO:  Expose some of these settings in a generic way.
            physicsSpace = new Space();

            deletionList = new List<BEPUActor>();

            // Create synchronization handles
            startFrame = new ManualResetEvent(false);
            endFrame = new ManualResetEvent(false);
            endThread = new ManualResetEvent(false);

            // Create and start the physics processing thread.
            processingThread = new Thread(SceneProcessing);
            processingThread.Priority = ThreadPriority.Normal;
            processingThread.IsBackground = false;
            processingThread.Start();
        }

        /// <summary>
        /// Set the simulations update frequency
        /// </summary>
        /// <param name="stepsPerSecond"></param>
        public void SetPhysicsTimeStep( int stepsPerSecond )
        {
            fixedStepFrequency = 1.0f / stepsPerSecond;
        }

        /// <summary>
        /// Creates a new physics actor.
        /// </summary>
        /// <param name="desc">The actor descriptor.</param>
        /// <returns>The new actor instance.</returns>
        public IPhysicsActor CreateActor(ActorDesc desc)
        {
            if (this.bFixedTimeMet)
            {
                // This should pause the physics frame ever so slightly so we can add the
                // actor to the physics scene.
                this.startFrame.Reset();
            }

            IPhysicsActor actor;
            switch (desc.Type)
            {
                case ActorType.Basic:
                    actor = new BasicBEPUActor(desc);
                    break;
                case ActorType.Character:
                    actor = new CharacterBEPUActor(desc);
                    break;
                case ActorType.Terrain:
                    actor = new TerrainBEPUActor(desc);
                    break;
                case ActorType.Water:
                    actor = new WaterVolumeBEPUActor(desc);
                    break;
                case ActorType.Static:
                    actor = new StaticBEPUActor(desc);
                    break;
                default:
                    throw new ArgumentException("Unknown actor type, new type was added and this switch wasn't updated to handle it.");
            }

            if (this.bFixedTimeMet)
            {
                // Unpause physics now that new actor is in the physics scene
                this.startFrame.Set();
            }

            return actor;
        }

        public void AddActor( IPhysicsActor actor )
        {
            this.physicsSpace.Add(actor.SpaceObject);
        }

        /// <summary>
        /// Schedule an actor for deletion. Deletion will occur before the next physics simulation step.
        /// </summary>
        /// <param name="actor">Actor to delete</param>
        public void ScheduleForDeletion(IPhysicsActor actor)
        {
            if (actor is BEPUActor)
            {
                deletionList.Add((actor as BEPUActor));
            }
        }
        
        /// <summary>
        /// Begins physics processing.
        /// </summary>
        /// <param name="gameTime">The XNA <see cref="GameTime"/> structure for this frame.</param>
        public bool BeginFrame(GameTime gameTime)
        {
            for (int i = deletionList.Count - 1; i >= 0; --i)
            {
                RemoveActor( deletionList[i] );
                deletionList.RemoveAt( i );
            }

            // Save the time delta and signal the physics thread to start processing.
            this.totalDt += (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            // We want a steady physics simulation, don't run physics this frame
            // if it has been less than the fixed timestep time (usually 1/30th - 1/60th of a second)
            if (totalDt >= fixedStepFrequency)
            {
                this.bFixedTimeMet = true;

                this.startFrame.Set();

                return true;
            }
            else
            {
                this.bFixedTimeMet = false;

                return false;
            }        
        }

        /// <summary>
        /// Blocks until the current frame's processing is complete.
        /// </summary>
        public bool EndFrame()
        {
            if (bFixedTimeMet)
            {
                // Block until the physics frame is finished processing.
                this.endFrame.WaitOne(Timeout.Infinite, false);
                this.endFrame.Reset();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Releases all unmanaged resources for the scene.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Kills the physics processing thread.
        /// </summary>
        public void KillProcessingThread()
        {
            endThread.Set();
        }

        /// <summary>
        /// The main scene processing thread process.
        /// </summary>
        private void SceneProcessing()
        {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] {4});
#endif //XBOX
            try
            {
                // Execute until we are exiting.
                while(!endThread.WaitOne(0, false))
                {
                    // Wait for frame start event.
                    if(!startFrame.WaitOne(10, false))
                    {
                        continue;
                    }

                    // Reset the event.
                    startFrame.Reset();

                    int steps = 0;

                    // We only run the physics simulation if there is still enough time left to
                    // simulation, and we do not allow more than a few steps per frame, otherwise
                    // a low framerate could lead to many physics steps, which would lower framerate
                    // even further.
                    while (steps < 3 && (totalDt >= fixedStepFrequency) )
                    {
                        //Console.WriteLine("totalDt: " + this.totalDt);

                        // Process the frame.
                        physicsSpace.Update(fixedStepFrequency);

                        totalDt -= fixedStepFrequency;
                        ++steps;
                    }

                    // Signal frame completion.
                    endFrame.Set();
                }
            }
            catch(ThreadAbortException)
            {
            }
        }

        /// <summary>
        /// Remove an actor from the physics simulation
        /// </summary>
        /// <param name="actor"></param>
        private void RemoveActor( BEPUActor actor )
        {
            physicsSpace.Remove(actor.SpaceObject);
        }

        /// <summary>
        /// Cast a ray through the physics scene until it hits something. It can only hit
        /// something that its CollisionSkinPredicate allows it to hit.
        /// </summary>
        /// <param name="distance">The distance at which the impact occurred from the starting point</param>
        /// <param name="skin">The CollisionSkin that was collided with</param>
        /// <param name="pos">The position of collision</param>
        /// <param name="normal">The normal of the surface at which the collision occurred</param>
        /// <param name="seg">The segment to use for the collision query</param>
        /// <param name="pred">The CollisionSkinPredicate acts like a collision filter, allowing
        /// collisions with only certain other bodies.</param>
        public SegmentIntersectInfo PerformSegmentIntersectQuery( Vector3 rayOrigin, Vector3 direction )
        {
            SegmentIntersectInfo info = new SegmentIntersectInfo(); 

            RayCastResult result;            
            this.physicsSpace.RayCast(new Ray(rayOrigin, direction), out result);

            info.position = result.HitData.Location;
            info.normal = result.HitData.Normal;
            info.distance = result.HitData.T;

            var tag = result.HitObject.Tag as IEntityTag;
            if ( null == tag )
            {
                throw new Exception("All physics objects should have the QS Engine's custom tag data.");                
            }

            info.entityID = tag.EntityID;

            return info;
        }
    }
}
