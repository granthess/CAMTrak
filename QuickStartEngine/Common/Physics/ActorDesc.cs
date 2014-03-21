// ActorDesc.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace QuickStart.Physics
{
    public enum ActorType
    {
        Basic = 0,
        Character,
        Terrain,
        Water,
        Static,
    }

    /// <summary>
    /// Descriptor for physics actors.
    /// </summary>
    public class ActorDesc
    {
        private Vector3 position = Vector3.Zero;
        private Matrix orientation = Matrix.Identity;
        private float mass;
        private bool dynamic;
        private bool affectedByGravity = true;
        private ActorType type;
        private Int64 entityID;
        private QSGame game;
        private readonly List<ShapeDesc> shapes = new List<ShapeDesc>();

        /// <summary>
        /// Initial position of the actor.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Initial orientation of the actor, as a transformation matrix.
        /// </summary>
        public Matrix Orientation
        {
            get { return this.orientation; }
            set { this.orientation = value; }
        }

        /// <summary>
        /// Initial mass of the actor.
        /// </summary>
        public float Mass
        {
            get { return this.mass; }
            set { this.mass = value; }
        }

        /// <summary>
        /// Dynamic flag.  True is mobile, false is static.
        /// </summary>
        public bool Dynamic
        {
            get { return this.dynamic; }
            set { this.dynamic = value; }
        }

        public bool AffectedByGravity
        {
            get { return this.affectedByGravity; }
            set { this.affectedByGravity = value; }
        }
        
        public ActorType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        /// <summary>
        /// The ID of the entity the physics body will be part of
        /// </summary>
        public Int64 EntityID
        {
            get { return this.entityID; }
            set { this.entityID = value; }
        }

        /// <summary>
        /// Contains a reference of to the game
        /// </summary>
        public QSGame Game
        {
            get { return this.game; }
            set { this.game = value; }
        }

        /// <summary>
        /// List of shapes that compose the actor.
        /// </summary>
        public List<ShapeDesc> Shapes
        {
            get { return this.shapes; }
        }
    }
}
