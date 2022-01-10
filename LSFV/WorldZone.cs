using BlueLightSoftware.Common;
using LiteDB;
using LSFV.Extensions;
using LSFV.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSFV
{
    /// <summary>
    /// This class contains a series of spawnable locations within a specific zone
    /// </summary>
    public class WorldZone
    {
        /// <summary>
        /// Contains a hash table of zones
        /// </summary>
        /// <remarks>[ ZoneScriptName => ZoneInfo class ]</remarks>
        private static Dictionary<string, WorldZone> ZoneCache { get; set; } = new Dictionary<string, WorldZone>();

        /// <summary>
        /// Gets the average daily crime calls
        /// </summary>
        [BsonId]
        public int Id { get; internal set; }

        /// <summary>
        /// Gets the Zone script name (ex: SANDY)
        /// </summary>
        public string ScriptName { get; private set; }

        /// <summary>
        /// Gets the full Zone name (Ex: Sandy Shores)
        /// </summary>
        public string DisplayName { get; internal set; }

        /// <summary>
        /// Gets the <see cref="LSFV.County"/> in game this zone belongs in
        /// </summary>
        public County County { get; internal set; }

        /// <summary>
        /// Gets the population density of the zone
        /// </summary>
        public Population Population { get; internal set; }

        /// <summary>
        /// Gets the zone size
        /// </summary>
        public ZoneSize Size { get; internal set; }

        /// <summary>
        /// Gets the social class of this zones citizens
        /// </summary>
        public SocialClass SocialClass { get; internal set; }

        /// <summary>
        /// Gets the primary zone type for this zone
        /// </summary>
        public List<ZoneFlags> Flags { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="WorldZone"/>. This constructor is designed to be used by <see cref="LiteDB"/>
        /// </summary>
        [BsonCtor]
        public WorldZone(int _id, string scriptName, string displayName, County county, Population population, ZoneSize size, SocialClass socialClass, BsonArray flags)
        {
            // Set properties
            Id = _id;
            ScriptName = scriptName ?? throw new ArgumentNullException("scriptName");
            DisplayName = displayName ?? throw new ArgumentNullException("displayName");
            County = county;
            Population = population;
            Size = size;
            SocialClass = socialClass;
            Flags = flags.Select(x => (ZoneFlags)Enum.Parse(typeof(ZoneFlags), x.AsString)).ToList();

            // Add to cache if not existing already
            if (!ZoneCache.ContainsKey(scriptName))
            {
                ZoneCache.Add(scriptName, this);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="WorldZone"/>.
        /// </summary>
        public WorldZone(string scriptName, string displayName, County county, Population population, ZoneSize size, SocialClass socialClass, List<ZoneFlags> flags)
        {
            // Set properties
            ScriptName = scriptName?.ToUpper() ?? throw new ArgumentNullException("scriptName");
            DisplayName = displayName ?? throw new ArgumentNullException("displayName");
            County = county;
            Population = population;
            Size = size;
            SocialClass = socialClass;
            Flags = flags;

            // Add to cache if not existing already
            if (!ZoneCache.ContainsKey(scriptName))
            {
                ZoneCache.Add(scriptName, this);
            }
        }

        /// <summary>
        /// Gets the total number of Locations in this collection, regardless of type
        /// </summary>
        /// <returns></returns>
        public int GetTotalNumberOfLocations()
        {
            // Add up location counts
            var count = Locations.RoadShoulders.Query().Where(x => x.Zone.Id == Id).Count();
                count += Locations.Residences.Query().Where(x => x.Zone.Id == Id).Count();

            // Final count
            return count;
        }

        /// <summary>
        /// This is where the magic happens. This method Gets a random <see cref="WorldLocation"/> from a pool
        /// of locations, applying filters and checking to see if the location is already in use
        /// </summary>
        /// <param name="type"></param>
        /// <param name="locationPool"></param>
        /// <param name="inactiveOnly">If true, will only return a <see cref="WorldLocation"/> that is not currently in use</param>
        /// <returns></returns>
        /// <seealso cref="https://github.com/mbdavid/LiteDB/issues/1666"/>
        protected virtual T GetRandomLocationFromPool<T>(ILiteQueryable<T> locationPool, FlagFilterGroup filters) where T : WorldLocation
        {
            // If we have no locations, return null
            if (!locationPool?.Exists() ?? false)
            {
                Log.Debug($"WorldZone.GetRandomLocationFromPool<T>(): Unable to pull a {typeof(T).Name} from zone '{ScriptName}' because there are no locations in the database");
                return null;
            }

            // Filter results
            var locations = locationPool.Include(x => x.Zone).Where(x => x.Zone.Id == Id).ToArray();

            // Filtering by flags? Do this first so we can log debugging info if there are no available locations with these required flags in this zone
            if (filters != null && filters.Requirements.Count > 0)
            {
                locations = locations.Filter(filters).ToArray();
                if (locations.Length == 0)
                {
                    Log.Warning($"WorldZone.GetRandomLocationFromPool<T>(): There are no locations of type '{typeof(T).Name}' in zone '{ScriptName}' using the following flags:");
                    Log.Warning($"\t{filters}");
                    return null;
                }
            }

            // If no locations are available
            if (locations.Length == 0)
            {
                Log.Debug($"WorldZone.GetRandomLocationFromPool<T>(): Unable to pull an available '{typeof(T).Name}' location from zone '{ScriptName}' because they are all in use");
                return null;
            }

            // Load randomizer
            var random = new CryptoRandom();
            return random.PickOne(locations);
        }

        /// <summary>
        /// Gets a random Side of the Road location in this zone
        /// </summary>
        public RoadShoulder GetRandomRoadShoulder(FlagFilterGroup filters)
        {
            // Get random location
            var queryable = Locations.RoadShoulders.Query();
            return GetRandomLocationFromPool(queryable, filters);
        }

        /// <summary>
        /// Gets a random <see cref="Residence"/> in this zone
        /// </summary>
        public Residence GetRandomResidence(FlagFilterGroup filters)
        {
            // Get random location
            var queryable = Locations.Residences.Query();
            return GetRandomLocationFromPool(queryable, filters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inactiveOnly">If true, will only return a <see cref="WorldLocation"/> that is not currently in use</param>
        public WorldLocation GetRandomLocation(LocationTypeCode locationType, FlagFilterGroup filters)
        {
            switch (locationType)
            {
                case LocationTypeCode.Business:
                    break;
                case LocationTypeCode.Coordinate:
                    break;
                case LocationTypeCode.Intersection:
                    break;
                case LocationTypeCode.Residence:
                    return GetRandomResidence(filters);
                case LocationTypeCode.RoadNode:
                    break;
                case LocationTypeCode.RoadShoulder:
                    return GetRandomRoadShoulder(filters);
            }

            return null;
        }

        /// <summary>
        /// Loads the specified zones from the database and from the Locations.xml into
        /// memory, caches the instances, and returns the total number of locations.
        /// </summary>
        /// <param name="names">An array of zones to load (should be all uppercase)</param>
        /// <returns>returns the number of locations loaded</returns>
        /// <param name="loaded">Returns the number of zones that were loaded (not cached yet).</param>
        public static WorldZone[] GetZonesByName(string[] names, out int loaded, out int totalLocations)
        {
            // Create instance of not already!
            if (ZoneCache == null)
            {
                ZoneCache = new Dictionary<string, WorldZone>();
            }

            // Local return variables
            totalLocations = 0;
            int zonesAdded = 0;
            List<WorldZone> zones = new List<WorldZone>();

            // Cycle through each child node (Zone)
            foreach (string zoneName in names)
            {
                // Create our spawn point collection and store it
                try
                {
                    var dbZone = GetZoneByName(zoneName);

                    // Add zone to return
                    zones.Add(dbZone);

                    // Up the location counters
                    totalLocations += dbZone.GetTotalNumberOfLocations();
                    zonesAdded++;
                }
                catch (FormatException e)
                {
                    Log.Error($"WorldZone.LoadZones(): Unable to load location data for zone '{zoneName}'. Missing node/attribute '{e.Message}'");
                    continue;
                }
                catch (FileNotFoundException)
                {
                    Log.Warning($"WorldZone.LoadZones(): Missing xml file for zone '{zoneName}'");
                    continue;
                }
                catch (Exception fe)
                {
                    Log.Exception(fe);
                    continue;
                }
            }

            loaded = zonesAdded;
            return zones.ToArray();
        }

        /// <summary>
        /// Gets a <see cref="WorldZone"/> instance by name from the database and fills its <see cref="CrimeProjection"/> 
        /// from the XML file.
        /// </summary>
        /// <param name="name">The script name of the zone as written in the Locations.xml</param>
        /// <returns>return a <see cref="WorldZone"/>, or null if the zone has not been loaded yet</returns>
        /// <exception cref="FileNotFoundException">thrown if the XML file for the zone does not exist"</exception>
        /// <exception cref="FormatException">thrown in the XML file is missing nodes and/or attributes</exception>
        public static WorldZone GetZoneByName(string name)
        {
            // We dont allow null here...
            if (String.IsNullOrWhiteSpace(name)) return null;

            // If we have loaded this zone already, skip it
            if (ZoneCache.ContainsKey(name))
            {
                return ZoneCache[name];
            }

            // Check file exists
            string path = Path.Combine(EntryPoint.FrameworkFolderPath, "Locations", $"{name}.xml");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"WorldZone.LoadZones(): Missing xml file for zone '{name}'");  
            }

            // Indicates whether the zone exists in the database
            var needToAdd = false;

            // Grab zone from database
            var dbZone = Locations.WorldZones.FindOne(x => x.ScriptName.Equals(name));
            if (dbZone == null)
            {
                Log.Warning($"Attempted to fetch zone named '{name}' from database but it did not exist");
                needToAdd = true;
            }

            // Load XML document
            using (var file = new WorldZoneFile(path))
            {
                // Parse the XML contents. It is OK to pass null here!
                file.Parse(dbZone);

                // Do we need to add?
                if (needToAdd)
                {
                    // Insert
                    dbZone = file.Zone;
                    var id = Locations.WorldZones.Insert(dbZone);

                    // Set the ID
                    dbZone.Id = id.AsInt32;
                }

                return dbZone;
            }
        }

        #region overrides

        public override int GetHashCode()
        {
            return (Id != 0) ? Id.GetHashCode() : ScriptName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as WorldZone;
            return (Id != 0 && other.Id != 0) ? Id.Equals(other.Id) : ScriptName.Equals(other.ScriptName);
        }

        #endregion overrides
    }
}
