using BlueLightSoftware.Common.Extensions;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace LSFV
{
    /// <summary>
    /// Represents a conenction between an <see cref="LSFV.Intersection"/> and
    /// a <see cref="LSFV.RoadSegment"/>
    /// </summary>
    public class Interconnection : WorldLocation
    {
        /// <summary>
        /// Gets the id of this Interconnection
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets the <see cref="LSFV.Intersection"/>
        /// </summary>
        [BsonRef("Intersections")]
        public Intersection Intersection { get; set; }

        /// <summary>
        /// Gets the <see cref="LSFV.RoadSegment"/>
        /// </summary>
        [BsonRef("RoadSegments")]
        public RoadSegment RoadSegment { get; set; }

        /// <summary>
        /// Gets the X value coordinate of this location
        /// </summary>
        public override float X { get; set; }

        /// <summary>
        /// Gets the Y value coordinate of this location
        /// </summary>
        public override float Y { get; set; }

        /// <summary>
        /// Gets the Z value coordinate of this location
        /// </summary>
        public override float Z { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="LSFV.RoadSegment"/> is entering the <see cref="LSFV.Intersection"/>
        /// </summary>
        public bool IsRoadEntering { get; set; }

        /// <summary>
        /// Gets an array of <see cref="InterconnectionFlags"/> that describe this <see cref="Interconnection"/>.
        /// Only <see cref="Interconnection"/>s entering an <see cref="LSFV.Intersection"/> will have flags
        /// </summary>
        public List<InterconnectionFlags> Flags { get; set; }

        [BsonIgnore]
        public override LocationTypeCode LocationType => LocationTypeCode.Interconnection;

        public Interconnection()
        {

        }

        [BsonCtor]
        public Interconnection(int _id, Intersection intersection, RoadSegment roadSegment, float x, float y, float z, bool isRoadEntering)
        {
            Id = _id;
            Intersection = intersection;
            RoadSegment = roadSegment;
            IsRoadEntering = isRoadEntering;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the heading of the road relative to the center of intersection
        /// </summary>
        /// <returns></returns>
        public float GetConnectionHeading()
        {
            // Did we forget an include?
            if (Intersection == null)
            {
                // Grab the Intersection from the database
                var inter = Locations.Interconnections.Include(x => x.Intersection).Include(x => x.RoadSegment).FindById(Id);
                Intersection = inter?.Intersection;
                RoadSegment = inter?.RoadSegment;

                // Are we still null?
                if (Intersection == null) return -1f;
            }

            // Grab the closest position for this road segment to the intersection
            return Intersection.Position.CalculateHeadingTowardsPosition(Position);
        }

        /// <summary>
        /// Converts our <see cref="InterconnectionFlags"/> to intergers and returns them
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
