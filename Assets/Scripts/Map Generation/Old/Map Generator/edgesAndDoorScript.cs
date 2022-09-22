using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Classes;
using Enums;

public class edgesAndDoorScript
{
    public const int minEdgeLength = 3;
    GameObject Door;
    GameObject grid;

    public edgesAndDoorScript(GameObject Door, ref GameObject grid)
    {
        this.Door = Door;
        this.grid = grid;
    }

    public void checkAndInstallDoorConnections(ref GameObject currentRoom)
    {
        roomProperties roomProps = currentRoom.GetComponent<roomProperties>();
        DimensionList dimList = roomProps.dimList;

        // Go along the perimeter of the room
        int xGrid = dimList.xMin;
        int yGrid = dimList.yMin;

        GameObject adjacentRoom = null;

        for (int x = 0; x < dimList.grid.Count - 1; x++)
        {
            yGrid = dimList.yMin;
            for (int y = 0; y < dimList.grid[x].Count - 1; y++)
            {
                bool close = false;
                if (checkNorthWallForConnections(dimList, currentRoom, x, y, xGrid, yGrid, ref close))
                {
                    // Check North Wall checks both y+1 and y+2, so we need to know which matched
                    int xCurrent = xGrid;
                    int yCurrent = (close == true) ? yGrid : yGrid + 1;
                    int xAdjacent = xGrid;
                    int yAdjacent = (close == true) ? yGrid + 1 : yGrid + 2;

                    adjacentRoom = grid.GetComponent<gridManagerScript>().grid[xAdjacent, yAdjacent].GetComponent<gridUnitScript>().room;
                    if (gridRightIsAvailable(currentRoom, xCurrent, yCurrent) == true &&
                        gridRightIsAvailable(adjacentRoom, xAdjacent, yAdjacent) == true)
                    {
                        addConnectingDoors(ref currentRoom, x, y, xCurrent, yCurrent, generalDirection.North,
                                           ref adjacentRoom, xAdjacent, yAdjacent);
                    }
                }
                if (checkSouthWallForConnections(dimList, currentRoom, x, y, xGrid, yGrid, ref close))
                {
                    // Check South Wall checks both y and y-1, so we need to know which matched
                    int xCurrent = xGrid;
                    int yCurrent = (close == true) ? yGrid + 1 : yGrid;
                    int xAdjacent = xGrid;
                    int yAdjacent = (close == true) ? yGrid : yGrid - 1;

                    adjacentRoom = grid.GetComponent<gridManagerScript>().grid[xAdjacent, yAdjacent].GetComponent<gridUnitScript>().room;
                    if (gridRightIsAvailable(currentRoom, xCurrent, yCurrent) == true &&
                        gridRightIsAvailable(adjacentRoom, xAdjacent, yAdjacent) == true)
                    {
                        addConnectingDoors(ref currentRoom, x, y, xCurrent, yCurrent, generalDirection.South,
                                           ref adjacentRoom, xAdjacent, yAdjacent);
                    }
                }
                if (checkEastWallForConnections(dimList, currentRoom, x, y, xGrid, yGrid, ref close))
                {
                    // Check East Wall checks both x+1 and x+2, so we need to know which matched
                    int xCurrent = (close == true) ? xGrid : xGrid + 1;
                    int yCurrent = yGrid;
                    int xAdjacent = (close == true) ? xGrid + 1 : xGrid + 2;
                    int yAdjacent = yGrid;

                    adjacentRoom = grid.GetComponent<gridManagerScript>().grid[xAdjacent, yAdjacent].GetComponent<gridUnitScript>().room;
                    if (gridUpIsAvailable(currentRoom, xCurrent, yCurrent) == true &&
                        gridUpIsAvailable(adjacentRoom, xAdjacent, yAdjacent) == true)
                    {
                        addConnectingDoors(ref currentRoom, x, y, xCurrent, yCurrent, generalDirection.East,
                                           ref adjacentRoom, xAdjacent, yAdjacent);
                    }
                }
                if (checkWestWallForConnections(dimList, currentRoom, x, y, xGrid, yGrid, ref close))
                {
                    // Check West Wall checks both x and x, so we need to know which matched
                    int xCurrent = (close == true) ? xGrid + 1 : xGrid;
                    int yCurrent = yGrid;
                    int xAdjacent = (close == true) ? xGrid : xGrid - 1;
                    int yAdjacent = yGrid;

                    adjacentRoom = grid.GetComponent<gridManagerScript>().grid[xAdjacent, yAdjacent].GetComponent<gridUnitScript>().room;
                    if (gridUpIsAvailable(currentRoom, xCurrent, yCurrent) == true &&
                        gridUpIsAvailable(adjacentRoom, xAdjacent, yAdjacent) == true)
                    {
                        addConnectingDoors(ref currentRoom, x, y, xCurrent, yCurrent, generalDirection.West,
                                           ref adjacentRoom, xAdjacent, yAdjacent);
                    }
                }
                yGrid++;
            }
            xGrid++;
        }
    }

    void addConnectingDoors(ref GameObject currentRoom, int xDoorToCurrentRoom, int yDoorToCurrentRoom, int xDoorInGrid, int yDoorInGrid, generalDirection currentRoomDoorDir,
                            ref GameObject adjacentRoom, int xAdjacentDoorInGrid, int yAdjacentDoorInGrid)
    {
        roomProperties roomProps = currentRoom.GetComponent<roomProperties>();
        roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

        bool doorUsedBool = true; bool doorLocked = false;
        int currentDoorId = roomProps.numDoors; 
        int adjacentDoorId = adjacentRoomProps.numDoors;

        // Get the adjcent room door dir and door coords
        generalDirection adjacentRoomDoorDir = generalDirection.North;
        if (currentRoomDoorDir == generalDirection.North)
            adjacentRoomDoorDir = generalDirection.South;
        else if (currentRoomDoorDir == generalDirection.South)
            adjacentRoomDoorDir = generalDirection.North;
        else if (currentRoomDoorDir == generalDirection.East)
            adjacentRoomDoorDir = generalDirection.West;
        else if (currentRoomDoorDir == generalDirection.West)
            adjacentRoomDoorDir = generalDirection.East;

        int xDoorToAdjacentRoom = xAdjacentDoorInGrid - adjacentRoomProps.dimList.xMin;
        int yDoorToAdjacentRoom = yAdjacentDoorInGrid - adjacentRoomProps.dimList.yMin;

        xDoorToCurrentRoom = xDoorInGrid - roomProps.dimList.xMin;
        yDoorToCurrentRoom = yDoorInGrid - roomProps.dimList.yMin;

        // Create the door for the current room and the door GO
        GameObject newDoorForCurrentRoom = GameObject.Instantiate(Door, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        newDoorForCurrentRoom.name = "Door" + currentDoorId;
        newDoorForCurrentRoom.transform.parent = currentRoom.transform;
        newDoorForCurrentRoom.gameObject.SetActive(true);

        roomProps.addNewDoor(adjacentRoom, doorUsedBool, doorLocked, currentRoomDoorDir, adjacentDoorId, xDoorToCurrentRoom, yDoorToCurrentRoom);
        snapDoorToGrid(ref newDoorForCurrentRoom, xDoorInGrid, yDoorInGrid, currentRoomDoorDir);


        // Create the door for the current room and the door GO
        GameObject newDoorForAdjacentRoom = GameObject.Instantiate(Door, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        newDoorForAdjacentRoom.name = "Door" + adjacentDoorId;
        newDoorForAdjacentRoom.transform.parent = adjacentRoom.transform;
        newDoorForAdjacentRoom.gameObject.SetActive(true);

        adjacentRoomProps.addNewDoor(currentRoom, doorUsedBool, doorLocked, adjacentRoomDoorDir, currentDoorId, xDoorToAdjacentRoom, yDoorToAdjacentRoom);
        snapDoorToGrid(ref newDoorForAdjacentRoom, xAdjacentDoorInGrid, yAdjacentDoorInGrid, adjacentRoomDoorDir);
    }

    public bool gridRightIsAvailable(GameObject room, int xGrid, int yGrid)
    {
        bool isAvailable = false;
        if (grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().room.name == room.name)
        {
            isAvailable = true;
        }

        return isAvailable;
    }

    public bool gridUpIsAvailable(GameObject room, int xGrid, int yGrid)
    {
        bool isAvailable = false;

        if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().room.name == room.name)
        {
            isAvailable = true;
        }

        return isAvailable;
    }

    public bool checkForConnections(GameObject currentRoom, DimensionList possibleRoomExpansion)
    {
        bool newConnection = false;

        // Go along the perimeter of the possible room expansion
        int xGrid = possibleRoomExpansion.xMin;
        int yGrid = possibleRoomExpansion.yMin;

        for (int x = 0; x < possibleRoomExpansion.grid.Count - 1; x++)
        {
            if (newConnection) break;

            yGrid = possibleRoomExpansion.yMin;
            for (int y = 0; y < possibleRoomExpansion.grid[x].Count - 1; y++)
            {
                bool close = false; // This bool is not used in this function
                if (checkNorthWallForConnections(possibleRoomExpansion, currentRoom, x, y, xGrid, yGrid, ref close) ||
                    checkSouthWallForConnections(possibleRoomExpansion, currentRoom, x, y, xGrid, yGrid, ref close) ||
                    checkEastWallForConnections(possibleRoomExpansion, currentRoom, x, y, xGrid, yGrid, ref close) ||
                    checkWestWallForConnections(possibleRoomExpansion, currentRoom, x, y, xGrid, yGrid, ref close))
                {
                    newConnection = true;
                    break;
                }
                yGrid++;
            }
            xGrid++;
        }

        return newConnection;
    }

    bool checkNorthWallForConnections(DimensionList dimList, GameObject room, int x, int y, int xGrid, int yGrid, ref bool close)
    {
        // If the outer west most part is a wall, or an inner part is a wall
        bool newConnection = false;
        if (dimList.grid[x][y] == 1 && dimList.grid[x][y + 1] == 0)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();
                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 1].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = true;
                    newConnection = true;
                }
            }
        }
        else if (y + 1 == dimList.grid[x].Count - 1 && dimList.grid[x][y + 1] == 1)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 2].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 2].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 2].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid + 2].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = false;
                    newConnection = true;
                }
            }
        }
        return newConnection;
    }

    bool checkSouthWallForConnections(DimensionList dimList, GameObject room, int x, int y, int xGrid, int yGrid, ref bool close)
    {
        // If the outer west most part is a wall, or an inner part is a wall
        bool newConnection = false;

        if (dimList.grid[x][y] == 0 && dimList.grid[x][y + 1] == 1)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = true;
                    newConnection = true;
                }
            }
        }
        else if (y == 0 && dimList.grid[x][y] == 1)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid - 1].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid - 1].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid - 1].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid - 1].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = false;
                    newConnection = true;
                }
            }
        }


        return newConnection;
    }

    bool checkEastWallForConnections(DimensionList dimList, GameObject room, int x, int y, int xGrid, int yGrid, ref bool close)
    {
        // If the outer west most part is a wall, or an inner part is a wall
        bool newConnection = false;
        if (dimList.grid[x][y] == 1 && dimList.grid[x + 1][y] == 0)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid + 1, yGrid].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = true;
                    newConnection = true;
                }
            }
        }
        else if (x + 1 == dimList.grid.Count - 1 && dimList.grid[x + 1][y] == 1)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid + 2, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid + 2, yGrid].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid + 2, yGrid].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid + 2, yGrid].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = false;
                    newConnection = true;
                }
            }
        }
        return newConnection;
    }

    bool checkWestWallForConnections(DimensionList dimList, GameObject room, int x, int y, int xGrid, int yGrid, ref bool close)
    {
        // If the outer west most part is a wall, or an inner part is a wall
        bool newConnection = false;
        if (dimList.grid[x][y] == 0 && dimList.grid[x + 1][y] == 1)
        {

            if (grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid, yGrid].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = true;
                    newConnection = true;
                }
            }
        }
        else if (x == 0 && dimList.grid[x][y] == 1)
        {
            if (grid.GetComponent<gridManagerScript>().grid[xGrid - 1, yGrid].GetComponent<gridUnitScript>().isOccupied &&
                grid.GetComponent<gridManagerScript>().grid[xGrid - 1, yGrid].GetComponent<gridUnitScript>().room.name != room.name)
            {
                roomProperties otherRoomProps = grid.GetComponent<gridManagerScript>().grid[xGrid - 1, yGrid].GetComponent<gridUnitScript>().room.GetComponent<roomProperties>();

                if (isLessThanOneRoomAway(room, grid.GetComponent<gridManagerScript>().grid[xGrid - 1, yGrid].GetComponent<gridUnitScript>().room) == false &&
                    otherRoomProps.canAddDoors == true)
                {
                    close = false;
                    newConnection = true;
                }
            }
        }
        return newConnection;
    }

    public void transferDoorList(ref GameObject recipientRoom, ref GameObject donorRoom)
    {
        roomProperties donorRoomProps = donorRoom.GetComponent<roomProperties>();
        roomProperties recipientRoomProps = recipientRoom.GetComponent<roomProperties>();

        for (int i = 0; i < donorRoomProps.doorList.Count; i++)
        {
            Door currentDoor = donorRoomProps.doorList[i];

            // Get the original door coords
            int x = donorRoomProps.doorList[i].doorCoords.x + donorRoomProps.gridCoords.x;
            int y = donorRoomProps.doorList[i].doorCoords.y + donorRoomProps.gridCoords.y;

            // Adjust them to the new room coords
            x = x - recipientRoomProps.gridCoords.x;
            y = y - recipientRoomProps.gridCoords.y;

            // Add the door to the new room
            recipientRoomProps.addNewDoor(currentDoor.adjacentRoom, currentDoor.doorUsedBool, currentDoor.doorLocked,
                                          currentDoor.doorsOrient, currentDoor.linkedDoorId, x, y);

            // Transfer the door game objects also
            GameObject doorGO = getDoorGameObject(donorRoom, i);
            doorGO.transform.parent = recipientRoom.transform;
        }
    }

    GameObject getDoorGameObject(GameObject donorRoom, int doorId)
    {
        GameObject doorGO = null;
        foreach (Transform child in donorRoom.transform)
        {
            if (child.gameObject.name == ("Door" + doorId))
            {
                doorGO = child.gameObject;
            }
        }

        return doorGO;
    }

    bool isLessThanOneRoomAway(GameObject roomOne, GameObject roomTwo)
    {
        bool isLessThanOneRoomAway = false;

        roomProperties roomOneProps = roomOne.GetComponent<roomProperties>();
        // Go through all doors for room one
        for (int i = 0; i < roomOneProps.doorList.Count; i++)
        {
            // If it is used
            if (roomOneProps.doorList[i].doorUsedBool == true)
            {
                // If they are already connected then we are less than one room away
                if (roomOneProps.doorList[i].adjacentRoom.name == roomTwo.name)
                {
                    isLessThanOneRoomAway = true;
                    break;
                }

                roomProperties adjacentRoomProps = roomOneProps.doorList[i].adjacentRoom.GetComponent<roomProperties>();

                // Go through all adjacent room doors
                for (int j = 0; j < adjacentRoomProps.doorList.Count; j++)
                {
                    // If it is used
                    if (adjacentRoomProps.doorList[j].doorUsedBool == true)
                    {
                        // If there is a door that match the second room name, then we are less than one room away
                        if (adjacentRoomProps.doorList[j].adjacentRoom.name == roomTwo.name)
                        {
                            isLessThanOneRoomAway = true;
                            break;
                        }
                    }
                }
                if (isLessThanOneRoomAway) break;
            }
        }

        return isLessThanOneRoomAway;
    }
    
    public void findNextBookMarks(ref GameObject newRoom, ref Queue<BookMark> bookMarks)
    {
        // Find edges of the room that could be the next room
        roomProperties newRoomProperties = newRoom.GetComponent<roomProperties>();

        // Get raw edges
        List<List<WallPiece>> listOfUnfilteredEdges = new List<List<WallPiece>>();
        getEdgesFromRoom(ref newRoom, ref listOfUnfilteredEdges);



        List<List<WallPiece>> listOfViableEdges = new List<List<WallPiece>>();
        filterViableEdges(ref newRoom, listOfUnfilteredEdges, ref listOfViableEdges);

        //Debug.Log("NEXT BOOKMARK:" + newRoom.name + "  " + listOfViableEdges.Count);

        // If we have a viable edge, add a door and bookmark
        if (listOfViableEdges.Count != 0)
        {

            
            List<List<WallPiece>> existingDoorCoordsList = new List<List<WallPiece>>();

            // Find all existing doors
            for (int i = 0; i < newRoomProperties.numDoors; i++)
            {
                List<WallPiece> temp = new List<WallPiece>();
                if (newRoomProperties.doorList[i].doorUsedBool == true)
                {
                    if (newRoomProperties.doorList[i].doorsOrient == generalDirection.North)
                    {
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y, "WN"));
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x + 1, newRoomProperties.doorList[i].doorCoords.y, "WN"));
                        existingDoorCoordsList.Add(temp);
                    }
                    else if (newRoomProperties.doorList[i].doorsOrient == generalDirection.South)
                    {
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y, "WS"));
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x + 1, newRoomProperties.doorList[i].doorCoords.y, "WS"));
                        existingDoorCoordsList.Add(temp);
                    }
                    else if (newRoomProperties.doorList[i].doorsOrient == generalDirection.East)
                    {
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y, "WE"));
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y + 1, "WE"));
                        existingDoorCoordsList.Add(temp);
                    }
                    else if (newRoomProperties.doorList[i].doorsOrient == generalDirection.West)
                    {
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y, "WW"));
                        temp.Add(
                            new WallPiece(newRoomProperties.doorList[i].doorCoords.x, newRoomProperties.doorList[i].doorCoords.y + 1, "WW"));
                        existingDoorCoordsList.Add(temp);
                    }
                }
            }

            for (int i = 0; i < listOfViableEdges.Count; i++)
            {
                // Find a viable door locations

                List<List<WallPiece>> newDoorCoordsList = new List<List<WallPiece>>();
                findNewDoorsInEdge(listOfViableEdges[i], ref newDoorCoordsList, existingDoorCoordsList);

                for (int j = 0; j < newDoorCoordsList.Count; j++)
                {
                    
                    WallPiece newDoorCoords = newDoorCoordsList[j][0];
                    WallPiece secondaryNewDoorCoords = newDoorCoordsList[j][1];

                    if (newDoorCoords != null)
                    {
                        // Add to existing door coords list
                        List <WallPiece> temp = new List<WallPiece>();
                        temp.Add(newDoorCoords);
                        temp.Add(secondaryNewDoorCoords);
                        existingDoorCoordsList.Add(temp);

                        int xBookMark = 0;
                        int yBookMark = 0;

                        // Add some door property changes
                        int x = newRoomProperties.gridCoords.x + newDoorCoords.coords.x;
                        int y = newRoomProperties.gridCoords.y + newDoorCoords.coords.y;
                        grid.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isDoor = true;

                        generalDirection newDoorOrient = generalDirection.North;
                        if (newDoorCoords.wallType == "WN" || secondaryNewDoorCoords.wallType == "WN")
                        {
                            newDoorOrient = generalDirection.North;
                            grid.GetComponent<gridManagerScript>().grid[x + 1, y].GetComponent<gridUnitScript>().isDoor = true;
                        }
                        else if (newDoorCoords.wallType == "WS" || secondaryNewDoorCoords.wallType == "WS")
                        {
                            newDoorOrient = generalDirection.South;
                            grid.GetComponent<gridManagerScript>().grid[x + 1, y].GetComponent<gridUnitScript>().isDoor = true;
                        }
                        else if (newDoorCoords.wallType == "WE" || secondaryNewDoorCoords.wallType == "WE")
                        {
                            newDoorOrient = generalDirection.East;
                            grid.GetComponent<gridManagerScript>().grid[x, y + 1].GetComponent<gridUnitScript>().isDoor = true;
                        }
                        else if (newDoorCoords.wallType == "WW" || secondaryNewDoorCoords.wallType == "WW")
                        {
                            newDoorOrient = generalDirection.West;
                            grid.GetComponent<gridManagerScript>().grid[x, y + 1].GetComponent<gridUnitScript>().isDoor = true;
                        }
                        //newRoomProperties.doorsOrient.Add(newDoorOrient);


                        int newDoorId = newRoomProperties.numDoors;
                        GameObject newDoor = GameObject.Instantiate(Door, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        newDoor.name = "Door" + (newDoorId);
                        newDoor.transform.parent = newRoom.transform;

                        newRoomProperties.addEmptyDoor(newDoorCoords.coords.x, newDoorCoords.coords.y, newDoorOrient);


                        snapDoorToGrid(ref newDoor, x, y, newDoorOrient);

                        // Configure new BookMark
                        generalDirection orientation = generalDirection.North;
                        if (newDoorCoords.wallType == "WN" || secondaryNewDoorCoords.wallType == "WN")
                        {
                            orientation = generalDirection.North;
                            xBookMark = newRoomProperties.gridCoords.x + newDoorCoords.coords.x;
                            yBookMark = newRoomProperties.gridCoords.y + newDoorCoords.coords.y + 1;
                        }
                        else if (newDoorCoords.wallType == "WS" || secondaryNewDoorCoords.wallType == "WS")
                        {
                            orientation = generalDirection.South;
                            xBookMark = newRoomProperties.gridCoords.x + newDoorCoords.coords.x;
                            yBookMark = newRoomProperties.gridCoords.y + newDoorCoords.coords.y - 1;
                        }
                        else if (newDoorCoords.wallType == "WE" || secondaryNewDoorCoords.wallType == "WE")
                        {
                            orientation = generalDirection.East;
                            xBookMark = newRoomProperties.gridCoords.x + newDoorCoords.coords.x + 1;
                            yBookMark = newRoomProperties.gridCoords.y + newDoorCoords.coords.y;
                        }
                        else if (newDoorCoords.wallType == "WW" || secondaryNewDoorCoords.wallType == "WW")
                        {
                            orientation = generalDirection.West;
                            xBookMark = newRoomProperties.gridCoords.x + newDoorCoords.coords.x - 1;
                            yBookMark = newRoomProperties.gridCoords.y + newDoorCoords.coords.y;
                        }

                        //Debug.Log("1ST WALLTYPE: " + newDoorCoords.wallType);
                        //Debug.Log("2ND WALLTYPE: " + secondaryNewDoorCoords.wallType);

                        //Debug.Log("NEW ROOMCOORDS: " + newRoomProperties.gridCoords.x + "," + newRoomProperties.gridCoords.y);
                        //Debug.Log("NEW DOORCOORDS: " + newDoorCoords.coords.x + "," + newDoorCoords.coords.y);
                        //Debug.Log("NEW BOOKMARK: " + xBookMark + "," + yBookMark);

                        // New BookMark
                        bookMarks.Enqueue(new BookMark(newDoorId, orientation, newRoom, xBookMark, yBookMark));

                    }
                }
            }
        }

        //Debug.Log("EDGE COUNT: " + listOfEdges.Count);
        //Debug.Log("!!!!!!!!!! NEED TO CHECK IF THE START PIECE EDGE AND END PIECE EDGE ARE THE SAME EDGE");

        // For each edge found add a potential door to the bookMarks queue
    }

    void getEdgesFromRoom(ref GameObject newRoom, ref List<List<WallPiece>> listOfEdges)
    {
        List<WallPiece> roomWalls = new List<WallPiece>();
        List<WallPiece> edge = new List<WallPiece>();
        List<WallPiece> tempEdge = new List<WallPiece>();

        roomProperties newRoomProperties = newRoom.GetComponent<roomProperties>();
        WallPiece startPiece = null;
        WallPiece currentPiece = null;

        // Get the first wall of the loop needs to be a corner
        bool breakOut = false;
        for (int x = 0; x < newRoomProperties.innerGrid.Count; x++)
        {
            for (int y = 0; y < newRoomProperties.innerGrid[0].Count; y++)
            {
                if (newRoomProperties.innerGrid[x][y] == "ICNE" || newRoomProperties.innerGrid[x][y] == "ICNW" ||
                    newRoomProperties.innerGrid[x][y] == "ICSE" || newRoomProperties.innerGrid[x][y] == "ICSW")
                {
                    startPiece = new WallPiece(x, y, newRoomProperties.innerGrid[x][y]);

                    breakOut = true;
                    break;
                }
            }
            if (breakOut) break;
        }

        roomWalls.Add(startPiece);

        //Debug.Log("START PIECE: " + startPiece.coords.x + "," + startPiece.coords.y + "     " + startPiece.wallType);

        currentPiece = startPiece;// findNextPiece(startPiece, newRoom);

        // Go in a full loop around the room to create a chain of walls
        bool loopCompleted = false;
        bool justStarted = true;
        while (loopCompleted == false)
        {
            // Add the next piece until we loop all the way around to the startPiece
            if (currentPiece.coords.x == startPiece.coords.x &&
                currentPiece.coords.y == startPiece.coords.y)
            {
                if (justStarted == true)
                {
                    //Debug.Log("JUST STARTED PIECE: " + currentPiece.wallType);
                    edge.Add(currentPiece);
                    justStarted = false;
                }
                else
                {
                    //Debug.Log("FULL LOOP");
                    loopCompleted = true;

                    if (isInnerCorner(currentPiece) == true)
                    {
                        //Debug.Log("CORNER: " + currentPiece.wallType);

                        edge.Add(currentPiece);
                    }
                    if (edge.Count >= minEdgeLength)
                    {
                        //Debug.Log("ADDED EDGE");

                        listOfEdges.Add(edge);
                    }
                }
            }
            else
            {
                //Debug.Log(currentPiece.wallType);
                roomWalls.Add(currentPiece);

                if (isACorner(currentPiece) == false)
                {
                    //Debug.Log("NON CORNER: " + currentPiece.wallType);

                    edge.Add(currentPiece);
                }
                else
                {
                    // Add the current piece and last corner to the edge list
                    if (isInnerCorner(currentPiece) == true)
                    {
                        //Debug.Log("CORNER: " + currentPiece.wallType);

                        edge.Add(currentPiece);
                    }
                    if (edge.Count >= minEdgeLength)
                    {
                        //Debug.Log("ADDED EDGE");

                        listOfEdges.Add(edge);
                    }

                    edge = new List<WallPiece>();
                    if (isInnerCorner(currentPiece) == true)
                    {
                        //Debug.Log("CORNER: " + currentPiece.wallType);

                        edge.Add(currentPiece);
                    }

                    //Debug.Log("BEFORE CORNER: " + tempEdge[tempEdge.Count - 1].wallType);
                }
            }

            currentPiece = findNextPiece(currentPiece, newRoom);
        }
    }

    WallPiece findNextPiece(WallPiece piece, GameObject newRoom)
    {
        // Next piece is based on a clockwise loop
        // ---->------
        // |         |
        // ^         v
        // |         |
        // ----<------
        roomProperties roomProperties = newRoom.GetComponent<roomProperties>();
        WallPiece nextPiece = null;
        int xNext = 0;
        int yNext = 0;

        if (piece.wallType == "WN" || piece.wallType == "ICNW" || piece.wallType == "OCSW")
        {
            xNext = piece.coords.x + 1;
            yNext = piece.coords.y;
        }
        else if (piece.wallType == "WS" || piece.wallType == "ICSE" || piece.wallType == "OCNE")
        {
            xNext = piece.coords.x - 1;
            yNext = piece.coords.y;
        }
        else if (piece.wallType == "WE" || piece.wallType == "ICNE" || piece.wallType == "OCNW")
        {
            xNext = piece.coords.x;
            yNext = piece.coords.y - 1;
        }
        else if (piece.wallType == "WW" || piece.wallType == "ICSW" || piece.wallType == "OCSE")
        {
            xNext = piece.coords.x;
            yNext = piece.coords.y + 1;
        }


        nextPiece = new WallPiece(xNext, yNext, roomProperties.innerGrid[xNext][yNext]);

        return nextPiece;
    }

    void filterViableEdges(ref GameObject newRoom, List<List<WallPiece>> listOfUnfilteredEdges, ref List<List<WallPiece>> listOfViableEdges)
    {
        roomProperties newRoomProperties = newRoom.GetComponent<roomProperties>();

        for (int i = 0; i < listOfUnfilteredEdges.Count; i++)
        {
            generalDirection dir = generalDirection.North;

            // Figure out what direction to check
            for (int j = 0; j < listOfUnfilteredEdges[i].Count; j++)
            {
                if (listOfUnfilteredEdges[i][j].wallType == "WN")
                {
                    dir = generalDirection.North;
                    break;
                }
                else if (listOfUnfilteredEdges[i][j].wallType == "WS")
                {
                    dir = generalDirection.South;
                    break;
                }
                else if (listOfUnfilteredEdges[i][j].wallType == "WE")
                {
                    dir = generalDirection.East;
                    break;
                }
                else if (listOfUnfilteredEdges[i][j].wallType == "WW")
                {
                    dir = generalDirection.West;
                    break;
                }
            }

            List<WallPiece> edge = new List<WallPiece>();
            for (int j = 0; j < listOfUnfilteredEdges[i].Count; j++)
            {
                if (isEdgeNextToEmptyVein(listOfUnfilteredEdges[i][j], newRoomProperties.gridCoords, dir))
                {
                    edge.Add(listOfUnfilteredEdges[i][j]);
                }
                else
                {
                    if (edge.Count != 0 && edge.Count >= minEdgeLength)
                    {
                        listOfViableEdges.Add(edge);
                    }
                    edge = new List<WallPiece>();
                }
            }

            if (edge.Count != 0 && edge.Count >= minEdgeLength)
            {
                listOfViableEdges.Add(edge);
            }
            edge = new List<WallPiece>();
        }

        //for (int i = 0; i < listOfViableEdges.Count; i++)
        //{
        //    Debug.Log("NEXT EDGE");

        //    for (int j = 0; j < listOfViableEdges[i].Count; j++)
        //    {
        //        Debug.Log("         " + listOfViableEdges[i][j].wallType);
        //    }
        //}
    }

    void findNewDoorsInEdge(List<WallPiece> listOfEdges, ref List<List<WallPiece>> listOfPotentialDoors, List<List<WallPiece>> existingDoorCoordsList)
    {
        // Find all doors in an edge, but you can't find a door from the same wall type that already exists

        // 0 = find door at the start of edge, 1 = middle, 2 = end
        int findDoorLocation = Random.Range(0, 3);
        //Debug.Log("DOOR RANDOM: " + findDoorLocation);

        //WallPiece newDoorCoords = null;
        int middle = (listOfEdges.Count / 2) - 1;
        if (middle == 0)
        {
            middle = 1;
        }

        bool foundNorthDoor = false;
        bool foundSouthDoor = false;
        bool foundEastDoor = false;
        bool foundWestDoor = false;

        List<WallPiece> temp = new List<WallPiece>();
        if (findDoorLocation == 0)
        {
            // Search up from the start
            for (int i = 1; i < listOfEdges.Count; i++)
            {
                if (isViableEdge(listOfEdges[i], listOfEdges[i - 1], listOfPotentialDoors, existingDoorCoordsList))
                {
                    bool wallTypeExists = false;

                    if (listOfEdges[i].wallType == "WN" || listOfEdges[i - 1].wallType == "WN")
                    {
                        if (foundNorthDoor) wallTypeExists = true;
                        foundNorthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WS" || listOfEdges[i - 1].wallType == "WS")
                    {
                        if (foundSouthDoor) wallTypeExists = true;
                        foundSouthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WE" || listOfEdges[i - 1].wallType == "WE")
                    {
                        if (foundEastDoor) wallTypeExists = true;
                        foundEastDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WW" || listOfEdges[i - 1].wallType == "WW")
                    {
                        if (foundWestDoor) wallTypeExists = true;
                        foundWestDoor = true;
                    }

                    if (wallTypeExists == false)
                    {
                        // Add both wallpieces to the list, first one add is the main door reference
                        temp = new List<WallPiece>();
                        temp.Add(getLowerLeftCorner(listOfEdges[i], listOfEdges[i - 1]));

                        //Debug.Log(listOfEdges[i].wallType + "      " + listOfEdges[i - 1].wallType);
                        //Debug.Log(listOfEdges[i].coords.x + "," + listOfEdges[i].coords.y);
                        //Debug.Log(listOfEdges[i-1].coords.x + "," + listOfEdges[i - 1].coords.y);

                        if (temp[0].coords.x == listOfEdges[i].coords.x && temp[0].coords.y == listOfEdges[i].coords.y)
                        {
                            temp.Add(new WallPiece(listOfEdges[i - 1].coords.x, listOfEdges[i - 1].coords.y, listOfEdges[i - 1].wallType));
                        }
                        else
                        {
                            temp.Add(new WallPiece(listOfEdges[i].coords.x, listOfEdges[i].coords.y, listOfEdges[i].wallType));
                        }

                        listOfPotentialDoors.Add(temp);
                    }
                }
            }
        }
        else if (findDoorLocation == 1)
        {
            // Search up from middle
            for (int i = middle; i < listOfEdges.Count; i++)
            {
                if (isViableEdge(listOfEdges[i], listOfEdges[i - 1], listOfPotentialDoors, existingDoorCoordsList))
                {
                    bool wallTypeExists = false;

                    if (listOfEdges[i].wallType == "WN" || listOfEdges[i - 1].wallType == "WN")
                    {
                        if (foundNorthDoor) wallTypeExists = true;
                        foundNorthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WS" || listOfEdges[i - 1].wallType == "WS")
                    {
                        if (foundSouthDoor) wallTypeExists = true;
                        foundSouthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WE" || listOfEdges[i - 1].wallType == "WE")
                    {
                        if (foundEastDoor) wallTypeExists = true;
                        foundEastDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WW" || listOfEdges[i - 1].wallType == "WW")
                    {
                        if (foundWestDoor) wallTypeExists = true;
                        foundWestDoor = true;
                    }

                    if (wallTypeExists == false)
                    {
                        // Add both wallpieces to the list, first one add is the main door reference
                        temp = new List<WallPiece>();
                        temp.Add(getLowerLeftCorner(listOfEdges[i], listOfEdges[i - 1]));

                        //Debug.Log(listOfEdges[i].wallType + "      " + listOfEdges[i - 1].wallType);
                        //Debug.Log(listOfEdges[i].coords.x + "," + listOfEdges[i].coords.y);
                        //Debug.Log(listOfEdges[i-1].coords.x + "," + listOfEdges[i - 1].coords.y);

                        if (temp[0].coords.x == listOfEdges[i].coords.x && temp[0].coords.y == listOfEdges[i].coords.y)
                        {
                            temp.Add(new WallPiece(listOfEdges[i - 1].coords.x, listOfEdges[i - 1].coords.y, listOfEdges[i - 1].wallType));
                        }
                        else
                        {
                            temp.Add(new WallPiece(listOfEdges[i].coords.x, listOfEdges[i].coords.y, listOfEdges[i].wallType));
                        }

                        listOfPotentialDoors.Add(temp);
                    }
                }
            }

            // Search down from middle
            for (int i = middle; i > 0; i--)
            {
                if (isViableEdge(listOfEdges[i], listOfEdges[i - 1], listOfPotentialDoors, existingDoorCoordsList))
                {
                    bool wallTypeExists = false;
                    if (listOfEdges[i].wallType == "WN" || listOfEdges[i - 1].wallType == "WN")
                    {
                        if (foundNorthDoor) wallTypeExists = true;
                        foundNorthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WS" || listOfEdges[i - 1].wallType == "WS")
                    {
                        if (foundSouthDoor) wallTypeExists = true;
                        foundSouthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WE" || listOfEdges[i - 1].wallType == "WE")
                    {
                        if (foundEastDoor) wallTypeExists = true;
                        foundEastDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WW" || listOfEdges[i - 1].wallType == "WW")
                    {
                        if (foundWestDoor) wallTypeExists = true;
                        foundWestDoor = true;
                    }

                    if (wallTypeExists == false)
                    {
                        // Add both wallpieces to the list, first one add is the main door reference
                        temp = new List<WallPiece>();
                        temp.Add(getLowerLeftCorner(listOfEdges[i], listOfEdges[i - 1]));

                        //Debug.Log(listOfEdges[i].wallType + "      " + listOfEdges[i - 1].wallType);
                        //Debug.Log(listOfEdges[i].coords.x + "," + listOfEdges[i].coords.y);
                        //Debug.Log(listOfEdges[i - 1].coords.x + "," + listOfEdges[i - 1].coords.y);

                        if (temp[0].coords.x == listOfEdges[i].coords.x && temp[0].coords.y == listOfEdges[i].coords.y)
                        {
                            temp.Add(new WallPiece(listOfEdges[i - 1].coords.x, listOfEdges[i - 1].coords.y, listOfEdges[i - 1].wallType));
                        }
                        else
                        {
                            temp.Add(new WallPiece(listOfEdges[i].coords.x, listOfEdges[i].coords.y, listOfEdges[i].wallType));
                        }

                        listOfPotentialDoors.Add(temp);
                    }
                }
            }
        }
        else if (findDoorLocation == 2)
        {
            // Search down from the end
            for (int i = listOfEdges.Count - 1; i > 0; i--)
            {
                if (isViableEdge(listOfEdges[i], listOfEdges[i - 1], listOfPotentialDoors, existingDoorCoordsList))
                {
                    bool wallTypeExists = false;
                    if (listOfEdges[i].wallType == "WN" || listOfEdges[i - 1].wallType == "WN")
                    {
                        if (foundNorthDoor) wallTypeExists = true;
                        foundNorthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WS" || listOfEdges[i - 1].wallType == "WS")
                    {
                        if (foundSouthDoor) wallTypeExists = true;
                        foundSouthDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WE" || listOfEdges[i - 1].wallType == "WE")
                    {
                        if (foundEastDoor) wallTypeExists = true;
                        foundEastDoor = true;
                    }
                    else if (listOfEdges[i].wallType == "WW" || listOfEdges[i - 1].wallType == "WW")
                    {
                        if (foundWestDoor) wallTypeExists = true;
                        foundWestDoor = true;
                    }

                    if (wallTypeExists == false)
                    {
                        // Add both wallpieces to the list, first one add is the main door reference
                        temp = new List<WallPiece>();
                        temp.Add(getLowerLeftCorner(listOfEdges[i], listOfEdges[i - 1]));

                        //Debug.Log(listOfEdges[i].wallType + "      " + listOfEdges[i - 1].wallType);
                        //Debug.Log(listOfEdges[i].coords.x + "," + listOfEdges[i].coords.y);
                        //Debug.Log(listOfEdges[i - 1].coords.x + "," + listOfEdges[i - 1].coords.y);

                        if (temp[0].coords.x == listOfEdges[i].coords.x && temp[0].coords.y == listOfEdges[i].coords.y)
                        {
                            temp.Add(new WallPiece(listOfEdges[i - 1].coords.x, listOfEdges[i - 1].coords.y, listOfEdges[i - 1].wallType));
                        }
                        else
                        {
                            temp.Add(new WallPiece(listOfEdges[i].coords.x, listOfEdges[i].coords.y, listOfEdges[i].wallType));
                        }

                        listOfPotentialDoors.Add(temp);
                    }
                }
            }
        }
        //Debug.Log("NEW DOOR: " + newDoorCoords.coords.x + "," + newDoorCoords.coords.y + "      " + newDoorCoords.wallType);

        //return newDoorCoords;
    }

    bool isViableEdge(WallPiece pieceOne, WallPiece pieceTwo, List<List<WallPiece>> listOfPotentialDoors, List<List<WallPiece>> existingDoorCoordsList)
    {
        //Debug.Log(pieceOne.wallType + "    " + pieceTwo.wallType);

        // Edge can be viable if
        //      1. Two of the same pieces
        //      2. Inner corner with the coresponding walltype
        //      3. Can't be 1 away from an existing door

        bool isViableEdge = false;
        if ((pieceOne.wallType == "WN" && pieceOne.wallType == pieceTwo.wallType) ||
            (pieceOne.wallType == "WS" && pieceOne.wallType == pieceTwo.wallType) ||
            (pieceOne.wallType == "WE" && pieceOne.wallType == pieceTwo.wallType) ||
            (pieceOne.wallType == "WW" && pieceOne.wallType == pieceTwo.wallType) ||

            (pieceOne.wallType == "WN" && (pieceTwo.wallType == "ICNW" || pieceTwo.wallType == "ICNE")) ||
            (pieceTwo.wallType == "WN" && (pieceOne.wallType == "ICNW" || pieceOne.wallType == "ICNE")) ||
            (pieceOne.wallType == "WS" && (pieceTwo.wallType == "ICSW" || pieceTwo.wallType == "ICSE")) ||
            (pieceTwo.wallType == "WS" && (pieceOne.wallType == "ICSW" || pieceOne.wallType == "ICSE")) ||
            (pieceOne.wallType == "WE" && (pieceTwo.wallType == "ICNE" || pieceTwo.wallType == "ICSE")) ||
            (pieceTwo.wallType == "WE" && (pieceOne.wallType == "ICNE" || pieceOne.wallType == "ICSE")) ||
            (pieceOne.wallType == "WW" && (pieceTwo.wallType == "ICNW" || pieceTwo.wallType == "ICSW")) ||
            (pieceTwo.wallType == "WW" && (pieceOne.wallType == "ICNW" || pieceOne.wallType == "ICSW")))
        {
            if (isCloseToExistingDoor(pieceOne, pieceTwo, listOfPotentialDoors, existingDoorCoordsList) == false)
            {
                isViableEdge = true;
            }
        }

        return isViableEdge;
    }

    bool isCloseToExistingDoor(WallPiece pieceOne, WallPiece pieceTwo, List<List<WallPiece>> listOfPotentialDoors, List<List<WallPiece>> existingDoorCoordsList)
    {
        bool isCloseToExistingDoor = false;

        // Check new doors
        for (int i = 0; i < listOfPotentialDoors.Count; i++)
        {
            WallPiece existingDoorPartOne = listOfPotentialDoors[i][0];
            WallPiece existingDoorPartTwo = listOfPotentialDoors[i][1];

            if (isOneAway(existingDoorPartOne.coords, pieceOne.coords) ||
                isOneAway(existingDoorPartOne.coords, pieceTwo.coords) ||
                isOneAway(existingDoorPartTwo.coords, pieceOne.coords) ||
                isOneAway(existingDoorPartTwo.coords, pieceTwo.coords))
            {
                isCloseToExistingDoor = true;
                break;
            }
        }

        // Check existing doors
        if (isCloseToExistingDoor == false)
        {
            for (int i = 0; i < existingDoorCoordsList.Count; i++)
            {
                WallPiece existingDoorPartOne = existingDoorCoordsList[i][0];
                WallPiece existingDoorPartTwo = existingDoorCoordsList[i][1];

                if (isOneAway(existingDoorPartOne.coords, pieceOne.coords) ||
                    isOneAway(existingDoorPartOne.coords, pieceTwo.coords) ||
                    isOneAway(existingDoorPartTwo.coords, pieceOne.coords) ||
                    isOneAway(existingDoorPartTwo.coords, pieceTwo.coords))
                {
                    isCloseToExistingDoor = true;
                    break;
                }
            }
        }

        return isCloseToExistingDoor;
    }

    bool isOneAway(Coords bounds, Coords check)
    {
        bool isOneAway = false;

        if (// Left/Right check
            (bounds.x - 1 <= check.x && check.x <= bounds.x + 1 &&
            bounds.y <= check.y && check.y <= bounds.y) ||

            // Up/Down check
            (bounds.x <= check.x && check.x <= bounds.x &&
            bounds.y - 1 <= check.y && check.y <= bounds.y + 1))
        {
            isOneAway = true;
        }
        return isOneAway;
    }

    public void snapDoorToGrid(ref GameObject newDoor, int x, int y, generalDirection orientation)
    {
        float xPos = 0f;
        float yPos = 0f;

        float gridSize = grid.GetComponent<gridManagerScript>().gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.x;
        float xScale = newDoor.transform.localScale.x;


        grid.GetComponent<gridManagerScript>().getGridPosition(ref x, ref y, ref xPos, ref yPos);



        if (orientation == generalDirection.North)
        {
            xPos = xPos + (gridSize);
            yPos = yPos + (newDoor.GetComponent<SpriteRenderer>().bounds.size.y / 2) + (gridSize - newDoor.GetComponent<SpriteRenderer>().bounds.size.y);
        }
        else if (orientation == generalDirection.South)
        {
            xPos = xPos + (gridSize);
            yPos = yPos + (newDoor.GetComponent<SpriteRenderer>().bounds.size.y / 2);
        }
        else if (orientation == generalDirection.East)
        {
            newDoor.transform.Rotate(0f, 0f, 90f, Space.Self);

            xPos = xPos + (newDoor.GetComponent<SpriteRenderer>().bounds.size.x / 2) + (gridSize - newDoor.GetComponent<SpriteRenderer>().bounds.size.x);
            yPos = yPos + (gridSize);
        }
        else if (orientation == generalDirection.West)
        {
            newDoor.transform.Rotate(0f, 0f, 90f, Space.Self);

            xPos = xPos + (newDoor.GetComponent<SpriteRenderer>().bounds.size.x / 2);
            yPos = yPos + (gridSize);
        }

        newDoor.transform.position = new Vector3(xPos, yPos, 0);
    }

    bool isInnerCorner(WallPiece piece)
    {
        bool isInnerCorner = false;
        if (piece.wallType == "ICNW" || piece.wallType == "ICNE" || piece.wallType == "ICSW" || piece.wallType == "ICSE")
        {
            isInnerCorner = true;
        }
        return isInnerCorner;
    }

    bool isInnerCorner(string piece)
    {
        bool isInnerCorner = false;
        if (piece == "ICNW" || piece == "ICNE" || piece == "ICSW" || piece == "ICSE")
        {
            isInnerCorner = true;
        }
        return isInnerCorner;
    }

    bool isACorner(WallPiece piece)
    {
        bool isACorner = false;
        if (piece.wallType == "ICNE" || piece.wallType == "ICNW" || piece.wallType == "OCSE" || piece.wallType == "OCSW" ||
            piece.wallType == "ICSE" || piece.wallType == "ICSW" || piece.wallType == "OCNE" || piece.wallType == "OCNW")
        {
            isACorner = true;
        }
        return isACorner;
    }

    bool isEdgeNextToEmptyVein(WallPiece wall, Coords roomCoords, generalDirection dir)
    {
        bool isViable = false;
        int xCheck = 0;
        int yCheck = 0;

        if (dir == generalDirection.North)
        {
            xCheck = roomCoords.x + wall.coords.x;
            yCheck = roomCoords.y + wall.coords.y + 1;
            if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
            {
                yCheck = roomCoords.y + wall.coords.y + 2;
                if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                    grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
                {
                    isViable = true;
                }
            }
        }
        else if (dir == generalDirection.South)
        {
            xCheck = roomCoords.x + wall.coords.x;
            yCheck = roomCoords.y + wall.coords.y - 1;
            if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
            {
                yCheck = roomCoords.y + wall.coords.y - 2;
                if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                    grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
                {
                    isViable = true;
                }
            }
        }
        else if (dir == generalDirection.East)
        {
            xCheck = roomCoords.x + wall.coords.x + 1;
            yCheck = roomCoords.y + wall.coords.y;
            if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
            {
                xCheck = roomCoords.x + wall.coords.x + 2;
                if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                    grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
                {
                    isViable = true;
                }
            }
        }
        else if (dir == generalDirection.West)
        {
            xCheck = roomCoords.x + wall.coords.x - 1;
            yCheck = roomCoords.y + wall.coords.y;
            if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
            {
                xCheck = roomCoords.x + wall.coords.x - 2;
                if (grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isOccupied == false &&
                    grid.GetComponent<gridManagerScript>().grid[xCheck, yCheck].GetComponent<gridUnitScript>().isVein == true)
                {
                    isViable = true;
                }
            }
        }

        return isViable;
    }

    WallPiece getLowerLeftCorner(WallPiece pieceOne, WallPiece pieceTwo)
    {
        int x = 0;
        int y = 0;

        if (pieceOne.coords.x < pieceTwo.coords.x ||
            pieceOne.coords.y < pieceTwo.coords.y)
        {
            x = pieceOne.coords.x;
            y = pieceOne.coords.y;
            return new WallPiece(x, y, pieceOne.wallType);
        }
        else
        {
            x = pieceTwo.coords.x;
            y = pieceTwo.coords.y;
            return new WallPiece(x, y, pieceTwo.wallType);
        }

        //if (pieceOne.coords.y <= pieceTwo.coords.y)
        //{
        //}
        //else
        //{
        //    y = pieceTwo.coords.y;
        //}

    }

    bool isLinkedDoorOnInnerCorner(roomProperties room, int door, ref bool primaryDoorIsCorner, ref bool secondaryDoorIsCorner)
    {
        bool doorIsOnInnerCorner = false;

        int linkedDoorId = room.doorList[door].linkedDoorId;
        roomProperties linkedRoom = room.doorList[door].adjacentRoom.GetComponent<roomProperties>();
        Coords linkedDoorCoords = linkedRoom.doorList[linkedDoorId].doorCoords;

        if (room.doorList[door].doorsOrient == generalDirection.North || room.doorList[door].doorsOrient == generalDirection.South)
        {
            if (isInnerCorner(linkedRoom.innerGrid[linkedDoorCoords.x][linkedDoorCoords.y]))
            {
               primaryDoorIsCorner = true; doorIsOnInnerCorner = true;
            }
            else if (isInnerCorner(linkedRoom.innerGrid[linkedDoorCoords.x + 1][linkedDoorCoords.y]))
            {
                secondaryDoorIsCorner = true; doorIsOnInnerCorner = true;
            }

        }
        else if (room.doorList[door].doorsOrient == generalDirection.West || room.doorList[door].doorsOrient == generalDirection.East)
        {
            if (isInnerCorner(linkedRoom.innerGrid[linkedDoorCoords.x][linkedDoorCoords.y]))
            {
                primaryDoorIsCorner = true; doorIsOnInnerCorner = true;
            }
            else if (isInnerCorner(linkedRoom.innerGrid[linkedDoorCoords.x][linkedDoorCoords.y + 1]))
            {
                secondaryDoorIsCorner = true; doorIsOnInnerCorner = true;
            }
        }
         

        return doorIsOnInnerCorner;
    }

    public void installDoorsToInnerGrid(ref List<GameObject> roomList)
    {
        // When you change the original roomList innerGrid you won't be able to reference the original
        //    You want to reference an unchanged copy
        List<GameObject> roomListNoChanges = roomList;

        for (int i = 0; i < roomList.Count; i++)
        {
            //Debug.Log("============== ROOM_" + i);
            roomProperties room = roomList[i].GetComponent<roomProperties>();
            room.startInnerGridChange();

            for (int door = 0; door < room.numDoors; door++)
            {
                if (room.doorList[door].doorUsedBool == true)
                {
                    bool primaryDoorIsCorner = false;
                    bool secondaryDoorIsCorner = false;

                    // Check the linked door to see if it's on the Inner Corner, will need to change it a bit if it is
                    isLinkedDoorOnInnerCorner(room, door, ref primaryDoorIsCorner, ref secondaryDoorIsCorner);

                    if (room.doorList[door].doorsOrient == generalDirection.North || room.doorList[door].doorsOrient == generalDirection.South)
                    {

                        // Check left/primary door
                        if (room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y] == "ICNW" ||
                            room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y] == "ICSW")
                        {
                            //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "WW";
                            room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "WW");
                        }
                        else
                        {
                            if (primaryDoorIsCorner)
                            {
                                if (room.doorList[door].doorsOrient == generalDirection.North)
                                {
                                    // THINK IT'S CORRECT
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "OCSE";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "OCSE");
                                }
                                else
                                {
                                    // THINK IT'S CORRECT
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "OCNE";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "OCNE");
                                }
                            }
                            else
                            {
                                //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "0";
                                room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "0");
                            }
                        }

                        // Check right/secondary door
                        if (room.innerGrid[room.doorList[door].doorCoords.x + 1][room.doorList[door].doorCoords.y] == "ICNE" ||
                            room.innerGrid[room.doorList[door].doorCoords.x + 1][room.doorList[door].doorCoords.y] == "ICSE")
                        {
                            //room.innerGrid[room.doorCoords[door].x + 1][room.doorCoords[door].y] = "WE";
                            room.editInnerGridChange(room.doorList[door].doorCoords.x + 1, room.doorList[door].doorCoords.y, "WE");
                        }
                        else
                        {
                            if (secondaryDoorIsCorner)
                            {
                                if (room.doorList[door].doorsOrient == generalDirection.North)
                                {
                                    // VERIFIED
                                    //room.innerGrid[room.doorCoords[door].x + 1][room.doorCoords[door].y] = "OCSW";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x + 1, room.doorList[door].doorCoords.y, "OCSW");
                                }
                                else
                                {
                                    // THINK IT'S CORRECT
                                    //room.innerGrid[room.doorCoords[door].x + 1][room.doorCoords[door].y] = "OCNW";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x + 1, room.doorList[door].doorCoords.y, "OCNW");
                                }
                            }
                            else
                            {
                                //room.innerGrid[room.doorCoords[door].x + 1][room.doorCoords[door].y] = "0";
                                room.editInnerGridChange(room.doorList[door].doorCoords.x + 1, room.doorList[door].doorCoords.y, "0");
                            }
                        }
                        //room.innerGrid[room.doorCoords[door].x + 1][room.doorCoords[door].y] = "0";
                    }
                    else if (room.doorList[door].doorsOrient == generalDirection.East || room.doorList[door].doorsOrient == generalDirection.West)
                    {
                        // Check bottom/primary door
                        if (room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y] == "ICSE" ||
                            room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y] == "ICSW")
                        {
                            room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "WS");
                            //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "WS";
                        }
                        else
                        {
                            if (primaryDoorIsCorner)
                            {
                                if (room.doorList[door].doorsOrient == generalDirection.East)
                                {
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "OCNW";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "OCNW");
                                }
                                else
                                {
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "OCNE";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "OCNE");
                                }
                            }
                            else
                            {
                                //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y] = "0";
                                room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y, "0");
                            }
                        }

                        // Check top/secondary door
                        if (room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y + 1] == "ICNE" ||
                            room.innerGrid[room.doorList[door].doorCoords.x][room.doorList[door].doorCoords.y + 1] == "ICNW")
                        {
                            //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y + 1] = "WN";
                            room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y + 1, "WN");
                        }
                        else
                        {
                            if (secondaryDoorIsCorner)
                            {
                                if (room.doorList[door].doorsOrient == generalDirection.East)
                                {
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y + 1] = "OCSW";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y + 1, "OCSW");
                                }
                                else
                                {
                                    //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y + 1] = "OCSE";
                                    room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y + 1, "OCSE");
                                }
                            }
                            else
                            {
                                //room.innerGrid[room.doorCoords[door].x][room.doorCoords[door].y + 1] = "0";
                                room.editInnerGridChange(room.doorList[door].doorCoords.x, room.doorList[door].doorCoords.y + 1, "0");
                            }
                        }
                    }
                }
            }

            
        }

        // Make edits permanent
        for (int i = 0; i < roomList.Count; i++)
        {
            roomProperties room = roomList[i].GetComponent<roomProperties>();
            room.stopInnerGridChange();
        }
    }
}
