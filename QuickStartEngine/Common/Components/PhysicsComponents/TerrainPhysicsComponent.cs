//
// TerrainPhysicsComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Physics;
using QuickStart.Graphics;

namespace QuickStart.Components
{
    public class TerrainPhysicsComponent : PhysicsComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.TerrainPhysicsComponent; }

        public static new BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            var compDef = content.Load<TerrainPhysicsComponentDefinition>(definitionPath);
           
            var newComponent = new TerrainPhysicsComponent(parent, compDef);
            return newComponent;
        }

        public TerrainPhysicsComponent(BaseEntity parent, TerrainPhysicsComponentDefinition compDef)
            : base(parent)
        {
            TerrainComponent terrainComp = this.parentEntity.GetComponentByType(ComponentType.TerrainComponent) as TerrainComponent;
            if (null == terrainComp)
            {
                throw new Exception("Terrain Components must be added to an entity prior to TerrainPhysicsComponent. Make sure in the EntityDefinition XML file that TerrainComponent comes first.");
            }

            this.collisionGroupType = CollisionGroups.Terrain;
            this.shapeType = ShapeType.Heightfield;

            var getPhysSceneMsg = ObjectPool.Aquire<MsgGetPhysicsScene>();
            getPhysSceneMsg.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendInterfaceMessage(getPhysSceneMsg, InterfaceType.Physics);

            IPhysicsScene physScene = getPhysSceneMsg.PhysicsScene;
            if (physScene != null)
            {
                CreateHeightfieldActor(physScene, terrainComp.heightData, terrainComp.ScaleFactor);

                if (this.actor != null)
                {
                    var addToSceneMsg = ObjectPool.Aquire<MsgAddEntityToPhysicsScene>();
                    addToSceneMsg.Actor = this.actor;
                    addToSceneMsg.EntityID = this.parentEntity.UniqueID;
                    this.parentEntity.Game.SendInterfaceMessage(addToSceneMsg, InterfaceType.Physics);

                    var setCollGroup = ObjectPool.Aquire<MsgSetActorToCollisionGroup>();
                    setCollGroup.Actor = this.actor;
                    setCollGroup.GroupType = this.collisionGroupType;
                    this.parentEntity.Game.SendInterfaceMessage(setCollGroup, InterfaceType.Physics);
                }
            }
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        /// <param name="density">Density of physics object</param>
        private void CreateHeightfieldActor(IPhysicsScene PhysicsScene, float[,] heightData, float scaleFactor)
        {
            HeightFieldShapeDesc heightfieldShape = CreateHeightfieldShape(heightData, scaleFactor);

            if (heightfieldShape == null)
            {
                throw new Exception("Shape did not load properly");
            }

            var desc = new ActorDesc();
            desc.Orientation = parentEntity.Rotation;
            desc.Mass = 1.0f;
            desc.Dynamic = false;
            desc.AffectedByGravity = false;
            desc.Position = parentEntity.Position;
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Shapes.Add(heightfieldShape);
            desc.Type = ActorType.Terrain;

            actor = PhysicsScene.CreateActor(desc);
        }

        /// <summary>
        /// Creates a heightfield shape descriptor for the physics system.
        /// </summary>
        /// <param name="heightData">Array of all height values on the heightfield</param>
        /// <param name="scaleFactor">Terrain scale factor. 4x means a spacing of 4 units (along horz plane) between adjacent vertices</param>
        /// <returns>Heightfield shape descriptor.</returns>
        private HeightFieldShapeDesc CreateHeightfieldShape(float[,] heightData, float scaleFactor)
        {
            var heightFieldDesc = new HeightFieldShapeDesc();
            heightFieldDesc.HeightField = heightData;
            heightFieldDesc.SizeX = heightData.GetLength(0) * scaleFactor;
            heightFieldDesc.SizeZ = heightData.GetLength(1) * scaleFactor;

            return heightFieldDesc;
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        protected override void CreateActor(IPhysicsScene PhysicsScene)
        {
            ShapeDesc newShape = CreateShapeFromType(shapeType);

            // Phantoms cannot currently have dynamic physics
            this.isDynamic = false;

            var desc = new ActorDesc();
            desc.Orientation = parentEntity.Rotation;
            desc.Mass = this.mass;
            desc.Dynamic = this.isDynamic;
            desc.Position = parentEntity.Position;
            desc.Shapes.Add(newShape);
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Game = this.parentEntity.Game;
            desc.Type = ActorType.Terrain;

            this.actor = PhysicsScene.CreateActor(desc);

            if (this.actor != null)
            {
                this.actor.EnableCollisionListening();
            }
        }

        /// <summary>
        /// Handles a message sent to this component.
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <returns>True, if handled, otherwise false</returns>
        public override bool ExecuteMessage(IMessage message)
        {
            // ALL components derived from non-abstract parents need to pass the message to their parent
            // to let the parent class have a chance to use it.
            return base.ExecuteMessage(message);
        }
    }
}
