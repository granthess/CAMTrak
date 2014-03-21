//
// WaterVolumePhysicsComponent.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using QuickStart.Entities;
using QuickStart.Interfaces;
using QuickStart.Mathmatics;
using QuickStart.Physics;

namespace QuickStart.Components
{
    public class WaterVolumePhysicsComponent : PhysicsComponent
    {
        public override ComponentType GetComponentType() { return ComponentType.WaterVolumePhysicsComponent; }

        public static new BaseComponent LoadFromDefinition(ContentManager content, string definitionPath, BaseEntity parent)
        {
            var compDef = content.Load<PhysicsComponentDefinition>(definitionPath);
            compDef.IsDynamic = false;  // Phantoms cannot have dynamic physics
            compDef.CollisionGroupType = CollisionGroups.Phantom;

            var newComponent = new WaterVolumePhysicsComponent(parent, compDef);
            return newComponent;
        }

        private Dictionary<Int64, Vector3> entitiesInWater;
        private Dictionary<Int64, Vector3> dirtyValues;

        public WaterVolumePhysicsComponent(BaseEntity parent, PhysicsComponentDefinition compDef)
            : base(parent)
        {            
            var waterComp = this.parentEntity.GetComponentByType(ComponentType.WaterComponent) as WaterComponent;
            if (null == waterComp)
            {
                throw new Exception("Water Component must be added to an entity prior to WaterVolumePhysicsComponent. Make sure in the EntityDefinition XML file that WaterComponent comes first.");
            }

            this.entitiesInWater = new Dictionary<Int64, Vector3>();
            this.dirtyValues = new Dictionary<Int64, Vector3>();

            this.updatesEntityPosition = false;

            this.shapeType = ShapeType.Box;
            this.mass = 1.0f;
            this.collisionGroupType = compDef.CollisionGroupType;
            this.isDynamic = false;

            this.height = compDef.Height;
            this.width = waterComp.Width;
            this.depth = waterComp.Length;            

            var getPhysSceneMsg = ObjectPool.Aquire<MsgGetPhysicsScene>();
            getPhysSceneMsg.UniqueTarget = this.parentEntity.UniqueID;
            this.parentEntity.Game.SendInterfaceMessage(getPhysSceneMsg, InterfaceType.Physics);

            IPhysicsScene physScene = getPhysSceneMsg.PhysicsScene;
            if (physScene != null)
            {
                CreateActor(physScene);

                if (this.actor != null)
                {
                    var setCollGroup = ObjectPool.Aquire<MsgSetActorToCollisionGroup>();
                    setCollGroup.Actor = this.actor;
                    setCollGroup.GroupType = this.collisionGroupType;
                    this.parentEntity.Game.SendInterfaceMessage(setCollGroup, InterfaceType.Physics);

                    var addToSceneMsg = ObjectPool.Aquire<MsgAddEntityToPhysicsScene>();
                    addToSceneMsg.Actor = this.actor;
                    addToSceneMsg.EntityID = this.parentEntity.UniqueID;
                    this.parentEntity.Game.SendInterfaceMessage(addToSceneMsg, InterfaceType.Physics);
                }
            }

            ActivateComponent();
        }

        /// <summary>
        /// Creates an actor using shape info and information from the parent BaseEntity.
        /// </summary>
        /// <param name="PhysicsScene">Reference to the physics scene</param>
        protected override void CreateActor( IPhysicsScene PhysicsScene )
        {
            ShapeDesc newShape = CreateShapeFromType(shapeType);

            if (newShape == null)
            {
                throw new Exception("Shape did not load properly");
            }

            ActorDesc desc = new ActorDesc();
            desc.Orientation = this.parentEntity.Rotation;
            desc.Mass = this.mass;
            desc.Dynamic = false;
            desc.AffectedByGravity = false;
            desc.Position = this.parentEntity.Position;
            desc.Shapes.Add(newShape);
            desc.EntityID = this.parentEntity.UniqueID;
            desc.Game = this.parentEntity.Game;
            desc.Type = ActorType.Water;

            this.actor = PhysicsScene.CreateActor(desc);
        }
    }
}
