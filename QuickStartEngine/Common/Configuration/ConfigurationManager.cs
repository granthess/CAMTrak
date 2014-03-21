//
// ConfigurationManager.cs
//
// This file is part of the QuickStart Engine. See http://www.codeplex.com/QuickStartEngine
// for license details.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;

namespace QuickStart
{
    /// <summary>
    /// The central repository for client-configurable type bindings.  This class wraps the configuration XML file
    /// for the game and provides type information to the engine for all pluggable types.
    /// </summary>
    public sealed class ConfigurationManager
    {
        private XmlDocument document;

        private const string AttributeValue = "value";
        private const string ConfigurationFile = "Configuration.xml";

        private const string SectionConfiguration = "configuration";
        public const string SectionManagers = "managers";
        private const string SectionInputManager = "inputmanager";

#if WINDOWS
        public const string SectionWindowsOnlyManagers = "windowsOnlyManagers";
#endif //!WINDOWS

        /// <summary>
        /// Initializes the configuration
        /// </summary>
        public void Initialize()
        {
            this.document = new XmlDocument();
            this.document.Load(ConfigurationManager.ConfigurationFile);
        }

        /// <summary>
        /// Gets a value from the configuration file
        /// </summary>
        /// <typeparam name="T">Type of information expected</typeparam>
        /// <param name="section">Name of the section to get</param>
        /// <returns>Returns the value from the configuration as the specified type</returns>
        /// <exception cref="InvalidCastException"> if the type retrieved could be be casted to the expected type</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="section"/> was null</exception>
        public T GetValue<T>(string section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            List<string> path = SplitSectionName(section);
            XmlNode node = GetNode(path);
            string value = node.Attributes[ConfigurationManager.AttributeValue].Value;

            return (T)(object)value;
        }

        /// <summary>
        /// Splits the section name into seperate strings
        /// </summary>
        /// <param name="section">Name of the section to split</param>
        /// <returns>A list of strings coorsponding to the <paramref name="section"/></returns>
        /// <exception cref="ArgumentException">if the path didn't at least contain one '/'</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="section"/> was null</exception>
        private static List<string> SplitSectionName(string section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            List<string> path = new List<string>(section.Split("/".ToCharArray()));
            if (path.Count <= 1)
            {
                throw new ArgumentException("section parameter has an illigal structure");
            }
            return path;
        }

        /// <summary>
        /// Gets a specific node inside the configuration
        /// </summary>
        /// <param name="path">Full path to the node</param>
        /// <returns>The node at the specified path</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="path"/> was null</exception>
        /// <exception cref="ArgumentException">if the section was not found</exception>
        private XmlNode GetNode(List<string> path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            XmlNode node = this.document;
            while (path.Count > 0)
            {
                string name = path[0];
                path.RemoveAt(0);
                node = node[name];
                if (node == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "section '{0}' was not found", name));
                }
            }
            return node;
        }

        /// <summary>
        /// Gets the list of managers from the configuration
        /// </summary>
        /// <param name="game">Instance of the current <see cref="QSGame"/></param>
        /// <returns>A list of managers</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="game"/> is null</exception>
        /// <exception cref="ArgumentException">If the configuration does not contain a managers section</exception>
        /// <exception cref="XmlException">If the specified manager entry is invalid</exception>
        /// <exception cref="TypeLoadException">If the manager could not be loaded or created</exception>
        public List<BaseManager> GetManagers(QSGame game, string section)
        {
            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            List<string> path = new List<string>(new string[] { ConfigurationManager.SectionConfiguration, section });
            XmlNode managersNode = this.GetNode(path);
            if (managersNode == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "section '{0}' was not found", section));
            }

            List<BaseManager> managers = new List<BaseManager>();

            for (int i = 0; i < managersNode.ChildNodes.Count; ++i)
            {
                XmlNode node = managersNode.ChildNodes[i];

                // Skip comment nodes
                if(node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                string typeName = node.Attributes[ConfigurationManager.AttributeValue].Value;
                if (string.IsNullOrEmpty(typeName) == true)
                {
                    throw new XmlException("The manager type was not specified in configuration file");
                }

                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, "Could not find manager type '{0}'", typeName));
                }

                BaseManager manager = QSActivator.CreateInstance(type, game) as BaseManager;
                if (manager == null)
                {
                    throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, "Manager type '{0}' was not found", typeName));
                }

                managers.Add(manager);
            }

            return managers;
        }

        /// <summary>
        /// Gets the list of input handlers
        /// </summary>
        /// <param name="game">Instance of the current <see cref="QSGame"/></param>
        /// <returns>A list of handlers</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="game"/> is null</exception>
        /// <exception cref="ArgumentException">If the configuration does not contain a handlers section</exception>
        /// <exception cref="XmlException">If the specified handler entry is invalid</exception>
        /// <exception cref="TypeLoadException">If the handler could not be loaded or created</exception>
        public List<InputHandler> GetInputHandlers(QSGame game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            List<string> path = new List<string>(new string[] { ConfigurationManager.SectionConfiguration, ConfigurationManager.SectionInputManager });
            XmlNode managerNode = this.GetNode(path);
            if (managerNode == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "section '{0}' was not found", "managers"));
            }

            List<InputHandler> handlers = new List<InputHandler>();

            for (int i = managerNode.ChildNodes.Count - 1; i >= 0; --i)
            {
                XmlNode node = managerNode.ChildNodes[i];

                // Skip comment nodes
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                string typeName = node.Attributes[ConfigurationManager.AttributeValue].Value;
                if (string.IsNullOrEmpty(typeName) == true)
                {
                    throw new XmlException("The input handler type was not specified in configuration file");
                }
                
                Type type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, "Could not find input handler type '{0}'", typeName));
                }

                InputHandler handler = QSActivator.CreateInstance(type, game) as InputHandler;
                if (handler == null)
                {
                    throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, "Input handler type '{0}' was not found", typeName));
                }

                handlers.Add(handler);
            }

            return handlers;
        }
    }
}
