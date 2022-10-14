using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using TileManagerClasses;
using CommonlyUsedClasses;

public class DebugControllerManager : MonoBehaviour
{
    GeneratorWrapper generatorInst;

    TileManager tileManagerRef;

    // Colors
    Color purple      = new Color(.29f, .025f, .76f, .5f);
    Color orange      = new Color(1f, .5f, .0f, .85f);
    Color red         = new Color(.9725f, 0f, .0412f, .76f);

    Color green       = new Color(.085f, .85f, .12f, .88f);
    Color lightGreen  = new Color(.575f, .8773f, .6497f, .78f);
    Color darkGreen   = new Color(.07f, .51f, .07f, .50f);

    Color blue        = new Color(0f, .56f, .87f, 1f);
    Color white       = new Color(1f, 1f, 1f, 1f);
    Color black       = new Color(0f, 0f, 0f, 1f);
    Color tileDefault = new Color(255f, 255f, 255f, .27f);


    public void init(ref GeneratorWrapper generatorInst)
    {
        this.generatorInst = generatorInst;

        zoneListCount = generatorInst.getZoneContainer().getZoneListCount();
        tileManagerRef = generatorInst.getTileManager();

        selectedZone = generatorInst.getZoneContainer().getZone(0);
        prevZone = generatorInst.getZoneContainer().getZone(zoneListCount - 1);
    }


    int zoneIndex = 0;
    int zoneListCount;
    Zone_New selectedZone;
    Zone_New prevZone;
    void getNextZone()
    {
        prevZone = selectedZone.deepCopy();
        selectedZone = generatorInst.getZoneContainer().getZone(zoneIndex);

        zoneIndex++;
        if (zoneIndex >= zoneListCount)
            zoneIndex = 0;
    }

    // ==============================================================================================
    //                                      Vein Debug
    // ==============================================================================================
    bool toggleVeins = false;
    bool toggleVeinConnections = false;

    public void selectVeins()
    {
        List<VeinBase> veinList = generatorInst.getVeinManager().getVeinList();
        toggleVeins = !toggleVeins;

        foreach (var vein in veinList)
        {
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
        List<List<int>> grid;
        Coords<int> startCoords;
        zoneDimList.getGrid(out grid, out startCoords);

        Debug.Log(zone.getId());
        zoneDimList.printMinMax();

        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[0].Count; y++)
            {
                if (grid[x][y] == 1)
                {
                    bool accessSuccesful = false;
                    Coords<int> tileCoords = new Coords<int>(x + startCoords.getX(), y + startCoords.getY());
                    Tile currentTile = tileManagerRef.tileAccessor.getTile(tileCoords, ref accessSuccesful);

                    if (accessSuccesful == true)
                        changeTileColor(ref currentTile, color);
                }
            }
        }
    }

    public void selectVeinZoneDim()
    {
        // Clear previous zone vein dim
        changeDimGridColor(ref prevZone, tileDefault);

        // Highlight next grid
        changeDimGridColor(ref selectedZone, lightGreen);

        getNextZone();
    }

    public void clearGrid()
    {
        //int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        //int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);


        //for (int x = 0; x < xMax; x++)
        //{
        //    for (int y = 0; y < yMax; y++)
        //    {
        //        gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
        //    }
        //}
        //gridIsOccupiedSelect = false;
        //gridDoorSelect = false;
        //gridZoneSelect = 0;
    }

    // ==============================================================================================
    //                                      Tile Debug
    // ==============================================================================================

    void changeRoomColor(ref GameObject room, Color color)
    {
        roomProperties roomProps = room.GetComponent<roomProperties>();

        if (roomProps.isFluid)
        {
            for (int i = 0; i < roomProps.mapPieces.Count; i++)
            {
                foreach (var piece in roomProps.mapPieces[i])
                {
                    piece.GetComponent<SpriteRenderer>().color = color;
                }
            }
        }
        else
        {
            room.GetComponent<SpriteRenderer>().color = color;
        }
    }

    void changeDoorColor(ref GameObject door, Color color)
    {
        door.GetComponent<SpriteRenderer>().color = color;
    }

    void changeTileColor(ref Tile tile, Color color)
    {
        tile.getTileGameObject().GetComponent<SpriteRenderer>().color = color;
    }
}
