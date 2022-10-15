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

        public static int getVeinIdMinRange()
        {
            return VeinIdRange.getMin();
        }

        public static int getZoneIdMinRange()
        {
            return ZoneIdRange.getMin();
        }
    }

    // Directions
    public enum Direction { North, East, South, West, None };

    // Depth
    public enum Depth { Above, Level, Shallow, Deep, Very_Deep };

    // Horizontal Displacement
    public enum HorizontalDisplacement { Far_Left, Left, Center, Right, Far_Right };

    // Tile Room Type
    public enum TileRoomType { None_Set, Zone, Vein, GreatTunnel }

}
