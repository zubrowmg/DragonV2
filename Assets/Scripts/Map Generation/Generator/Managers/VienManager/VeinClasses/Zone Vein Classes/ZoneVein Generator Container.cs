using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;
using VeinManagerClasses;

public class ZoneVeinGeneratorContainer
{
    public bool debugMode = false;

    // Main two controllers
    public ZoneVeinNavigationController zoneVeinNavigationController;
    public ZoneVeinDiGraphContoller zoneVeinDiGraphController;
    public DimVeinZoneCreator dimVeinZoneCreator;

    public Zone_New currentZone;

    // Tile Map Connections
    //              Double<         CanTravelTo, Tile >
    public TwoDList<Double<TileTraveledToMarker, Tile>> tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>();

    // Vein passes (Trunk = pass0, branches are pass1, 2, 3, etc)
    public int currentVeinPass = 0;

    // The output vein zone
    public VeinZone currentVeinZone;

    public ZoneVeinGeneratorContainer(bool debugMode)
    {
        this.debugMode = debugMode;

    }

    // =====================================================================================
    //                                    Init Functions
    // =====================================================================================

    public void assignControllerInstances(ref ZoneVeinNavigationController navControllerInst, 
                                          ref ZoneVeinDiGraphContoller diGraphControllerInst,
                                          ref DimVeinZoneCreator dimVeinZoneCreator)
    {
        this.zoneVeinNavigationController = navControllerInst;
        this.zoneVeinDiGraphController = diGraphControllerInst;
        this.dimVeinZoneCreator = dimVeinZoneCreator;
    }

    // =====================================================================================
    //                                    
    // =====================================================================================

    public CoordsInt getTileMapCoordsFromTileMapConns(CoordsInt coords)
    {
        return this.tileMapConnections.getElement(coords).getTwo().getTileMapCoords();
    }


    // Checks for boundries and if the point cannot be traveled to
    public bool checkTileMapConnPoint(CoordsInt coords)
    {
        // Check Boundry
        bool rejected = !this.tileMapConnections.isInsideBounds(coords);

        if (rejected == true)
            return rejected;

        Double<TileTraveledToMarker, Tile> attemptedTileMapConnElement = this.tileMapConnections.getElement(coords);

        // Tile has already been traveled to
        if (attemptedTileMapConnElement.getOne().isPassLocked(this.currentVeinPass) == true)
            rejected = true;

        return rejected;
    }
}
