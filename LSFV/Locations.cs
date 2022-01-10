using LiteDB;
using LSFV.Extensions;
using LSFV.Xml;
using Rage;
using System.IO;

namespace LSFV
{
    /// <summary>
    /// A LiteDB singleton instance for our LocationsData.db database file
    /// </summary>
    public static class Locations
    {
        /// <summary>
        /// Contains our singleton database connection (thread-safe)
        /// </summary>
        private static LiteDatabase Database { get; set; }

        /// <summary>
        /// Indicates whether the data has been loaded
        /// </summary>
        private static bool Initialized { get; set; }

        /// <summary>
        /// A queryable collection of WorldZone documents
        /// </summary>
        public static ILiteCollection<WorldZone> WorldZones { get; private set; }

        /// <summary>
        /// A queryable collection of Intersection documents
        /// </summary>
        public static ILiteCollection<Intersection> Intersections { get; private set; }

        /// <summary>
        /// A queryable collection of RoadShoulder documents
        /// </summary>
        public static ILiteCollection<RoadShoulder> RoadShoulders { get; private set; }

        /// <summary>
        /// A queryable collection of Residence documents
        /// </summary>
        public static ILiteCollection<Residence> Residences { get; private set; }

        /// <summary>
        /// Initializes the database 
        /// </summary>
        internal static void Initialize()
        {
            // Flag
            if (Initialized) return;
            Initialized = true;

            // Create connection string to our database
            var filePath = Path.Combine(EntryPoint.FrameworkFolderPath, "LocationData.db");
            var connectionString = $"Filename={filePath}";
            Log.Debug("Loading LocationsData.db LiteDatabase Connection");

            // Create database connection
            Database = new LiteDatabase(connectionString);
            Log.Debug("Created LiteDatabase Connection");

            // Ensure worldzones exist
            WorldZones = Database.GetCollection<WorldZone>("WorldZones", BsonAutoId.Int32);
            var recordCount = WorldZones.Count();
            if (recordCount == 0)
            {
                Log.Debug("Created WorldZone Collection");
            }
            else
            {
                Log.Debug($"Loaded existing WorldZone Collection with {recordCount} documents");
            }

            // Create index
            WorldZones.EnsureIndex(x => x.ScriptName, true);
            Log.Debug("Ensured Index on WorldZones");

            // Ensure Intersections Exist
            Intersections = Database.GetCollection<Intersection>("Intersections", BsonAutoId.Int32);
            recordCount = Intersections.Count();
            if (recordCount == 0)
            {
                Log.Debug("Created Intersections Collection");
            }
            else
            {
                Log.Debug($"Loaded existing Intersections Collection with {recordCount} documents");
            }

            // Create index
            Intersections.EnsureIndex(x => x.Zone.Id);
            Log.Debug("Ensured Index on Intersections");

            // Ensure RoadShoulders Exist
            RoadShoulders = Database.GetCollection<RoadShoulder>("RoadShoulders", BsonAutoId.Int32);
            recordCount = RoadShoulders.Count();
            if (recordCount == 0)
            {
                Log.Debug("Created RoadShoulders Collection");
            }
            else
            {
                Log.Debug($"Loaded existing RoadShoulders Collection with {recordCount} documents");
            }

            // Create index
            RoadShoulders.EnsureIndex(x => x.Zone.Id);
            Log.Debug("Ensured Index on RoadShoulders");

            // Ensure RoadShoulders Exist
            Residences = Database.GetCollection<Residence>("Residences", BsonAutoId.Int32);
            recordCount = Residences.Count();
            if (recordCount == 0)
            {
                Log.Debug("Created Residences Collection");
            }
            else
            {
                Log.Debug($"Loaded existing Residences Collection with {recordCount} documents");
            }

            // Create index
            Residences.EnsureIndex(x => x.Zone.Id);
            Log.Debug("Ensured Index on Residences");

            // Be nice and prevent locking up
            GameFiber.Yield();

            // Teach the BsonMapper how to serialize and un-serialize a Vector3
            BsonMapper.Global.RegisterType<Vector3>
            (
                serialize: (vector) => $"{vector.X},{vector.Y},{vector.Z}",
                deserialize: (bson) =>
                {
                    // Try and parse value. If it fails, Vector3.Zero is the output returned
                    Vector3Extensions.TryParse(bson.AsString, out Vector3 v);
                    return v;
                }
            );

            // Fill world zone data if empty! This may happen if the AppData.db 
            // file is deleted
            if (WorldZones.Count() == 0)
            {
                InsertDefaultZoneData();
            }
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        internal static void Shutdown()
        {
            Database?.Dispose();
            Database = null;
        }

        /// <summary>
        /// Loads all the location .xml file data into the database
        /// </summary>
        private static void InsertDefaultZoneData()
        {
            var path = Path.Combine(EntryPoint.FrameworkFolderPath, "Locations");
            DirectoryInfo directory = new DirectoryInfo(path);
            Log.Debug("Creating default zone data in database");

            // Parse each file and enter it into the database
            foreach (var file in directory.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
            {
                var fileName = file.FullName;
                using (var zoneFile = new WorldZoneFile(fileName))
                {
                    Log.Debug($"Parsing WorldZone file '{file.Name}'");
                    zoneFile.Parse();

                    // Insert
                    WorldZones.Insert(zoneFile.Zone);
                }

                // Be nice and prevent locking up
                GameFiber.Yield();
            }

            // Set database version to one
            Database.UserVersion = 1;
        }
    }
}
