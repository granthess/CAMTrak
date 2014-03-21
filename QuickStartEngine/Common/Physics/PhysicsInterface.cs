//
// PhysicsInterface.cs
// 
// This file is part of the QuickStart Game Engine.  See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;

using QuickStart.Entities;
using QuickStart.Geometry;
using QuickStart.Interfaces;
using QuickStart.Mathmatics;

using BEPUphysics.CollisionRuleManagement;

namespace QuickStart.Physics
{
    public enum CollisionGroups
    {
        CollideWithAll = 0,     // Collides with all other groups
        Phantom,     // Collides with rigid bodies
        RigidBody,   // Collides with other rigid bodies, phantoms, and terrain
        Terrain      // Collides with rigid bodies
    }

    public class PhysicsInterface : QSInterface
    {
        private CollisionGroup rigidBodyGroup;
        private CollisionGroup phantomGroup;
        private CollisionGroup terrainGroup;

        /// <summary>
        /// Gets the <see cref="IPhysicsScene"/> reference for this scene.
        /// </summary>
        public IPhysicsScene PhysicsScene
        {
            get { return this.physicsScene; }
        }
        private IPhysicsScene physicsScene;

        /// <summary>
        /// A list of all the physics actors in the overall scene
        /// </summary>
        private readonly Dictionary<Int64, IPhysicsActor> actors;

        /// <summary>
        /// Create a <see cref="PhysicsInterface"/>.
        /// </summary>
        public PhysicsInterface(QSGame game)
            : base(game, InterfaceType.Physics)
        {
            this.game.GameMessage += this.Game_GameMessage;

            this.actors = new Dictionary<Int64, IPhysicsActor>();

            this.physicsScene = this.game.Physics.CreateScene();

            this.rigidBodyGroup = new CollisionGroup();
            this.phantomGroup = new CollisionGroup();
            this.terrainGroup = new CollisionGroup();

            // Phantoms do not collide with terrain
            var pair = new CollisionGroupPair(this.terrainGroup, this.phantomGroup);
            CollisionRules.CollisionGroupRules.Add(pair, CollisionRule.NoBroadPhase);

            pair = new CollisionGroupPair(this.terrainGroup, this.terrainGroup);
            CollisionRules.CollisionGroupRules.Add(pair, CollisionRule.NoBroadPhase);

            pair = new CollisionGroupPair(this.phantomGroup, this.phantomGroup);
            CollisionRules.CollisionGroupRules.Add(pair, CollisionRule.NoBroadPhase);

            // Phantoms collide with rigid bodies, but do not affect each others' movement.
            pair = new CollisionGroupPair(this.phantomGroup, this.rigidBodyGroup);
            CollisionRules.CollisionGroupRules.Add(pair, CollisionRule.NoSolver);

            pair = new CollisionGroupPair(this.terrainGroup, this.rigidBodyGroup);
            CollisionRules.CollisionGroupRules.Add(pair, CollisionRule.Normal);
        }

        public override void Shutdown()
        {
            foreach (IPhysicsActor actor in this.actors.Values)
            {
                actor.Dispose();
            }

            this.actors.Clear();

            if (null != this.physicsScene)
            {
                this.physicsScene.KillProcessingThread();
                this.physicsScene.Dispose();
                this.physicsScene = null;
            }
        }

        /// <summary>
        /// Adds a <see cref="IPhysicsActor"/> to the overall scene.
        /// </summary>
        /// <param name="inActor">Actor to be placed in the scene.</param>
        public void AddActor(Int64 EntityUniqueID, IPhysicsActor inActor)
        {
            this.actors.Add(EntityUniqueID, inActor);
            this.PhysicsScene.AddActor(inActor);

#if WINDOWS && DEBUG
            if (inActor.ActorType != ActorType.Terrain)
            {
                this.game.PhysicsRenderer.modelDrawer.Add(inActor.SpaceObject);
            }
#endif //WINDOWS && DEBUG
        }

        public void AddSpaceObjectForModelViewer( ISpaceObject spaceObject )
        {
#if WINDOWS && DEBUG
            this.game.PhysicsRenderer.modelDrawer.Add(spaceObject);
#endif //WINDOWS && DEBUG
        }

        /// <summary>
        /// Removes an actor from the overall scene.
        /// </summary>
        /// <param name="targetActor">Actor to be removed.</param>
        public void RemoveActor(Int64 EntityUniqueID, IPhysicsActor targetActor)
        {
#if WINDOWS && DEBUG
            if (targetActor.ActorType != ActorType.Terrain)
            {
                this.game.PhysicsRenderer.modelDrawer.Remove(targetActor.SpaceObject);
            }
#endif //WINDOWS && DEBUG

            this.physicsScene.ScheduleForDeletion(targetActor);
            this.actors.Remove(EntityUniqueID);
        }

        /// <summary>
        /// Have the physics scene performs a query for collisions along a segment.
        /// </summary>
        /// <param name="lineSeg">To line segment to check collision against</param>
        /// <param name="predicate">The collision filter, predicates filter out collisions to only desired bodies</param>
        /// <param name="info">The resulting info from the query</param>
        public SegmentIntersectInfo PerformSegmentIntersectQuery(LineSegment lineSeg)
        {
            Vector3 direction = ( lineSeg.end - lineSeg.start );
            direction.Normalize();
            return this.physicsScene.PerformSegmentIntersectQuery(lineSeg.start, direction);
        }

        private CollisionGroup GetCollisionGroup( CollisionGroups groupType )
        {
            switch (groupType)
            {
                case CollisionGroups.CollideWithAll:
                    return null;
                case CollisionGroups.Phantom:
                    return this.phantomGroup;
                case CollisionGroups.RigidBody:
                    return this.rigidBodyGroup;
                case CollisionGroups.Terrain:
                    return this.terrainGroup;
                default:
                    Debug.Assert(false, "Unknown CollisionGroup type, a new type was added without this switch being updated to handle it.");
                    return null;
            }
        }

        /// <summary>
        /// Message listener for messages that are not directed at any particular Entity or Interface.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        protected virtual void Game_GameMessage(IMessage message)
        {
            ExecuteMessage(message);
        }

        /// <summary>
        /// Message handler for all incoming messages.
        /// </summary>
        /// <param name="message">Incoming message</param>
        /// <exception cref="ArgumentException">Thrown if a <see cref="MessageType"/> is not handled properly."/></exception>
        public override bool ExecuteMessage(IMessage message)
        {
            switch (message.Type)
            {
                case MessageType.GetPhysicsScene:
                    {
                        var getPhysSceneMsg = message as MsgGetPhysicsScene;
                        message.TypeCheck(getPhysSceneMsg);

                        getPhysSceneMsg.PhysicsScene = this.physicsScene;
                    }
                    return true;
                case MessageType.AddEntityToPhysicsScene:
                    {
                        var msgAddEntity = message as MsgAddEntityToPhysicsScene;
                        message.TypeCheck(msgAddEntity);

                        if (msgAddEntity.EntityID != QSGame.UniqueIDEmpty
                            && msgAddEntity.Actor != null)
                        {
                            AddActor(msgAddEntity.EntityID, msgAddEntity.Actor);
                        }
                    }
                    return true;
                case MessageType.RemoveEntityFromPhysicsScene:
                    {
                        var remPhysMsg = message as MsgRemoveEntityFromPhysicsScene;
                        message.TypeCheck(remPhysMsg);

                        if (remPhysMsg.EntityID != QSGame.UniqueIDEmpty)
                        {
                            IPhysicsActor actor;
                            if (this.actors.TryGetValue(remPhysMsg.EntityID, out actor))
                            {
                                RemoveActor(remPhysMsg.EntityID, actor);
                            }
                        }
                    }
                    return true;
                case MessageType.BeginPhysicsFrame:
                    {
                        var msgBeginFrame = message as MsgBeginPhysicsFrame;
                        message.TypeCheck(msgBeginFrame);

                        // Begin next physics frame. If not enough time has elapsed since the last
                        // physics frame this will do nothing this frame.
                        msgBeginFrame.FrameBegan = this.physicsScene.BeginFrame(msgBeginFrame.GameTime);
                    }
                    return true;
                case MessageType.EndPhysicsFrame:
                    {
                        var msgEndFrame = message as MsgEndPhysicsFrame;
                        message.TypeCheck(msgEndFrame);

                        // Wait for previous physics frame to finish.
                        msgEndFrame.FrameEnded = this.physicsScene.EndFrame();
                    }
                    return true;
                case MessageType.SetGravity:
                    {
                        var msgSetGrav = message as MsgSetGravity;
                        message.TypeCheck(msgSetGrav);

                        this.physicsScene.Gravity = msgSetGrav.Gravity;
                    }
                    return true;
                case MessageType.GetGravity:
                    {
                        var msgGetGrav = message as MsgGetGravity;
                        message.TypeCheck(msgGetGrav);

                        msgGetGrav.Gravity = this.physicsScene.Gravity;
                    }
                    return true;
                case MessageType.SetPhysicsTimeStep:
                    {
                        var msgSetPhysStep = message as MsgSetPhysicsTimeStep;
                        message.TypeCheck(msgSetPhysStep);

                        this.physicsScene.SetPhysicsTimeStep(msgSetPhysStep.TimeStep);
                    }
                    return true;
                case MessageType.GetPhysicsActor:
                    {
                        var msgGetActor = message as MsgGetPhysicsActor;
                        message.TypeCheck(msgGetActor);

                        Debug.Assert(msgGetActor.UniqueTarget != QSGame.UniqueIDEmpty, "You must specify a target entity ID when sending the MsgGetPhysicsActor message.");

                        IPhysicsActor actor = null;
                        if (this.actors.TryGetValue(msgGetActor.UniqueTarget, out actor))
                        {
                            msgGetActor.Actor = actor;
                        }
                    }
                    return true;
                case MessageType.SetActorToCollisionGroup:
                    {
                        var msgSetCollGroup = message as MsgSetActorToCollisionGroup;
                        message.TypeCheck(msgSetCollGroup);

                        if ( msgSetCollGroup.Actor.SpaceObject is Entity )
                        {
                            var entity = msgSetCollGroup.Actor.SpaceObject as Entity;
                            entity.CollisionInformation.CollisionRules.Group = GetCollisionGroup(msgSetCollGroup.GroupType);
                        }
                        else if ( msgSetCollGroup.Actor.SpaceObject is BroadPhaseEntry )
                        {
                            var entry = msgSetCollGroup.Actor.SpaceObject as BroadPhaseEntry;
                            entry.CollisionRules.Group = GetCollisionGroup(msgSetCollGroup.GroupType);
                        }
                    }
                    return true;
                case MessageType.AddPhysicsToModelViewer:
                    {
                        var msgAddPhys = message as MsgAddPhysicsToModelViewer;
                        message.TypeCheck(msgAddPhys);

                        Debug.Assert(msgAddPhys.SpaceObject != null, "You cannot add a null object to the model viewer.");

                        AddSpaceObjectForModelViewer(msgAddPhys.SpaceObject);
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
