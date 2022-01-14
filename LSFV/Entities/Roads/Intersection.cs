using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace LSFV
{
    public class Intersection : WorldLocation
    {
        /// <summary>
        /// Gets the id of this <see cref="Intersection"/>
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Gets an array of RoadFlags that describe this <see cref="RoadNode"/>
        /// </summary>
        public List<IntersectionFlags> Flags { get; set; }

        [BsonIgnore]
        public override LocationTypeCode LocationType => LocationTypeCode.Intersection;

        public Intersection()
        {

        }

        [BsonCtor]
        public Intersection(int _id, BsonArray flags, float x, float y, float z, string streetName, string hint, WorldZone zone)
        {
            Id = _id;
            X = x;
            Y = y;
            Z = z;
            StreetName = streetName;
            Hint = hint;
            Flags = flags?.Select(xx => (IntersectionFlags)xx.AsInt32).ToList();
        }

        /// <summary>
        /// Converts our <see cref="IntersectionFlags"/> to intergers and returns them
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
