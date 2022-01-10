using LiteDB;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace LSFV
{
    /// <summary>
    /// A <see cref="WorldLocation"/> that represents a position on a paved or dirt road,
    /// used to spawn a <see cref="Vehicle"/>
    /// </summary>
    public class RoadNode : WorldLocation
    {
        /// <summary>
        /// Gets the id of this location
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets the heading of an <see cref="Entity"/> on this <see cref="RoadNode"/>
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// Gets the index of the lane on the road from the center. Do not count turn only lanes. All turn lanes 
        /// should be the index from center, only counting OTHER turn lanes only
        /// </summary>
        public int LaneIndex { get; set; } = 1;

        /// <summary>
        /// Gets an array of RoadFlags that describe this <see cref="RoadNode"/>
        /// </summary>
        public List<RoadNodeFlags> Flags { get; set; }

        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment RoadSegment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [BsonIgnore]
        public override string StreetName
        {
            get => RoadSegment?.Road?.Name ?? GameWorld.GetStreetNameAtLocation(Position);
            set => base.StreetName = value;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonIgnore]
        public override LocationTypeCode LocationType => LocationTypeCode.RoadNode;

        /// <summary>
        /// Creates a new instance of <see cref="RoadNode"/>
        /// </summary>
        public RoadNode()
        {

        }

        /// <summary>
        /// Converts our <see cref="ResidenceFlags"/> to intergers and returns them
        /// </summary>
        /// <remarks>
        /// Used for filtering locations based on flags
        /// </remarks>
        /// <returns>An array of filters as integers</returns>
        public override int[] GetIntFlags()
        {
            return Flags?.Select(x => (int)x).ToArray();
        }
    }
}
