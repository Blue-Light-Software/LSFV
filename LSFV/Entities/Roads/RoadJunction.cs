using LiteDB;
using Rage;

namespace LSFV
{
    /// <summary>
    /// Represents a conenction between two <see cref="RoadSegment"/>s
    /// </summary>
    public class RoadJunction
    {
        /// <summary>
        /// Gets the id of this location
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets the <see cref="RoadSegment"/>
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment StartingSegment { get; set; }

        /// <summary>
        /// Gets the first <see cref="RoadSegment"/>
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment EndingSegment { get; set; }
    }
}
