using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonlyUsedDefinesAndEnums
{
    static class CommonDefines
    {
        public static int DefualtId = -999;

    }

    // Directions
    public enum Direction { North, East, South, West };

    // Tile Room Type
    public enum TileRoomType { None_Set, Zone, Vein, GreatTunnel }

}
