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
        /// Gets the <see cref="RoadSegment"/>
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment StartingSegment { get; set; }

        /// <summary>
        /// Gets the first <see cref="RoadSegment"/>
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment EndingSegment { get; set; }

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
        /// Gets the <see cref="Vector3"/> coordinates of this location
        /// </summary>
        [BsonIgnore]
        public Vector3 Position
        {
            get => new Vector3(X, Y, Z);
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }
    }
}
