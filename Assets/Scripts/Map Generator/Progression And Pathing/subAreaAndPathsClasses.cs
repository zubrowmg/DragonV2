using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;

namespace PathingClasses
{

    public class SimplePath
    {
        public List<GameObject> path;
        public List<GameObject> endRooms;

        public SimplePath()
        {
            this.path = new List<GameObject>();
            this.endRooms = new List<GameObject>();
        }

        public SimplePath(List<GameObject> orderedPath)
        {
            this.path = orderedPath;
            this.endRooms = new List<GameObject> { orderedPath[0], orderedPath[orderedPath.Count-1]};
        }

        public SimplePath(GameObject head, GameObject tail, List<GameObject> path)
        {
            this.endRooms = new List<GameObject>();
            this.endRooms.Add(head);
            this.endRooms.Add(tail);

            this.path = new List<GameObject>(path);
        }

        public bool isRoomInPath(GameObject room)
        {
            bool roomIsInPath = false;
            if (path.Contains(room))
                roomIsInPath = true;
            return roomIsInPath;
        }

        public GameObject getRoomInPathViaIndex(GameObject startEndRoom, int index)
        {
            GameObject room = new GameObject();

            List<GameObject> tempPath = path;
            if (tempPath[0].Equals(startEndRoom) == false)
                tempPath.Reverse();

            if (index >= tempPath.Count)
                Debug.LogError("SimplePath - getRoomInPathViaIndex(). Requested a room outside of the path index: " + index);
            else
                room = tempPath[index];

            return room;
        }

        public Door getConnectingDoorInPath(GameObject roomOne, GameObject roomTwo)
        {
            Door connectingDoor = null;
            bool doorFound = false;

            List<Door> roomOneDoors = roomOne.GetComponent<roomProperties>().getListOfUsedDoors();

            for (int i = 0; i < roomOneDoors.Count; i++)
            {
                Door currentDoor = roomOneDoors[i];
                if (currentDoor.adjacentRoom.Equals(roomTwo))
                {
                    doorFound = true;
                    connectingDoor = currentDoor;
                }
            }

            if (doorFound == false)
                Debug.LogError("SimplePath - getConnectingDoorInPath(). Requested connecting door between non adjacent rooms. \n\tRoom1: " + roomOne.name + "\t\n\tRoom2: " + roomTwo.name);


            return connectingDoor;
        }

        public List<GameObject> getPath()
        {
            return path;
        }

        public List<GameObject> getEndRoomList()
        {
            return endRooms;
        }

        public void setPath(List<GameObject> newPath)
        {
            path = newPath;
        }

        public void setEndRooms(GameObject head, GameObject tail)
        {
            endRooms.Add(head);
            endRooms.Add(tail);
        }

        public int getPathCount()
        {
            return path.Count;
        }
    }


    // In the future use Class SimplePath inside of this class
    public class Paths
    {
        public SubArea subAreaParent;

        public List<GameObject> endRooms;
        public List<GameObject> path;
        public List<Paths> branches;

        public bool isConnectedToCommonPath;

        public Paths()
        {
            this.subAreaParent = new SubArea();
            this.endRooms = new List<GameObject>();
            this.path = new List<GameObject>();
            this.branches = new List<Paths>();
            this.isConnectedToCommonPath = false;
        }

        public Paths(GameObject head, GameObject tail, List<GameObject> path, bool isConnectedToCommonPath)
        {
            this.subAreaParent = new SubArea();
            this.endRooms = new List<GameObject>();
            this.endRooms.Add(head);
            this.endRooms.Add(tail);

            this.path = new List<GameObject>(path);
            this.branches = new List<Paths>();
            this.isConnectedToCommonPath = false;

            
        }

        public void setIsConnectedToCommonPathTrue()
        {
            isConnectedToCommonPath = true;
        }

        public bool isRoomInPath(GameObject room)
        {
            bool roomIsInPath = false;
            if (path.Contains(room))
                roomIsInPath = true;
            return roomIsInPath;
        }

        public List<GameObject> getPathList()
        {
            return path;
        }

        public List<GameObject> getEndRoomList()
        {
            return endRooms;
        }

        public bool getIsConnectedToCommonPath()
        {
            return isConnectedToCommonPath;
        }
    }

    public class CommonPath
    {
        public List<GameObject> path;

        public CommonPath()
        {
            this.path = new List<GameObject>();
        }

        public bool isRoomInPath(GameObject room)
        {
            bool roomIsInPath = false;
            if (path.Contains(room))
                roomIsInPath = true;
            return roomIsInPath;
        }
    }

    public class SubArea
    {
        // A sub area is defined as:
        //      A clump of paths off of the common path
        //      Or a clump of paths off of another subarea, with only 1 path connecting the two sub areas
        // A clump should ideally be 10-15ish rooms, with at least 3-4 paths

        public bool isUsed;
        public List<Paths> mainPath;
        public List<Paths> deadEnd; // Meant for a path that connects to this SubArea via 1 path, with only 1 total Paths 

        public List<SubArea> connectingSubArea; // Meant for a sub area that connects off of this current sub area with more than 2 paths
        public SubArea parentSubArea;
        // Could probably add an entrance dictionary to get sub area entrances quicker

        public List<GameObject> allRooms;

        public bool offOfCommonPath;
        public bool containsBossRoom;

        public SubArea()
        {
            this.isUsed = false;
            this.mainPath = new List<Paths>();
            this.deadEnd = new List<Paths>();

            this.connectingSubArea = new List<SubArea>();
            this.parentSubArea = null;

            this.allRooms = new List<GameObject>();
            this.offOfCommonPath = false;
            this.containsBossRoom = false;
        }

        public SubArea(List<Paths> paths, bool isConnectedToCommonPath)
        {
            this.isUsed = true;
            this.mainPath = new List<Paths>();
            this.mainPath.AddRange(paths);
            this.deadEnd = new List<Paths>();

            this.connectingSubArea = new List<SubArea>();
            this.parentSubArea = null;

            this.allRooms = getAllRoomsFromPathsList(paths);
            this.offOfCommonPath = isConnectedToCommonPath;

            bool pathsContainsBossRoom = false;
            for (int i = 0; i < paths.Count; i++)
            {
                if (pathsContainsBossRoom)
                    break;

                for (int j = 0; j < paths[i].path.Count; j++)
                {
                    GameObject currentRoom = paths[i].path[j];
                    if (currentRoom.name.Contains(GlobalDefines.bossRoomNamePrefix))
                    {
                        pathsContainsBossRoom = true;
                    }

                    currentRoom.GetComponent<roomProperties>().setParentSubArea(this);
                }
            }
            this.containsBossRoom = pathsContainsBossRoom;
        }

        List<GameObject> getAllRoomsFromPathsList(List<Paths> pathList)
        {
            List<GameObject> allRoomsNoDuplicates = new List<GameObject>();
            for (int i = 0; i < pathList.Count; i++)
            {
                for (int j = 0; j < pathList[i].path.Count; j++)
                {
                    if (allRoomsNoDuplicates.Contains(pathList[i].path[j]) == false)
                        allRoomsNoDuplicates.Add(pathList[i].path[j]);
                }
            }
            return allRoomsNoDuplicates;
        }

        public void addDeadEnd(List<Paths> newDeadEnd)
        {
            if (newDeadEnd.Count > 1)
                Debug.LogError("SubArea addDeadEnd() - Adding a new dead end that has a more paths than 1");
            deadEnd.AddRange(newDeadEnd);
        }

        public void addDeadEnd(Paths newDeadEnd)
        {
            deadEnd.Add(newDeadEnd);
        }

        public void addConnectingSubArea(SubArea newConnectingSubArea)
        {
            connectingSubArea.Add(newConnectingSubArea);
        }

        public void addParentSubArea(SubArea parentSubArea)
        {
            this.parentSubArea = parentSubArea;
        }

        public bool doesSubAreaContainBossRoom()
        {
            return containsBossRoom;
        }

        public bool isConnectedToCommonPath()
        {
            return offOfCommonPath;
        }

        public List<Paths> getPathConnectedToCommonPath()
        {
            List<Paths> pathsConnectedToCommonPath = new List<Paths>();

            for (int i = 0; i < mainPath.Count; i++)
            {
                if (mainPath[i].isConnectedToCommonPath == true)
                    pathsConnectedToCommonPath.Add(mainPath[i]);
            }

            return pathsConnectedToCommonPath;
        }

        public List<GameObject> getAllRooms()
        {
            return allRooms;
        }

        public List<Paths> getAllPaths()
        {
            List<Paths> allPaths = new List<Paths>();
            allPaths.AddRange(mainPath);
            allPaths.AddRange(deadEnd);
            return allPaths;
        }

        public List<GameObject> getAllCommonPathEntrances(List<GameObject> commonPath)
        {
            List<GameObject> commonPathEntrances = new List<GameObject>();

            for (int i = 0; i < mainPath.Count; i++)
            {
                Paths currentPath = mainPath[i];
                if (currentPath.getIsConnectedToCommonPath())
                {
                    // Only need to check the end rooms
                    List<GameObject> roomList = currentPath.getEndRoomList();
                    for (int j = 0; j < roomList.Count; j++)
                    {
                        if (commonPath.Contains(roomList[j]) == true)
                        {
                            commonPathEntrances.Add(roomList[j]);
                        }
                    }
                }
                
            }

            return commonPathEntrances;
        }
    }

    public class SubAreaClump
    {
        List<SubArea> listOfSubAreas;
        List<SubArea> commonPathSubAreas;

        SubAreaClumpType clumpType;

        public SubAreaClump()
        {
            listOfSubAreas = new List<SubArea>();
            commonPathSubAreas = new List<SubArea>();

            clumpType = SubAreaClumpType.None;
        }

        void addSubArea(SubArea newSubArea)
        {
            listOfSubAreas.Add(newSubArea);
            if (newSubArea.isConnectedToCommonPath())
                commonPathSubAreas.Add(newSubArea);
        }

        public void addSubAreaList(List<SubArea> newSubAreaList)
        {
            for (int i = 0; i < newSubAreaList.Count; i++)
            {
                addSubArea(newSubAreaList[i]);
            }
        }

        public bool listContains(SubArea subAreaToCheck)
        {
            bool clumpDoesContainSubArea = listOfSubAreas.Contains(subAreaToCheck);
            return clumpDoesContainSubArea;
        }
        
        public List<SubArea> getCommonPathSubAreas()
        {
            return commonPathSubAreas;
        }

        public List<SubArea> getListOfSubAreas()
        {
            return listOfSubAreas;
        }

        public void setClumpType(SubAreaClumpType type)
        {
            clumpType = type;
        }

        public SubAreaClumpType getClumpType()
        {
            return clumpType;
        }

        public bool roomIsInSubAreaClump(GameObject room)
        {
            bool roomExists = false;
            for (int i = 0; i < listOfSubAreas.Count; i++)
            {
                if (listOfSubAreas[i].allRooms.Contains(room))
                {
                    roomExists = true;
                    break;
                }
            }
            return roomExists;
        }

        public List<GameObject> getAllRoomsInClump()
        {
            List<GameObject> allRooms = new List<GameObject>();
            foreach (var subArea in listOfSubAreas)
            {
                List<GameObject> subAreaRooms = subArea.getAllRooms();
                foreach (var room in subAreaRooms)
                {
                    if (allRooms.Contains(room) == false)
                    {
                        allRooms.Add(room);
                    }
                }
            }
            return allRooms;
        }
    }

    public class PathMapper
    {
        // This class will take a zone and break it down into subarea/paths/common path
        //      Does not do any progression generation
        public CommonPath commonPath;
        public List<Paths> branchingPaths;

        public List<SubArea> subAreas;


        public PathMapper()
        {
            this.commonPath = new CommonPath();
            this.branchingPaths = new List<Paths>();
            this.subAreas = new List<SubArea>();
        }

        // ==========================================================
        // Part 2
        // ==========================================================

        // Returns all subareas that aren't in the accounted for list in the provided zone
        public void getAllSubAreasInAZoneThatArentInProvidedList(ref List<SubArea> listOfRemainingSubAreas, List<SubArea> listOfAccountedForSubAreas)
        {
            for (int i = 0; i < subAreas.Count; i++)
            {
                SubArea currentSubArea = subAreas[i];
                if (listOfAccountedForSubAreas.Contains(currentSubArea) == false)
                    listOfRemainingSubAreas.Add(currentSubArea);
            }
        }

        // Will get all subareas that touch a subarea that leads to the common path, starting from the boss room
        public void getSubAreasThatLeadToBossRoom(ref Zone zone, ref SubArea bossSubArea, ref List<SubArea> listOfSubAreasLeadingToBossRoom, ref List<SubArea> listOfSubAreasBeyondBossSubArea)
        {
            List<SubArea> alreadyTraveledToSubAreas = new List<SubArea>();
            SubArea currentSubArea = new SubArea();
            bool resetTraveledToSubAreaList = false;

            // Find the boss sub area
            while (currentSubArea.containsBossRoom == false)
            {
                currentSubArea = getNextSubAreaStart(currentSubArea, ref alreadyTraveledToSubAreas, ref resetTraveledToSubAreaList);
                if (currentSubArea.containsBossRoom)
                    bossSubArea = currentSubArea;
            }

            // Get all sub area "paths" that lead to the common path
            //      Also get all sub areas that are blocked off by the boss subarea, aka subareas connected to the boss subarea but they don't connect to the main path
            getSubAreaPathsToCommonPathStart(bossSubArea, ref listOfSubAreasLeadingToBossRoom, ref listOfSubAreasBeyondBossSubArea);
        }

        // Will get all subareas that lead to the common path (and any subareas that touch subareas that touch the common path)
        //      If a clump of subareas does not touch the common path, then it does not count
        void getSubAreaPathsToCommonPathStart(SubArea currentSubArea,
                                                            ref List<SubArea> listOfSubAreasConnectedToCommonPath, ref List<SubArea> listOfSubAreasNotTouchingCommonPath)
        {
            List<SubArea> doNotTraveledToSubAreas = new List<SubArea>();
            doNotTraveledToSubAreas.Add(currentSubArea);

            List<SubArea> connectedSubAreas = getConnectingSubAreas(currentSubArea);

            // Go through all connecting subareas
            for (int i = 0; i < connectedSubAreas.Count; i++)
            {
                SubArea nextSubArea = connectedSubAreas[i];
                List<SubArea> currentSubAreasTraveledTo = new List<SubArea>();
                bool isConnectedToCommonPath = false;

                // If we have not already traveled to this subarea
                if (doNotTraveledToSubAreas.Contains(nextSubArea) == false)
                {
                    // If this clump of subareas touches the common path then add it
                    getSubAreaPathsToCommonPath(nextSubArea, ref isConnectedToCommonPath, ref doNotTraveledToSubAreas, ref currentSubAreasTraveledTo);
                    if (isConnectedToCommonPath == true)
                        listOfSubAreasConnectedToCommonPath.AddRange(currentSubAreasTraveledTo);
                    else
                        listOfSubAreasNotTouchingCommonPath.AddRange(currentSubAreasTraveledTo);
                }
            }
        }

        // Will get all SubAreas that touch and get a touching SubArea clump
        List<SubArea> getAllTouchingSubAreasStart(SubArea startSubArea, ref List<SubArea> doNotTraveledToSubAreas)
        {
            List<SubArea> listOfSubAreaClumps = new List<SubArea>();

            getAllTouchingSubAreas(startSubArea, ref listOfSubAreaClumps, ref doNotTraveledToSubAreas);

            return listOfSubAreaClumps;
        }

        void getAllTouchingSubAreas(SubArea currentSubArea, ref List<SubArea> listOfSubAreaClumps, ref List<SubArea> doNotTraveledToSubAreas)
        {
            doNotTraveledToSubAreas.Add(currentSubArea);
            listOfSubAreaClumps.Add(currentSubArea);

            List<SubArea> connectedSubAreas = getConnectingSubAreas(currentSubArea);

            // Go through all connecting subareas
            for (int i = 0; i < connectedSubAreas.Count; i++)
            {
                SubArea nextSubArea = connectedSubAreas[i];

                // If we have not already traveled to this subarea
                if (doNotTraveledToSubAreas.Contains(nextSubArea) == false)
                {
                    getAllTouchingSubAreas(nextSubArea, ref listOfSubAreaClumps, ref doNotTraveledToSubAreas);
                }
            }
        }

        void getSubAreaPathsToCommonPath(SubArea currentSubArea, ref bool isConnectedToCommonPath, ref List<SubArea> doNotTraveledToSubAreas, ref List<SubArea> currentSubAreasTraveledTo)
        {
            if (currentSubArea.offOfCommonPath)
                isConnectedToCommonPath = true;

            doNotTraveledToSubAreas.Add(currentSubArea);
            currentSubAreasTraveledTo.Add(currentSubArea);

            List<SubArea> connectedSubAreas = getConnectingSubAreas(currentSubArea);

            // Go through all connecting subareas
            for (int i = 0; i < connectedSubAreas.Count; i++)
            {
                SubArea nextSubArea = connectedSubAreas[i];

                // If we have not already traveled to this subarea
                if (doNotTraveledToSubAreas.Contains(nextSubArea) == false)
                {
                    getSubAreaPathsToCommonPath(nextSubArea, ref isConnectedToCommonPath, ref doNotTraveledToSubAreas, ref currentSubAreasTraveledTo);
                }
            }
        }

        List<SubArea> getConnectingSubAreas(SubArea currentSubArea)
        {
            List<SubArea> connectedSubAreas = new List<SubArea>();

            for (int i = 0; i < currentSubArea.connectingSubArea.Count; i++)
            {
                connectedSubAreas.Add(currentSubArea.connectingSubArea[i]);
            }

            if (currentSubArea.parentSubArea != null)
                connectedSubAreas.Add(currentSubArea.parentSubArea);

            return connectedSubAreas;
        }


        // ==========================================================

        // Part 1

        // ==========================================================

        public void setupPathsAndSubAreas(ref Zone zone)
        {
            // Get Main Common Path
            List<GameObject> commonPath = new List<GameObject>(getCommonPath(ref zone));
            this.setCommonPath(commonPath);

            // Get paths that branch from the common path
            getZoneBranchingPathsStart(ref zone);

            // Divide branching paths into sub areas
            divideBranchingPathsIntoSubAreas(ref zone);
        }

        List<GameObject> getCommonPath(ref Zone zone)
        {
            List<GameObject> commonPath = new List<GameObject>();
            List<GameObject> zoneEntrancesToUseForCommonPath = zone.getPrimaryZoneEntrances();

            // If the zone only has 1 primary entrance, then create a common path
            //      Will get the longest path possible by default
            if (zoneEntrancesToUseForCommonPath.Count == 1)
            {
                Debug.Log("getCommonPath() - Generating a random common path for zone: " + zone.getZoneId());
                Debug.Log("WHEN PROFILING A ZONE LIKE THIS, MAKE SURE THAT IT KNOWS THAT THERE IS ONLY 1 ENTRANCE TO THIS ZONE");

                bool getShortestPath = true;
                commonPath = createRandomCommonPath(zoneEntrancesToUseForCommonPath[0], ref zone, getShortestPath);
            }
            // If they are close then find the min path to each other
            //      Technically it's going to be one of the smallest paths, or else the common path will always go straight through a zone
            else
            {
                bool getShortestPath = true;
                commonPath = createMainCommonPath(ref zone, getShortestPath);
            }

            return commonPath;
        }

        // Will create a main common path with the least amount of rooms possible
        List<GameObject> createMainCommonPath(ref Zone zone, bool getShortestPath)
        {
            List<GameObject> commonPath = new List<GameObject>();
            //List<GameObject> zoneEntrancesToUseForCommonPath = zone.getEntranceRooms();
            List<GameObject> zoneEntrancesToUseForCommonPath = zone.getPrimaryZoneEntrances();

            
            // Do not want to use the boss room as the common path
            List<GameObject> doNotTravelList = new List<GameObject>();
            doNotTravelList.Add(zone.bossRoom);

            // Loop through each zone entrance and try to find a common path between each zone entrance
            for (int i = 0; i < zoneEntrancesToUseForCommonPath.Count; i++)
            {
                if (commonPath.Contains(zoneEntrancesToUseForCommonPath[i]))
                {
                    // If the entrance already exists then no need to search
                    //Debug.Log("Zone: " + zone.getZoneId() + "   has room: " + zoneEntrances[i]);
                }
                else if (i > 0)
                {
                    // If a common path already exists, then try to find the link to the next entrance based on the current common path
                    //      Try to find the shortest link between entrances that go to the same zone
                    //      Find longest links paths between entrances that are in different zones
                    //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //  We require that common paths only have 2 entrances, so this else if statement doesn't do anything
                    //  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    List<GameObject> minPath = new List<GameObject>();

                    for (int j = 0; j < commonPath.Count; j++)
                    {
                        List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
                        List<GameObject> endRooms = new List<GameObject>();
                        endRooms.Add(zone.zoneEntranceRooms[i]);

                        traverseZoneRoomListStart(zone.getZoneId(), commonPath[j], commonPath[j], endRooms, ref listOfPaths, doNotTravelList);

                        // There's a chance that no path can be found from one room to another
                        //      Mainly because we don't want to cross over the already existing common path
                        if (listOfPaths.Count != 0)
                        {
                            List<GameObject> currentMinPath = randomlyChooseAList(listOfPaths);

                            if (minPath.Count == 0)
                                minPath = currentMinPath;
                            else if (currentMinPath.Count < minPath.Count)
                                minPath = currentMinPath;
                        }
                    }

                    commonPath.AddRange(minPath);
                    doNotTravelList.AddRange(minPath);
                }
                else
                {
                    // The first attempt at finding a common path
                    List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
                    List<GameObject> endRooms = new List<GameObject>();
                    endRooms.Add(zoneEntrancesToUseForCommonPath[i + 1]);

                    traverseZoneRoomListStart(zone.getZoneId(), zoneEntrancesToUseForCommonPath[i], zoneEntrancesToUseForCommonPath[i], endRooms, ref listOfPaths, doNotTravelList);

                    List<GameObject> tempList = null;

                    if (getShortestPath == true)
                        tempList = randomlySelectASmallCountInList(listOfPaths);
                    else
                        tempList = getLargestCountInList(listOfPaths);

                    commonPath = tempList;
                    doNotTravelList.AddRange(commonPath);
                }
            }
            

            return commonPath;
        }

        // Will create a main common path where:
        //      Shortest link found when entrances are from the same zone
        //      Longest link found when entrances are from different zones
        // THIS IS NOT FINISHED AND MAY NOT MAKE ANY SENSE TO DO ANYWAYS
        List<GameObject> getMainCommonPathNOTUSED(ref Zone zone)
        {
            List<GameObject> commonPath = new List<GameObject>();

            if (zone.zoneEntranceRooms.Count == 1)
            {
                //commonPath.Add(zone.zoneEntranceRooms[0]);
                Debug.Log("getMainCommonPath() - Generating a random common path for zone: " + zone.getZoneId());
                Debug.Log("WHEN PROFILING A ZONE LIKE THIS, MAKE SURE THAT IT KNOWS THAT THERE IS ONLY 1 ENTRANCE TO THIS ZONE");

                // Create a long looping path, that either creates a circle back to the start
                //          Or goes in and out
                bool getShortest = false;
                commonPath = createRandomCommonPath(zone.zoneEntranceRooms[0], ref zone, getShortest);
            }
            else
            {
                // Do not want to use the boss room as the common path
                List<GameObject> doNotTravelList = new List<GameObject>();
                doNotTravelList.Add(zone.bossRoom);

                // Want to get a list of list of rooms, 
                List<List<GameObject>> entrancesBrokenUpByZones = breakUpEntrancesByZones(zone);
                List<List<GameObject>> brokenUpCommonPath = new List<List<GameObject>>();

                // First get all of the min paths for the entrances that are touching
                for (int i = 0; i < entrancesBrokenUpByZones.Count; i++)
                {
                    if (entrancesBrokenUpByZones[i].Count == 1)
                    {
                        List<GameObject> tempList = new List<GameObject>();
                        tempList.Add(entrancesBrokenUpByZones[i][0]);
                    }
                    else
                    {
                        //List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
                        //List<GameObject> endRooms = new List<GameObject>();
                        //endRooms.Add(zone.zoneEntranceRooms[i + 1]);

                        //traverseZoneRoomListStart(zone.getZoneId(), zone.zoneEntranceRooms[i], zone.zoneEntranceRooms[i], endRooms, ref listOfPaths, doNotTravelList);
                        //List<GameObject> tempList = getSmallestCountInList(listOfPaths);
                        //commonPath = tempList;
                        //doNotTravelList.AddRange(commonPath);
                    }
                }

                // Then combine the min paths, by the longest possible path
            }

            return commonPath;
        }

        // This will take all entrances from a zone, check what zone they connect to and group them based on if they touch in that zone
        //      Needs to be done this way because vein rooms have the same id, but may not be touching if they are on both sides of a zone
        List<List<GameObject>> breakUpEntrancesByZones(Zone zone)
        {
            List<GameObject> accountedForEntrances = new List<GameObject>();
            List<List<GameObject>> entrancesBrokenUpByZones = new List<List<GameObject>>();

            // Loop through each zone entrance and divide them into entrances from the same zone
            for (int i = 0; i < zone.zoneEntranceRooms.Count; i++)
            {
                GameObject currentEntrance = zone.zoneEntranceRooms[i];

                // If we havent use this entrance yet, then check it out
                if (accountedForEntrances.Contains(currentEntrance) == false)
                {
                    accountedForEntrances.Add(currentEntrance);

                    List<GameObject> touchingEntrances = findTouchingEntrancesInTheOtherZone(zone.getZoneId(), currentEntrance, zone.zoneEntranceRooms);
                    entrancesBrokenUpByZones.Add(touchingEntrances);

                    for (int j = 0; j < touchingEntrances.Count; j++)
                    {
                        if (accountedForEntrances.Contains(touchingEntrances[j]) == false)
                            accountedForEntrances.Add(touchingEntrances[j]);
                    }
                }
            }

            return entrancesBrokenUpByZones;
        }

        List<GameObject> findTouchingEntrancesInTheOtherZone(int zoneId, GameObject currentEntrance, List<GameObject> zoneEntranceRooms)
        {
            roomProperties currentEntranceProps = currentEntrance.GetComponent<roomProperties>();
            List<GameObject> allOtherZones = new List<GameObject>();

            // Check all adjacent rooms and get each other zone id
            for (int i = 0; i < currentEntranceProps.doorList.Count; i++)
            {
                // If door is used
                if (currentEntranceProps.doorList[i].doorUsedBool == true)
                {
                    GameObject adjacentRoom = currentEntranceProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

                    if (adjacentRoomProps.getZoneId() != zoneId)
                    {
                        allOtherZones.Add(adjacentRoom);
                    }
                }
            }

            List<GameObject> entrancesThatAreFromTheSameZone = new List<GameObject>();
            entrancesThatAreFromTheSameZone.Add(currentEntrance);

            // Go through each zone id and check to see if any of the other zone entrances are in them
            if (allOtherZones.Count != 0)
            {
                List<GameObject> otherEntrances = new List<GameObject>();
                for (int i = 0; i < zoneEntranceRooms.Count; i++)
                {
                    if (zoneEntranceRooms[i] != currentEntrance)
                        otherEntrances.Add(zoneEntranceRooms[i]);
                }

                for (int i = 0; i < allOtherZones.Count; i++)
                {
                    entrancesThatAreFromTheSameZone.AddRange(
                        findEntrancesAsAdjacentRoomsInOtherZoneStart(allOtherZones[i], otherEntrances));
                }
            }

            return entrancesThatAreFromTheSameZone;
        }

        List<GameObject> findEntrancesAsAdjacentRoomsInOtherZoneStart(GameObject otherZoneEntrance, List<GameObject> lookingForTheseEntrances)
        {
            List<GameObject> doNotTravelList = new List<GameObject>();
            List<GameObject> foundTheseEntrances = new List<GameObject>();
            int otherZoneId = otherZoneEntrance.GetComponent<roomProperties>().getZoneId();

            doNotTravelList.Add(otherZoneEntrance);

            findEntrancesAsAdjacentRoomsInOtherZone(otherZoneEntrance, otherZoneId, ref doNotTravelList, lookingForTheseEntrances, ref foundTheseEntrances);

            return foundTheseEntrances;
        }

        void findEntrancesAsAdjacentRoomsInOtherZone(GameObject currentRoom, int otherZoneId, ref List<GameObject> doNotTravelList,
                        List<GameObject> lookingForTheseEntrances, ref List<GameObject> foundTheseEntrances)
        {
            roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();
            doNotTravelList.Add(currentRoom);

            // Check all adjacent rooms and get each other zone id
            for (int i = 0; i < currentRoomProps.doorList.Count; i++)
            {
                // If door is used
                if (currentRoomProps.doorList[i].doorUsedBool == true)
                {
                    GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

                    // If the next room is in the same zone, then move on to that room as long as we haven't already travled to it
                    if (adjacentRoomProps.getZoneId() == otherZoneId && doNotTravelList.Contains(adjacentRoom) == false)
                    {
                        findEntrancesAsAdjacentRoomsInOtherZone(adjacentRoom, otherZoneId, ref doNotTravelList, lookingForTheseEntrances, ref foundTheseEntrances);
                    }
                    else if (adjacentRoomProps.getZoneId() != otherZoneId)
                    {
                        // Check to see if it's one of the rooms that we are looking for
                        if (lookingForTheseEntrances.Contains(adjacentRoom) == true)
                            foundTheseEntrances.Add(adjacentRoom);
                    }
                }
            }
        }

        List<GameObject> createRandomCommonPath(GameObject startRoom, ref Zone zone, bool getShortest)
        {
            // Need to keep count of how far away we are from the starting room 
            //      We start comming back based on the amount of rooms in the zone
            List<GameObject> commonPath = new List<GameObject>();
            List<GameObject> doNotTravelList = new List<GameObject>();
            List<GameObject> endRooms = new List<GameObject>();
            List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
            doNotTravelList.Add(zone.bossRoom);

            GameObject farRoom = getFurthestRoomInZone(startRoom, ref zone, doNotTravelList, true);
            endRooms.Add(farRoom);

            traverseZoneRoomListStart(zone.getZoneId(), startRoom, startRoom, endRooms, ref listOfPaths, doNotTravelList);

            if (getShortest == false)
                commonPath = getLargestCountInList(listOfPaths);
            else
                commonPath = randomlySelectASmallCountInList(listOfPaths);

            return commonPath;
        }

        // Will take all of the branching paths and divide them into sub areas
        void divideBranchingPathsIntoSubAreas(ref Zone zone)
        {
            // Get each clump of branches off of the common path
            List<List<Paths>> branchingPathClumps = getBranchingPathClumpsOffOfCommonPathStart();

            //Debug.Log("ZONE: " + zone.getZoneId() + "     PATH CLUMPS COUNT: " + branchingPathClumps.Count);
            Debug.Log("MAKE SURE THAT SUBAREAS WITH DEAD ENDS ARE WORKING");

            // Check if any of the paths in the clump have only 1 path, that leads back to the common path
            //      If so then add it as a new sub area
            for (int i = 0; i < branchingPathClumps.Count; i++)
            {
                // First get the sub areas that are connected directly to the common path
                //      Because they are off of the common path, they have no parent sub area
                bool getSubAreaFromCommonPath = true;
                List<Paths> excludedPaths = new List<Paths>();
                SubArea nullParentSubArea = new SubArea();

                SubArea newSubArea = breakUpPathClump(branchingPathClumps[i], ref excludedPaths, getSubAreaFromCommonPath, ref nullParentSubArea);
                subAreas.Add(newSubArea);

                // Use the same proccess on the newly added sub area (connected to the common path) to get other sub areas from the excluded paths
                getSubAreaFromCommonPath = false;
                breakUpExcludedPathsIntoSubAreas(ref newSubArea, excludedPaths, getSubAreaFromCommonPath);
            }
        }

        void breakUpExcludedPathsIntoSubAreas(ref SubArea currentSubArea, List<Paths> excludedPaths, bool getSubAreaFromCommonPath)
        {
            if (excludedPaths.Count != 0)
            {
                Debug.Log("divideBranchingPathsIntoSubAreas() in PathMapper has an excluded path. THIS HAS NOT BEEN TESTED YET!");
                // Sort the excluded paths into touching clumps
                List<List<Paths>> newPathClumps = sortExcludedTouchingPathsIntoClumps(excludedPaths);

                // Go through each clump and find a starting room
                for (int i = 0; i < newPathClumps.Count; i++)
                {
                    // If it's a dead end path
                    if (newPathClumps[i].Count <= 1)
                        currentSubArea.addDeadEnd(newPathClumps[i]);
                    // Else get the next sub area and any excluded paths from the new sub area
                    else
                    {
                        List<Paths> nextExcludedPaths = new List<Paths>();
                        SubArea newSubArea = breakUpPathClump(newPathClumps[i], ref nextExcludedPaths, getSubAreaFromCommonPath, ref currentSubArea);
                        currentSubArea.addConnectingSubArea(newSubArea);
                        newSubArea.addParentSubArea(currentSubArea);

                        // Check the next excluded paths if there are any
                        breakUpExcludedPathsIntoSubAreas(ref newSubArea, nextExcludedPaths, getSubAreaFromCommonPath);
                    }
                }
            }
        }

        List<List<Paths>> sortExcludedTouchingPathsIntoClumps(List<Paths> excludedPaths)
        {
            // Take all excluded paths and seperate them into clumps of paths, based on if they touch or not
            List<Paths> alreadyAddedList = new List<Paths>();
            List<List<Paths>> touchingPathClumps = new List<List<Paths>>();

            for (int i = 0; i < excludedPaths.Count; i++)
            {
                Paths currentPath = excludedPaths[i];

                if (alreadyAddedList.Contains(currentPath) == false)
                {
                    List<Paths> doNotTravelList = new List<Paths>();
                    List<Paths> touchingPaths = new List<Paths>();
                    findAllTouchingPathsThatAreInList(currentPath, ref touchingPaths, excludedPaths, doNotTravelList);
                    alreadyAddedList.AddRange(touchingPaths);
                    touchingPathClumps.Add(touchingPaths);
                }
            }
            return touchingPathClumps;
        }

        void findAllTouchingPathsThatAreInList(Paths currentPath, ref List<Paths> newListOfTouchingPaths, List<Paths> listOfAllowablePaths, List<Paths> doNotTravelList)
        {
            newListOfTouchingPaths.Add(currentPath);
            doNotTravelList.Add(currentPath);

            // Got through all of the current paths branches
            for (int i = 0; i < currentPath.branches.Count; i++)
            {
                Paths nextPath = currentPath.branches[i];

                // If the next path has not already been traveled to AND if the next path is in the inputed list of allowable paths
                if (doNotTravelList.Contains(nextPath) == false && listOfAllowablePaths.Contains(nextPath) == true)
                {
                    findAllTouchingPathsThatAreInList(nextPath, ref newListOfTouchingPaths, listOfAllowablePaths, doNotTravelList);
                }
            }
        }

        SubArea breakUpPathClump(List<Paths> branchingPathClump, ref List<Paths> excludedPaths, bool getSubAreaFromCommonPath, ref SubArea parentSubArea)
        {
            Paths startPath = null;

            // Find a path that starts at the common path is getSubAreaFromCommonPath is true
            //      Else get it from the parent sub area
            for (int i = 0; i < branchingPathClump.Count; i++)
            {
                Paths nextBranchingPathClump = branchingPathClump[i];
                if (getSubAreaFromCommonPath == true && nextBranchingPathClump.isConnectedToCommonPath)
                {
                    startPath = nextBranchingPathClump;
                    break;
                }
                else if (getSubAreaFromCommonPath == false)
                {
                    if (pathConnectsDirectlyToSubArea(parentSubArea, nextBranchingPathClump))
                    {
                        startPath = nextBranchingPathClump;
                        break;
                    }
                }
            }

            List<Paths> newSubAreaPaths = new List<Paths>();
            List<Paths> doNotTravelList = new List<Paths>();
            bool previousPathIsConectedToPreviousSubArea = true;
            getSubAreaFromPaths(startPath, startPath, parentSubArea, ref newSubAreaPaths, ref excludedPaths, ref doNotTravelList, previousPathIsConectedToPreviousSubArea, getSubAreaFromCommonPath);

            SubArea newSubArea = null;
            if (getSubAreaFromCommonPath)
                newSubArea = new SubArea(newSubAreaPaths, true); // Make a subarea where it's off of the common path
            else
                newSubArea = new SubArea(newSubAreaPaths, false); // Make a subarea where it's not off of the common path

            return newSubArea;
        }

        bool pathConnectsDirectlyToSubArea(SubArea subArea, Paths pathToCheck)
        {
            // Go through each path in the submitted sub area and check the branches for the path submitted
            bool pathConnectsDirectlyToSubArea = false;

            for (int i = 0; i < subArea.mainPath.Count; i++)
            {
                for (int j = 0; j < subArea.mainPath[i].branches.Count; j++)
                {
                    if (subArea.mainPath[i].branches[j] == pathToCheck)
                    {
                        pathConnectsDirectlyToSubArea = true;
                        break;
                    }
                }
                if (pathConnectsDirectlyToSubArea)
                    break;
            }
            return pathConnectsDirectlyToSubArea;
        }

        void getSubAreaFromPaths(Paths currentPath, Paths previousPath, SubArea parentSubArea, ref List<Paths> newSubAreaPaths, ref List<Paths> excludedPaths, ref List<Paths> doNotTravelList,
                                    bool previousPathIsConectedToPreviousSubArea, bool getSubAreaFromCommonPath)
        {
            bool isCurrentPathConnected = false;

            doNotTravelList.Add(currentPath);

            // If we are checking if it's connected to the common path
            if (getSubAreaFromCommonPath)
                isCurrentPathConnected = currentPath.isConnectedToCommonPath;
            else
                isCurrentPathConnected = pathConnectsDirectlyToSubArea(parentSubArea, currentPath);

            if (isCurrentPathConnected)
            {
                newSubAreaPaths.Add(currentPath);

                // If the current path is connected then move onto the paths branches
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    Paths nextPath = currentPath.branches[i];

                    if (doNotTravelList.Contains(nextPath) == false)
                    {
                        getSubAreaFromPaths(nextPath, currentPath, parentSubArea, ref newSubAreaPaths, ref excludedPaths, ref doNotTravelList, true, getSubAreaFromCommonPath);
                    }
                }
            }
            else
            {
                // If the current path is not directly connected, then check if any of the end rooms are connected
                bool endRoomOneIsConnected = false;
                bool endRoomTwoIsConnected = false;

                if (previousPathIsConectedToPreviousSubArea)
                {
                    checkWhichEndRoomIsConnected(currentPath, previousPath, ref endRoomOneIsConnected, ref endRoomTwoIsConnected);
                }

                // First check the first end room
                GameObject endRoom = currentPath.endRooms[0];

                if (endRoomOneIsConnected == false)
                {
                    for (int i = 0; i < currentPath.branches.Count; i++)
                    {
                        Paths nextPath = currentPath.branches[i];

                        // Only search paths that have endroom in them
                        if (doNotTravelList.Contains(nextPath) == false && nextPath.endRooms.Contains(endRoom))
                        {
                            if (getSubAreaFromCommonPath)
                            {
                                List<Paths> tempDoNotTravelList = new List<Paths>();
                                endRoomOneIsConnected = isPathConnectedToCommonPath(nextPath, ref tempDoNotTravelList);
                            }
                            else
                            {
                                List<Paths> tempDoNotTravelList = new List<Paths>();
                                tempDoNotTravelList.AddRange(parentSubArea.mainPath);

                                endRoomOneIsConnected = isPathConnectedToSubArea(parentSubArea, nextPath, ref tempDoNotTravelList);
                            }

                            if (endRoomOneIsConnected)
                                break;
                        }
                    }
                }

                // Then check the second end room
                endRoom = currentPath.endRooms[1];

                if (endRoomTwoIsConnected == false)
                {
                    for (int i = 0; i < currentPath.branches.Count; i++)
                    {
                        Paths nextPath = currentPath.branches[i];

                        // Only search paths that have endroom in them
                        if (doNotTravelList.Contains(nextPath) == false && nextPath.endRooms.Contains(endRoom))
                        {
                            if (getSubAreaFromCommonPath)
                            {
                                List<Paths> tempDoNotTravelList = new List<Paths>();
                                endRoomTwoIsConnected = isPathConnectedToCommonPath(nextPath, ref tempDoNotTravelList);
                            }
                            else
                            {
                                List<Paths> tempDoNotTravelList = new List<Paths>();
                                tempDoNotTravelList.AddRange(parentSubArea.mainPath);

                                endRoomOneIsConnected = isPathConnectedToSubArea(parentSubArea, nextPath, ref tempDoNotTravelList);
                            }
                        }

                        if (endRoomTwoIsConnected)
                            break;
                    }
                }

                // Add to the new subarea list
                if (endRoomOneIsConnected && endRoomTwoIsConnected)
                    newSubAreaPaths.Add(currentPath);
                // Add to excluded list
                else
                    excludedPaths.Add(currentPath);

                // Finally move onto the next path
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    Paths nextPath = currentPath.branches[i];

                    if (doNotTravelList.Contains(nextPath) == false)
                    {
                        getSubAreaFromPaths(nextPath, currentPath, parentSubArea, ref newSubAreaPaths, ref excludedPaths, ref doNotTravelList,
                                            endRoomOneIsConnected || endRoomTwoIsConnected, getSubAreaFromCommonPath);
                    }
                }

            }
        }

        bool isPathConnectedToSubArea(SubArea subArea, Paths currentPath, ref List<Paths> tempDoNotTravelList)
        {
            bool isRoomConnectedToPath = false;
            tempDoNotTravelList.Add(currentPath);

            if (pathConnectsDirectlyToSubArea(subArea, currentPath))
                isRoomConnectedToPath = true;
            else
            {
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    Paths nextPath = currentPath.branches[i];

                    // Do not select an already traveled to path
                    if (tempDoNotTravelList.Contains(nextPath) == false)
                    {
                        isRoomConnectedToPath = isPathConnectedToSubArea(subArea, nextPath, ref tempDoNotTravelList);
                    }
                }
            }

            return isRoomConnectedToPath;
        }

        bool isPathConnectedToCommonPath(Paths currentPath, ref List<Paths> tempDoNotTravelList)
        {
            bool isRoomConnectedToPath = false;
            tempDoNotTravelList.Add(currentPath);

            if (currentPath.isConnectedToCommonPath)
                isRoomConnectedToPath = true;
            else
            {
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    Paths nextPath = currentPath.branches[i];

                    // Do not select an already traveled to path
                    if (tempDoNotTravelList.Contains(nextPath) == false)
                    {
                        isRoomConnectedToPath = isPathConnectedToCommonPath(nextPath, ref tempDoNotTravelList);
                    }
                }
            }

            return isRoomConnectedToPath;
        }

        void checkWhichEndRoomIsConnected(Paths currentPath, Paths previousPath, ref bool endRoomOneIsConnected, ref bool endRoomTwoIsConnected)
        {
            if (currentPath.endRooms[0].name == previousPath.endRooms[0].name ||
                currentPath.endRooms[0].name == previousPath.endRooms[1].name)
            {
                endRoomOneIsConnected = true;
            }
            else if (currentPath.endRooms[1].name == previousPath.endRooms[0].name ||
                     currentPath.endRooms[1].name == previousPath.endRooms[1].name)
            {
                endRoomTwoIsConnected = true;
            }
        }

        //======================
        // Branching
        //======================

        List<List<Paths>> getBranchingPathClumpsOffOfCommonPathStart()
        {
            List<List<Paths>> listOfClumpedPaths = new List<List<Paths>>();
            List<Paths> doNotTravelList = new List<Paths>();

            for (int i = 0; i < branchingPaths.Count; i++)
            {
                List<Paths> currentClumpOfPaths = new List<Paths>();
                Paths currentBranch = branchingPaths[i];

                if (doNotTravelList.Contains(currentBranch) == false)
                {
                    getBranchingPathClumpsOffOfCommonPath(currentBranch, ref doNotTravelList, ref currentClumpOfPaths);
                    listOfClumpedPaths.Add(currentClumpOfPaths);
                }
            }

            return listOfClumpedPaths;
        }

        void getBranchingPathClumpsOffOfCommonPath(Paths currentBranch, ref List<Paths> doNotTravelList, ref List<Paths> currentClumpOfPaths)
        {
            doNotTravelList.Add(currentBranch);
            currentClumpOfPaths.Add(currentBranch);

            for (int i = 0; i < currentBranch.branches.Count; i++)
            {
                Paths nextBranch = currentBranch.branches[i];
                if (doNotTravelList.Contains(nextBranch) == false)
                    getBranchingPathClumpsOffOfCommonPath(nextBranch, ref doNotTravelList, ref currentClumpOfPaths);
            }
        }

        //bool print = false;
        // This will find paths off of the main common path that create single one off paths
        public void getZoneBranchingPathsStart(ref Zone zone)
        {
            List<GameObject> doNotTravelList = new List<GameObject>();

            PathMapper zonePathMapper = zone.pathMapper;
            List<GameObject> commonPath = zonePathMapper.getCommonPath();
            doNotTravelList.AddRange(commonPath);

            //print = false;
            //if (zone.getZoneId() == 2)
            //    print = true;
            //if (print)
            //    Debug.Log("----------------------START OF BRANCHING----------------------");

            for (int i = 0; i < commonPath.Count; i++)
            {
                bool branchingDetected = false;
                List<GameObject> branchingRooms = new List<GameObject>();

                GameObject commonPathRoom = commonPath[i];

                branchingDetected = getBranchesOffOfCommonPathRoomInZone(commonPathRoom, ref branchingRooms, zone.getZoneId(), commonPath);

                // If the selected common room has an adjacent room that is not in the common path, then a branch is detected
                if (branchingDetected)
                {
                    // Go through each branching room detected
                    for (int j = 0; j < branchingRooms.Count; j++)
                    {


                        List<GameObject> currentPath = new List<GameObject>();
                        currentPath.Add(commonPathRoom);
                        GameObject branchingPathRoom = branchingRooms[j];

                        // Only travel if the room is not already in a branching path
                        if (doNotTravelList.Contains(branchingPathRoom) == false)
                        {
                            //if (print)
                            //    Debug.Log("COMMON START: " + commonPathRoom.name);
                            getZoneBranchingPaths(zone, branchingPathRoom, commonPathRoom, currentPath, doNotTravelList);
                        }
                    }
                }
            }
        }

        void handleBranching(Zone zone, GameObject currentRoom, GameObject previousRoom, GameObject adjacentRoom, List<GameObject> currentPath, List<GameObject> doNotTravelList)
        {
            if (doNotTravelList.Contains(adjacentRoom) == true)
            {
                //if (print)
                //    Debug.Log("ROOM ALREADY TRAVELED TO: " + adjacentRoom.name);

                currentPath = new List<GameObject>();
                currentPath.Add(currentRoom);
                currentPath.Add(adjacentRoom);

                zone.pathMapper.addBranchingPath(currentPath);
            }
            else
            {
                //if (print)
                //    Debug.Log("NEW ROOM: " + adjacentRoom.name);

                // Need to create a new current path for the next branching path
                currentPath = new List<GameObject>();
                currentPath.Add(currentRoom);

                getZoneBranchingPaths(zone, adjacentRoom, currentRoom, currentPath, doNotTravelList);
            }

            // Need to create a new current path after we recorded a branching path, 
            //          because Lists in C# retain their contents over recursion and functions() in general
            currentPath = new List<GameObject>();
            currentPath.Add(currentRoom);
        }

        void getZoneBranchingPaths(Zone zone, GameObject currentRoom, GameObject previousRoom, List<GameObject> currentPath, List<GameObject> doNotTravelList)
        {
            roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();
            bool currentRoomBranches = false;
            bool recordedBasePath = false;
            bool currentRoomIsDeadEnd = false;

            currentPath.Add(currentRoom);
            doNotTravelList.Add(currentRoom);

            // Check how many doors are being used
            if (currentRoomProps.totalDoorsUsed > 2)
                currentRoomBranches = true;
            else if (currentRoomProps.totalDoorsUsed == 1)
                currentRoomIsDeadEnd = true;

            //Debug.Log("BOOLS:" + currentRoomBranches + "   " + currentRoomIsDeadEnd);
            //if (print)
            //    Debug.Log("IN LOOP: " + currentRoom.name);

            if (currentRoomIsDeadEnd == true)
            {
                //if (print)
                //    Debug.Log("DEAD END");

                // If it's a dead end then end the current path
                zone.pathMapper.addBranchingPath(currentPath);
            }
            else
            {
                // Basically check all adjacent rooms
                for (int i = 0; i < currentRoomProps.doorList.Count; i++)
                {
                    // If door is used
                    if (currentRoomProps.doorList[i].doorUsedBool == true)
                    {
                        // If door doesn't lead back to the previous room or a different zone then we can check it out
                        if (currentRoomProps.doorList[i].adjacentRoom.name != previousRoom.name &&
                            currentRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>().getZoneId() == zone.getZoneId())
                        {
                            GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                            roomProperties adjacentRoomProperties = adjacentRoom.GetComponent<roomProperties>();


                            // If it branches then save the current path and start a new one
                            if (currentRoomBranches == true)
                            {
                                if (recordedBasePath == false)
                                {
                                    //if (print)
                                    //    Debug.Log("BRANCHING - NOT RECORDED");

                                    // We only want to record the current path once (in this branching case it's the "base" path for the branches)
                                    zone.pathMapper.addBranchingPath(currentPath);
                                    recordedBasePath = true;

                                    handleBranching(zone, currentRoom, previousRoom, adjacentRoom, currentPath, doNotTravelList);
                                }
                                else
                                {
                                    //if (print)
                                    //    Debug.Log("BRANCHING - ALREADY RECORDED");

                                    handleBranching(zone, currentRoom, previousRoom, adjacentRoom, currentPath, doNotTravelList);
                                }
                            }
                            // If we end up at an already traveled to room then end the current path
                            else if (doNotTravelList.Contains(adjacentRoom) == true)
                            {
                                //if (print)
                                //    Debug.Log("ROOM ALREADY TRAVELED TO: " + adjacentRoom.name);

                                currentPath.Add(adjacentRoom);
                                zone.pathMapper.addBranchingPath(currentPath);
                            }
                            // Else if there is just a single adjacent room
                            else
                            {
                                //if (print)
                                //    Debug.Log("NON BRANCHING ROOM");
                                getZoneBranchingPaths(zone, adjacentRoom, currentRoom, currentPath, doNotTravelList);
                            }

                        }
                    }
                }
            }
        }

        void addBranchingPath(List<GameObject> path)
        {
            GameObject head = path[0];
            GameObject tail = path[path.Count - 1];


            Paths newPath = new Paths(head, tail, path, false);


            // You want to add a new path for the head
            //      You will also want to add a link for the tail, only in the case where the tail room already exists in a different path

            //Paths branchingPath = null;

            for (int i = 0; i < 2; i++)
            {
                GameObject endRoom = null;
                if (i == 0)
                    endRoom = head;
                else
                    endRoom = tail;

                // If the branching path is in the common path
                if (commonPath.isRoomInPath(endRoom))
                {
                    // Only if the path doesn't already exist
                    if (branchingPaths.Contains(newPath) == false)
                    {
                        newPath.setIsConnectedToCommonPathTrue();
                        branchingPaths.Add(newPath);

                        //if (print)
                        //{
                        //    Debug.Log("                                     ADDING PATH - COMMON ROOM END");
                        //    printAllRoomsInList(newPath.path);
                        //}
                    }

                }
                // Find the branching path to add this path to
                else
                {
                    //Debug.Log("HELP___0");
                    Paths branchingPath = getBranchingPathThatContainsStart(endRoom, newPath);
                    //Debug.Log("HELP___1");

                    if (branchingPath.path.Count != 0)
                    {
                        if (branchingPath.branches.Contains(newPath) == false)
                        {
                            //if (print)
                            //{
                            //    Debug.Log("                                     ADDING PATH - TO AN EXISTING BRANCH");
                            //    Debug.Log("                                     NEW PATH: ");
                            //    printAllRoomsInList(newPath.path);
                            //    Debug.Log("                                     TO BRANCH: ");
                            //    printAllRoomsInList(branchingPath.path);
                            //}

                            // Add the found branch to the new path
                            newPath.branches.Add(branchingPath);

                            // Need to add all branches that the found path has to the new Path
                            for (int j = 0; j < branchingPath.branches.Count; j++)
                            {
                                // Only if the branch contains the searched for room
                                if (branchingPath.branches[j].path.Contains(endRoom))
                                {
                                    newPath.branches.Add(branchingPath.branches[j]);
                                }
                            }

                            // Add the new path to the found branching path
                            branchingPath.branches.Add(newPath);

                            // Also need to add the new path to all branches that the found branch has
                            for (int j = 0; j < branchingPath.branches.Count; j++)
                            {
                                // Only if the search for room is in the path and don't add the new path to itself
                                if (branchingPath.branches[j].Equals(newPath) == false &&
                                    branchingPath.branches[j].path.Contains(endRoom))
                                {
                                    branchingPath.branches[j].branches.Add(newPath);
                                }
                            }

                            //if (debug)
                            //{
                            //    Debug.Log("                              ADDING PATH - TO BRANCH");
                            //    printAllRoomsInList(branchingPath.path);
                            //}
                        }
                    }
                }
            }
        }

        Paths getBranchingPathThatContainsStart(GameObject roomSearch, Paths newPath)
        {
            Paths pathThatContainsRoom = new Paths();
            bool foundPath = false;
            List<Paths> doNotTravel = new List<Paths>();

            //if (debug)
            //    Debug.Log("SEARCH: " + roomSearch.name);

            for (int i = 0; i < branchingPaths.Count; i++)
            {
                if (foundPath)
                    break;

                //if (debug)
                //    Debug.Log("TOP LOOP");

                Paths currentPath = branchingPaths[i];
                getBranchingPathThatContains(roomSearch, currentPath, doNotTravel, ref foundPath, ref pathThatContainsRoom, newPath);
            }

            //if (foundPath == false)
            //{
            //    //if (debug)
            //    Debug.Log("NO BRANCHING PATH FOUND");
            //}


            return pathThatContainsRoom;
        }

        void getBranchingPathThatContains(GameObject roomSearch, Paths currentPath, List<Paths> doNotTravel, ref bool foundPath, ref Paths pathThatContainsRoom, Paths newPath)
        {
            //if (debug)
            //{
            //    Debug.Log("SEARCH LOOP");
            //    printAllRoomsInList(currentPath.path);
            //}

            doNotTravel.Add(currentPath);

            // If you find a path, but not if it's the same path
            //          Don't want to add a branching path to itself
            if (currentPath.path.Contains(roomSearch) && newPath.path.Equals(currentPath.path) == false)
            {
                foundPath = true;
                pathThatContainsRoom = currentPath;
                return;
            }
            else
            {
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    if (foundPath)
                        break;

                    Paths nextSearchPath = currentPath.branches[i];
                    if (doNotTravel.Contains(nextSearchPath) == false)
                        getBranchingPathThatContains(roomSearch, nextSearchPath, doNotTravel, ref foundPath, ref pathThatContainsRoom, newPath);
                }
            }
        }

        bool getBranchesOffOfCommonPathRoomInZone(GameObject commonPathRoom, ref List<GameObject> branchingRooms, int zoneId, List<GameObject> commonPath)
        {
            // Will get any non common rooms connected to the passed in room
            bool branchingRoomDetected = false;
            roomProperties commonPathRoomProps = commonPathRoom.GetComponent<roomProperties>();

            // Basically check all adjacent rooms
            for (int i = 0; i < commonPathRoomProps.doorList.Count; i++)
            {
                // If door is used
                if (commonPathRoomProps.doorList[i].doorUsedBool == true)
                {
                    GameObject adjacentRoom = commonPathRoomProps.doorList[i].adjacentRoom;

                    // If the door leads to the same zone then and is not a part of the common path
                    if (adjacentRoom.GetComponent<roomProperties>().getZoneId() == zoneId &&
                         commonPath.Contains(adjacentRoom) == false)
                    {
                        branchingRoomDetected = true;
                        branchingRooms.Add(adjacentRoom);
                    }
                }
            }

            return branchingRoomDetected;
        }

        void branchNotVisted(Paths path, ref int index, ref bool branchBranchesHasBeenVisted, List<Paths> alreadyTraveledToPath)
        {
            for (int i = 0; i < path.branches.Count; i++)
            {
                if (alreadyTraveledToPath.Contains(path.branches[i]) == false)
                {
                    branchBranchesHasBeenVisted = false;
                    index = i;
                }
            }
        }

        // Used for debugging (grid generator) and finding the boss subarea
        public SubArea getNextSubAreaStart(SubArea currentSubArea, ref List<SubArea> alreadyTraveledToSubAreas, ref bool resetTraveledToSubAreaList)
        {
            resetTraveledToSubAreaList = false;
            SubArea selectedSubArea = null;
            List<SubArea> currentSubAreaList = new List<SubArea>();

            // Fresh start, where no sub area is actually passed in
            if (currentSubArea.mainPath.Count == 0)
            {
                currentSubArea = subAreas[0];
            }

            selectedSubArea = getNextSubArea(currentSubArea, alreadyTraveledToSubAreas, currentSubAreaList);

            // If we didn't find a path that hasn't been traveled to, try a different branching path off of the common path
            if (selectedSubArea == null)
            {
                int subAreaIndex = 0;
                while (selectedSubArea == null)
                {
                    currentSubArea = subAreas[subAreaIndex];

                    if (alreadyTraveledToSubAreas.Contains(currentSubArea) == false)
                    {
                        selectedSubArea = getNextSubArea(currentSubArea, alreadyTraveledToSubAreas, currentSubAreaList);
                    }

                    // If we didn't find the next branching path
                    if (selectedSubArea == null)
                    {
                        subAreaIndex++;

                        // If we have searched through all common paths then reset
                        if (subAreaIndex >= subAreas.Count)
                        {
                            resetTraveledToSubAreaList = true;
                            alreadyTraveledToSubAreas = new List<SubArea>();
                            subAreaIndex = 0;
                            currentSubAreaList = new List<SubArea>();
                        }
                    }
                }
            }

            return selectedSubArea;
        }

        // Need to grab the deepest sub area farthest away from the common path
        SubArea getNextSubArea(SubArea currentSubArea, List<SubArea> alreadyTraveledToSubAreas, List<SubArea> currentSubAreaList)
        {
            currentSubAreaList.Add(currentSubArea);
            SubArea selectedSubArea = null;

            if (selectedSubArea == null)
            {
                for (int i = 0; i < currentSubArea.connectingSubArea.Count; i++)
                {
                    // If the child sub area has not yet been selected or if it's not in the currently traversed sub area list then select it
                    if (alreadyTraveledToSubAreas.Contains(currentSubArea.connectingSubArea[i]) == false && currentSubAreaList.Contains(currentSubArea.connectingSubArea[i]) == false)
                    {
                        selectedSubArea = getNextSubArea(currentSubArea.connectingSubArea[i], alreadyTraveledToSubAreas, currentSubAreaList);

                        if (selectedSubArea != null)
                            break;
                    }
                }

                // If there is no deeper sub area then we either select the current path, or go back up
                if (selectedSubArea == null && alreadyTraveledToSubAreas.Contains(currentSubArea) == false)
                {
                    selectedSubArea = currentSubArea;
                    alreadyTraveledToSubAreas.Add(selectedSubArea);
                }
            }

            return selectedSubArea;
        }

        // Used for debugging purposes
        public Paths getNextBranchingPathStart(Paths currentPath, ref List<Paths> alreadyTraveledToPath)
        {
            Paths selectedBranchingPath = null;
            List<Paths> currentPathList = new List<Paths>();

            // Fresh start, where no path is actually passed in
            if (currentPath.path.Count == 0)
            {
                currentPath = branchingPaths[0];
            }

            selectedBranchingPath = getNextBranchingPath(currentPath, alreadyTraveledToPath, currentPathList);

            // If we didn't find a path that hasn't been traveled to, try a different branching path off of the common path
            if (selectedBranchingPath == null)
            {
                int branchIndex = 0;
                while (selectedBranchingPath == null)
                {
                    currentPath = branchingPaths[branchIndex];

                    if (alreadyTraveledToPath.Contains(currentPath) == false)
                    {
                        selectedBranchingPath = getNextBranchingPath(currentPath, alreadyTraveledToPath, currentPathList);
                    }

                    // If we didn't find the next branching path
                    if (selectedBranchingPath == null)
                    {
                        branchIndex++;

                        // If we have searched through all common paths then reset
                        if (branchIndex >= branchingPaths.Count)
                        {
                            alreadyTraveledToPath = new List<Paths>();
                            branchIndex = 0;
                            currentPathList = new List<Paths>();
                        }
                    }
                }
            }

            return selectedBranchingPath;
        }

        // Need to grab the deepest path farthest away from the common path
        Paths getNextBranchingPath(Paths currentPath, List<Paths> alreadyTraveledToPath, List<Paths> currentPathList)
        {
            currentPathList.Add(currentPath);
            Paths selectedBranchingPath = null;

            //Debug.Log("Branching: " + index[indexIndex] + "   " + currentPath.branches.Count);
            if (selectedBranchingPath == null)
            {
                for (int i = 0; i < currentPath.branches.Count; i++)
                {
                    // If the branching path has not yet been selected or if it's not in the currently traversed  path list
                    if (alreadyTraveledToPath.Contains(currentPath.branches[i]) == false && currentPathList.Contains(currentPath.branches[i]) == false)
                    {
                        selectedBranchingPath = getNextBranchingPath(currentPath.branches[i], alreadyTraveledToPath, currentPathList);

                        if (selectedBranchingPath != null)
                            break;
                    }
                }

                // If there is no deeper path then we either select the current path, or go back up
                if (selectedBranchingPath == null && alreadyTraveledToPath.Contains(currentPath) == false)
                {
                    selectedBranchingPath = currentPath;
                    alreadyTraveledToPath.Add(selectedBranchingPath);
                }
            }

            return selectedBranchingPath;
        }

        //======================

        public void traverseZoneRoomListStart(int zoneId, GameObject startRoom, GameObject previousRoom, List<GameObject> endRooms, 
                                    ref List<List<GameObject>> listOfPaths, List<GameObject> doNotTravelList, List<GameObject> onlyTravelInList)
        {
            List<GameObject> currentPath = new List<GameObject>();
            traverseZoneRoomList(zoneId, startRoom, previousRoom, endRooms, ref listOfPaths, currentPath, doNotTravelList, onlyTravelInList);
        }

        public void traverseZoneRoomListStart(int zoneId, GameObject startRoom, GameObject previousRoom, GameObject endRoom,
                                   ref List<List<GameObject>> listOfPaths, List<GameObject> doNotTravelList, List<GameObject> onlyTravelInList)
        {
            List<GameObject> currentPath = new List<GameObject>();
            List<GameObject> endRooms = new List<GameObject> { endRoom };
            traverseZoneRoomList(zoneId, startRoom, previousRoom, endRooms, ref listOfPaths, currentPath, doNotTravelList, onlyTravelInList);
        }

        public void traverseZoneRoomListStart(int zoneId, GameObject startRoom, GameObject previousRoom, List<GameObject> endRooms,
                                    ref List<List<GameObject>> listOfPaths, List<GameObject> doNotTravelList)
        {
            List<GameObject> currentPath = new List<GameObject>();
            List<GameObject> onlyTravelInList = new List<GameObject>();
            traverseZoneRoomList(zoneId, startRoom, previousRoom, endRooms, ref listOfPaths, currentPath, doNotTravelList, onlyTravelInList);
        }

        void traverseZoneRoomList(int zoneId, GameObject currentRoom, GameObject previousRoom, List<GameObject> endRooms, ref List<List<GameObject>> listOfPaths,
                                         List<GameObject> currentPath, List<GameObject> doNotTravelList, List<GameObject> onlyTravelInList)
        {
            roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

            currentPath.Add(currentRoom);

            // Basically check all adjacent rooms
            for (int i = 0; i < currentRoomProps.doorList.Count; i++)
            {
                // If door is used
                if (currentRoomProps.doorList[i].doorUsedBool == true)
                {
                    // If door doesn't lead back to the previous room or a different zone then we can check it out
                    if (currentRoomProps.doorList[i].adjacentRoom.name != previousRoom.name &&
                        currentRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>().getZoneId() == zoneId)
                    {
                        GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                        roomProperties adjacentRoomProperties = adjacentRoom.GetComponent<roomProperties>();

                        // If the adjacent room is the end destination add it to the list of paths and stop searching
                        if (endRooms.Contains(adjacentRoom))
                        {
                            //currentPath.Add(adjacentRoom);//  Can't add, it will screw up further searches
                            List<GameObject> tempPath = new List<GameObject>(currentPath); // Need to create a new list
                            tempPath.Add(adjacentRoom);
                            listOfPaths.Add(tempPath);
                        }
                        else if (currentPath.Contains(adjacentRoom) || doNotTravelList.Contains(adjacentRoom) == true ||
                            (onlyTravelInList.Count != 0 && onlyTravelInList.Contains(adjacentRoom) == false))
                        {
                            // If the adjacent room is already a part of the current path then stop the path here
                            //          Or it's a room that we specified to not travel 
                            //          Or if the onlyTravelInList is not empty and the adjacent room is not in the only travel in list
                        }
                        else
                        {
                            //                           next room,    prev room,
                            traverseZoneRoomList(zoneId, adjacentRoom, currentRoom, endRooms, ref listOfPaths, currentPath, doNotTravelList, onlyTravelInList);
                        }
                    }
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current room
            currentPath.Remove(currentRoom);
        }

        List<GameObject> randomlyChooseAList(List<List<GameObject>> list)
        {
            int randNum = UnityEngine.Random.Range(0, list.Count);
            List<GameObject> randList = list[randNum];

            return randList;
        }

        public List<GameObject> getSmallestCountInList(List<List<GameObject>> list)
        {
            List<GameObject> minList = new List<GameObject>();
            int minCount = int.MaxValue;
            // Find the shortest path
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Count < minCount)
                {
                    minList = list[i];
                    minCount = list[i].Count;
                }
            }

            return minList;
        }

        public List<GameObject> randomlySelectASmallCountInList(List<List<GameObject>> list)
        {
            List<GameObject> minList = new List<GameObject>();
            List<GameObject> minList2 = new List<GameObject>();
            int minCount = int.MaxValue;
            int minCount2 = int.MaxValue;

            // Find the shortest paths
            if (list.Count == 0)
            {
                // Do nothing
            }
            if (list.Count == 1)
            {
                minList = list[0];
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Count < minCount)
                    {
                        minList2 = minList;
                        minCount2 = minCount;

                        minList = list[i];
                        minCount = list[i].Count;
                    }
                    else if (list[i].Count < minCount2)
                    {
                        minList2 = list[i];
                        minCount2 = list[i].Count;
                    }
                }

                int rand = Random.Range(0, 2);
                if (rand == 1)
                    minList = minList2;
            }

            return minList;
        }

        public List<GameObject> getLargestCountInList(List<List<GameObject>> list)
        {
            List<GameObject> maxList = new List<GameObject>();
            int maxCount = int.MinValue;
            // Find the shortest path
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Count > maxCount)
                {
                    maxList = list[i];
                    maxCount = list[i].Count;
                }
            }

            return maxList;
        }

        GameObject getFurthestRoomInZone(GameObject room, ref Zone zone, List<GameObject> doNotTravelList, bool recalculateTempInt)
        {
            // Finds the room that is the furthest away from the specified room
            if (recalculateTempInt)
                identifyRoomDistanceStart(room, ref zone);

            GameObject farRoom = zone.roomsList[0];

            for (int i = 0; i < zone.roomsList.Count; i++)
            {
                GameObject currentRoom = zone.roomsList[i];
                roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

                if (currentRoomProps.tempInt > farRoom.GetComponent<roomProperties>().tempInt)
                {
                    if (doNotTravelList.Contains(currentRoom) == false)
                        farRoom = currentRoom;
                }
            }

            return farRoom;
        }

        void identifyRoomDistanceStart(GameObject startRoom, ref Zone zone)
        {
            int currentDistance = -1;
            for (int i = 0; i < zone.roomsList.Count; i++)
            {
                zone.roomsList[i].GetComponent<roomProperties>().tempInt = System.Int32.MaxValue;
            }
            identifyRoomDistance(startRoom, ref zone, currentDistance);

            //Debug.Log("PRRRINTING DISTANCE");
            //for (int i = 0; i < zone.roomsList.Count; i++)
            //{
            //    Debug.Log("Room: " + zone.roomsList[i].name + "   Distance: " + zone.roomsList[i].GetComponent<roomProperties>().tempInt);
            //}
        }

        void identifyRoomDistance(GameObject currentRoom, ref Zone zone, int currentDistance)
        {
            roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();
            currentDistance++;

            //Debug.Log("Room: " + currentRoom.name + "   Distance: " + currentDistance);

            bool changedDistance = false;
            if (currentDistance < currentRoomProps.tempInt)
            {
                changedDistance = true;
                currentRoomProps.tempInt = currentDistance;
            }

            // Check all adjacent rooms only if we updated the current room distance
            if (changedDistance == true)
            {
                for (int i = 0; i < currentRoomProps.doorList.Count; i++)
                {
                    // If door is used
                    if (currentRoomProps.doorList[i].doorUsedBool == true)
                    {
                        GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                        // If door doesn't lead to a different zone then we can check it out
                        if (currentRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>().getZoneId() == zone.getZoneId())
                        {
                            identifyRoomDistance(adjacentRoom, ref zone, currentDistance);
                        }
                    }
                }
            }
        }

        public List<GameObject> getAllTouchingRoomsWithTheSameZoneTypeIdStart(GameObject startRoom, ref Zone zone)
        {
            List<GameObject> currentList = new List<GameObject>();

            getAllTouchingRoomsWithTheSameZoneTypeId(startRoom, zone.getZoneTypeId(), ref currentList);

            return currentList;
        }

        void getAllTouchingRoomsWithTheSameZoneTypeId(GameObject currentRoom, int zoneTypeId, ref List<GameObject> touchingRooms)
        {
            roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

            touchingRooms.Add(currentRoom);

            // Basically check all adjacent rooms
            for (int i = 0; i < currentRoomProps.doorList.Count; i++)
            {
                // If door is used
                if (currentRoomProps.doorList[i].doorUsedBool == true)
                {
                    // If door doesn't lead to a different zone or to a room that we already traveled to then we can check it out
                    if (currentRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>().getZoneTypeId() == zoneTypeId &&
                        touchingRooms.Contains(currentRoomProps.doorList[i].adjacentRoom) == false)
                    {
                        GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                        getAllTouchingRoomsWithTheSameZoneTypeId(adjacentRoom, zoneTypeId, ref touchingRooms);
                    }
                }
            }
        }

        // Getter and Setters

        public void setCommonPath(List<GameObject> path)
        {
            commonPath.path = path;
        }

        public List<GameObject> getCommonPath()
        {
            return commonPath.path;
        }

        public void printAllRoomsInList(List<GameObject> roomList)
        {
            string list = "";
            for (int i = 0; i < roomList.Count; i++)
            {
                list = list + roomList[i].name + " ";
            }
            Debug.Log(list);
        }

        public void printAllRoomsSubAreaList(List<SubArea> subAreaList, string message)
        {
            if (message.Equals("") == false)
            {
                Debug.Log("============================");
                Debug.Log("Printing List: " + message);
                Debug.Log("============================");
            }

            for (int i = 0; i < subAreaList.Count; i++)
            {
                printAllRoomsInList(subAreaList[i].allRooms);
            }
        }

        public void printAllRoomsSubAreaList(List<SubAreaClump> subAreaList, string message)
        {
            Debug.Log("============================");
            Debug.Log("Printing List: " + message);
            Debug.Log("============================");

            for (int i = 0; i < subAreaList.Count; i++)
            {
                printAllRoomsSubAreaList(subAreaList[i].getListOfSubAreas(), "");
            }
        }

        // Sub Area Clumping Code

        public List<SubAreaClump> sortSubAreaListIntoClumps(List<SubArea> listOfSubAreas)
        {
            List<SubAreaClump> sortedSubAreaClumpList = new List<SubAreaClump>();
            List<SubArea> doNotTravelList = new List<SubArea>();

            for (int i = 0; i < listOfSubAreas.Count; i++)
            {
                SubArea currentSubArea = listOfSubAreas[i];

                // If it's in the sorted list already skip it
                bool subAreaAlreadyRecorded = false;
                for (int j = 0; j < sortedSubAreaClumpList.Count;  j++)
                {
                    if (sortedSubAreaClumpList[j].listContains(currentSubArea) == true)
                    {
                        subAreaAlreadyRecorded = true;
                        break;
                    }
                    
                }
                if (subAreaAlreadyRecorded)
                    continue;

                // Get all touching sub areas and add them to a clump, then add to the sorted SubAreaClump list
                List<SubArea> subAreaList = getAllTouchingSubAreasStart(currentSubArea, ref doNotTravelList);
                SubAreaClump newSubAreaClump = new SubAreaClump();
                newSubAreaClump.addSubAreaList(subAreaList);

                sortedSubAreaClumpList.Add(newSubAreaClump);
            }

            return sortedSubAreaClumpList;
        }

        public List<GameObject> getShortestPath(GameObject startRoom, GameObject endRoom, int zoneId)
        {
            List<GameObject> doNotTravelList = new List<GameObject>();
            List<GameObject> onlyTravelInList = new List<GameObject>();
            List<GameObject> endRooms = new List<GameObject> { endRoom };
            List<List<GameObject>> listOfPaths = new List<List<GameObject>>();

            traverseZoneRoomListStart(zoneId, startRoom, startRoom, endRooms,
                                                    ref listOfPaths, doNotTravelList, onlyTravelInList);

            return getSmallestCountInList(listOfPaths);
        }

        public int getRoomDistanceInList(GameObject startRoom, GameObject endRoom, int zoneId, List<GameObject> onlyTravelInList)
        {
            List<GameObject> doNotTravelList = new List<GameObject>();
            List<GameObject> endRooms = new List<GameObject> { endRoom };
            List<List<GameObject>> listOfPaths = new List<List<GameObject>>();

            traverseZoneRoomListStart(zoneId, startRoom, startRoom, endRooms,
                                                    ref listOfPaths, doNotTravelList, onlyTravelInList);

            return getSmallestCountInList(listOfPaths).Count;
        }
    }

}
