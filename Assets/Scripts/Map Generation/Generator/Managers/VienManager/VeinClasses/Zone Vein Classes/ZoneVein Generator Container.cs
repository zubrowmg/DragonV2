using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;
using VeinManagerClasses;
using DimCreatorClasses;
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
    private TwoDList<Double<TileTraveledToMarker, Tile>> tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>();

    // Vein passes (Trunk = pass0, branches are pass1, 2, 3, etc)
    private int currentVeinPass = 0;

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
    //                  Current vein pass functions                  
    // =====================================================================================

    public void incCurrentVeinPass()
    {
        this.currentVeinPass++;
    }

    public int getCurrentVeinPass()
    {
        return this.currentVeinPass;
    }

    public void resetCurrentVeinPass()
    {
        this.currentVeinPass = 0;
    }

    // =====================================================================================
    //                  Tile Map Connections functions                  
    // =====================================================================================

    public CoordsInt getWorldMapCoordsFromTileMapConns(CoordsInt coords)
    {
        return this.tileMapConnections.getElement(coords).getTwo().getTileMapCoords();
    }

    public List<CoordsInt> getWorldMapCoordsFromTileMapConns(List<CoordsInt> coordsList)
    {
        List<CoordsInt> worldCoords = new List<CoordsInt>();
        foreach (var coords in coordsList)
        {
            worldCoords.Add(getWorldMapCoordsFromTileMapConns(coords));
        }
        return worldCoords;
    }

    // Checks for boundries and if the point can be traveled to
    public bool checkTileMapConnPoint(CoordsInt coords)
    {
        // Check Boundry
        bool rejected = !this.coordsAreInsideTileMapBoundries(coords);

        if (rejected == true)
            return rejected;

        // Tile has already been traveled to
        if (tileMapConnCoordIsLocked__ForAllPasses(coords) == true)
            rejected = true;

        return rejected;
    }

    public enum TileLockTest { Perma_Locked, Current_Pass, All_Passes }

    public bool tileMapConnCoordIsLocked(CoordsInt coords, TileLockTest lockTestType)
    {
        bool tileConnIsLocked = true;
        Double<TileTraveledToMarker, Tile> attemptedTileMapConnElement;

        // Only test if it is inside of the bounds
        if (tileMapConnections.isInsideBounds(coords) == true)
        {
            attemptedTileMapConnElement = getTileMapConnElement(coords);

            switch (lockTestType)
            {
                case TileLockTest.All_Passes:
                    tileConnIsLocked = attemptedTileMapConnElement.getOne().isAnyPassLocked();
                    break;

                case TileLockTest.Current_Pass:
                    tileConnIsLocked = attemptedTileMapConnElement.getOne().isPassLocked(getCurrentVeinPass());
                    break;

                case TileLockTest.Perma_Locked:
                    tileConnIsLocked = attemptedTileMapConnElement.getOne().isPermaLocked();
                    break;
            }
        }

        return tileConnIsLocked;
    }

    public bool tileMapConnCoordIsPermaLocked(CoordsInt coords)
    {
        return tileMapConnCoordIsLocked(coords, TileLockTest.Perma_Locked);
    }

    public bool tileMapConnCoordIsLocked__ForCurrentPass(CoordsInt coords)
    {
        return tileMapConnCoordIsLocked(coords, TileLockTest.Current_Pass);
    }

    public bool tileMapConnCoordIsLocked__ForAllPasses(CoordsInt coords)
    {
        return tileMapConnCoordIsLocked(coords, TileLockTest.All_Passes);
    }

    public bool coordsAreInsideTileMapBoundries(CoordsInt coords)
    {
        return tileMapConnections.isInsideBounds(coords);
    }

    public ref Double<TileTraveledToMarker, Tile> getTileMapConnElement(CoordsInt coords)
    {
        return ref tileMapConnections.getElement(coords);
    }

    public int getTileMapConnX()
    {
        return tileMapConnections.getXCount();
    }

    public int getTileMapConnY()
    {
        return tileMapConnections.getYCount();
    }

    public List<CoordsInt> getTileMapConnReducedCoordsList()
    {
        return tileMapConnections.getReducedCoordsList();
    }

    public void resetTileMapConnections()
    {
        this.tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>();
    }

    public void tileMapConnAddRefElement(CoordsInt newCoords, ref Double<TileTraveledToMarker, Tile> newEntry)
    {
        tileMapConnections.addRefElement(newCoords, ref newEntry);
    }

    public void attachTileMapConnToZone()
    {
        currentZone.setVeinZoneConnectionList(ref tileMapConnections);
    }

    public ref Tile getTileFromTileMapConn(CoordsInt coords)
    {
        return ref this.getTileMapConnElement(coords).getTwo();
    }

    public void incCurrentPassLock(CoordsInt coords)
    {
        getTileMapConnElement(coords).getOne().incLockPass(getCurrentVeinPass());
    }

    public void decCurrentPassLock(CoordsInt coords)
    {
        getTileMapConnElement(coords).getOne().decLockPass(getCurrentVeinPass());
    }
}
