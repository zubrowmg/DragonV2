using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

namespace CommonlyUsedDefinesAndEnums
{
    static class CommonDefines
    {
        public static int DefualtId = -999;

        public static RangeLimit VeinIdRange = new RangeLimit(100, 199);
        public static RangeLimit ZoneIdRange = new RangeLimit(200, 299);
    }

    // Directions
    public enum Direction { North, East, South, West };

    // Tile Room Type
    public enum TileRoomType { None_Set, Zone, Vein, GreatTunnel }

}
