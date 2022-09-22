using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums
{
    // Directions
    public enum generalDirection { North, East, South, West};
    public enum generalDirectionTwo { Up, Down, Left, Right};

    // Zones
    public enum intendedUnitType { None, Vein, Zone, GreatTunnel };
    public enum zoneThemes { None, Rock, Fire, Forest, Wind, Lake /* Earth */};
    public enum uniqueThemes { None, GreatTunnel, DeepHole };

    // Powerups
    public enum abilities { None, DoubleJump, Dash, WallJump, MagicBullet, AirMask };

    // General Game stage progression
    public enum gameTiming { Early, Mid, Late, Post, Null };

    // SubArea Clump Types
    public enum SubAreaClumpType { None, Normal, PreBoss }; // Boss is a single sub area, can't be a clump
}
