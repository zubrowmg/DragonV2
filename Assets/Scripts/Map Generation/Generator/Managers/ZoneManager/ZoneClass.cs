using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using ZoneConfigEnums;
using AbilityAndThemeClasses;

using CommonlyUsedClasses;
using TileManagerClasses;

public class Zone_New
{
    // Zone properties
    int id = CommonDefines.DefualtId;
    ZoneAbilities zoneAbility = ZoneAbilities.None;
    ZoneThemes zoneTheme = ZoneThemes.None;

    // Zone Vein Generation Properties
    GameTiming gameTiming = GameTiming.Early;
    ZoneVeinGenType zoneVeinGenType = ZoneVeinGenType.Default;
    DimensionList associatedVeinZoneDim;
    TwoDList<Tile> associatedTileMap = new TwoDList<Tile>();
    TwoDList<Tile> tileMapConnections = new TwoDList<Tile>();
    DirectionBias zoneGenerationDirection;


    // Zones are created during vein zone generation
    public Zone_New(GameTiming timing, int id, ZoneAbilities ability, ZoneThemes theme, ZoneVeinGenType zoneVeinGenType, DirectionBias zoneGenerationDirection, ref DimensionList zoneDimList, ref TwoDList<Tile> tileMap)
    {
        this.gameTiming = timing;
        this.id = id;
        this.zoneAbility = ability;
        this.zoneTheme = theme;
        this.zoneVeinGenType = zoneVeinGenType;

        this.associatedTileMap = tileMap;
        this.associatedVeinZoneDim = zoneDimList;
        this.zoneGenerationDirection = zoneGenerationDirection;
    }


    public Zone_New deepCopy()
    {
        // associatedVeinZoneDim and associatedTileMap has to be passed by reference, this might cause issues for a deep copy
        return new Zone_New(this.gameTiming, this.id, this.zoneAbility, this.zoneTheme, this.zoneVeinGenType, this.zoneGenerationDirection, ref this.associatedVeinZoneDim, ref this.associatedTileMap);
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

    public ref TwoDList<Tile> getTileMapRef()
    {
        return ref this.associatedTileMap;
    }

    public void setVeinZoneConnectionList(ref TwoDList<Tile> tileMapConnections)
    {
        this.tileMapConnections = tileMapConnections;
    }

    public ref TwoDList<Tile> getVeinZoneConnectionList()
    {
        return ref this.tileMapConnections;
    }

    public int getId()
    {
        return this.id;
    }
}
