using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using TileManagerClasses;
using CommonlyUsedClasses;

public partial class DebugControllerManager : MonoBehaviour
{
    // ==============================================================================================
    //                                      Vein Debug
    // ==============================================================================================
    int zoneIndex = 0;
    int zoneListCount;
    Zone_New selectedZone;
    Zone_New prevZone;
    public void getNextZone()
    {
        // Reset certain variables
        floodedFreeSpaceIdx = 0;

        // Get the next zone
        prevZone = selectedZone.deepCopy();
        selectedZone = generatorInst.getZoneContainer().getZone(zoneIndex);

        // Handle zone indexing
        zoneIndex++;
        if (zoneIndex >= zoneListCount)
            zoneIndex = 0;
    }

    bool toggleVeins = false;
    bool toggleVeinConnections = false;
    bool toggleVeinZone = false;

    public void selectVeins()
    {
        List<VeinBase> veinList = generatorInst.getVeinManager().getVeinList();
        toggleVeins = !toggleVeins;

        foreach (var vein in veinList)
        {
            //Debug.Log("Vein Id: " + vein.getId());

            var veinRef = vein;
            List<Tile> associatedTiles = veinRef.getAssociatedTiles();

            foreach (var tile in associatedTiles)
            {
                var tileRef = tile;
                if (toggleVeins)
                {
                    if (tileRef.getIsVeinMain() == false)
                        changeTileColor(ref tileRef, green);
                    else
                        changeTileColor(ref tileRef, darkGreen);
                }
                else
                    changeTileColor(ref tileRef, tileDefault);
            }
        }
    }

    public void selectVeinConnections()
    {
        List<VeinBase> veinList = generatorInst.getVeinManager().getVeinList();
        toggleVeinConnections = !toggleVeinConnections;

        foreach (var vein in veinList)
        {
            var veinRef = vein;
            List<VeinConnection> veinConnectors = veinRef.getVeinConnections();

            foreach (var conn in veinConnectors)
            {
                var tileRef = conn.getAssociatedTile();
                if (toggleVeinConnections)
                {
                    changeTileColor(ref tileRef, orange);
                }
                else
                    changeTileColor(ref tileRef, tileDefault);
            }
        }
    }

    public void changeDimGridColor(ref Zone_New zone, Color color)
    {
        DimensionList zoneDimList = zone.getVeinZoneDimList();
        zoneDimList.getGrid(out TwoDList<int> grid, out CoordsInt startCoords);

        //Debug.Log(zone.getId());
        //zoneDimList.printMinMax();

        for (int x = 0; x < grid.getXCount(); x++)
        {
            for (int y = 0; y < grid.getYCount(); y++)
            {
                if (grid.getElement(new CoordsInt(x, y)) == 1)
                {
                    CoordsInt tileCoords = new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                    Tile currentTile = tileManagerRef.tileAccessor.getTile(tileCoords, out bool accessSuccesful);

                    if (accessSuccesful == true)
                        changeTileColor(ref currentTile, color);
                }
            }
        }
    }

    public void changeDimConnectionColor(ref Zone_New zone, Color color)
    {
        TwoDList<Double<TileTraveledToMarker, Tile>> zoneConnTileMapRef = zone.getVeinZoneConnectionList();

        //Debug.Log(zone.getId());
        //zoneDimList.printMinMax();

        for (int x = 0; x < zoneConnTileMapRef.getXCount(); x++)
        {
            for (int y = 0; y < zoneConnTileMapRef.getYCount(x); y++)
            {
                CoordsInt connCoords = new CoordsInt(x, y);
                Tile currentTile = zoneConnTileMapRef.getElement(connCoords).getTwo();

                changeTileColor(ref currentTile, color);
            }
        }
    }

    public void selectVeinZoneDim()
    {
        toggleVeinZone = !toggleVeinZone;

        for (int i = 0; i < zoneListCount; i++)
        {
            Zone_New currentZone = generatorInst.getZoneContainer().getZone(i);

            // Highlight next grid
            if (toggleVeinZone == true)
            {
                changeDimGridColor(ref currentZone, lightGreen);
                changeDimConnectionColor(ref currentZone, darkGreen);
            }
            // Clear previous zone vein dim
            else
            {
                changeDimGridColor(ref currentZone, tileDefault);

            }

        }
    }


    int floodedFreeSpaceIdx = 0;
    bool firstClick = true;
    public void plotFloodedFreeSpaceDimList()
    {
        int blockLength = 1;
        List<CoordsInt> floodedFreeSpaceCoords = selectedZone.floodedFreeSpaces[floodedFreeSpaceIdx];

        // Unmark previous coords
        if (firstClick == false)
        {
            selectVeinZoneDim();
            selectVeins();
        }
        selectVeinZoneDim();
        selectVeins();

        foreach (var coords in floodedFreeSpaceCoords)
        {
            //coords.print("FLOODED: ");
            List<Tile> blockOfTiles = tileManagerRef.tileAccessor.getBlockOfTiles(coords, blockLength);

            for (int i = 0; i < blockOfTiles.Count; i++)
            {
                Tile tile = blockOfTiles[i];
                changeTileColor(ref tile, blue);
            }
        }

        floodedFreeSpaceIdx++;
        if (floodedFreeSpaceIdx >= selectedZone.floodedFreeSpaces.Count)
            floodedFreeSpaceIdx = 0;

        if (firstClick)
            firstClick = false;
    }
}
