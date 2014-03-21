using System;
using System.Collections.Generic;

using QuickStart.Components;

namespace QuickStart.Entities
{
    public class ComponentDefinition
    {
        /// <summary>
        /// Type of component.
        /// </summary>
        public ComponentType type;

        /// <summary>
        /// Path of the XML that holds the loading information for this component
        /// </summary>
        public string XMLDefinitionPath;
    }
}
