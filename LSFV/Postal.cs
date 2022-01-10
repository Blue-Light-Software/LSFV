﻿using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace LSFV
{
    /// <summary>
    /// Reprents a postal code within the <see cref="GameWorld"/>
    /// </summary>
    public class Postal : IEquatable<Postal>
    {
        #region instance

        /// <summary>
        /// Gets the postal code
        /// </summary>
        public int Code { get; internal set; }

        /// <summary>
        /// Gets a central position of this <see cref="Postal"/>
        /// </summary>
        public Vector3 Location { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="Postal"/>
        /// </summary>
        protected Postal(int code, Vector3 position)
        {
            Code = code;
            Location = position;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Postal);
        }

        public bool Equals(Postal other)
        {
            if (other == null) return false;
            return other.Code == Code;
        }

        public override string ToString() => $"{Code} ({Location})";

        public override int GetHashCode() => Code.GetHashCode();

        #endregion instance

        #region static

        /// <summary>
        /// Gets a hashset of all loaded postals
        /// </summary>
        internal static List<Postal> Postals { get; set; }

        /// <summary>
        /// Loads the postals xml file
        /// </summary>
        internal static void Initialize()
        {
            // Clear
            Postals = new List<Postal>();

            // Load XML document
            var document = new XmlDocument();
            var filePath = Path.Combine(EntryPoint.FrameworkFolderPath, "Postals", $"{Settings.PostalsFileName}.xml");
            using (var file = new FileStream(filePath, FileMode.Open))
            {
                document.Load(file);
            }

            // Ensure we have data
            XmlNode rootNode = document.DocumentElement;
            if (rootNode == null || !rootNode.HasChildNodes)
            {
                throw new FormatException($"Postal.Initialize(): Postals file is empty");
            }

            // Add postals
            foreach (XmlNode node in rootNode.SelectNodes("row"))
            {
                // Grab postal code
                string value = node.SelectSingleNode("code")?.InnerText;
                if (value == null)
                    continue;

                // Some postals are formatted as "code-[A|B]"
                if (value.Contains('-'))
                {
                    var parts = value.Split('-');
                    value = parts[0];
                }

                if (!Int32.TryParse(value, out int code))
                {
                    Log.Warning($"Postal.Initialize(): Unable to parse code value '{value ?? "null" }'");
                    continue;
                }

                value = node.SelectSingleNode("x")?.InnerText;
                if (value == null || !float.TryParse(value, out float x))
                {
                    Log.Warning($"Postal.Initialize(): Unable to parse X value '{value ?? "null" }'");
                    continue;
                }

                value = node.SelectSingleNode("y")?.InnerText;
                if (value == null || !float.TryParse(value, out float y))
                {
                    Log.Warning($"Postal.Initialize(): Unable to extract Y value '{value ?? "null" }'");
                    continue;
                }

                // Create instance and add it
                var instance = new Postal(code, new Vector3(x, y, 0));
                Postals.Add(instance);
            }
        }

        /// <summary>
        /// Gets the <see cref="Postal"/> instance that is closest to the location
        /// </summary>
        /// <param name="location"></param>
        public static Postal FromVector(Vector3 location)
        {
            return (from x in Postals orderby x.Location.DistanceTo2D(location) select x).FirstOrDefault();
        }

        #endregion static
    }
}
