using BlueLightSoftware.Common.Xml;
using LSFV.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LSFV.Xml
{
    /// <summary>
    /// Loads and parses the XML data from a location.xml
    /// </summary>
    internal class WorldZoneFile : XmlFileBase
    {
        /// <summary>
        /// Gets the Zone name
        /// </summary>
        public WorldZone Zone { get; protected set; }

        /// <summary>
        /// Creates a new instance of <see cref="WorldZoneFile"/>
        /// </summary>
        /// <param name="filePath"></param>
        public WorldZoneFile(string filePath) : base(filePath)
        {

        }

        /// <summary>
        /// Parses the zone XML data in the file, and throws an exception on failure
        /// </summary>
        /// <exception cref="FormatException">thrown if the XML is missing nodes or attributes</exception>
        public void Parse(WorldZone zone = null)
        {
            // Get zone name
            var zoneName = Path.GetFileNameWithoutExtension(FilePath);

            // Grab zone node
            XmlNode node = Document.SelectSingleNode(zoneName);
            if (node == null || !node.HasChildNodes)
            {
                throw new FormatException($"WorldZoneFile.Parse(): Missing data for zone '{zoneName}'");
            }

            Zone = zone;
            if (zone == null)
            {
                // Load zone info
                XmlNode catagoryNode = node.SelectSingleNode("Name");
                var displayName = catagoryNode?.InnerText ?? throw new FormatException("Name");
                var scriptName = node.Name;

                // Extract size
                catagoryNode = node.SelectSingleNode("Size");
                if (String.IsNullOrWhiteSpace(catagoryNode?.InnerText) || !Enum.TryParse(catagoryNode.InnerText, out ZoneSize size))
                {
                    throw new FormatException("Size");
                }

                // Extract type
                catagoryNode = node.SelectSingleNode("Type");
                if (String.IsNullOrWhiteSpace(catagoryNode?.InnerText))
                {
                    throw new FormatException("Type");
                }
                var flags = ParseZoneFlags(catagoryNode);

                // Extract social class
                catagoryNode = node.SelectSingleNode("Class");
                if (String.IsNullOrWhiteSpace(catagoryNode?.InnerText) || !Enum.TryParse(catagoryNode.InnerText, out SocialClass sclass))
                {
                    throw new FormatException("Class");
                }

                // Extract population
                catagoryNode = node.SelectSingleNode("Population");
                if (String.IsNullOrWhiteSpace(catagoryNode?.InnerText) || !Enum.TryParse(catagoryNode.InnerText, out Population pop))
                {
                    throw new FormatException("Population");
                }

                // Extract county
                catagoryNode = node.SelectSingleNode("County");
                if (String.IsNullOrWhiteSpace(catagoryNode?.InnerText) || !Enum.TryParse(catagoryNode.InnerText, out County c))
                {
                    throw new FormatException("County");
                }

                // Create new instance
                Zone = new WorldZone(scriptName, displayName, c, pop, size, sclass, flags);
            }
        }

        /// <summary>
        /// Reads and parses an <see cref="XmlNode"/> containing <see cref="ZoneFlags"/>
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static List<ZoneFlags> ParseZoneFlags(XmlNode node)
        {
            // Default return value
            string val = node?.InnerText;

            // Check for empty strings
            if (String.IsNullOrWhiteSpace(val))
            {
                return new List<ZoneFlags>();
            }

            // Parse comma seperated values
            return val.CSVToEnumList<ZoneFlags>();
        }
    }
}
