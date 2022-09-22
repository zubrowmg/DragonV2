using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Classes;
using Enums;

public class roomManagerScript : MonoBehaviour
{
    const int numDoors_3 = 3;
    const int numDoors_4 = 4;
    const int numDoors_6 = 6;
    const int numDoors_8 = 8;

    public GameObject MapPlain;
    public GameObject MapWall;
    public GameObject MapOuterCorner;
    public GameObject MapInnerCorner;

    // This script is used to initialize rooms, when created

    public enum roomShape { Rectangular, Unique, Fluid };
    public enum roomType { StartRoom, SendoffRoom, LargeRoom, SmallRoom, LRoom_0, Fluid };

    public void configureNewBossRoom(ref GameObject newRoom, RoomPreset bossRoomLocation, float gridUnitLength, ref DimensionList dimensionList)
    {
        populateBossRoomDimensions(ref newRoom, ref dimensionList, bossRoomLocation);

        roomProperties roomProps = newRoom.GetComponent<roomProperties>();
        roomProps.roomShape = roomShape.Fluid;
        roomProps.roomType = roomType.Fluid;

        roomProps.isFluid = true;
        roomProps.canAddDoors = false;

        roomProps.width = (dimensionList.xMax - dimensionList.xMin + 1) * gridUnitLength;
        roomProps.height = (dimensionList.yMax - dimensionList.yMin + 1) * gridUnitLength;

        roomProps.gridRange = new Coords(dimensionList.xMax - dimensionList.xMin + 1, dimensionList.yMax - dimensionList.yMin + 1);
        roomProps.gridCoords = new Coords(dimensionList.xMin, dimensionList.yMin);
        roomProps.grids2 = dimensionList.grid;

        populateInnerGridAndMapGrid(ref newRoom, gridUnitLength);


    }

    
    public void configureNewFluidRoom(ref GameObject newRoom, float gridUnitLength, DimensionList dimensionList)
    {
        roomProperties roomProps = newRoom.GetComponent<roomProperties>();
        roomProps.dimList = dimensionList;
        roomProps.roomShape = roomShape.Fluid;
        roomProps.roomType = roomType.Fluid;

        roomProps.isFluid = true;
        roomProps.canAddDoors = true;

        roomProps.width = (dimensionList.xMax - dimensionList.xMin + 1) * gridUnitLength;
        roomProps.height = (dimensionList.yMax - dimensionList.yMin + 1) * gridUnitLength;

        roomProps.gridRange = new Coords(dimensionList.xMax - dimensionList.xMin + 1, dimensionList.yMax - dimensionList.yMin + 1);
        roomProps.gridCoords = new Coords(dimensionList.xMin, dimensionList.yMin);
        roomProps.grids2 = dimensionList.grid;

        populateInnerGridAndMapGrid(ref newRoom, gridUnitLength);
        
        // Delete all other room properties
    }

    public void configureNewRoom(ref GameObject room, float gridUnitLength)
    {
        roomShape shape = room.GetComponent<roomProperties>().roomShape;
        roomType type = room.GetComponent<roomProperties>().roomType;

        // General config for all rooms
        room.GetComponent<roomProperties>().isFluid = false;
        room.GetComponent<roomProperties>().canAddDoors = false;
        room.GetComponent<roomProperties>().width = room.GetComponent<SpriteRenderer>().bounds.size.x;
        room.GetComponent<roomProperties>().height = room.GetComponent<SpriteRenderer>().bounds.size.y;
        room.GetComponent<roomProperties>().allRoomsUsed = false;
        room.GetComponent<roomProperties>().totalDoorsUsed = 0;
        room.GetComponent<roomProperties>().gridCoords = new Coords(0, 0);

        room.GetComponent<roomProperties>().innerGrid = new List<List<string>>();

        // Specific configs
        if (shape == roomShape.Rectangular)
        {
            configureRectangularRoom(ref room, ref type);
        }
        else
        {
            configureUniqueRoom(ref room, ref type);
        }

        populateGrids2(ref room);
        populateInnerGridAndMapGrid(ref room, gridUnitLength);
        //changeRoomGrids(ref room);

    }

    void populateGrids2(ref GameObject room)
    {
        List<List<int>> grid2 = new List<List<int>>();
        List<int> temp = new List<int>();
        roomProperties currentRoomProperties = room.GetComponent<roomProperties>();

        int xMax = currentRoomProperties.grids.GetLength(0);
        int yMax = currentRoomProperties.grids.GetLength(1);
        
        for (int x = 0; x < xMax; x++)
        {
            temp = new List<int>();

            for (int y = 0; y < yMax; y++)
            {
                if (currentRoomProperties.grids[x,y] == 1)
                {
                    temp.Add(1);
                }
                else
                {
                    temp.Add(0);
                }
            }

            grid2.Add(temp);
        }

        currentRoomProperties.grids2 = grid2;
    }

    void populateBossRoomDimensions(ref GameObject newRoom, ref DimensionList dimensionList, RoomPreset bossRoomLocation)
    {
        List<Door> doorList = new List<Door>();

        int xMin = 0; int yMin = 0;
        int xMax = 0; int yMax = 0;
        int xCore = 0;
        int yCore = 0;

        if (bossRoomLocation.bossRoomId == "B0")
        {
            xMin = bossRoomLocation.coords.x; yMin = bossRoomLocation.coords.y;
            xMax = bossRoomLocation.coords.x + 16; yMax = bossRoomLocation.coords.y + 15;
            xCore = (xMax - xMin) / 2;
            yCore = (yMax - yMin) / 2;
            dimensionList.addDimension(new SquareArea(xMin, yMin, xMax, yMax, xCore, yCore));

            int doorId = 0; // Just a placeholder
            doorList.Add(new Door(doorId, 1, 0, generalDirection.South));
            doorList.Add(new Door(doorId, 4, 0, generalDirection.South));
            doorList.Add(new Door(doorId, 11, 0, generalDirection.South));
            doorList.Add(new Door(doorId, 16, 2, generalDirection.East));

            applyDoorsToRoom(ref newRoom, doorList, bossRoomLocation, ref dimensionList, xMin, xMax);
        }
    }

    void applyDoorsToRoom(ref GameObject newRoom, List<Door> doorList, RoomPreset bossRoomLocation, ref DimensionList dimensionList, int xMin, int xMax)
    {
        roomProperties roomProps = newRoom.GetComponent<roomProperties>();

        if (bossRoomLocation.presetDirection == generalDirectionTwo.Left)
        {
            // Adjust the room position
            dimensionList.xMax = dimensionList.xMax - (xMax - xMin);
            dimensionList.xMin = dimensionList.xMin - (xMax - xMin);
        }

        for (int i = 0; i < doorList.Count; i++)
        {
            int newX = doorList[i].doorCoords.x;
            int newY = doorList[i].doorCoords.y;
            generalDirection newOrient = doorList[i].doorsOrient;

            // Need to reverse it if the boss room is to the left
            if (bossRoomLocation.presetDirection == generalDirectionTwo.Left)
            {
                // Adjust where the doors go
                newX = (dimensionList.xMax - dimensionList.xMin) - doorList[i].doorCoords.x;

                if (doorList[i].doorsOrient == generalDirection.East) newOrient = generalDirection.West;
                else if (doorList[i].doorsOrient == generalDirection.West) newOrient = generalDirection.East;
            }

            roomProps.addEmptyDoor(newX, newY, newOrient);
        }
    }

    void populateInnerGridAndMapGrid(ref GameObject room, float gridSize)
    {
        // Shift grid so that we can ready from it normally, x=0 to xmax and y=0 to ymax
        List<List<string>> innerGrid = new List<List<string>>();
        List<string> tempInnerGrid = new List<string>();

        // Map Piece Saver
        List<List<string>> mapPiecesSave = new List<List<string>>();
        List<string> tempMapPiecesSave = new List<string>();

        // Map Pieces
        List<List<GameObject>> mapPieces = new List<List<GameObject>>();
        List<GameObject> tempMapPieces = new List<GameObject>();
        roomProperties currentRoomProperties = room.GetComponent<roomProperties>();
        int xMax = currentRoomProperties.grids2.Count;
        int yMax = currentRoomProperties.grids2[0].Count;

        GameObject tempMP = null;
        float newXPos = 0f;
        float newYPos = 0f;

        bool fluidRoom = currentRoomProperties.isFluid;

        for (int x = 0; x < xMax; x++)
        {
            tempInnerGrid = new List<string>();
            tempMapPiecesSave = new List<string>();
            tempMapPieces = new List<GameObject>();
            //str = "";

            for (int y = 0; y < yMax; y++)
            {
                int xPlus = x + 1;
                int xMinus = x - 1;
                int yPlus = y + 1;
                int yMinus = y - 1;

                //Debug.Log(x + "," + y);

                // Check Corner Pieces
                if (x == 0 && y == 0 && currentRoomProperties.grids2[x][y] == 1)
                {
                    tempInnerGrid.Add("ICSW");
                    tempMapPiecesSave.Add("ICSW");

                    if (fluidRoom) instInnerSWCorner(ref tempMP);
                }
                else if (x == 0 && y == yMax -1 && currentRoomProperties.grids2[x][y] == 1)
                {
                    tempInnerGrid.Add("ICNW");
                    tempMapPiecesSave.Add("ICNW");

                    if (fluidRoom) instInnerNWCorner(ref tempMP);
                }
                else if (x == xMax - 1 && y == 0 && currentRoomProperties.grids2[x][y] == 1)
                {
                    tempInnerGrid.Add("ICSE");
                    tempMapPiecesSave.Add("ICSE");

                    if (fluidRoom) instInnerSECorner(ref tempMP);
                }
                else if (x == xMax-1 && y == yMax - 1 && currentRoomProperties.grids2[x][y] == 1)
                {
                    tempInnerGrid.Add("ICNE");
                    tempMapPiecesSave.Add("ICNE");

                    if (fluidRoom) instInnerNECorner(ref tempMP);
                }
                // Check Edges
                else if (y == 0 && currentRoomProperties.grids2[x][y] == 1)
                {
                    // South Edge
                    if (checkInnerSECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSE");
                        tempMapPiecesSave.Add("ICSE");

                        if (fluidRoom) instInnerSECorner(ref tempMP);
                    }
                    else if (checkInnerSWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSW");
                        tempMapPiecesSave.Add("ICSW");

                        if (fluidRoom) instInnerSWCorner(ref tempMP);
                    }
                    else if(checkSouthWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WS");
                        tempMapPiecesSave.Add("WS");

                        if (fluidRoom) instSouthWall(ref tempMP);
                    }
                }
                else if (y+1 == yMax && currentRoomProperties.grids2[x][y] == 1)
                {
                    // North Edge
                    if (checkInnerNECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNE");
                        tempMapPiecesSave.Add("ICNE");

                        if (fluidRoom) instInnerNECorner(ref tempMP);
                    }
                    else if (checkInnerNWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNW");
                        tempMapPiecesSave.Add("ICNW");

                        if (fluidRoom) instInnerNWCorner(ref tempMP);
                    }
                    else if (checkNorthWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WN");
                        tempMapPiecesSave.Add("WN");

                        if (fluidRoom) instNorthWall(ref tempMP);
                    }
                }
                else if (x == 0 && currentRoomProperties.grids2[x][y] == 1)
                {
                    // West Edge
                    if (checkInnerNWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNW");
                        tempMapPiecesSave.Add("ICNW");

                        if (fluidRoom) instInnerNWCorner(ref tempMP);
                    }
                    else if (checkInnerSWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSW");
                        tempMapPiecesSave.Add("ICSW");

                        if (fluidRoom) instInnerSWCorner(ref tempMP);
                    }
                    else if (checkWestWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WW");
                        tempMapPiecesSave.Add("WW");

                        if (fluidRoom) instWestWall(ref tempMP);
                    }
                }
                else if (x+1 == xMax && currentRoomProperties.grids2[x][y] == 1)
                {
                    // East Edge
                    if (checkInnerNECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNE");
                        tempMapPiecesSave.Add("ICNE");

                        if (fluidRoom) instInnerNECorner(ref tempMP);
                    }
                    else if (checkInnerSECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSE");
                        tempMapPiecesSave.Add("ICSE");

                        if (fluidRoom) instInnerSECorner(ref tempMP);
                    }
                    else if (checkEastWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WE");
                        tempMapPiecesSave.Add("WE");

                        if (fluidRoom) instEastWall(ref tempMP);
                    }
                }
                // Non Edges and Non Corners
                else if (currentRoomProperties.grids2[x][y] == 1)
                {
                    // Inner Corners
                    if (checkInnerNECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNE");
                        tempMapPiecesSave.Add("ICNE");

                        if (fluidRoom) instInnerNECorner(ref tempMP);
                    }
                    else if (checkInnerSECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSE");
                        tempMapPiecesSave.Add("ICSE");

                        if (fluidRoom) instInnerSECorner(ref tempMP);
                    }
                    else if (checkInnerNWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICNW");
                        tempMapPiecesSave.Add("ICNW");

                        if (fluidRoom) instInnerNWCorner(ref tempMP);
                    }
                    else if (checkInnerSWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("ICSW");
                        tempMapPiecesSave.Add("ICSW");

                        if (fluidRoom) instInnerSWCorner(ref tempMP);
                    }

                    // Outer Corners
                    else if (checkOuterNECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("OCNE");
                        tempMapPiecesSave.Add("OCNE");

                        if (fluidRoom) instOuterNECorner(ref tempMP);
                    }
                    else if (checkOuterNWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("OCNW");
                        tempMapPiecesSave.Add("OCNW");

                        if (fluidRoom) instOuterNWCorner(ref tempMP);
                    }
                    else if (checkOuterSECorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("OCSE");
                        tempMapPiecesSave.Add("OCSE");

                        if (fluidRoom) instOuterSECorner(ref tempMP);
                    }
                    else if (checkOuterSWCorner(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("OCSW");
                        tempMapPiecesSave.Add("OCSW");

                        if (fluidRoom) instOuterSWCorner(ref tempMP);
                    }

                    // Normal Walls
                    else if (isNotWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("0");
                        tempMapPiecesSave.Add("P");

                        if (fluidRoom) instPlainWall(ref tempMP);
                    }
                    else if(checkNorthWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WN");
                        tempMapPiecesSave.Add("WN");

                        if (fluidRoom) instNorthWall(ref tempMP);
                    }
                    else if (checkSouthWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WS");
                        tempMapPiecesSave.Add("WS");

                        if (fluidRoom) instSouthWall(ref tempMP);
                    }
                    else if (checkEastWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WE");
                        tempMapPiecesSave.Add("WE");

                        if (fluidRoom) instEastWall(ref tempMP);
                    }
                    else if (checkWestWall(currentRoomProperties.grids2, x, y))
                    {
                        tempInnerGrid.Add("WW");
                        tempMapPiecesSave.Add("WW");

                        if (fluidRoom) instWestWall(ref tempMP);
                    }
                }
                else
                {
                    tempInnerGrid.Add("0");
                    tempMapPiecesSave.Add("0");

                    tempMP = null;
                }


                if (tempMP != null && fluidRoom)
                {
                    // Put the pieces in the "Pieces" gameobject under fluid rooms
                    tempMP.transform.parent = room.transform.GetChild(0).transform;

                    //         Shift position   recenter to 0,0    shift back         
                    newXPos = (gridSize * x) + (gridSize / 2) - ((xMax * gridSize) / 2);
                    newYPos = (gridSize * y) + (gridSize / 2) - ((yMax * gridSize) / 2);
                    tempMP.transform.localPosition = new Vector3(newXPos, newYPos, 0);
                    tempMapPieces.Add(tempMP);
                }
            }
            //for (int k = 0; k < temp.Count; k++)
            //{
            //    str = str + "," + temp[k];
            //}
            //Debug.Log(str);
            innerGrid.Add(tempInnerGrid);
            mapPiecesSave.Add(tempMapPiecesSave);
            if (fluidRoom) mapPieces.Add(tempMapPieces);
        }

        if (room.name == "Room28")
        {
            string str = "";
            for (int x = 0; x < innerGrid.Count; x++)
            {
                str = "";

                for (int y = 0; y < innerGrid[x].Count; y++)
                {
                    str = str + "," + innerGrid[x][y];
                }

                Debug.Log(str);
            }
        }

        currentRoomProperties.innerGrid = innerGrid;
        currentRoomProperties.mapPiecesSave = mapPiecesSave;
        if (fluidRoom) currentRoomProperties.mapPieces = mapPieces;

    }

    void changeRoomGrids(ref GameObject room)
    {
        // Shift grid so that we can ready from it normally, x=0 to xmax and y=0 to ymax
        List<List<int>> newGrid = new List<List<int>>();
        List<int> temp = new List<int>();

        string str = "";

        for (int x = 0; x < room.GetComponent<roomProperties>().grids.GetLength(0); x++)
        {
            temp = new List<int>();
            str = "";
            for (int y = 0; y < room.GetComponent<roomProperties>().grids.GetLength(1); y++)
            {
                str = str + "," + room.GetComponent<roomProperties>().grids[x, y].ToString();
                temp.Add(room.GetComponent<roomProperties>().grids[x, y]);
            }
            Debug.Log(str);
            newGrid.Add(temp);
        }
    }

    void configureUniqueRoom(ref GameObject room, ref roomType type)
    {
        switch (type)
        {
            case roomType.SendoffRoom:
                roomProperties props = room.GetComponent<roomProperties>();

                props.gridRange = new Coords(48, 12);
                //props.numDoors = numDoors_6;

                GameObject adjacentRoom = null; int linkedDoorId = -1;
                bool doorUsed = false; bool doorLocked = true;

                //room.GetComponent<roomProperties>().doorCoords = new List<Coords>();
                props.doorList = new List<Door>();
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.East, linkedDoorId, 5, 8);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.North, linkedDoorId, 23, 5);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.West, linkedDoorId, 42, 8);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.East, linkedDoorId, 47, 5);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.South, linkedDoorId, 23, 0);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.West, linkedDoorId, 0, 5);

                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(5,8));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(23,5));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(42,8));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(47,5));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(23,0));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(0,5));


                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.East);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.North);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.West);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.East);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.South);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.West);

                //for (int i = 0; i < numDoors_6; i++)
                //{
                //    room.GetComponent<roomProperties>().adjacentRooms.Add(null);
                //    room.GetComponent<roomProperties>().doorUsedBool.Add(false);
                //    room.GetComponent<roomProperties>().linkedDoorId.Add(-1);
                //}
                
                room.GetComponent<roomProperties>().grids = new int[48, 12]
                    { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };


                break;
            /*case roomType.LRoom_0:
                //room.GetComponent<roomProperties>().rotate = canRotate.Yes;
                room.GetComponent<roomProperties>().gridXRange = 12;
                room.GetComponent<roomProperties>().gridYRange = 12;
                room.GetComponent<roomProperties>().numDoors = numDoors_8;

                room.GetComponent<roomProperties>().doorCoords = new int[numDoors_8] { 2,  5, 8, 11, 8, 2, 0, 0 };
                room.GetComponent<roomProperties>().doorYPos = new int[numDoors_8] { 11, 8, 5, 2, 0, 0, 2, 8 };
                room.GetComponent<roomProperties>().adjacentRooms = new GameObject[numDoors_8] { null, null, null, null, null, null, null, null };
                room.GetComponent<roomProperties>().doorUsedBool = new bool[numDoors_8] { false, false, false, false, false, false, false, false };
                room.GetComponent<roomProperties>().doorsOrient = new roomProperties.doorOrientation[numDoors_8]
                    { roomProperties.doorOrientation.North, roomProperties.doorOrientation.East,
                      roomProperties.doorOrientation.North,  roomProperties.doorOrientation.East,
                      roomProperties.doorOrientation.South, roomProperties.doorOrientation.South,
                      roomProperties.doorOrientation.West,  roomProperties.doorOrientation.West };

                room.GetComponent<roomProperties>().grids = new int[12, 12]
                    { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
                      { 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },};


                break;
                */
        }
    }

    void configureRectangularRoom(ref GameObject room, ref roomType type)
    {
        switch(type)
        {
            case roomType.StartRoom:

                roomProperties props = room.GetComponent<roomProperties>();

                props.gridRange = new Coords(36, 24);


                GameObject adjacentRoom = null; int linkedDoorId = -1;
                bool doorUsed = false; bool doorLocked = true;

                props.doorList = new List<Door>();
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.East, linkedDoorId, 35, 2);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.South, linkedDoorId, 17, 0);
                props.addNewDoor(adjacentRoom, doorUsed, doorLocked, generalDirection.West, linkedDoorId, 0, 2);


                //room.GetComponent<roomProperties>().numDoors = numDoors_3;

                //room.GetComponent<roomProperties>().doorCoords = new List<Coords>();
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(35,2));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(17,0));
                //room.GetComponent<roomProperties>().doorCoords.Add(new Coords(0,2));


                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.East);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.South);
                //room.GetComponent<roomProperties>().doorsOrient.Add(roomProperties.doorOrientation.West);

                //for (int i = 0; i < numDoors_3; i++)
                //{
                //    room.GetComponent<roomProperties>().adjacentRooms.Add(null);
                //    room.GetComponent<roomProperties>().doorUsedBool.Add(false);
                //    room.GetComponent<roomProperties>().linkedDoorId.Add(-1);
                //}

                room.GetComponent<roomProperties>().grids = new int[36, 24]
                    { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };


                break;
            /*case roomType.LargeRoom:
                //room.GetComponent<roomProperties>().rotate = canRotate.No;
                room.GetComponent<roomProperties>().gridXRange = 12;
                room.GetComponent<roomProperties>().gridYRange = 12;
                room.GetComponent<roomProperties>().numDoors = numDoors_8;

                room.GetComponent<roomProperties>().doorCoords = new int[numDoors_8] {  2,  8, 11, 11, 8, 2, 0, 0 };
                room.GetComponent<roomProperties>().doorYPos = new int[numDoors_8] { 11, 11,  8,  2, 0, 0, 2, 8 };
                room.GetComponent<roomProperties>().adjacentRooms = new GameObject[numDoors_8] { null, null, null, null, null, null, null, null };
                room.GetComponent<roomProperties>().doorUsedBool = new bool[numDoors_8] { false, false, false, false, false, false, false, false };
                room.GetComponent<roomProperties>().doorsOrient = new roomProperties.doorOrientation[numDoors_8] 
                    { roomProperties.doorOrientation.North, roomProperties.doorOrientation.North,
                      roomProperties.doorOrientation.East,  roomProperties.doorOrientation.East,
                      roomProperties.doorOrientation.South, roomProperties.doorOrientation.South,
                      roomProperties.doorOrientation.West,  roomProperties.doorOrientation.West };

                room.GetComponent<roomProperties>().grids = new int[12, 12]
                    { { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 } };


                break;
            case roomType.SmallRoom:
                //room.GetComponent<roomProperties>().rotate = canRotate.No;
                room.GetComponent<roomProperties>().gridXRange = 6;
                room.GetComponent<roomProperties>().gridYRange = 6;
                room.GetComponent<roomProperties>().numDoors = numDoors_4;

                room.GetComponent<roomProperties>().doorCoords = new int[numDoors_4] { 2, 5, 2, 0 };
                room.GetComponent<roomProperties>().doorYPos = new int[numDoors_4] { 5, 2, 0, 2 };
                room.GetComponent<roomProperties>().adjacentRooms = new GameObject[numDoors_4] { null, null, null, null };
                room.GetComponent<roomProperties>().doorUsedBool = new bool[numDoors_4] { false, false, false, false };
                room.GetComponent<roomProperties>().doorsOrient = new roomProperties.doorOrientation[numDoors_4]
                    { roomProperties.doorOrientation.North,
                      roomProperties.doorOrientation.East,
                      roomProperties.doorOrientation.South,
                      roomProperties.doorOrientation.West };

                room.GetComponent<roomProperties>().grids = new int[6, 6]
                    { { 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1 },
                      { 1, 1, 1, 1, 1, 1 } };
                break;
                */
        }
    }


    // ------------------------------------------------------------------------------------------------------
    //                              Map Piece GameObject Instantiation
    // ------------------------------------------------------------------------------------------------------
    void instPlainWall(ref GameObject piece)
    {
        piece = Instantiate(MapPlain, new Vector3(0, 0, 0), Quaternion.identity);
    }

    void instNorthWall(ref GameObject piece)
    {
        piece = Instantiate(MapWall, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, -90f, Space.Self);
    }
    void instSouthWall(ref GameObject piece)
    {
        piece = Instantiate(MapWall, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 90f, Space.Self);
    }
    void instEastWall(ref GameObject piece)
    {
        piece = Instantiate(MapWall, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 180f, Space.Self);
    }
    void instWestWall(ref GameObject piece)
    {
        piece = Instantiate(MapWall, new Vector3(0, 0, 0), Quaternion.identity);
    }

    void instOuterNECorner(ref GameObject piece)
    {
        piece = Instantiate(MapOuterCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 90f, Space.Self);
    }
    void instOuterNWCorner(ref GameObject piece)
    {
        piece = Instantiate(MapOuterCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 180f, Space.Self);
    }
    void instOuterSECorner(ref GameObject piece)
    {
        piece = Instantiate(MapOuterCorner, new Vector3(0, 0, 0), Quaternion.identity);
    }
    void instOuterSWCorner(ref GameObject piece)
    {
        piece = Instantiate(MapOuterCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, -90f, Space.Self);
    }

    void instInnerNECorner(ref GameObject piece)
    {
        piece = Instantiate(MapInnerCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 270f, Space.Self);
    }
    void instInnerNWCorner(ref GameObject piece)
    {
        piece = Instantiate(MapInnerCorner, new Vector3(0, 0, 0), Quaternion.identity);
    }
    void instInnerSECorner(ref GameObject piece)
    {
        piece = Instantiate(MapInnerCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 180f, Space.Self);
    }
    void instInnerSWCorner(ref GameObject piece)
    {
        piece = Instantiate(MapInnerCorner, new Vector3(0, 0, 0), Quaternion.identity);
        piece.transform.Rotate(0f, 0f, 90f, Space.Self);
    }

    // ------------------------------------------------------------------------------------------------------
    //                                        Grid Checking
    // ------------------------------------------------------------------------------------------------------
    bool isNotWall(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("NOT WALL");

        int yMax = grids2[0].Count;
        if (grids2[x][y - 1] == 1 && grids2[x][y + 1] == 1 && grids2[x - 1][y] == 1 && grids2[x + 1][y] == 1)
        {
            check = true;
        }
        return check;
    }

    bool checkNorthWall(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("N");

        int yMax = grids2[0].Count;
        if (y + 1 == yMax)
        {
            if (grids2[x][y - 1] == 1)
            {
                check = true;
            }
        }
        else if (grids2[x][y - 1] == 1 && grids2[x][y + 1] == 0)
        {
            check = true;
        }
        return check;
    }
    bool checkSouthWall(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("S");

        if (y == 0)
        {
            if (grids2[x][y + 1] == 1)
            {
                check = true;
            }
        }
        else if (grids2[x][y + 1] == 1 && grids2[x][y - 1] == 0)
        {
            check = true;
        }
        return check;
    }
    bool checkEastWall(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("E");

        int xMax = grids2.Count;
        if (x + 1 == xMax)
        {
            if (grids2[x - 1][y] == 1)
            {
                check = true;
            }
        }
        else if (grids2[x - 1][y] == 1)
        {
            check = true;
        }
        return check;
    }
    bool checkWestWall(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("W");

        if (x == 0)
        {
            if (grids2[x + 1][y] == 1)
            {
                check = true;
            }
        }
        else if (grids2[x + 1][y] == 1 && grids2[x - 1][y] == 0)
        {
            check = true;
        }

        return check;
    }

    bool checkInnerNECorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("NE");

        int xMax = grids2.Count;
        int yMax = grids2[0].Count;
        if (y + 1 == yMax)
        {
            if (grids2[x + 1][y] == 0 && grids2[x][y - 1] == 1)
            {
                // ---
                // 110
                // 110
                check = true;
            }
        }
        else if (x + 1 == xMax)
        {
            if (grids2[x][y + 1] == 0 && grids2[x - 1][y] == 1)
            {
                // 00|
                // 11|
                // 11|
                check = true;
            }
        }
        else if (grids2[x + 1][y] == 0 && grids2[x][y - 1] == 1 && grids2[x][y + 1] == 0 && grids2[x - 1][y] == 1)
        {
            // 000 
            // 110
            // 110
            check = true;
        }
        return check;
    }
    bool checkInnerNWCorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("NW");

        int yMax = grids2[0].Count;
        if (y + 1 == yMax)
        {
            if (grids2[x - 1][y] == 0 && grids2[x][y - 1] == 1)
            {
                // ---
                // 011
                // 011
                check = true;
            }
        }
        else if (x == 0)
        {
            if (grids2[x][y + 1] == 0 && grids2[x + 1][y] == 1)
            {
                // |00
                // |11
                // |11
                check = true;
            }
        }
        else if (grids2[x + 1][y] == 1 && grids2[x][y - 1] == 1 && grids2[x][y + 1] == 0 && grids2[x - 1][y] == 0)
        {
            // 000 
            // 011
            // 011
            check = true;
        }
        return check;
    }
    bool checkInnerSECorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("SE");

        int xMax = grids2.Count;
        if (y == 0)
        {
            if (grids2[x + 1][y] == 0 && grids2[x][y + 1] == 1)
            {
                // 110
                // 110
                // ---
                check = true;
            }
        }
        else if (x + 1 == xMax)
        {
            if (grids2[x][y - 1] == 0 && grids2[x - 1][y] == 1)
            {
                // 11|
                // 11|
                // 00|
                check = true;
            }
        }
        else if (grids2[x + 1][y] == 0 && grids2[x][y - 1] == 0 && grids2[x][y + 1] == 1 && grids2[x - 1][y] == 1)
        {
            // 110
            // 110
            // 000 
            check = true;
        }
        return check;
    }
    bool checkInnerSWCorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("SW");

        if (y == 0)
        {
            if (grids2[x - 1][y] == 0 && grids2[x][y + 1] == 1)
            {
                // 011
                // 011
                // ---
                check = true;
            }
        }
        else if (x == 0)
        {
            if (grids2[x][y - 1] == 0 && grids2[x + 1][y] == 1)
            {
                // |11
                // |11
                // |00
                check = true;
            }
        }
        else if (grids2[x + 1][y] == 1 && grids2[x][y - 1] == 0 && grids2[x][y + 1] == 1 && grids2[x - 1][y] == 0)
        {
            // 011
            // 011
            // 000 
            check = true;
        }
        return check;
    }

    bool checkOuterNECorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("NE");

        if (grids2[x - 1][y] == 1 && grids2[x][y - 1] == 1 && grids2[x - 1][y - 1] == 0)
        {
            // 111 
            // 001
            // 001
            check = true;
        }
        return check;
    }
    bool checkOuterNWCorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("NW");

        if (grids2[x + 1][y] == 1 && grids2[x][y - 1] == 1 && grids2[x + 1][y - 1] == 0)
        {
            // 111 
            // 100
            // 100
            check = true;
        }
        return check;
    }
    bool checkOuterSECorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        //print("SE");

        if (grids2[x - 1][y] == 1 && grids2[x][y + 1] == 1 && grids2[x - 1][y + 1] == 0)
        {
            // 001
            // 001
            // 111

            check = true;
        }

        return check;
    }
    bool checkOuterSWCorner(List<List<int>> grids2, int x, int y)
    {
        bool check = false;
        if (grids2[x + 1][y] == 1 && grids2[x][y + 1] == 1 && grids2[x + 1][y + 1] == 0)
        {
            // 100
            // 100
            // 111

            check = true;
        }

        return check;
    }


}
