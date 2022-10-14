using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using ZoneConfigEnums;
using AbilityAndThemeClasses;

using CommonlyUsedClasses;

public class Zone_New
{
    // Zone properties
    int id = CommonDefines.DefualtId;
    ZoneAbilities zoneAbility = ZoneAbilities.None;
    ZoneThemes zoneTheme = ZoneThemes.None;

    // Zone Vein Generation Properties
    GameTiming gameTiming = GameTiming.Early;
    ZoneVeinGenType zoneVeinGenType = ZoneVeinGenType.Default;
    DimensionList associatedVeinZoneDim = new DimensionList();


    // Zones are created during vein zone generation
    public Zone_New(GameTiming timing, int id, ZoneAbilities ability, ZoneThemes theme, ZoneVeinGenType zoneVeinGenType)
    {
        this.gameTiming = timing;
        this.id = id;
        this.zoneAbility = ability;
        this.zoneTheme = theme;
        this.zoneVeinGenType = zoneVeinGenType;
    }

    public Zone_New deepCopy()
    {
        return new Zone_New(this.gameTiming, this.id, this.zoneAbility, this.zoneTheme, this.zoneVeinGenType);
    }


    // ==============================================================
    //                       Setters/Getters
    // ==============================================================
    public void setVeinZoneDimList(ref DimensionList dimList)
    {
        this.associatedVeinZoneDim = dimList;
    }

    public ref DimensionList getVeinZoneDimList()
    {
        return ref this.associatedVeinZoneDim;
    }

    public int getId()
    {
        return this.id;
    }
}
