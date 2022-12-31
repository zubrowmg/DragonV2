using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using TileManagerClasses;
using CommonlyUsedClasses;

public partial class DebugControllerManager : MonoBehaviour
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




    // ==============================================================================================
    //                                      Save Temp Buttons (SHOULD MOVE THIS TO ANOTHER DEBUG CONTROLLER FILE)
    // ==============================================================================================

    int tempIndex = 0;
    public void tempButtonPlotAllFreeSpaces()
    {
        // This plotted each free space that was determined via world coords. Which was depricated to use Tile Map Connection Points instead
        for (int zoneIdx = 0; zoneIdx < zoneListCount; zoneIdx++)
        {
            Zone_New zone = generatorInst.getZoneContainer().getZone(zoneIdx);

            if (tempIndex < zone.freeSpaces.Count)
            {
                DimensionList dimlist = zone.freeSpaces[tempIndex];
                dimlist.getGrid(out TwoDList<int> grid, out CoordsInt startCoords);

                List<CoordsInt> freeSpaceCoords = dimlist.getAllSelectedGridCoords();
                Debug.Log("===============================================================================");
                dimlist.getCenterCoord().print("CENTER COORDS: ");
                dimlist.printMinMax("");
                //dimlist.printGrid(true);
                dimlist.updateCenterCoord();
                //Debug.Log("X AXIS: " + grid.getXCount() + "Y AXIS: " + grid.getYCount());

                //Debug.Log("FREE SPACE COUNT: " + zone.freeSpaces.Count + "\nCOORDS IN SPACE: " + zone.freeSpaces[tempIndex].Count);
                if (tempIndex != 0)
                {
                    List<CoordsInt> prevFreeSpaceCoords = zone.freeSpaces[tempIndex - 1].getAllSelectedGridCoords();

                    foreach (var coord in prevFreeSpaceCoords)
                    {
                        //coord.print("\tIDK: ");
                        Tile currentTile = tileManagerRef.tileAccessor.getTile(coord, out bool accessSuccesful);

                        if (accessSuccesful == true)
                            changeTileColor(ref currentTile, purple);
                    }
                }

                foreach (var coord in freeSpaceCoords)
                {
                    //coord.print("\tIDK: ");
                    Tile currentTile = tileManagerRef.tileAccessor.getTile(coord, out bool accessSuccesful);

                    if (accessSuccesful == true)
                        changeTileColor(ref currentTile, black);
                }

                
            }
        }
        tempIndex++;
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
