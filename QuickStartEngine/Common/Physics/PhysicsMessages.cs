// PhysicsMessages.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;

using Microsoft.Xna.Framework;

using QuickStart.Physics;

namespace QuickStart.Interfaces
{
    public struct AddEntityPhysicsActor
    {
        public Int64 EntityUniqueID;
        public IPhysicsActor Actor;
    }    
}
