using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Content;

using QuickStart.Components;

namespace QuickStart.Entities
{
    public class EntityLoader
    {
        private QSGame game;
        private ContentManager EntityContentLoader;
        private ContentManager EntityDefinitionsContentLoader;
        private ContentManager ComponentDefinitionsContentLoader;

        /// <summary>
        /// Holds the definitions required to load any type of entity based on its template ID.
        /// The 'int' is the entity template ID.
        /// </summary>
        private Dictionary<int, EntityDefinition> entityDefinitions;
        
        public EntityLoader(QSGame Game)
        {
            this.game = Game;
            this.EntityContentLoader = new ContentManager(this.game.Services, this.game.Settings.EntitiesDirectory);
            this.EntityDefinitionsContentLoader = new ContentManager(this.game.Services, this.game.Settings.EntityDefinitionsDirectory);
            this.ComponentDefinitionsContentLoader = new ContentManager(this.game.Services, this.game.Settings.ComponentDefinitionsDirectory);

            this.entityDefinitions = new Dictionary<int, EntityDefinition>();

            CacheEntityDefinitions();
        }

        public void CacheEntityDefinitions()
        {
            // We only cache the entity definitions once.
            if (entityDefinitions.Count > 0)
                return;            

            // Load the entity definitions from XML into an array
            EntityDefinition[] entityDefArray = this.EntityContentLoader.Load<EntityDefinition[]>("EntityTemplates");

            // Convert the array into a dictionary (we don't use LINQ for this because
            // we want to throw an exception if duplicated templateIDs are found.
            foreach (EntityDefinition entityDef in entityDefArray)
            {
                EntityDefinition newDef;
                if (!entityDefinitions.TryGetValue(entityDef.TemplateID, out newDef))
                {
                    entityDefinitions.Add(entityDef.TemplateID, entityDef);
                }
                else
                {
                    throw new ArgumentException("Every entity in the Entities.XML must have a unique template ID");
                }
            }

            // Now go through each of our entity's definitions, and load the component definitions
            foreach (KeyValuePair<int, EntityDefinition> entry in entityDefinitions)
            {
                if (entry.Value.DefinitionXML.Count() > 0)
                {
                    ComponentDefinition[] entityComponentsArray = this.EntityDefinitionsContentLoader.Load<ComponentDefinition[]>(entry.Value.DefinitionXML);

                    foreach ( ComponentDefinition compDef in entityComponentsArray )
                    {
                        entry.Value.AddDefinition(compDef);
                    }      
                }
                else
                {
                    throw new ArgumentException("Entity defintions contains an entity with no definition", "DefinitionXML"); 
                }
            }
        }

        public BaseEntity LoadEntity(int TemplateID)
        {
            BaseEntity newEntity;

            // Load components, use component definitions to load info used within the component
            EntityDefinition newDef;
            if (!entityDefinitions.TryGetValue(TemplateID, out newDef))
            {
                throw new Exception(String.Format("No entity templateID {0}", TemplateID));                
            }
            else
            {
                // Create an entity
                newEntity = new BaseEntity(this.game, newDef.TemplateID);
                newEntity.Name = newDef.Name;

                // Create any needed components
                foreach (KeyValuePair<ComponentType, string> entry in newDef.ComponentDefinitions)
                {
                    BaseComponent newComponent = LoadComponent(newEntity, entry.Key, this.ComponentDefinitionsContentLoader, entry.Value);

                    if (null != newComponent)
                    {
                        newEntity.AddComponent(newComponent);
                    }
                }
            }

            return newEntity;
        }

        public void LoadEntity(BaseEntity entity, int TemplateID)
        {            
            // Load components, use component definitions to load info used within the component
            EntityDefinition newDef;
            if (!entityDefinitions.TryGetValue(TemplateID, out newDef))
            {
                throw new Exception(String.Format("No entity templateID {0}", TemplateID));
            }
            else
            {                                
                entity.Name = newDef.Name;
                entity.TemplateID = newDef.TemplateID;

                // Create any needed components
                foreach (KeyValuePair<ComponentType, string> entry in newDef.ComponentDefinitions)
                {
                    BaseComponent newComponent = LoadComponent(entity, entry.Key, this.ComponentDefinitionsContentLoader, entry.Value);

                    if (null != newComponent)
                    {
                        entity.AddComponent(newComponent);
                    }
                }
            }
        }

        static public void CheckComponentLoadedProperly(BaseComponent newComponent, ComponentType typeToLoad)
        {
            if (null != newComponent)
            {
                if (typeToLoad != newComponent.GetComponentType())
                {
                    throw new Exception(String.Format("EntityLoader::LoadComponent not properly setup to load component type: {0}. Loaded the wrong type: {1}", typeToLoad.ToString(), newComponent.GetType()));
                }
            }
            else
            {
                throw new Exception(String.Format("EntityLoader::LoadComponent failed to load a component of type: {0}", typeToLoad.ToString()));
            }
        }

        static public BaseComponent LoadComponent(BaseEntity parent, ComponentType type, ContentManager contentManager, string XMLDefinitionPath)
        {
            // This is a factory for creating any component based on a ComponentType
            switch (type)
            {
                case ComponentType.RenderComponent:
                    {
                        BaseComponent newComponent = RenderComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }                    
                case ComponentType.CameraComponent:
                    {
                        BaseComponent newComponent = CameraComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }                    
                case ComponentType.ConstantMovementComponent:
                    {
                        BaseComponent newComponent = ConstantMovementComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.ConstantRotationComponent:
                    {
                        BaseComponent newComponent = ConstantRotationComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.FreeCameraInputComponent:
                    {
                        BaseComponent newComponent = FreeCameraInputComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.LightEmitterComponent:
                    {
                        BaseComponent newComponent = LightEmitterComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.PhysicsComponent:
                    {
                        BaseComponent newComponent = PhysicsComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.PhantomPhysicsComponent:
                    {
                        BaseComponent newComponent = PhantomPhysicsComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.CharacterPhysicsComponent:
                    {
                        BaseComponent newComponent = CharacterPhysicsComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.CharacterInputComponent:
                    {
                        BaseComponent newComponent = CharacterInputComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.WaterComponent:
                    {
                        BaseComponent newComponent = WaterComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.ArcBallCameraInputComponent:
                    {
                        BaseComponent newComponent = ArcBallCameraInputComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.ParticleEmitterComponent:
                    {
                        BaseComponent newComponent = ParticleEmitterComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }                
                case ComponentType.SkyDomeComponent:
                    {
                        BaseComponent newComponent = SkyDomeComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.CollisionTriggerComponent:
                    {
                        BaseComponent newComponent = CollisionTriggerComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.TerrainComponent:
                    {
                        BaseComponent newComponent = TerrainComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.TerrainPhysicsComponent:
                    {
                        BaseComponent newComponent = TerrainPhysicsComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.WaterVolumePhysicsComponent:
                    {
                        BaseComponent newComponent = WaterVolumePhysicsComponent.LoadFromDefinition(contentManager, XMLDefinitionPath, parent);
                        CheckComponentLoadedProperly(newComponent, type);

                        return newComponent;
                    }
                case ComponentType.Invalid:
                    {
                        throw new ArgumentException(String.Format("Invalid component type passed, type {0} is not valid", type));
                    }                    
                default:
                    {
                        throw new ArgumentException(String.Format("Invalid component type passed, type {0} is not valid", type));
                    }                    
            }
        }        
    }
}
