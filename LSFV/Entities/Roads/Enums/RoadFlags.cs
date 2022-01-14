namespace LSFV
{
    /// <summary>
    /// A series of flags that help describe a <see cref="RoadSegment"/>. 
    /// </summary>
    public enum RoadFlags
    {
        /// <summary>
        /// Describes the location as being along an unpaved road
        /// </summary>
        /// <remarks>Intended location types: RoadShoulder, Residence, Store</remarks>
        DirtRoad,

        /// <summary>
        /// Describes the location as being along a one way road
        /// </summary>
        /// <remarks>Intended location types: RoadShoulder, Residence, Store</remarks>
        OneWayRoad,

        /// <summary>
        /// Describes the location as having a center left turn lane
        /// </summary>
        /// <remarks>Intended location types: RoadShoulderResidence, Store</remarks>
        HasCenterTurnRoad,

        /// <summary>
        /// Describes the location as having a Right turn only lane
        /// </summary>
        /// <remarks>Intended location types: RoadShoulderResidence, Store</remarks>
        HasRightTurnOnlyLane,

        /// <summary>
        /// Describes a road with a dotted center line, allowing passing into on coming traffic
        /// </summary>
        PassingZone,

        /// <summary>
        /// Describes the location as being on the interstate
        /// </summary>
        OnInterstate,

        /// <summary>
        /// Describes the location as being on the an interstate freeway ramp
        /// </summary>
        InterstateOnRamp,

        /// <summary>
        /// Describes the location as being on the an interstate freeway ramp
        /// </summary>
        InterstateOffRamp,

        /// <summary>
        /// Describes the location as being on a bridge
        /// </summary>
        OnBridge,

        /// <summary>
        /// Describes the location as being inside of a tunnel
        /// </summary>
        InsideTunnel,

        /// <summary>
        /// Describes a location as being along a road with driveways
        /// </summary>
        DrivewaysLeft,

        /// <summary>
        /// Describes a location as being along a road with driveways
        /// </summary>
        DrivewaysRight,

        /// <summary>
        /// Describes a location as being along a road with businesses on the left side
        /// </summary>
        BusinessesLeft,

        /// <summary>
        /// Describes a location as being along a road with businesses on the right side
        /// </summary>
        BusinessesRight,

        /// <summary>
        /// Describes a location that doesn't support large vehicles
        /// </summary>
        NoBigVehicles,
    }
}
