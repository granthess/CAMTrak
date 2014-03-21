using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Content;

using QuickStart.Components;

namespace QuickStart.Entities
{
    public class EntityDefinition
    {
        [ContentSerializer]
        public int TemplateID = -1;

        [ContentSerializer]
        public string Name = "Name Unspecified";

        [ContentSerializer]
        public string DefinitionXML;

        [ContentSerializer]
        public string Description = "No definition";

        /// <summary>
        /// A Dictionary of the components that make up this Entity and the definitions for each component.
        /// The key is the <seealso cref="ComponentType"/> and the value is the path to the XML definition for that component
        /// </summary>
        [ContentSerializerIgnore]
        private Dictionary<ComponentType, string> componentDefinitions;

        [ContentSerializerIgnore]
        public Dictionary<ComponentType, string> ComponentDefinitions
        {
            get { return componentDefinitions; }
        }

        public EntityDefinition()
        {
            componentDefinitions = new Dictionary<ComponentType, string>();
        }

        public void AddDefinition(ComponentDefinition newDefinition)
        {
            string XMLPath;
            if (!componentDefinitions.TryGetValue(newDefinition.type, out XMLPath))
            {
                componentDefinitions.Add(newDefinition.type, newDefinition.XMLDefinitionPath);
            }
            else
            {
                throw new ArgumentException("An entity cannot have two of the same component type");
            }
        }
    }
}
