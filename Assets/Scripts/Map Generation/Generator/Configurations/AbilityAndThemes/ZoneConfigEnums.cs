using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneConfigEnums
{
    // Themes
    public enum ZoneThemes { None, Rock, Fire, Forest, Wind, Lake /* Earth */};
    public enum UniqueThemes { None, GreatTunnel, DeepHole };

    // Powerups
    public enum ZoneAbilities { None, DoubleJump, Dash, WallJump, MagicBullet, AirMask };

    // General Game stage progression
    public enum GameTiming { Early, Mid, Late, Post, Null };

    // Zone Vein Generatrion
    public enum ZoneVeinGenType { Default, LongVertical, LongHorizontal };
}
