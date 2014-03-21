//
// EntityCollisionInfo.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

namespace QuickStart.Physics
{
    public struct EntityCollisionInfo
    {
        public Int64 EntityID;
    }

    public struct CollisionData
    {
        public EntityCollisionInfo ListeningEntity;
        public EntityCollisionInfo OtherEntity;
    }

    public struct CollisionGroupInfo
    {
        public IPhysicsActor actor;
        public CollisionGroups groupType;
    }
}
