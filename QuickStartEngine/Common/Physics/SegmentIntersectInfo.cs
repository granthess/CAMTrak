//
// SegmentIntersectInfo.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;

using QuickStart.Physics;

namespace QuickStart.Physics
{
    public struct SegmentIntersectInfo
    {
        public float distance;
        public Vector3 position;
        public Vector3 normal;
        public Int64 entityID;
    }
}
