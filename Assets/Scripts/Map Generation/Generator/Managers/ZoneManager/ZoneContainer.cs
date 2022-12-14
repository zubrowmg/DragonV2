using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AbilityAndThemeClasses;
using ZoneConfigEnums;
using CommonlyUsedClasses;
using TileManagerClasses;

public partial class ContainerAccessor
{
    public Zone_New createNewZoneAndAddToContainer(GameTiming gameTiming, DirectionBias zoneGenerationDirection, ref DimensionList zoneDimList, ref TwoDList<Tile> tileMap, CoordsInt startCoords)
    {
        // Creates a new zone and adds it to the zoneContainer
        Zone_New newZone = this.contInst.zoneConfigurator.getNewZone(gameTiming, zoneGenerationDirection, ref zoneDimList, ref tileMap, startCoords);

        this.contInst.zoneContainer.addZone(ref newZone);

        return newZone;
    }

    public Zone_New getZone(int index)
    {
        return this.contInst.zoneContainer.getZone(index);
    }

    public int getZoneListCount()
    {
        return this.contInst.zoneContainer.getZoneListCount();
    }
}
public class ZoneContainer
{
    List<Zone_New> zoneList = new List<Zone_New>();

    public ZoneContainer()
    {

    }

    public void addZone(ref Zone_New newZone)
    {
        zoneList.Add(newZone);
    }

    public Zone_New getZone(int index)
    {
        return zoneList[index];
    }

    public int getZoneListCount()
    {
        return zoneList.Count;
    }
}
