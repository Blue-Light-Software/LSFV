using LiteDB;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace LSFV
{
    /// <summary>
    /// Represents a segment of a road going in a single direction of trafic
    /// </summary>
    /// <remarks>
    /// When to create a new RoadSegment:
    ///     - When a speed limit changes
    ///     - When the number of lanes change
    ///     - When the flags that describe the road change
    ///         - PassingZone / Non PassingZone
    ///     - When the zone changes
    ///     - When the road name changes
    /// </remarks>
    public class RoadSegment : WorldLocation
    {
        /// <summary>
        /// Gets the id of this <see cref="RoadSegment
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Speed limit on the <see cref="RoadSegment"/>
        /// </summary>
        public int SpeedLimit { get; set; }

        /// <summary>
        /// Gets or sets the number of lanes on this <see cref="RoadSegment"/>
        /// </summary>
        public int LaneCount { get; set; }

        /// <summary>
        /// Gets the travel distance of this road segement
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Gets the X value coordinate of this location's starting point
        /// </summary>
        public float StartX { get; set; }

        /// <summary>
        /// Gets the Y value coordinate of this location's starting point
        /// </summary>
        public float StartY { get; set; }

        /// <summary>
        /// Gets the Z value coordinate of this location's starting point
        /// </summary>
        public float StartZ { get; set; }

        /// <summary>
        /// Gets the heading of this location's starting point
        /// </summary>
        public float StartHeading { get; set; }

        /// <summary>
        /// Gets the X value coordinate of this location's ending point
        /// </summary>
        public float EndX { get; set; }

        /// <summary>
        /// Gets the Y value coordinate of this location's ending point
        /// </summary>
        public float EndY { get; set; }

        /// <summary>
        /// Gets the Z value coordinate of this location's ending point
        /// </summary>
        public float EndZ { get; set; }

        /// <summary>
        /// Gets the heading of this location's ending point
        /// </summary>
        public float EndHeading { get; set; }

        /// <summary>
        /// Gets the <see cref="Vector3"/> coordinates of this location's ending point
        /// </summary>
        [BsonIgnore]
        public Vector3 EndPosition
        {
            get => new Vector3(EndX, EndY, EndZ);
            set
            {
                EndX = value.X;
                EndY = value.Y;
                EndZ = value.Z;
            }
        }

        /// <summary>
        /// Gets an array of RoadFlags that describe this <see cref="RoadSegment"/>
        /// </summary>
        public List<RoadFlags> Flags { get; set; }

        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("Roads")]
        public Road Road { get; set; }

        [BsonIgnore]
        public override LocationTypeCode LocationType => LocationTypeCode.RoadSegment;

        [BsonIgnore]
        public override float X
        {
            get => StartX;
            set => StartX = value;
        }

        [BsonIgnore]
        public override float Y
        {
            get => StartY;
            set => StartY = value;
        }

        [BsonIgnore]
        public override float Z
        {
            get => StartZ;
            set => StartZ = value;
        }

        [BsonIgnore]
        public override string StreetName
        {
            get => Road?.Name ?? GameWorld.GetStreetNameAtLocation(Position);
            set => base.StreetName = value;
        }

        [BsonIgnore]
        public override string Hint
        {
            get => base.Hint;
            set => base.StreetName = value;
        }

        public RoadSegment()
        {

        }

        [BsonCtor]
        public RoadSegment(
            int _id, int speedLimit, int laneCount, int length, float startX, float startY, float startZ, 
            float startHeading, float endX, float endY, float endZ, float endHeading, BsonArray flags,
            Road road, WorldZone zone)
        {
            Id = _id;
            SpeedLimit = speedLimit;
            LaneCount = laneCount;
            Length = length;
            StartX = startX;
            StartY = startY;
            StartZ = startZ;
            StartHeading = startHeading;
            EndX = endX;
            EndY = endY;
            EndZ = endZ;
            EndHeading = endHeading;
            Flags = flags.Select(x => (RoadFlags)x.AsInt32).ToList();
            Road = road;
            Zone = zone;
        }

        /// <summary>
        /// Converts our <see cref="RoadFlags"/> to intergers and returns them
        /// </summary>
        /// <remarks>
        /// Used for filtering locations based on flags
        /// </remarks>
        /// <returns>An array of filters as integers</returns>
        public override int[] GetIntFlags()
        {
            return Flags?.Select(x => (int)x).ToArray();
        }

        /// <summary>
        /// Gets an array of <see cref="RoadNode"/>s that reference this <see cref="RoadSegment"/>
        /// </summary>
        /// <returns></returns>
        public RoadNode[] GetRoadNodes()
        {
            var nodes = Locations.RoadNodes.Query()
                .Include(x => x.Zone)
                .Include(x => x.RoadSegment)
                .Include(x => x.RoadSegment.Zone)
                .Where(x => x.RoadSegment.Id == Id);

            return nodes.ToArray();
        }

        /// <summary>
        /// Gets an array <see cref="RoadJunction"/>s this <see cref="RoadSegment"/> starts
        /// or ends at
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public RoadJunction[] GetJunctions()
        {
            // Fetch junction
            return Locations.RoadJunctions.Query()
                .Include(x => x.StartingSegment)
                .Include(x => x.StartingSegment.Zone)
                .Include(x => x.StartingSegment.Road)
                .Include(x => x.EndingSegment)
                .Include(x => x.EndingSegment.Zone)
                .Include(x => x.EndingSegment.Road)
                .Where(x => x.EndingSegment.Id == Id || x.StartingSegment.Id == Id)
                .ToArray();
        }

        /// <summary>
        /// Gets the next <see cref="RoadSegment"/> attached to this <see cref="RoadSegment"/>
        /// if any, and returns the result. Returns null if there is no segment attached.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public RoadSegment GetNextSegment()
        {
            // Fetch junction
            var junction = Locations.RoadJunctions.Query()
                .Include(x => x.EndingSegment)
                .Include(x => x.EndingSegment.Zone)
                .Include(x => x.EndingSegment.Road)
                .Include(x => x.StartingSegment)
                .Include(x => x.StartingSegment.Zone)
                .Include(x => x.StartingSegment.Road)
                .Where(x => x.EndingSegment.Id == Id)
                .FirstOrDefault();

            // No connection
            return junction?.StartingSegment;
        }

        /// <summary>
        /// Attempts to fetch the next <see cref="RoadSegment"/> and returns the result
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool TryGetNextSegment(out RoadSegment segment)
        {
            // Fetch junction
            var junction = Locations.RoadJunctions.Query()
                .Include(x => x.EndingSegment)
                .Include(x => x.EndingSegment.Zone)
                .Include(x => x.StartingSegment)
                .Include(x => x.StartingSegment.Road)
                .Include(x => x.StartingSegment.Zone)
                .Where(x => x.EndingSegment.Id == Id)
                .FirstOrDefault();

            // No connection
            if (junction == null)
            {
                segment = null;
                return false;
            }

            segment = junction.StartingSegment;
            return true;
        }

        /// <summary>
        /// Gets the previous <see cref="RoadSegment"/> attached to this <see cref="RoadSegment"/>
        /// if any, and returns the result. Returns null if there is no segment attached.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public RoadSegment GetPreviousSegment()
        {
            // Fetch junction
            var junction = Locations.RoadJunctions.Query()
                .Include(x => x.StartingSegment)
                .Include(x => x.StartingSegment.Zone)
                .Include(x => x.EndingSegment)
                .Include(x => x.EndingSegment.Road)
                .Include(x => x.EndingSegment.Zone)
                .Where(x => x.StartingSegment.Id == Id)
                .FirstOrDefault();

            // No connection
            return junction?.EndingSegment;
        }

        /// <summary>
        /// Attempts to fetch the previos <see cref="RoadSegment"/> and returns the result
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool TryGetPreviousSegment(out RoadSegment segment)
        {
            // Fetch junction
            var junction = Locations.RoadJunctions.Query()
                .Include(x => x.StartingSegment)
                .Include(x => x.StartingSegment.Zone)
                .Include(x => x.EndingSegment)
                .Include(x => x.EndingSegment.Road)
                .Include(x => x.EndingSegment.Zone)
                .Where(x => x.StartingSegment.Id == Id)
                .FirstOrDefault();

            // No connection
            if (junction == null)
            {
                segment = null;
                return false;
            }

            segment = junction.EndingSegment;
            return true;
        }

        /// <summary>
        /// Gets an array <see cref="Interconnection"/>s this <see cref="RoadSegment"/> starts
        /// or ends at
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public Interconnection[] GetInterconnections()
        {
            // Fetch junction
            return Locations.Interconnections.Query()
                .Include(x => x.Zone)
                .Include(x => x.RoadSegment)
                .Include(x => x.RoadSegment.Zone)
                .Include(x => x.Intersection)
                .Include(x => x.Intersection.Zone)
                .Where(x => x.RoadSegment.Id == Id)
                .ToArray();
        }

        /// <summary>
        /// Attempts to fetch any <see cref="Interconnection"/>s this <see cref="RoadSegment"/> starts
        /// or ends at, and returns the result
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool TryGetInterconnections(out Interconnection[] connections)
        {
            // Fetch junction
            connections = Locations.Interconnections.Query()
                .Include(x => x.Zone)
                .Include(x => x.RoadSegment)
                .Include(x => x.RoadSegment.Zone)
                .Include(x => x.RoadSegment.Road)
                .Include(x => x.Intersection)
                .Include(x => x.Intersection.Zone)
                .Where(x => x.RoadSegment.Id == Id)
                .ToArray();

            // No connection
            return (connections != null && connections.Length > 0);
        }
    }
}
