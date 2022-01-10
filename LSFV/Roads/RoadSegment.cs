using LiteDB;
using System.Collections.Generic;

namespace LSFV
{
    /// <summary>
    /// Represents a segment of a road going in a single direction of trafic
    /// </summary>
    public class RoadSegment
    {
        /// <summary>
        /// Gets the id of this <see cref="RoadSegment
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Speed limit on the <see cref="RoadNode"/>
        /// </summary>
        public int SpeedLimit { get; set; }

        /// <summary>
        /// Gets an array of RoadFlags that describe this <see cref="RoadShoulder"/>
        /// </summary>
        public List<RoadFlags> Flags { get; set; }

        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("Roads")]
        public Road Road { get; set; }

        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("WorldZones")]
        public WorldZone Zone { get; set; }
    }
}
