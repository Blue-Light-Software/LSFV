using Rage;
using static Rage.Native.NativeFunction;

namespace LSFV
{
    /// <summary>
    /// Provides methods to get and set information within the Game World
    /// </summary>
    internal static class GameWorld
    {
        /// <summary>
        /// Gets the <see cref="WorldZone"/> based on the coordinates provided.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static WorldZone GetZoneAtLocation(Vector3 position)
        {
            var name = Natives.GetNameOfZone<string>(position.X, position.Y, position.Z);
            return WorldZone.GetZoneByName(name);
        }

        /// <summary>
        /// Gets the zone name based on the coordinates provided.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetZoneNameAtLocation(Vector3 position, bool fullName = false)
        {
            // Script name?
            if (!fullName)
                return Natives.GetNameOfZone<string>(position.X, position.Y, position.Z);

            // Fetch zone
            var zone = GetZoneAtLocation(position);
            return zone?.DisplayName;
        }

        /// <summary>
        /// Gets the street name based on the provided location
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetStreetNameAtLocation(Vector3 position)
        {
            return GetStreetNameAtLocation(position, out string crossingRoad);
        }

        /// <summary>
        /// Gets the street name based on the provided location, and its crossing intersection (if near)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetStreetNameAtLocation(Vector3 position, out string crossingRoad)
        {
            uint nameHash = 0, crossingRoadHash = 0;
            Natives.GetStreetNameAtCoord(position.X, position.Y, position.Z, ref nameHash, ref crossingRoadHash);

            crossingRoad = Natives.GetStreetNameFromHashKey<string>(crossingRoadHash);
            return Natives.GetStreetNameFromHashKey<string>(nameHash);
        }
    }
}
