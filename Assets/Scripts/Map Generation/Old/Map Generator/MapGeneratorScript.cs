 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

using Classes;
using Enums;
using RoomBuilder;
using PathingClasses;
public class MapGeneratorScript : MonoBehaviour
{
    public List<GameObject> roomList = new List<GameObject>();
    public GameObject mapSaver;

    public GameObject startRoom;
    public GameObject sendoffRoom;
    public GameObject largeRoom; // x/y = 138.24 pixels
    public GameObject smallRoom; // x/y = 69.12  pixels
    public GameObject LRoom;

    public GameObject fluidRoom;
    public GameObject Door;

    public GameObject map;
    public GameObject roomManager;
    //int totalRoomsToGenerate = 2;

    public GameObject grid;

    public enum Direction { Up, Down, Left, Right };

    public const int minEdgeLength = 3;

    edgesAndDoorScript edgesAndDoorManager;
    progressionGeneratorScript progressionManager;

    public static ZoneManager zoneManager = new ZoneManager(); // 0 is the start room and sendoff room
    static RoomBuilderManager roomBuilderManager = new RoomBuilderManager();

    // Start is called before the first frame update
    void Start_Old()
    {
        edgesAndDoorManager = new edgesAndDoorScript(Door, ref grid);

        int seed = 1550803247;// (int)System.DateTime.Now.Ticks;
        seed = seed < 0 ? seed * -1 : seed;
        Random.InitState(seed);
        Debug.Log("Seed: " + seed);

        /*

        // Start off grid generation, with room veins and zone presets
        List<RoomPreset> bossRoomLocations = new List<RoomPreset>();
        grid.GetComponent<gridManagerScript>().createGrid(ref bossRoomLocations);



        // Generate the rooms
        generateRandomRooms(bossRoomLocations);
        lastRoomUpdate();


        // Generate inner zone and zone to zone progression
        progressionManager = new progressionGeneratorScript(ref grid, ref roomList);
        progressionManager.generateProgression();
        roomList = progressionManager.getUpdatedRoomList();

        // Finally build each room


        // Save generated rooms in a save file
        mapSaver.GetComponent<mapSaverScript>().saveMap(roomList);
        */
    }

    void generateRandomRooms(List<RoomPreset> bossRoomLocations)
    {
        // Create the base starting room
        Coords position = new Coords(0, 0);

        GameObject currentRoom = createStartingRoom(ref position);

        // Initialize
        float doorHeight = currentRoom.transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.y;
        float doorWidth = currentRoom.transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x;

        // Set up start BookMark coords
        Queue<BookMark> bookMarks = new Queue<BookMark>();

        // Sendoff room properties, getting the doors to be the BookMarks. BookMarks need to be on unOccupied grids
        roomProperties tempRoomProperties = roomList[1].GetComponent<roomProperties>();
        bookMarks.Enqueue(new BookMark(5, tempRoomProperties.doorList[5].doorsOrient, roomList[1],
                                tempRoomProperties.gridCoords.x + tempRoomProperties.doorList[5].doorCoords.x - 1, 
                                tempRoomProperties.gridCoords.y + tempRoomProperties.doorList[5].doorCoords.y));
        bookMarks.Enqueue(new BookMark(4, tempRoomProperties.doorList[4].doorsOrient, roomList[1],
                                tempRoomProperties.gridCoords.x + tempRoomProperties.doorList[4].doorCoords.x, 
                                tempRoomProperties.gridCoords.y + tempRoomProperties.doorList[4].doorCoords.y - 1));
        bookMarks.Enqueue(new BookMark(3, tempRoomProperties.doorList[3].doorsOrient, roomList[1],
                                tempRoomProperties.gridCoords.x + tempRoomProperties.doorList[3].doorCoords.x + 1, 
                                tempRoomProperties.gridCoords.y + tempRoomProperties.doorList[3].doorCoords.y));

        // Create boos rooms and add bookmarks
        handleBossRoomPresets(bossRoomLocations, ref bookMarks);

        int count = 0;
        while (bookMarks.Count != 0)
        {
            //Debug.Log("TOP LOOP: " + bookMarks.Count);
            BookMark currentBookMark = bookMarks.Peek();

            // Maybe use rooms as bookmarks?
            GameObject newRoom = null;
            newRoom = getRoom(ref currentBookMark);
            bookMarks.Dequeue();
            //Debug.Log("TOP LOOP2: " + bookMarks.Count);


            if (newRoom != null)
            {
                // Make the room a child of MapGenerator.Map
                newRoom.transform.SetParent(map.transform);
                newRoom.name = GlobalDefines.roomNamePrefix + count;
                roomList.Add(newRoom);

                edgesAndDoorManager.findNextBookMarks(ref newRoom, ref bookMarks);
                //nameRoom = nameRoom.Remove(4, 1).Insert(4, (count + 1).ToString());


                count++;
            }
            //if (count > 46)
            //{
            //    break;
            //}
        }

        // Then go back through each room, check for unused doors and try to expand rooms if they touch an unconnected room
        squeezeInMoreRooms();

        // Installs door to all of the room's inner grid for saving purposes
        edgesAndDoorManager.installDoorsToInnerGrid(ref roomList);

    }

    void lastRoomUpdate()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            updateDoorsUsed(i);
        }
    }

    void updateDoorsUsed(int roomIndex)
    {
        GameObject room = roomList[roomIndex];
        roomProperties roomProps = room.GetComponent<roomProperties>();
        roomProps.totalDoorsUsed = 0;

        for (int i = 0; i < roomProps.doorList.Count; i++)
        {
            if (roomProps.doorList[i].doorUsedBool == true)
                roomProps.totalDoorsUsed++;
        }

        //Debug.Log("                     ROOM: " + room.name + "     Doors: " + roomProps.totalDoorsUsed);
    }

    void squeezeInMoreRooms()
    {
        int maxArea = 250;

        // Go through each room and try to expand rooms, only if the expansion lets them touch another room
        for (int i = 0; i < roomList.Count; i++)
        {
            roomProperties roomProps = roomList[i].GetComponent<roomProperties>();
            GameObject currentRoom = roomList[i];

            //print(currentRoom.name);

            // If you can add doors to the room
            if (roomProps.canAddDoors == true)
            {
                // Go through each door and try to expand the room if it touches a new room
                for (int j = 0; j < roomProps.doorList.Count; j++)
                {
                    DimensionList dimList = new DimensionList();

                    // If the door isn't used
                    if (roomProps.doorList[j].doorUsedBool == false)
                    {
                        //print("DOOR" + j + "   isUsed: " + roomProps.doorList[j].doorUsedBool);

                        int x = roomProps.gridCoords.x + roomProps.doorList[j].doorCoords.x;
                        int y = roomProps.gridCoords.y + roomProps.doorList[j].doorCoords.y;
                        if (roomProps.doorList[j].doorsOrient == generalDirection.North)
                            y++;
                        else if (roomProps.doorList[j].doorsOrient == generalDirection.South)
                            y--;
                        else if (roomProps.doorList[j].doorsOrient == generalDirection.East)
                            x++;
                        else if (roomProps.doorList[j].doorsOrient == generalDirection.West)
                            x--;

                        getDimensionsForFill(maxArea, x, y, ref dimList);

                        bool newConnections = false;

                        //if (currentRoom.name == "Room28" && j == 1)
                        //{
                        //    print("28  Door 1   DIMS: " + dimList.xMin + "," + dimList.yMin + " | " + dimList.xMax + "," + dimList.yMax);
                        //    dimList.printGrid();
                        //}

                        if (dimList.list.Count != 0)
                        {
                            //print("DIMS: " + dimList.xMin + "," + dimList.yMin + " | " + dimList.xMax + "," + dimList.yMax);
                            //print(currentRoom.name);

                            newConnections = edgesAndDoorManager.checkForConnections(currentRoom, dimList);
                            //print("HERE_3");

                            if (newConnections)
                            {
                                //print("=================DETECTED POSSIBLE EXPANSION=================");
                                //print(currentRoom.name + "    " + dimList.xMin + "," + dimList.yMin + " | " + dimList.xMax + "," + dimList.yMax);

                                // When we expand the room, we create a new room and delete the old room
                                //   So we have to refresh the reference in the roomList
                                expandRoom(ref currentRoom, dimList);
                                roomList[i] = currentRoom;
                            }
                        }
                    }
                }
            }
        }

        // Go through each room and find any possible door connections
        for (int i = 0; i < roomList.Count; i++)
        {
            roomProperties roomProps = roomList[i].GetComponent<roomProperties>();
            GameObject currentRoom = roomList[i];

            //print(currentRoom.name);

            // If you can add doors to the room
            if (roomProps.canAddDoors == true)
            {
                // Check all of the edges for new door connections and install them
                edgesAndDoorManager.checkAndInstallDoorConnections(ref currentRoom);
            }

            // Also refresh the adjacent rooms for each room
            for (int j = 0; j < roomProps.doorList.Count; j++)
            {
                if (roomProps.doorList[j].doorUsedBool == true)
                {
                    string name = roomProps.doorList[j].adjacentRoom.name;

                    for (int k = 0; k < roomList.Count; k++)
                    {
                        if (roomList[k].name == roomProps.doorList[j].adjacentRoom.name)
                        {
                            roomProps.doorList[j].adjacentRoom = roomList[k];
                            break;
                        }
                    }
                }
            }
        }
    }

    void expandRoom(ref GameObject room, DimensionList dimsToAdd)
    {
        roomProperties roomProps = room.GetComponent<roomProperties>();

        // Add new squares to the room
        for (int i = 0; i < dimsToAdd.list.Count; i++)
        {
            roomProps.dimList.addDimension(dimsToAdd.list[i]);
        }

        GameObject newRoom = configureFluidRoom(roomProps.dimList);
        newRoom.name = room.name;

        // Snap room before the doors
        // Tansfer all of the doors to the newRoom
        snapRoomToGridAndMarkGrids(ref roomProps.dimList.xMin, ref roomProps.dimList.yMin, ref newRoom);
        edgesAndDoorManager.transferDoorList(ref newRoom, ref room);

        // Reassign room
        Destroy(room);
        room = newRoom;
        newRoom.transform.SetParent(map.transform);
    }

    void handleBossRoomPresets(List<RoomPreset> bossRoomLocations, ref Queue<BookMark> bookMarks)
    {
        float gridSize = grid.GetComponent<gridManagerScript>().gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.x;

        for (int i = 0; i < bossRoomLocations.Count; i++)
        {
            //Debug.Log("BOSS ROOM ID: " + bossRoomLocations[i].bossRoomId);
            //Debug.Log("BOSS ROOM COORDS: " + bossRoomLocations[i].coords.x + "," + bossRoomLocations[i].coords.y);

            DimensionList dimensionList = new DimensionList();
            GameObject newRoom = Instantiate(fluidRoom, new Vector3(0, 0, 0), Quaternion.identity);
            roomProperties roomProps = newRoom.GetComponent<roomProperties>();

            // Setup the room properties
            roomManager.GetComponent<roomManagerScript>().configureNewBossRoom(ref newRoom,  bossRoomLocations[i], gridSize, ref dimensionList);

            List<GameObject> doorReferenceList = new List<GameObject>();
            for (int j = 0; j < roomProps.numDoors; j++)
            {
                // Instantiate new doors  
                GameObject newDoor = Instantiate(Door, new Vector3(0, 0, 0), Quaternion.identity);
                newDoor.name = "Door" + j;
                newDoor.transform.parent = newRoom.transform;
                doorReferenceList.Add(newDoor);
            }
            
            // Snap room before the doors
            snapRoomToGridAndMarkGrids(ref dimensionList.xMin, ref dimensionList.yMin, ref newRoom);

            for (int j = 0; j < roomProps.numDoors; j++)
            {
                GameObject door = doorReferenceList[j];
                edgesAndDoorManager.snapDoorToGrid(ref door, roomProps.doorList[j].doorCoords.x + roomProps.gridCoords.x,
                                        roomProps.doorList[j].doorCoords.y + roomProps.gridCoords.y, roomProps.doorList[j].doorsOrient);

                int xBookMark = roomProps.gridCoords.x + roomProps.doorList[j].doorCoords.x;
                int yBookMark = roomProps.gridCoords.y + roomProps.doorList[j].doorCoords.y;
                if (roomProps.doorList[j].doorsOrient == generalDirection.North)
                    yBookMark++;
                else if (roomProps.doorList[j].doorsOrient == generalDirection.South)
                    yBookMark--;
                else if (roomProps.doorList[j].doorsOrient == generalDirection.East)
                    xBookMark++;
                else if (roomProps.doorList[j].doorsOrient == generalDirection.West)
                    xBookMark--;

                if (grid.GetComponent<gridManagerScript>().grid[xBookMark, yBookMark].GetComponent<gridUnitScript>().isVein == true)
                {
                    //print("BOOKMARK: " + xBookMark + "," + yBookMark);
                    bookMarks.Enqueue(new BookMark(j, roomProps.doorList[j].doorsOrient, newRoom, xBookMark, yBookMark));
                }

            }

            // Make the room a child of MapGenerator.Map
            newRoom.transform.SetParent(map.transform);
            newRoom.name = GlobalDefines.bossRoomNamePrefix + i;
            roomList.Add(newRoom);


        }
    }

    GameObject getRoom(ref BookMark currentBookMark)
    {
        int maxArea = 160;

        int xMin = currentBookMark.x;
        int xMax = currentBookMark.x;
        int yMin = currentBookMark.y;
        int yMax = currentBookMark.y;

        DimensionList dimensionList = new DimensionList();

        //print("-----------------------------------------------------");
        //print("-------Start: " + xMin + "," + yMin);
        //print("-----------------------------------------------------");

        getDimensionsNormal(maxArea, xMin, yMin, ref dimensionList);

        //    print("MIN: " + dimensionList.xMin + "," + dimensionList.yMin);
        //    print("MAX: " + dimensionList.xMax + "," + dimensionList.yMax);

        GameObject newRoom = null;
        if (dimensionList.area < (maxArea / 2)){
            // Don't add this room
            return newRoom; 
        }
        newRoom = configureFluidRoom(dimensionList);

        Coords newDoorCoords = new Coords(currentBookMark.x - dimensionList.xMin, currentBookMark.y - dimensionList.yMin);
        roomProperties currentRoomProperties = newRoom.GetComponent<roomProperties>();
        if (currentBookMark.orientation == generalDirection.North)
            currentRoomProperties.addEmptyDoor(newDoorCoords, generalDirection.South);
        else if (currentBookMark.orientation == generalDirection.South)
            currentRoomProperties.addEmptyDoor(newDoorCoords, generalDirection.North);
        else if (currentBookMark.orientation == generalDirection.West)
            currentRoomProperties.addEmptyDoor(newDoorCoords, generalDirection.East);
        else if (currentBookMark.orientation == generalDirection.East)
            currentRoomProperties.addEmptyDoor(newDoorCoords, generalDirection.West);

        // Create a new door  
        GameObject newDoor = Instantiate(Door, new Vector3(0, 0, 0), Quaternion.identity);
        newDoor.name = "Door0";
        newDoor.transform.parent = newRoom.transform;

        // Snap room before the room
        snapRoomToGridAndMarkGrids(ref dimensionList.xMin, ref dimensionList.yMin, ref newRoom);

        edgesAndDoorManager.snapDoorToGrid(ref newDoor, currentBookMark.x, currentBookMark.y, newRoom.GetComponent<roomProperties>().doorList[0].doorsOrient);

        linkNewRoomAndCurrentRoom(ref newRoom, ref currentBookMark.room, ref currentBookMark.doorNum, 0);

        return newRoom;
    }

    GameObject configureFluidRoom(DimensionList dimensionList)
    {
        GameObject newRoom = Instantiate(fluidRoom, new Vector3(0, 0, 0), Quaternion.identity);

        float gridSize = grid.GetComponent<gridManagerScript>().gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.x;

        // Setup the room properties
        roomManager.GetComponent<roomManagerScript>().configureNewFluidRoom(ref newRoom, gridSize, dimensionList);

        return newRoom;
    }

    void getDimensions(int maxArea, int xStart, int yStart, ref DimensionList dimensionList, int minLength)
    {
        bool dimensionRejected = false;

        int xMin = xStart;
        int yMin = yStart;
        int xMax = xStart;
        int yMax = yStart;

        if (grid.GetComponent<gridManagerScript>().grid[xStart, yStart].GetComponent<gridUnitScript>().isOccupied == true)
        {
            // To speed up generation, if the bookmark isOccupied then return immediatly
            return;
        }

        Coords center = new Coords(xStart, yStart);

        LinkedList<Coords> coordsToCheck = new LinkedList<Coords>();
        coordsToCheck.AddFirst(new Coords(xStart, yStart));
        Coords currentCoords = coordsToCheck.First.Value;

        while (dimensionList.area < maxArea)
        {
            //print("TOP OF WHILE");
            if (coordsToCheck.Count == 0)
            {
                break;
            }
            else
            {
                currentCoords = coordsToCheck.First.Value;
                coordsToCheck.RemoveFirst();
            }

            xMin = currentCoords.x;
            yMin = currentCoords.y;
            xMax = currentCoords.x;
            yMax = currentCoords.y;

            //    print("CHECKING:     " + xMin + "," + yMin);

            center.x = currentCoords.x;
            center.y = currentCoords.y;

            expandAroundPoint(ref xMin, ref yMin, ref xMax, ref yMax);

            // If any of the sides a shorter than 5 then reject the square
            if (xMax - xMin < minLength - 1 || yMax - yMin < minLength - 1)
            {
                //    print("REJECTED: " + xMin + "," + yMin);
                //    print("REJECTED: " + xMax + "," + yMax);
                // Don't add anything
            }
            else
            {
                //    print("          ADDING:     " + xMin + "," + yMin);
                //    print("          ADDING:     " + xMax + "," + yMax);

                //print("HERE_0");
                //print("MIN: " + xMin + "," + yMin);
                //print("MAX: " + xMax + "," + yMax);
                dimensionRejected = dimensionList.addDimension(new SquareArea(xMin, yMin, xMax, yMax, center.x, center.y));
                //print("DIM REJECTED: " + dimensionRejected);

                if (dimensionRejected == false)
                {
                    // Search for more dimensions and add them to coordsToCheck
                    findAdjacentStartPoints(dimensionList, center, ref coordsToCheck, xStart, yStart);
                }
            }
            //print("coords To Check Count: " + coordsToCheck.Count);

            //print("Area: " + dimensionList.area);
            //print("Max Area: " + maxArea);

        }

        // Need to do a final check to make sure that there aren't any square areas in the dim list that are touching by only 1 unit
        dimensionList.finalCheck();
    }

    void getDimensionsForFill(int maxArea, int xStart, int yStart, ref DimensionList dimensionList)
    {
        int minLength = 3;
        getDimensions(maxArea, xStart, yStart, ref dimensionList, minLength);
    }

    void getDimensionsNormal(int maxArea, int xStart, int yStart, ref DimensionList dimensionList)
    {
        int minLength = 5;
        getDimensions(maxArea, xStart, yStart, ref dimensionList, minLength);
    }

    // Searches an added squares top/bot/left/right surrounding for a place to add a new square
    void findAdjacentStartPoints(DimensionList dimensionList, Coords center, ref LinkedList<Coords> coordsToCheck, int xStart, int yStart)
    {
        int startDisplacement = 11;
        //int wiggleDisplacement = 2; // Meant to move the displacement a little in the perpendicular directions


        int x = 0;
        int y = 0;

        bool foundNewPoint = false;

        // Four for each direction
        for (int i = 0; i < 4; i++)
        {
            foundNewPoint = false;
            int displacement = startDisplacement;

            if (i == 0)
            {
                x = center.x + displacement;
                y = center.y;
            }
            else if (i == 1)
            {
                x = center.x - displacement;
                y = center.y;
            }
            else if (i == 2)
            {
                x = center.x;
                y = center.y + displacement;
            }
            else if (i == 3)
            {
                x = center.x;
                y = center.y - displacement;
            }

            //print("DISPLACEMENT START: " + x + "," + y);

            while (!foundNewPoint)
            {
                //print("DISPLACEMENT: " + x + "," + y);
                bool tooCloseToPreviouslyAttemptedSquareCore = false;
                foundNewPoint = checkDisplacentAndWiggle(ref tooCloseToPreviouslyAttemptedSquareCore, dimensionList, ref coordsToCheck, x, y, xStart, yStart);
                if (tooCloseToPreviouslyAttemptedSquareCore)
                    break;
                
                // If not change the displacement so that it's closer to the original point
                if (foundNewPoint == false)
                {
                    displacement--;

                    if (i == 0)
                    {
                        x = center.x + displacement;
                    }
                    else if (i == 1)
                    {
                        x = center.x - displacement;
                    }
                    else if (i == 2)
                    {
                        y = center.y + displacement;
                    }
                    else if (i == 3)
                    {
                        y = center.y - displacement;
                    }

                    if (displacement <= 6)
                    {
                        // Don't get too close or else it's just the same square
                        break;
                    }
                }
            }
        }
    }

    bool checkDisplacentAndWiggle(ref bool tooCloseToPreviouslyAttemptedSquareCore, DimensionList dimensionList, ref LinkedList<Coords> coordsToCheck, int x, int y, int xStart, int yStart)
    {
        bool foundNewPoint = false;
        GameObject[,] gridAccess = grid.GetComponent<gridManagerScript>().grid;

        // If the grid is a vein and is not occupied and the point is not already added then add the point
        //    pointAlreadyChecked() is needed to avoid an infinite loop when 2 "gaps" between squares is deemed addable
        //        The dimension list will reject the square gaps, but 
        if (gridAccess[x, y].GetComponent<gridUnitScript>().isVein == true &&
            gridAccess[x, y].GetComponent<gridUnitScript>().isOccupied == false)
        {
            if (dimensionList.pointTooCloseToPreviouslyAttemptedSquareCore(x, y) == true)
            {
                tooCloseToPreviouslyAttemptedSquareCore = true;
            }
            else
            {
                addCoordsToList(ref coordsToCheck, x, y, xStart, yStart);
                foundNewPoint = true;
            }
        }

        return foundNewPoint;
    }

    void addCoordsToList(ref LinkedList<Coords> coordsToCheck, int x, int y, int xStart, int yStart)
    {
        // Need to add the coords into the list ordered by how far they are from the starting coords

        float newDistance = Mathf.Sqrt(Mathf.Pow(xStart - x, 2) + Mathf.Pow(yStart - y, 2));
        int listCount = coordsToCheck.Count;

        //print("Count: " + listCount);

        if (coordsToCheck.Count == 0)
        {
            //print("===========END");
            coordsToCheck.AddLast(new Coords(x, y));
        }
        else
        {
            for (LinkedListNode<Coords> node = coordsToCheck.First; node != null; node = node.Next){
                float currentDistance = Mathf.Sqrt(Mathf.Pow(xStart - node.Value.x, 2) + Mathf.Pow(yStart - node.Value.y, 2));

                //print("Count: " + listCount);

                // If we get to the end of the list
                if (node.Equals(coordsToCheck.Last))
                {
                    if (newDistance < currentDistance)
                    {
                        //print("======================================================================================SORTED");
                        //print("NODE: " + node.Value.x + "," + node.Value.y);
                        //print("NEW NODE: " + x + "," + y);
                        coordsToCheck.AddBefore(node, new Coords(x, y));
                    }
                    else
                    {
                        //print("=====================================================================================END");
                        coordsToCheck.AddAfter(node, new Coords(x, y));
                    }
                    
                    break;
                }
                // Add it to the list ordered by distance
                else if (newDistance < currentDistance)
                {
                    //print("======================================================================================SORTED");
                    coordsToCheck.AddBefore(node, new Coords(x, y));
                    
                    break;
                }
            }
        }
        
        //coordsToCheck.AddLast(new Coords(x, y));
    }

    void expandAroundPoint(ref int xMin, ref int yMin, ref int xMax, ref int yMax)
    {
        float maxPointArea = 120f;
        float area = 0f;

        float notVeinPercentage = .50f;

        int increment = 1;

        bool xMinLocked = false;
        bool xMaxLocked = false;
        bool yMinLocked = false;
        bool yMaxLocked = false;

        //bool debug = false;
        //if (xMin == 313 && yMin == 87)
        //{
        //    debug = true;
        //}


        // Every so often try to unlock the locked boundries
        //   Helps in very odd cases, where the start point is close to non veins
        int unlockTime = 3;
        int unlockMax = 2;
        int xMinLockedTimer = 0; int xMaxLockedTimer = 0;
        int yMinLockedTimer = 0; int yMaxLockedTimer = 0;
        int xMinUnlockCount = 0; int xMaxUnlockCount = 0;
        int yMinUnlockCount = 0; int yMaxUnlockCount = 0;
         
        while (area < maxPointArea)
        {
            
            // Unlock dimensions code
            if (xMinLocked && xMinLockedTimer > unlockTime && xMinUnlockCount < unlockMax)
            {
                xMinLocked = false;
                xMinUnlockCount++;
            }
            if (xMaxLocked && xMaxLockedTimer > unlockTime && xMaxUnlockCount < unlockMax)
            {
                xMaxLocked = false;
                xMaxUnlockCount++;
            }
            if (yMinLocked && yMinLockedTimer > unlockTime && yMinUnlockCount < unlockMax)
            {
                yMinLocked = false;
                yMinUnlockCount++;
            }
            if (yMaxLocked && yMaxLockedTimer > unlockTime && yMaxUnlockCount < unlockMax)
            {
                yMaxLocked = false;
                yMaxUnlockCount++;
            }

            // Increment y dimensions that aren't locked
            if (!yMinLocked)
            {
                yMin = yMin - increment;
                yMinLockedTimer = 0;
            }
            else
            {
                yMinLockedTimer++;
            }
            if (!yMaxLocked)
            {
                yMax = yMax + increment;
                yMaxLockedTimer = 0;
            }
            else
            {
                yMaxLockedTimer++;
            }

            int minNotVeinCount = 0;
            int maxNotVeinCount = 0;

            //if (debug)
            //{
                //print("     START     ");
                //print("XLOCKED: " + xMinLocked + "  " + xMaxLocked);
                //print("YLOCKED: " + yMinLocked + "  " + yMaxLocked);
                //print("x: " + xMin + ", " + xMax);
                //print("y: " + yMin + ", " + yMax);
            //}

            // Because we are checking the top and bottom first we can't take the changed x axis into account
            // The top and bottoms might be fine to expand, but xMin or xMax might be in an occupied room
            // Check the top/bottom perimeter
            for (int x = xMin; x <= xMax; x++) // +1 and -1 ARE NEEDED!!!! READ ABOVE
            {
                if (yMinLocked && yMaxLocked) break;

                if (!yMinLocked)
                {
                    //    print(" IN yMin: " + x + "," + yMin);
                    
                    // If it's occupied then don't expand the bounds
                    if (grid.GetComponent<gridManagerScript>().grid[x, yMin].GetComponent<gridUnitScript>().isOccupied == true)
                    {
                        yMinLocked = true;
                        yMin++;
                        //break;
                    }
                    // If it's not a vein then check the total non vein count
                    else if (grid.GetComponent<gridManagerScript>().grid[x, yMin].GetComponent<gridUnitScript>().isVein == false)
                    {
                        minNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)minNotVeinCount / Mathf.Abs(xMax - xMin)) > notVeinPercentage)
                        {
                            yMinLocked = true;
                            yMin++;
                            //break;
                        }
                    }
                }

                if (!yMaxLocked)
                {
                    //if (debug)
                    //{
                    //   print(" IN yMax: " + x + "," + yMax);

                    //}
                    if (grid.GetComponent<gridManagerScript>().grid[x, yMax].GetComponent<gridUnitScript>().isOccupied == true)
                    {
                        yMaxLocked = true;
                        yMax--;
                        //break;
                    }
                    // If it's not a vein then check the total non vein count
                    else if (grid.GetComponent<gridManagerScript>().grid[x, yMax].GetComponent<gridUnitScript>().isVein == false)
                    {
                        maxNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)maxNotVeinCount / Mathf.Abs(xMax - xMin)) > notVeinPercentage)
                        {
                            yMaxLocked = true;
                            yMax--;
                            //break;
                        }
                    }
                }
            }

            //if (debug)
            //{
            //    print("YLOCKED: " + yMinLocked + "  " + yMaxLocked);
            //    print("y: " + yMin + ", " + yMax);
            //}


            // Increment dimensions that aren't locked
            if (!xMinLocked)
            {
                xMin = xMin - increment;
                xMinLockedTimer = 0;
            }
            else
            {
                xMinLockedTimer++;
            }
            if (!xMaxLocked)
            {
                xMax = xMax + increment;
                xMaxLockedTimer = 0;
            }
            else
            {
                xMaxLockedTimer++;
            }

            // If all 4 dimensions are locked then break
            if (xMinLocked && xMaxLocked && yMinLocked && yMaxLocked) break;

            minNotVeinCount = 0;
            maxNotVeinCount = 0;

            // Check the left/right perimeter
            for (int y = yMin; y <= yMax; y++)
            {
                if (xMinLocked && xMaxLocked) break;


                //print("test  " + ((float)minNotVeinCount / Mathf.Abs(yMax - yMin)));
                if (!xMinLocked)
                {
                    // If it's occupied then don't expand the bounds
                    if (grid.GetComponent<gridManagerScript>().grid[xMin, y].GetComponent<gridUnitScript>().isOccupied == true)
                    {
                        xMinLocked = true;
                        xMin++;
                        //break;
                    }
                    // If it's not a vein then check the total non vein count
                    else if (grid.GetComponent<gridManagerScript>().grid[xMin, y].GetComponent<gridUnitScript>().isVein == false)
                    {
                        minNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)minNotVeinCount / Mathf.Abs(yMax - yMin)) > notVeinPercentage)
                        {

                            xMinLocked = true;
                            xMin++;
                            //break;
                        }
                    }
                }

                if (!xMaxLocked)
                {
                    /*if (debug)
                    {
                        print(" IN: " + xMax + "," + y);
                    }
                    */
                    if (grid.GetComponent<gridManagerScript>().grid[xMax, y].GetComponent<gridUnitScript>().isOccupied == true)
                    {
                        xMaxLocked = true;
                        xMax--;
                        //break;
                    }
                    // If it's not a vein then check the total non vein count
                    else if (grid.GetComponent<gridManagerScript>().grid[xMax, y].GetComponent<gridUnitScript>().isVein == false)
                    {
                        maxNotVeinCount++;
                        //print("RIGHT: " + (float)((float)maxNotVeinCount / Mathf.Abs(yMax - yMin)));
                        //print("            " + maxNotVeinCount);
                        // If there are too many non vein grids then don't expand
                        if ((float)((float)maxNotVeinCount / Mathf.Abs(yMax - yMin)) > notVeinPercentage)
                        {
                            xMaxLocked = true;
                            xMax--;
                            //break;

                        }
                    }
                }
            }

            //if (debug)
            //{
            //    print("XLOCKED: " + xMinLocked + "  " + xMaxLocked);
            //    print("x: " + xMin + ", " + xMax);
            //}

            area = ((Mathf.Abs(xMax - xMin) + 1) * (Mathf.Abs(yMax - yMin) + 1));
        }
    }

    void configureNewRoom(ref int newRoomXPos, ref int newRoomYPos, ref GameObject newRoom, ref GameObject currentRoom, ref int newUnusedDoor, ref int newRoomDoor)
    {
        snapRoomToGridAndMarkGrids(ref newRoomXPos, ref newRoomYPos, ref newRoom);

        linkNewRoomAndCurrentRoom(ref newRoom, ref currentRoom, ref newUnusedDoor, newRoomDoor);

        findAdjacentDoorsAndConnect(ref newRoom);

        // Check all rooms in the current room to see if we need
        if ((currentRoom.GetComponent<roomProperties>().totalDoorsUsed) == (currentRoom.GetComponent<roomProperties>().numDoors))
        {
            currentRoom.GetComponent<roomProperties>().allRoomsUsed = true;
        }
        if ((newRoom.GetComponent<roomProperties>().totalDoorsUsed) == (newRoom.GetComponent<roomProperties>().numDoors))
        {
            newRoom.GetComponent<roomProperties>().allRoomsUsed = true;
        }
    }

    void checkAdjacentRoom(ref GameObject newRoom, int x, int y, roomProperties.doorOrientation dir)
    {
        int xAdjacentRoom = x;
        int yAdjacentRoom = y;
        if (dir == roomProperties.doorOrientation.North)
        {
            yAdjacentRoom++;
        }
        else if (dir == roomProperties.doorOrientation.South)
        {
            yAdjacentRoom--;
        }
        else if (dir == roomProperties.doorOrientation.East)
        {
            xAdjacentRoom++;
        }
        else if (dir == roomProperties.doorOrientation.West)
        {
            xAdjacentRoom--;
        }

        if (grid.GetComponent<gridManagerScript>().grid[xAdjacentRoom, yAdjacentRoom].GetComponent<gridUnitScript>().isDoor == true &&
            grid.GetComponent<gridManagerScript>().grid[xAdjacentRoom, yAdjacentRoom].GetComponent<gridUnitScript>().room != newRoom)
        {
            GameObject adjacentRoom = grid.GetComponent<gridManagerScript>().grid[xAdjacentRoom, yAdjacentRoom].GetComponent<gridUnitScript>().room;

            // Turn on doors
            grid.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().door.SetActive(true); 
            grid.GetComponent<gridManagerScript>().grid[xAdjacentRoom, yAdjacentRoom].GetComponent<gridUnitScript>().door.SetActive(true);
            
            // Update totalDoorsUsed
            newRoom.GetComponent<roomProperties>().totalDoorsUsed++;
            adjacentRoom.GetComponent<roomProperties>().totalDoorsUsed++;

            // Get the door number from filtering out Door from door.name
            string newDoorStr = grid.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().door.name.Replace("Door", "");
            string adjacentDoorStr = grid.GetComponent<gridManagerScript>().grid[xAdjacentRoom, yAdjacentRoom].GetComponent<gridUnitScript>().door.name.Replace("Door", "");

            int newRoomDoor = System.Convert.ToInt32(newDoorStr);
            int adjacentRoomDoor = System.Convert.ToInt32(adjacentDoorStr);

            // Set adjacentRoom variable for the new room and the found adjacent
            newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].adjacentRoom = adjacentRoom;
            adjacentRoom.GetComponent<roomProperties>().doorList[adjacentRoomDoor].adjacentRoom = newRoom;
            newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].linkedDoorId = adjacentRoomDoor;
            adjacentRoom.GetComponent<roomProperties>().doorList[adjacentRoomDoor].linkedDoorId = newRoomDoor;

            // Set doorUsedBool variable for the new room and the found adjacent
            newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].doorUsedBool = true;
            adjacentRoom.GetComponent<roomProperties>().doorList[adjacentRoomDoor].doorUsedBool = true;
        }
    }

    void findAdjacentDoorsAndConnect(ref GameObject newRoom)
    {
        for (int i = 0; i < newRoom.GetComponent<roomProperties>().numDoors; i++)
        {
            
            // If door is already being used skip it
            if (newRoom.GetComponent<roomProperties>().doorList[i].doorUsedBool == true)
            {
                continue;
            }
            int x = newRoom.GetComponent<roomProperties>().doorList[i].doorCoords.x + newRoom.GetComponent<roomProperties>().gridCoords.x;
            int y = newRoom.GetComponent<roomProperties>().doorList[i].doorCoords.y + newRoom.GetComponent<roomProperties>().gridCoords.y;

            checkAdjacentRoom(ref newRoom, x, y, roomProperties.doorOrientation.North);
            checkAdjacentRoom(ref newRoom, x, y, roomProperties.doorOrientation.South);
            checkAdjacentRoom(ref newRoom, x, y, roomProperties.doorOrientation.East);
            checkAdjacentRoom(ref newRoom, x, y, roomProperties.doorOrientation.West);
        }
    }
 
    void linkNewRoomAndCurrentRoom(ref GameObject newRoom, ref GameObject currentRoom, ref int newUnusedDoor, int newRoomDoor)
    {
        // Links choosen rooms together, sets doors as visable and used

        // Link current room and new room
        currentRoom.GetComponent<roomProperties>().doorList[newUnusedDoor].adjacentRoom = newRoom;
        newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].adjacentRoom = currentRoom;
        currentRoom.GetComponent<roomProperties>().doorList[newUnusedDoor].linkedDoorId = newRoomDoor;
        newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].linkedDoorId = newUnusedDoor;

        // Set the choosen doors to being used
        currentRoom.GetComponent<roomProperties>().doorList[newUnusedDoor].doorUsedBool = true;
        newRoom.GetComponent<roomProperties>().doorList[newRoomDoor].doorUsedBool = true;

        currentRoom.transform.Find("Door" + newUnusedDoor).gameObject.SetActive(true);
        newRoom.transform.Find("Door" + newRoomDoor).gameObject.SetActive(true);

        currentRoom.GetComponent<roomProperties>().totalDoorsUsed++;
        newRoom.GetComponent<roomProperties>().totalDoorsUsed++;
    }
    
    GameObject createStartingRoom(ref Coords position)
    {
        float gridSize = grid.GetComponent<gridManagerScript>().gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.x;

        // Create the base starting room
        GameObject roomStart = Instantiate(startRoom, new Vector3(0, 0, 0), Quaternion.identity);
        roomManager.GetComponent<roomManagerScript>().configureNewRoom(ref roomStart, gridSize);
        roomStart.transform.SetParent(map.transform);
        roomStart.name = GlobalDefines.startRoomName;
        //rooms.Add(0, roomStart);
        roomList.Add(roomStart);

        // Get the grid center and transform room to be centered at it
        grid.GetComponent<gridManagerScript>().getGridStartRoom(ref position.x, ref position.y);
        position.x = position.x - (roomStart.GetComponent<roomProperties>().gridRange.x / 2);
        position.y = position.y - (roomStart.GetComponent<roomProperties>().gridRange.y / 2);

        snapRoomToGridAndMarkGrids(ref position.x, ref position.y, ref roomStart);

        // Setup the Sendoff room
        GameObject roomSendOff = Instantiate(sendoffRoom, new Vector3(0, 0, 0), Quaternion.identity);
        roomManager.GetComponent<roomManagerScript>().configureNewRoom(ref roomSendOff, gridSize);
        roomSendOff.transform.SetParent(map.transform);
        roomSendOff.name = GlobalDefines.sendOffRoomName;
        //rooms.Add(1, roomSendOff);
        roomList.Add(roomSendOff);

        int newUnusedDoor = 1;
        int newRoomDoor = 1;
        int newRoomXPos =
                roomStart.GetComponent<roomProperties>().gridCoords.x + roomStart.GetComponent<roomProperties>().doorList[newUnusedDoor].doorCoords.x
                - roomSendOff.GetComponent<roomProperties>().doorList[newRoomDoor].doorCoords.x;
        int newRoomYPos =
            roomStart.GetComponent<roomProperties>().gridCoords.y + roomStart.GetComponent<roomProperties>().doorList[newUnusedDoor].doorCoords.y
            - roomSendOff.GetComponent<roomProperties>().doorList[newRoomDoor].doorCoords.y - 1;

        
        configureNewRoom(ref newRoomXPos, ref newRoomYPos, ref roomSendOff, ref roomStart, ref newUnusedDoor, ref newRoomDoor);

        return roomSendOff;
    }
 
    void snapRoomToGridAndMarkGrids(ref int gridXPos, ref int gridYPos, ref GameObject room)
    {
        // Snaps rooms to x,y of the grid
        // Selected grids are marked as isOccupied, isDoor

        float xPos = 0;
        float yPos = 0;
        float roomHeight = room.GetComponent<roomProperties>().height;//.GetComponent<SpriteRenderer>().bounds.size.y;
        float roomWidth = room.GetComponent<roomProperties>().width;//.GetComponent<SpriteRenderer>().bounds.size.x;


        // getGridPosition returns the bottom left corner coordinats of the grid provided
        //     That would snap the rooms center at the left corner, so you have to offset by half the hight/width
        grid.GetComponent<gridManagerScript>().getGridPosition(ref gridXPos, ref gridYPos, ref xPos, ref yPos);
        room.transform.position = new Vector3(xPos + (roomWidth / 2), yPos + (roomHeight / 2), 0);
        room.GetComponent<roomProperties>().gridCoords.x = gridXPos;
        room.GetComponent<roomProperties>().gridCoords.y = gridYPos;

        //print("ROOM COORDS: " + gridXPos + "," + gridYPos);

        // Mark the doors
        for (int i = 0; i < room.GetComponent<roomProperties>().doorList.Count; i++)
        {
            int x = room.GetComponent<roomProperties>().doorList[i].doorCoords.x + gridXPos;
            int y = room.GetComponent<roomProperties>().doorList[i].doorCoords.y + gridYPos;
            //print(i + "     " + x + "," + y);

            grid.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isDoor = true;

            if (room.GetComponent<roomProperties>().doorList[i].doorsOrient == generalDirection.North ||
                room.GetComponent<roomProperties>().doorList[i].doorsOrient == generalDirection.South)
            {
                grid.GetComponent<gridManagerScript>().grid[x+1, y].GetComponent<gridUnitScript>().isDoor = true;
            }
            else 
            {
                grid.GetComponent<gridManagerScript>().grid[x, y + 1].GetComponent<gridUnitScript>().isDoor = true;
            }

            grid.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().door = room.transform.Find("Door" + i).gameObject;
        }

        // Mark the grid
        for (int x = 0; x < room.GetComponent<roomProperties>().grids2.Count; x++)
        {
            for (int y = 0; y < room.GetComponent<roomProperties>().grids2[0].Count; y++)
            {
                if (room.GetComponent<roomProperties>().getGridMap(x, y) == true)
                {
                    grid.GetComponent<gridManagerScript>().grid[x + gridXPos, y + gridYPos].GetComponent<gridUnitScript>().isOccupied = true;
                    grid.GetComponent<gridManagerScript>().grid[x + gridXPos, y + gridYPos].GetComponent<gridUnitScript>().isUsed = true;
                    grid.GetComponent<gridManagerScript>().grid[x + gridXPos, y + gridYPos].GetComponent<gridUnitScript>().room = room;
                }
            }
        }
    }

    public string getNextInt(string input, int index)
    {
        string output = "";
        //Debug.Log(input + "|");
        if (index + 1 >= input.Length)
        {
            output = "";
        }
        else if (input[index + 1] == '0' || input[index + 1] == '1' || input[index + 1] == '2' || input[index + 1] == '3' || input[index + 1] == '4' ||
            input[index + 1] == '5' || input[index + 1] == '6' || input[index + 1] == '7' || input[index + 1] == '8' || input[index + 1] == '9')
        {
            output = input[index + 1] + "" + getNextInt(input, index + 1);
        }

        return output;
    }
}
