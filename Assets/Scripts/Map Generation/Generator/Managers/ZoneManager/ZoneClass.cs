using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using ZoneConfigEnums;
using AbilityAndThemeClasses;

public class Zone_New
{
    // Zone properties
    int id = CommonDefines.DefualtId;
    ZoneAbilities zoneAbility = ZoneAbilities.None;
    ZoneThemes zoneTheme = ZoneThemes.None;

    // Zone Vein Generation Properties
    GameTiming gameTiming = GameTiming.Early;
    ZoneVeinGenType zoneVeinGenType = ZoneVeinGenType.Default;


    // Zones are created during vein zone generation
    public Zone_New(GameTiming timing, int id, ZoneAbilities ability, ZoneThemes theme, ZoneVeinGenType zoneVeinGenType)
    {
        this.gameTiming = timing;
        this.id = id;
        this.zoneAbility = ability;
        this.zoneTheme = theme;
        this.zoneVeinGenType = zoneVeinGenType;
    }


}
