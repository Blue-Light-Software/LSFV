using LiteDB;

namespace LSFV
{
    /// <summary>
    /// Represents a conenction between an <see cref="Locations.Intersection"/> and
    /// a <see cref="Locations.RoadSegment"/>
    /// </summary>
    public class Interconnection
    {
        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("Intersections")]
        public Intersection Intersection { get; set; }

        /// <summary>
        /// Gets the zone of this location, if known
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment RoadSegment { get; set; }

        /// <summary>
        /// Gets the X value coordinate of this location
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets the Y value coordinate of this location
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Gets the Z value coordinate of this location
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// Gets the heading of the <see cref="RoadSegment"/>
        /// </summary>
        public float Heading { get; set; }
    }
}
