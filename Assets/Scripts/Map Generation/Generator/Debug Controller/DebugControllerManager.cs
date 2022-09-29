using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using TileManagerClasses;

public class DebugControllerManager : MonoBehaviour
{
    GeneratorWrapper generatorInst;

    // Colors
    Color purple      = new Color(.29f, .025f, .76f, .5f);
    Color red         = new Color(.9725f, 0f, .0412f, .76f);
    Color green       = new Color(.085f, .85f, .12f, .88f);
    Color darkGreen   = new Color(.07f, .51f, .07f, .50f);
    Color blue        = new Color(0f, .56f, .87f, 1f);
    Color white       = new Color(1f, 1f, 1f, 1f);
    Color black       = new Color(0f, 0f, 0f, 1f);
    Color tileDefault = new Color(255f, 255f, 255f, .27f);

    // Button Toggles
    bool toggleVeins = false;


    public void init(ref GeneratorWrapper generatorInst)
    {
        this.generatorInst = generatorInst;
    }

    public void selectVeins()
    {
        List<Vein> veinList = generatorInst.getVeinManager().getVeinList();
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
