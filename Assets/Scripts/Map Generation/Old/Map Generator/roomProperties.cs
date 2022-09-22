using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LockingClasses;
using Classes;
using Enums;
using PathingClasses;

public class roomProperties : MonoBehaviour
{
    // Room manager config properties
    public roomManagerScript.roomShape roomShape;
    public roomManagerScript.roomType roomType;

    public enum doorOrientation { North, East, South, West };

    public float width;
    public float height;
    public bool allRoomsUsed;
    public int totalDoorsUsed; // Used, but it's only updated at the very end of map generation

    public Coords gridRange;

    public int numDoors = 0;

    public List<Door> doorList = new List<Door>();

    // The rooms x and y position is based on the bottom left corner
    public int[,] grids;   // depricated!!!!!!!
    public List<List<int>> grids2;   // Bunch of 1s and 0s mapping the room out

    // ONLY FLUID ROOMS HAVE A DIMENSIONLIST
    public DimensionList dimList = new DimensionList();

    // Temp variables meant to be used during calculations
    public int tempInt = 0;


    // =================================================================
    //                  Properities needed to be saved
    // =================================================================
    public Coords gridCoords;
    public bool isFluid;
    public bool canAddDoors;

    public List<List<string>> innerGrid;  // In game layout in savable form
    public List<List<string>> innerGridTemp;  // Needed for editing innerGrid, mainly for door installation
    public List<List<string>> mapPiecesSave;  // Map pieces but in a savable form
    public List<List<GameObject>> mapPieces;  // All of the gameobject pieces for the actual map

    // Properties for map progression
    public bool isZoneEntrance = false; // Used for loading/deloading an entire zone?
    public int zoneTypeId = GlobalDefines.defaultId;
    public int zoneId = GlobalDefines.defaultId;

    public List<RoomLock> roomLocks = new List<RoomLock>(); // Room is locked by abilities or story

    // =================================================================

    // =================================================================
    //                  Properities needed for Progression
    // =================================================================
    public List<SubArea> parentSubAreaList = new List<SubArea>();
    // =================================================================



    public void addEmptyDoor(Coords coords, generalDirection orient)
    {
        doorList.Add(new Door(numDoors, coords, orient));
        numDoors++;
    }

    public void addEmptyDoor(int x, int y, generalDirection orient)
    {
        doorList.Add(new Door(numDoors, x, y, orient));
        numDoors++;
    }

    public void addNewDoor(GameObject adjacentRoom, bool doorUsedBool, bool doorLocked, generalDirection doorsOrient,
                    int linkedDoorId, int x, int y)
    {
        doorList.Add(new Door(numDoors, adjacentRoom, doorUsedBool, doorLocked, 
                    doorsOrient, linkedDoorId, x, y));
        numDoors++;
    }

    public bool getGridMap(int x, int y)
    {
        // The grids property is going to be accessed 
        if (this.grids2[x][y] == 1)
        {
            return true;
        } 
        else
        {
            return false;
        }
    }

    public void startInnerGridChange()
    {
        innerGridTemp = new List<List<string>>();

        // Record what's originally there
        for (int x = 0; x < innerGrid.Count; x++)
        {
            List<string> temp = new List<string>();
            for (int y = 0; y < innerGrid[x].Count; y++)
            {
                temp.Add(innerGrid[x][y]);
            }
            innerGridTemp.Add(temp);
        }
    }

    public void editInnerGridChange(int x, int y, string val)
    {
        // Record changes
        innerGridTemp[x][y] = val;
    }

    public void stopInnerGridChange()
    {
        innerGrid = new List<List<string>>();

        // Record what's originally there
        for (int x = 0; x < innerGridTemp.Count; x++)
        {
            List<string> temp = new List<string>();
            for (int y = 0; y < innerGridTemp[x].Count; y++)
            {
                temp.Add(innerGridTemp[x][y]);
            }
            innerGrid.Add(temp);
        }
    }

    public int getZoneId()
    {
        return this.zoneId;
    }

    public void setZoneId(int zoneId)
    {
        this.zoneId = zoneId;
    }

    public int getZoneTypeId()
    {
        return this.zoneTypeId;
    }

    public void setZoneTypeId(int zoneTypeId)
    {
        this.zoneTypeId = zoneTypeId;
    }

    public void setParentSubArea(SubArea subArea)
    {
        this.parentSubAreaList.Add(subArea);
    }

    public List<SubArea> getParentSubArea()
    {
        return parentSubAreaList;
    }

    public List<Door> getListOfUsedDoors()
    {
        List<Door> listOfUsedDoors = new List<Door>();
        for (int i = 0; i < doorList.Count; i++)
        {
            if (doorList[i].doorUsedBool == true)
                listOfUsedDoors.Add(doorList[i]);
        }
        return listOfUsedDoors;
    }

    public void addRoomLock(RoomLock newLock)
    {
        //Debug.Log("LOOOOOOOOCKED");
        roomLocks.Add(newLock);
    }

    public List<RoomLock> getRoomLocks()
    {
        return roomLocks;
    }

    public bool roomIsLocked()
    {
        bool roomIsLocked = false;
        for (int i = 0; i < roomLocks.Count; i++)
        {
            if (roomLocks[i].isUsed)
            {
                roomIsLocked = true;
                break;
            }
        }

        return roomIsLocked;
    }
}
