using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;
using LockingClasses;
//using GridControllerClasses;

public class gridControllerScript : MonoBehaviour
{
    public GameObject gridManager;
    public GameObject mapGenerator;
    public GameObject map;

    public GameObject ProgressionTab;
    public GameObject ProgressionView;

    // Colors
    Color purple = new Color(.29f, .025f, .76f, .5f);
    Color red = new Color(.9725f, 0f, .0412f, .76f);
    Color green = new Color(.085f, .85f, .12f, .88f);
    Color blue = new Color(0f, .56f, .87f, 1f);
    Color white = new Color(1f, 1f, 1f, 1f);
    Color black = new Color(0f, 0f, 0f, 1f);

    // Grid bools
    public bool gridDoorSelect = false;
    public bool gridIsOccupiedSelect = false;
    public bool gridSelect = false;
    public bool gridIsVeinSelect = false;
    public int  gridZoneSelect = 0;

    intendedUnitType[] intendedTypeEnums = (intendedUnitType[])System.Enum.GetValues(typeof(intendedUnitType));
    int intendedEnumSelect = 0;
    public intendedUnitType intendedUnitTypeSelect = intendedUnitType.Vein;

    // Map bools
    public bool mapSelect = true;
    public bool firstTime = true;

    public int mapZoneIndexSelect = 0;

    int roomIndex = 0;

    // --------------------------------------------------------------------------
    // Pathing
    // --------------------------------------------------------------------------
    List<gridControllerClasses> locksController = new List<gridControllerClasses>();

    public gridControllerClasses findLockInLocksController(RoomLock passedInLock, GameObject room)
    {
        gridControllerClasses progButton = null;
        bool foundRecordedLock = false;

        if (locksController.Count > 0)
        {
            foreach (var recordedLock in locksController)
            {
                if (recordedLock.getRoomLock().Equals(passedInLock))
                {
                    progButton = recordedLock;
                    foundRecordedLock = true;
                    break;
                }
            }
        }
        

        if (foundRecordedLock == false)
        {
            Vector3 prevPosition = new Vector3();

            if (locksController.Count == 0)
                prevPosition = ProgressionView.transform.position;
            else
                prevPosition = locksController[locksController.Count - 1].getButtonPosition();

            Debug.Log("YOU CANT USE new() to create monobehavoir object. Need to attach the script to the object and init the object here");

            gridControllerClasses newRecordedLock = new gridControllerClasses(passedInLock, ref ProgressionTab, room, prevPosition);
            locksController.Add(newRecordedLock);
            progButton = newRecordedLock;
        }

        return progButton;
    }

    public void viewProgression()
    {

        List<GameObject> rooms = mapGenerator.GetComponent<MapGeneratorScript>().roomList;
        GameObject startRoom = null;

        foreach (var room in rooms)
        {
            if (room.name.Equals(GlobalDefines.startRoomName))
            {
                startRoom = room;
                break;
            }
        }

        List<GameObject> traversedRooms = new List<GameObject>();

        //Debug.Log("listOfUsedDoors: " + mapGenerator.GetComponent<MapGeneratorScript>().sendoffRoom.GetComponent<roomProperties>().getListOfUsedDoors().Count);

        progressThoughRooms(traversedRooms, startRoom);
    }

    void progressThoughRooms(List<GameObject> traversedRooms, GameObject currentRoom)
    {
        traversedRooms.Add(currentRoom);

        if (currentRoom.GetComponent<roomProperties>().roomIsLocked() == true)
        {
            Debug.Log("Room Is Locked: " + currentRoom.name);
        }

        List<Door> listOfDoors = currentRoom.GetComponent<roomProperties>().doorList;
        List<RoomLock> currentRoomLocks = currentRoom.GetComponent<roomProperties>().getRoomLocks();

        for (int i = 0; i < listOfDoors.Count; i++)
        {
            if (listOfDoors[i].doorUsedBool == true)
            {
                Door currentDoor = listOfDoors[i];
                RoomLock currentRoomLock = new RoomLock();
                bool doorIsLocked = false;
                bool doorIsLockedOnOtherSide = false;

                for (int j = 0; j < currentRoomLocks.Count; j++)
                {
                    if (currentRoomLocks[j].getDoorId() == i)
                    {
                        currentRoomLock = currentRoomLocks[j];
                        gridControllerClasses recordedButton = findLockInLocksController(currentRoomLock, currentRoom);

                        doorIsLocked = !recordedButton.getIsToggled();
                        //doorIsLockedOnOtherSide 
                    }
                }

                // If not already traveled to
                if (traversedRooms.Contains(currentDoor.adjacentRoom) == false && doorIsLocked == false)
                {
                    progressThoughRooms(traversedRooms, currentDoor.adjacentRoom);
                }
            }
            
        }
    }

    public void printAllLockedRooms()
    {
        Debug.Log("========== Locked Rooms ==========");
        MapGeneratorScript.zoneManager.printAllLockedAbilityRooms();
    }

    List<PathingClasses.Paths> alreadyTraveledToPath = new List<PathingClasses.Paths>();
    PathingClasses.Paths currentPath = new PathingClasses.Paths();
    List<PathingClasses.SubArea> alreadyTraveledToSubAreas = new List<PathingClasses.SubArea>();
    PathingClasses.SubArea currentSubArea = new PathingClasses.SubArea();
    public void selectZone()
    {
        selectNonVeinZone(true);

        // Reseting branch pathing selection
        alreadyTraveledToPath = new List<PathingClasses.Paths>();
        currentPath = new PathingClasses.Paths();

        // Reseting sub area selection
        alreadyTraveledToSubAreas = new List<PathingClasses.SubArea>();
        currentSubArea = new PathingClasses.SubArea();
    }

    public void selectZoneLocks()
    {
        int selectedIndex = selectAnyZone(true);

        bool zoneSelected = false;
        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;

        for (int i = 0; i < zones.zoneList.Count; i++)
        {
            // Get all locks for selected zone
            List<ZoneLock> locks = MapGeneratorScript.zoneManager.lockManager.getLocksWithZoneId(zones.zoneList[i].getZoneId());

            if (zones.zoneList[i].getZoneId() == selectedIndex)
            {
                zoneSelected = true;

                Debug.Log("ZONE: " + zones.zoneList[i].getZoneId() +
                        "\nLOCK COUNT: " + locks.Count);
            }
            else
                zoneSelected = false;

            

            for (int j = 0; j < locks.Count; j++)
            {
                GameObject lockedRoom = locks[j].getRoom();
                int doorIndex = locks[j].getDoorNum();
                GameObject door = lockedRoom.transform.Find("Door" + doorIndex.ToString()).gameObject;

                if (zoneSelected)
                {
                    changeDoorColor(ref door, Color.Lerp(red, white, 0.5f));
                }
                else
                {
                    changeDoorColor(ref door, black);
                }
            }
        }
        
    }

    public void clearZoneLocks()
    {

        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;

        for (int i = 0; i < zones.zoneList.Count; i++)
        {
            // Get all locks for current zone
            List<ZoneLock> locks = MapGeneratorScript.zoneManager.lockManager.getLocksWithZoneId(zones.zoneList[i].getZoneId());

            for (int j = 0; j < locks.Count; j++)
            {
                GameObject lockedRoom = locks[j].getRoom();
                int doorIndex = locks[j].getDoorNum();
                GameObject door = lockedRoom.transform.Find("Door" + doorIndex.ToString()).gameObject;

                changeDoorColor(ref door, black);
            }
        }
    }

    public void selectNextSubArea()
    {
        int selectedZoneId = selectNonVeinZone(false);

        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;
        PathingClasses.PathMapper pathingMapper = zones.getZone(selectedZoneId).pathMapper;

        bool resetTraveledToSubAreaList = false;
        currentSubArea = pathingMapper.getNextSubAreaStart(currentSubArea, ref alreadyTraveledToSubAreas, ref resetTraveledToSubAreaList);
        // PathingClasses.SubArea subArea = pathingManager.subAreas[subAreaSelect];
        // branchingPath = pathManager.getNextBranchingPathStart(currentPath, ref alreadyTraveledToPath);

        Debug.Log("THERE'S A CHANCE THAT selectNextSubArea() IS NOT FUNCTIONING CORRECTLY");

        List<GameObject> rooms = mapGenerator.GetComponent<MapGeneratorScript>().roomList;
        List<GameObject> selectedSubAreaRooms = new List<GameObject>();
        List<GameObject> selectedSubAreaDeadEndRooms = new List<GameObject>();

        for (int i = 0; i < currentSubArea.mainPath.Count; i++)
        {
            selectedSubAreaRooms.AddRange(currentSubArea.mainPath[i].path);
        }
        for (int i = 0; i < currentSubArea.deadEnd.Count; i++)
        {
            selectedSubAreaDeadEndRooms.AddRange(currentSubArea.deadEnd[i].path);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            GameObject room = rooms[i];
            if (selectedSubAreaRooms.Contains(room))
                changeRoomColor(ref room, Color.Lerp(purple, white, 0.5f));
            else if (selectedSubAreaDeadEndRooms.Contains(room))
                changeRoomColor(ref room, purple);
            else
                changeRoomColor(ref room, white);
        }
    }

    public void selectNextBranchPath()
    {
        int selectedZoneId = selectNonVeinZone(false);

        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;
        PathingClasses.Zone zone = zones.getZone(selectedZoneId);
        PathingClasses.PathMapper pathMapper = zone.pathMapper;
        PathingClasses.Paths branchingPath = null;

        // Get the next branching path
        branchingPath = pathMapper.getNextBranchingPathStart(currentPath, ref alreadyTraveledToPath);
        currentPath = branchingPath;

        List<GameObject> rooms = mapGenerator.GetComponent<MapGeneratorScript>().roomList;
        List<GameObject> branchingBranchRooms = new List<GameObject>();

        for (int i = 0; i < branchingPath.branches.Count; i++)
        {
            branchingBranchRooms.AddRange(branchingPath.branches[i].path);
        }


        for (int i = 0; i < rooms.Count; i++)
        {
            GameObject room = rooms[i];
            if (branchingPath.endRooms.Contains(room))
                changeRoomColor(ref room, Color.Lerp(green, white, 0.5f));
            else if (branchingPath.path.Contains(room))
                changeRoomColor(ref room, green);
            else if (branchingBranchRooms.Contains(room))
                changeRoomColor(ref room, Color.Lerp(green, black, 0.5f));
            else
                changeRoomColor(ref room, white);
        }

    }

    int selectNonVeinZone(bool incrementZone)
    {
        int selectedIdType = MapGeneratorScript.zoneManager.zoneList[mapZoneIndexSelect].getZoneTypeId();
        int selectedId = MapGeneratorScript.zoneManager.zoneList[mapZoneIndexSelect].getZoneId();

        while (selectedIdType == GlobalDefines.defaultId || selectedIdType == GlobalDefines.veinIntendedId || selectedIdType == GlobalDefines.startZoneId)
        {
            mapZoneIndexSelect++;
            if (mapZoneIndexSelect >= MapGeneratorScript.zoneManager.zoneList.Count)
                mapZoneIndexSelect = 0;
            selectedIdType = MapGeneratorScript.zoneManager.zoneList[mapZoneIndexSelect].getZoneTypeId();
            selectedId = MapGeneratorScript.zoneManager.zoneList[mapZoneIndexSelect].getZoneId();
        }

        if (incrementZone)
        {
            mapZoneIndexSelect++;
            if (mapZoneIndexSelect >= MapGeneratorScript.zoneManager.zoneList.Count)
                mapZoneIndexSelect = 0;
        }

        return selectedId;
    }

    int selectAnyZone(bool incrementZone)
    {
        int selectedId = MapGeneratorScript.zoneManager.zoneList[mapZoneIndexSelect].getZoneId();

        if (incrementZone)
        {
            mapZoneIndexSelect++;
            if (mapZoneIndexSelect >= MapGeneratorScript.zoneManager.zoneList.Count)
                mapZoneIndexSelect = 0;
        }

        return selectedId;
    }

    public void selectCommonPath()
    {
        bool zoneSelected = false;
        int selectedId = selectNonVeinZone(false);

        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;

        for (int i = 0; i < zones.zoneList.Count; i++)
        {
            if (zones.zoneList[i].getZoneId() == selectedId)
                zoneSelected = true;
            else
                zoneSelected = false;

            for (int j = 0; j < zones.zoneList[i].pathMapper.commonPath.path.Count; j++)
            {
                List<GameObject> commonPath = zones.zoneList[i].pathMapper.commonPath.path;


                GameObject room = commonPath[j];

                if (zoneSelected)
                {
                    roomProperties roomProps = room.GetComponent<roomProperties>();

                    if (roomProps.isZoneEntrance)
                        changeRoomColor(ref room, Color.Lerp(blue, white, 0.5f));
                    else
                        changeRoomColor(ref room, blue);
                }
                else
                {
                    changeRoomColor(ref room, white);
                }
            }
        }
    }

    public void selectRoomsInZone()
    {
        PathingClasses.ZoneManager zones = MapGeneratorScript.zoneManager;

        PathingClasses.Zone selectedZone = zones.zoneList[mapZoneIndexSelect];
        List<int> adjacentZoneIds = zones.zoneList[mapZoneIndexSelect].getAdjacentZoneIds();
        mapZoneIndexSelect++;
        if (mapZoneIndexSelect >= zones.zoneList.Count)
            mapZoneIndexSelect = 0;

        Debug.Log(  "ZONE: " + selectedZone.getZoneId() + "\n" 
                    + "ZONE TYPE: " + selectedZone.getZoneTypeId() + "\n"
                    + "ZONE ORDER: " + selectedZone.getZoneOrder());

        for (int i = 0; i < zones.zoneList.Count; i++)
        {
            bool zoneSelected = false;
            bool adjacentZoneSelected = false;
            if (zones.zoneList[i].getZoneId() == selectedZone.getZoneId())
                zoneSelected = true;
            else if (adjacentZoneIds.Contains(zones.zoneList[i].getZoneId()))
                adjacentZoneSelected = true;
            else
                zoneSelected = false;



            for (int j = 0; j < zones.zoneList[i].roomsList.Count; j++)
            {
                GameObject room = zones.zoneList[i].roomsList[j];

                if (zoneSelected)
                {
                    roomProperties roomProps = room.GetComponent<roomProperties>();

                    if (roomProps.isZoneEntrance) 
                        changeRoomColor(ref room, Color.Lerp(purple, white, 0.5f));
                    else
                        changeRoomColor(ref room, purple);
                }
                else if (adjacentZoneSelected)
                {
                    changeRoomColor(ref room, Color.Lerp(red, white, 0.5f));
                }
                else
                {
                    changeRoomColor(ref room, white);
                }
            }
        }
    }

    // --------------------------------------------------------------------------
    // 
    // --------------------------------------------------------------------------

    public void selectDoors()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                if (gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isDoor == true)
                {
                    if (!gridDoorSelect)
                    {
                        gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(230f, 0f, 0f, .27f);
                    }
                    else
                    {
                        gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
                    }
                }
            }
        }

        gridDoorSelect = !gridDoorSelect;
    }

    public void selectNextMainGridType()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                GameObject gridUnit = gridManager.GetComponent<gridManagerScript>().grid[x, y];
                if (gridUnit.GetComponent<gridUnitScript>().intendedType != intendedUnitType.None)
                {
                    // Clear previous select
                    if (intendedUnitTypeSelect != gridUnit.GetComponent<gridUnitScript>().intendedType)
                    {
                        gridUnit.GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
                    }

                    // Select new zone
                    if (intendedUnitTypeSelect == gridUnit.GetComponent<gridUnitScript>().intendedType)
                    {
                        gridUnit.GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(.9725f, .8074f, 0f, .7529f);
                    }
                    
                }
            }
        }

        intendedEnumSelect++;
        if (intendedEnumSelect >= intendedTypeEnums.Length)
        {
            intendedEnumSelect = 0;
        }
        intendedUnitTypeSelect = intendedTypeEnums[intendedEnumSelect];
    }

    public void selectNextZone()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                GameObject gridUnit = gridManager.GetComponent<gridManagerScript>().grid[x, y];
                if (gridUnit.GetComponent<gridUnitScript>().zoneArea.Count != 0)
                {
                    for (int i = 0; i < gridUnit.GetComponent<gridUnitScript>().zoneArea.Count; i++)
                    {
                        // Clear previous zone
                        if (gridZoneSelect-1 == gridUnit.GetComponent<gridUnitScript>().zoneArea[i])
                        {
                            gridUnit.GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
                        }

                        // Select new zone
                        if (gridZoneSelect == gridUnit.GetComponent<gridUnitScript>().zoneArea[i])
                        {
                            gridUnit.GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = purple; 
                        }
                    }
                }
            }
        }

        gridZoneSelect++;
        if (gridZoneSelect > gridManager.GetComponent<gridManagerScript>().xPOI.Count)
        {
            gridZoneSelect = 0;
        }
    }

    public void selectVeins()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                if (!gridIsVeinSelect)
                {
                    if (gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isVein == true ||
                    gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isVeinMain == true)
                    {
                        if (gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isVeinMain == true)
                        {
                            gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(.07f, .51f, .07f, .50f);
                        }
                        else
                        {
                            gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(.0251f, .86f, .02f, .50f);
                        }
                    }
                }
                else
                {
                    gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
                }
            }
        }

        if (!gridIsVeinSelect)
        {
            for (int k = 0; k < gridManager.GetComponent<gridManagerScript>().xPOICore.Count; k++)
            {
                int x = gridManager.GetComponent<gridManagerScript>().xPOICore[k];
                int y = gridManager.GetComponent<gridManagerScript>().yPOICore[k];
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (0 < x + i && x + i < xMax && 0 < y + j && y + j < yMax)
                        {
                            gridManager.GetComponent<gridManagerScript>().grid[x + i, y + j].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(1f, .42f, 0f, .58f);
                        }
                    }
                }
            }

            for (int k = 0; k < gridManager.GetComponent<gridManagerScript>().xPOI.Count; k++)
            {
                int x = gridManager.GetComponent<gridManagerScript>().xPOI[k];
                int y = gridManager.GetComponent<gridManagerScript>().yPOI[k];
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (0 < x + i && x + i < xMax && 0 < y + j && y + j < yMax)
                        {
                            gridManager.GetComponent<gridManagerScript>().grid[x + i, y + j].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(1f, .74f, 0f, .58f);
                        }
                    }
                }
            }
        }
       gridIsVeinSelect = !gridIsVeinSelect;
    }

    public void hideMap()
    {
        List<GameObject> rooms = mapGenerator.GetComponent<MapGeneratorScript>().roomList;

        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].gameObject.SetActive(!mapSelect);
        }
        mapSelect = !mapSelect;
    }

    public void hideGrid()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                gridManager.GetComponent<gridManagerScript>().grid[x, y].SetActive(!gridSelect);
            }
        }
        gridSelect = !gridSelect;
    }

    public void selectIsOccupied()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);

        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                if (gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().isOccupied == true)
                {
                    if (!gridIsOccupiedSelect)
                    {
                        gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(0f, 13f, 240f, .27f);
                    }
                    else
                    {
                        gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
                    }
                    
                }
            }
        }
        gridIsOccupiedSelect = !gridIsOccupiedSelect;
    }

    public void clearGrid()
    {
        int xMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(0);
        int yMax = gridManager.GetComponent<gridManagerScript>().grid.GetLength(1);


        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                gridManager.GetComponent<gridManagerScript>().grid[x, y].GetComponent<gridUnitScript>().GetComponent<SpriteRenderer>().color = new Color(255f, 255f, 255f, .27f);
            }
        }
        gridIsOccupiedSelect = false;
        gridDoorSelect = false;
        gridZoneSelect = 0;
    }


    public void showAdjacentRooms()
    {
        List<GameObject> rooms = mapGenerator.GetComponent<MapGeneratorScript>().roomList;
        GameObject tempRoom;

        for (int i = 0; i < rooms.Count; i++)
        {
            tempRoom = rooms[i];
            changeRoomColor(ref tempRoom, white);
        }

        GameObject selectedRoom = rooms[roomIndex];
        roomProperties indexroomProps = selectedRoom.GetComponent<roomProperties>();

        changeRoomColor(ref selectedRoom, red);

        for (int i = 0; i < indexroomProps.doorList.Count; i++)
        {
            if (indexroomProps.doorList[i].doorUsedBool == true)
            {
                tempRoom = indexroomProps.doorList[i].adjacentRoom;
                changeRoomColor(ref tempRoom, purple);
            }
        }

        roomIndex++;
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
}
