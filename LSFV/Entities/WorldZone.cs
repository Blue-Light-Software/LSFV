using BlueLightSoftware.Common;
using LiteDB;
using LSFV.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSFV
{
    /// <summary>
    /// This class contains a series of spawnable locations within a specific zone
    /// </summary>
    public class WorldZone
    {
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
            Flags = flags.Select(x => (ZoneFlags)x.AsInt32).ToList();
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
