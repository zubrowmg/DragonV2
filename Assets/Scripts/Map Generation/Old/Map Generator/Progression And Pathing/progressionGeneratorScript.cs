using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using LockingClasses;
using System;

using Classes;
using PathingClasses;
using ProgGeneratorClasses;

public class progressionGeneratorScript
{


    GameObject grid;
    List<GameObject> roomList;

    List<GameObject> sendOffRoomDungeonEntrance = new List<GameObject>();
    int sendoffRoomIndex;
    int startRoomIndex;

    int equalDistanceTolerance = 2; // 2 rooms
    int minDistanceForSinglePrimaryEntranceZones = 4; // 5 rooms

    public enum ProgressionType { None, Type1, Type2 };
    // Progression Characteristics
    //      Type1 
    //          1. Cut off the shortest path to the boss room with a story lock (opens when beating the boss)
    //          2. Create a long path to the ability, can lock rooms with an ability lock (current zone abilty)
    //          3.
    // ----------------------------------------------------------------------
    //      Type2

    //Dictionary<int, List<GameObject>> roomsInZones = new Dictionary<int, List<GameObject>>(); // 0 is the start room and sendoff room


    public progressionGeneratorScript(ref GameObject grid, ref List<GameObject> roomList)
    {
        this.grid = grid;
        this.roomList = roomList;
    }

    public List<GameObject> getUpdatedRoomList()
    {
        return roomList;
    }

    public void generateProgression()
    {
        // Find the starting area rooms
        sendoffRoomIndex = findRoomIndex(GlobalDefines.sendOffRoomName);
        startRoomIndex = findRoomIndex(GlobalDefines.startRoomName);

        // Divide the rooms into proper zones
        organizeRoomsIntoZonesByZoneType();
        assignUniqueZoneIds();

        // Link the zone to each other
        findZoneEntrances();
        linkZonesStart();

        // Determine the zone ordering
        setupEarlyGameEntrances();
        MapGeneratorScript.zoneManager.setUpZoneOrder();

        identifyBossRoomsInEachZone();

        //assignZoneThemes(); // Needed because progression may depend on the zone theme
        //      NEEDS TO BE DONE DURING PRESET VEIN PLACEMENT, SINCE SOME ZONES HAVE SPECIFIC PRESET LAYOUTS AND POWER UPS
        //      Ex: shadow theme where you fight the boss 2 times in different rooms before defeating him in the final boss room
        //      Ex: Lake theme where you where you have to unlock an air mask to get to the boss

        setupEarlyGameZones();
        setupMidAndLateGameZones();

        //findZoneIsolatedPockets();
        //selectPowerUpRoomsAndLockedRooms();

        // When you start creating a traverse paths function, it needs to be aware of locked rooms

        // Need to pass the zone manager into the room builder script, in the case that we find out that a lock needs to be moved due to physical limits
        // 


        MapGeneratorScript.zoneManager.printAllThemesAndAbilities();

    }

    void setupEarlyGameEntrances()
    {
        // Early game zones are the zones directly connected to the sendoff room
        //     All entraces to other zones are cut off, until the appropriate power up is gained
        //     The first zone can have a power up that will let you go into further beyond the first zone 
        //          Or it will unlock an entrance to another starting zone

        GameObject sendOffRoom = roomList[sendoffRoomIndex];
        GameObject startRoom = roomList[startRoomIndex];

        List<GameObject> zoneEntrances = new List<GameObject>();
        findEntranceRoomsToEarlyGameZones(sendOffRoom, startRoom, ref zoneEntrances);

        foreach (GameObject room in zoneEntrances)
        {
            MapGeneratorScript.zoneManager.setEarlyGameZoneBool(room.GetComponent<roomProperties>().getZoneId());
        }

    }

    void setupEarlyGameZones()
    {
        // Profile each zone and set up the progression
        for (int i = 0; i < MapGeneratorScript.zoneManager.zoneList.Count; i++)
        {
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[i];
            //int zoneId = MapGeneratorScript.zoneManager.zoneIdList[i];

            // We first profile all starting zones, decide which progress types can work
            //      Then we implement them
            if (currentZone.getIsEarlyGameZone() == true)
            {
                createEarlyGameZonePathing(i);

                List<ProgressionType> possibleProgressTypes = new List<ProgressionType>();
                possibleProgressTypes = profileZone(i);

                setupProgressionType(possibleProgressTypes);
            }
        }

    }

    void createEarlyGameZonePathing(int zoneIndex)
    {
        // This function will get all pathing info for a zone and then create a way to progress to through the zone

        Zone currentZone = MapGeneratorScript.zoneManager.zoneList[zoneIndex];
        PathMapper zonePathMapper = currentZone.pathMapper;
        PathOrderManager zoneOrderManager = currentZone.getZonePathOrderManager();
        

        // Will break up a zone into paths and sub areas
        //      In the future you might want to create more diverse common paths
        zonePathMapper.setupPathsAndSubAreas(ref currentZone);


        List<SubArea> listOfSubAreasLeadingToBossRoom = new List<SubArea>(); // Has all sub areas that touch any sub areas leading to the boss sub area
        List<SubArea> listOfSubAreasBeyondBossSubArea = new List<SubArea>();
        List<SubArea> listOfRemainingSubAreas = new List<SubArea>();
        List<SubArea> listOfAccountedForSubAreas = new List<SubArea>();
        SubArea bossSubArea = null;

        // Will get any subareas that touch the common path, starting from the boss subarea
        //      Any subareas connected to the boss subarea, but not to the common path will also be recorded
        zonePathMapper.getSubAreasThatLeadToBossRoom(ref currentZone, ref bossSubArea, ref listOfSubAreasLeadingToBossRoom, ref listOfSubAreasBeyondBossSubArea);

        listOfAccountedForSubAreas.AddRange(listOfSubAreasLeadingToBossRoom);
        listOfAccountedForSubAreas.AddRange(listOfSubAreasBeyondBossSubArea);
        listOfAccountedForSubAreas.Add(bossSubArea);

        // Any subareas left in the zone must also be recorded
        zonePathMapper.getAllSubAreasInAZoneThatArentInProvidedList(ref listOfRemainingSubAreas, listOfAccountedForSubAreas);

        bool nonBossSubAreasExist = listOfRemainingSubAreas.Count == 0 ? false : true;
        bool preBossSubAreasExist = listOfSubAreasLeadingToBossRoom.Count == 0 ? false : true;
        bool postBossSubAreasExist = listOfSubAreasBeyondBossSubArea.Count == 0 ? false : true;

        bool postBossSubAreaBypassBossRoom = true;


        // !!!! Change how the common path works !!!!
        //      IF THE SUBAREA ENTRANCE IS CLOSER TO THE HIGHER ORDER
        //          Just block the highest order of the two primary entrances
        //          Will need to interfacce the ZoneLockedOutManager with a canZoneBeBlocked() function, inside that function can be a re-orient order. 
        //              Which would probably just randomly re-roder every zone

        // Scratch ideas
        //  1. Always use the subarea that is equidistant from both primary entrances
        //      - Only non boss and pre boss sub areas
        //  2. Gather all other non boss and pre boss sub areas
        //      - Try to make an order out of them
        //  3. At some point 1 and 2 should have given the zone ability
        //  4. Boss sub area comes last



        // Get all subarea clumps that are connected to the common path
        List<SubAreaClump> listOfPotentialFirstSubAreaClumps = new List<SubAreaClump>();
        //List<SubAreaClump> listOfPotentialSecondPlusSubAreaClumps = new List<SubAreaClump>();
        //List<SubAreaClump> listOfPotentialFirstPreBossSubAreaClumps = new List<SubAreaClump>();
        //List<SubAreaClump> listOfPotentialSecondPlusPreBossSubAreaClumps = new List<SubAreaClump>();
        getAllPotentialFirstSubAreaClumps(ref zonePathMapper, listOfRemainingSubAreas, listOfSubAreasLeadingToBossRoom, ref listOfPotentialFirstSubAreaClumps); // listOfPotentialFirstSubAreaClumps is not boss related, also not pre boss related
        //listOfPotentialSecondPlusSubAreaClumps = listOfPotentialFirstSubAreaClumps;

        // Get the first sub area clump and figure out the pathing
        GameObject firstRoomInSubAreaClump = null;
        
        // Determine a sub area clump locking order, first index is firstSubAreaClump
        List<LockType> subAreaClumpLockOrder = determineSubAreaClumpLockOrder(listOfPotentialFirstSubAreaClumps);
        bool firstLoop = true;
        Key nextSubAreaKey = new Key();
        ZoneLockManager zoneLockManager = MapGeneratorScript.zoneManager.getLockManager();
        int count = 0; 

        // Go through the order determining process, this is only for non boss related and pre boss subareas
        while (listOfPotentialFirstSubAreaClumps.Count != 0)
        {
            SubAreaClump currentSubAreaClump = getFirstSubAreaClumpAndGetFirstSubAreaClumpPathing(ref currentZone, ref listOfPotentialFirstSubAreaClumps, ref firstRoomInSubAreaClump);
            
            // Need to lock 2nd, 3rd, etc subarea clumps from the common path/previous subarea clump (1st, 2nd, 3rd etc)
            if (firstLoop == false)
            {
                Door doorToCommonPath = findAdjacentDoorThatLeadsToCommonPath(firstRoomInSubAreaClump, currentZone);
                // If the subarea zone is not next to the common path, then you will need another function above

                // Use firstRoomInSubAreaClump and lock it from the common path
                zoneLockManager.addLockToZone(nextSubAreaKey, ref firstRoomInSubAreaClump, doorToCommonPath);
            }

            // Remove sub area clump so that the another clump can be choosen later
            listOfPotentialFirstSubAreaClumps.Remove(currentSubAreaClump);

            // Queue up the zone order
            zoneOrderManager.queueSubAreaClumpOrder(currentSubAreaClump);

            GameObject roomContainingNextSubAreaKey = new GameObject();

            determineSubAreaOrderInSubAreaClump(ref zoneOrderManager, ref currentZone, subAreaClumpLockOrder[count], currentSubAreaClump, firstRoomInSubAreaClump, roomContainingNextSubAreaKey);

            // Determine the key that opens up the next sub area clump
            LockType currentSubAreaLockType = subAreaClumpLockOrder[0];
            switch (currentSubAreaLockType)
            {
                case LockType.Ability:
                    nextSubAreaKey = new KeyAbility(currentZone.getZoneAbility());
                    break;
                case LockType.Button:
                    nextSubAreaKey = new KeyButton(roomContainingNextSubAreaKey);
                    break;
            }

            if (firstLoop)
                firstLoop = false;
            count++;
        }


        // Figure out first sub area clump order

        //  Try to get a sub area that is equidistant to both primary entrances
        //      If there are multiple then try to determine an order

        // For each subarea clump try to figure out the longest path that you can get 
        //      Install locks as needed


        // ??? General rule that if a sub area is small or just one path try to add a reward at the end?

        // Get the longest path in a subarea to get more out of it???

        // Progress is determined loosely in this order 
        //      1. list Of Remaining SubAreas   OR   any subareas in list Of SubAreas Leading To BossRoom that DONT contain the boss room
        //      2. list Of SubAreas Beyond Boss SubArea if there's a path that doesn't go through the boss room
        //      3. list Of SubAreas Leading To BossRoom  
        //      4. list Of SubAreas Beyond Boss SubArea

        // TO DO:
        //      1. post boss sub area bypass detection
        //      2. Function that determines "size" based on if it's a non boss, pre boss, post boss, and boss subarea
        //      3. Make a state machine based on these parameters

        if (nonBossSubAreasExist && preBossSubAreasExist)
        {
            // Need to determine which subarea to go to first, should depend on:
            //      1. Amount of available paths 

            // If either of them are small check post boss subareas?
        }
        else if (nonBossSubAreasExist && (postBossSubAreasExist && postBossSubAreaBypassBossRoom))
        {
            // Go post boss sub area first if:
            //      1. If non boss sub areas is small, but post boss sub area is large

            // Go non boss sub area first if:
            //      1. Both non boss subarea and post boss are small
            //      2. Post boss is small
        }
        else if (preBossSubAreasExist && (postBossSubAreasExist && postBossSubAreaBypassBossRoom))
        {
            // Go post boss sub area first if:
            //      1. If pre boss sub areas is small, but post boss sub area is large

            // Go pre boss sub area first if:
            //      1. Both pree boss subarea and post boss are small
            //      2. Post boss is small
        }
        else if (postBossSubAreasExist && postBossSubAreaBypassBossRoom)
        {
            // Go post boss sub area first if:
            //      1. Boss sub area is small

            // Go boss sub area first if:
            //      1. Post boss sub area is small
            //      If post boss is small and boss sub area is small, THEN SEND AN ERROR. Cause the generation messed up
        }
        else if (!preBossSubAreasExist && !postBossSubAreasExist && !nonBossSubAreasExist)
        {
            // If boss sub area is large then use that
            //      Else send an error, generation messed up
        }


        // Post boss loot room

        // IF ANY OF THESE SUBAREAS ARE CONNECTED TO ANOTHER STARTING ZONE, THEN YOU NEED TO ACCOMODATE PROGRESS BASED ON IF IT'S A STARTING ZONE OR NOT
        //      If a sub area does connect to another starting zone, but it preserves the zone progression then proceed like normal
        //      Else try to change the progression to accomodate the new entrance
    }




    //------------------------------------------------------------------------------------------------------------------------
    // createEarlyGameZonePathing helper functions
    //------------------------------------------------------------------------------------------------------------------------
    Door findAdjacentDoorThatLeadsToCommonPath(GameObject room, Zone zone)
    {
        Door doorToCommonPath = null;
        List<Door> listOfDoors = room.GetComponent<roomProperties>().getListOfUsedDoors();
        List<GameObject> commonPath = zone.getCommonPath();

        for (int i = 0; i < listOfDoors.Count; i++)
        {
            if (commonPath.Contains(listOfDoors[i].adjacentRoom) == true)
            {
                doorToCommonPath = listOfDoors[i];
                break;
            }
        }

        if (doorToCommonPath == null)
            Debug.LogError("Progression Generator Script - findAdjacentDoorThatLeadsToCommonPath() - No adjacent room leads to common path");

        return doorToCommonPath;
    }

    void determineSubAreaOrderInSubAreaClump(ref PathOrderManager zoneOrderManager, ref Zone zone, LockType firstSubAreaLockType, SubAreaClump firstSubAreaClump, GameObject firstRoomInSubAreaClump, GameObject roomContainingNextSubAreaKey)
    {
        PathMapper zonePathMapper = zone.getPathingMapper();

        bool subAreaClumpIsBossRelated = false;
        if (firstSubAreaClump.getClumpType() == SubAreaClumpType.Normal)
            subAreaClumpIsBossRelated = false;
        else
            subAreaClumpIsBossRelated = true;

        // Get all sub area clump entrances
        List<GameObject> subAreaClumpEntrances = getSubAreaClumpEntrances(firstSubAreaClump, zone.getCommonPath());
        GameObject firstRoom = firstRoomInSubAreaClump;

        subAreaClumpEntrances.Remove(firstRoom);
        GameObject currentRoomEntrance = firstRoom;

        RoomClump currentRoomClump = new RoomClump();
        currentRoomClump.setFirstRoom(firstRoom);
        currentRoomClump.addRooms(firstSubAreaClump.getAllRoomsInClump()); // Will need to redo this later FIXME

        // Determine the furthest entrance from the first entrance to the sub area clump
        //      If there are multiple entrances then you will need to determine shortcuts back to the common path
        //      If it's a pre boss sub area clump then you will need to account for the boss sub area clump
        //      If it's a non boss related clump then it's not possible for there to be another sub area clump connected to this one

        // For now the room closest to the last entrance will have the ability or key to unlocking the next subarea clump
        //      If subareaclump only has 1 entrance then place it at the very far end of the sub area clump

        // Will need to evaluate if handleDeviatingPaths() is worth using after the refactor


        if (subAreaClumpEntrances.Count == 0)
        {
            // It's an in and out sub area clump, create the key to the lock at the furthest room possible
            GameObject furthestRoom = getFurthestRoomInSubAreaClump(firstSubAreaClump, firstRoom);
            roomContainingNextSubAreaKey = furthestRoom;
        }
        else
        {
            // Did the lazy thing for now, furthest sub area clump entrance has the key for the next subarea
            GameObject futhestRoom = new GameObject();
            List<GameObject> furthestRoomPath = new List<GameObject>();

            getFurthestRoomInSubAreaClump(firstSubAreaClump, firstRoom, subAreaClumpEntrances, ref futhestRoom, ref furthestRoomPath);
            roomContainingNextSubAreaKey = futhestRoom;

            // Loop operations:
            //      1. Get the shortest path to the closest sub area clump
            //      2. Evaluate the path and see if there are any branching paths
            //      3. Only need to handle paths that loop back around to the targetRoomPath (and the expanded room clump)
            //          - Choose branch with the longest (or shortest?) path to an entrance, lock the other branch


            // This while loop does nothing
            // IT IS TURNED OFF, WHILE LOOP NEVER ENTERS. BROKEN FUNCTION
            bool done = true;
            while (done == false)
            {
                // Get the closest sub area clump entrance
                GameObject targetRoom = new GameObject();
                List<GameObject> targetRoomPath = new List<GameObject>();

                getClosestRoomInSubAreaClump(firstSubAreaClump, currentRoomEntrance, subAreaClumpEntrances, ref targetRoom, ref targetRoomPath);
                subAreaClumpEntrances.Remove(targetRoom);
                Debug.Log("TEST_____3.5");

                //currentRoomClump.setRooms(targetRoomPath);

                // Get all paths that branch/deviate from the target room path in the current sub area clump
                List<KeyValuePair<GameObject, Door>> listOfRoomDoorDeviations = getAllPathDeviations(firstSubAreaClump, currentRoomEntrance, targetRoom, targetRoomPath);
                Debug.Log("TEST_____3.6");

                if (listOfRoomDoorDeviations.Count != 0)
                { 
                    // Handle each deviation type
                    handleDeviatingPaths(ref zoneOrderManager, listOfRoomDoorDeviations, firstSubAreaClump, currentRoomEntrance, targetRoom, targetRoomPath, subAreaClumpEntrances);
                }

                done = true;
            }
        }
    }

    // Will determine what to do with the deviating paths
    void handleDeviatingPaths(ref PathOrderManager zoneOrderManager, List<KeyValuePair<GameObject, Door>> listOfRoomDoorDeviations, SubAreaClump subAreaClump, GameObject startingRoom,
                GameObject targetRoom, List<GameObject> targetRoomPath, List<GameObject> subAreaClumpEntrances)
    {
        // ------------------ Original Planning Thoughts ------------------
        // -------------- May not be how this function works --------------
        // If the deviating path leads back to the current path
        //      You can add a lock and key to this section
        //      You can create a reward for going this way
        //      If the deviating path is really short, maybe it's worth deleting the door that leads back to the target path

        // If the deviating path leads to a dead end (includes splits that dead end)
        //      You can create a reward for going this way

        // If the deviating path touches other entrances
        //      You need to add the this door to a list
        //      DO YOU REALLY NEED TO DO THIS ????? After this for loop, you need to decide which door should be the first introduction to the rest of the sub area clump
        // ----------------------------------------------------------------

        SimplePathStorage loopingBackStorage = new SimplePathStorage(true);

        for (int i = 0; i < listOfRoomDoorDeviations.Count; i++)
        {
            bool loopsBack = false;

            KeyValuePair<GameObject, Door> currentDeviation = listOfRoomDoorDeviations[i];
            SimplePath loopingPath = new SimplePath();
            loopsBack = doesDeviatingPathLoopBack(currentDeviation, subAreaClump, targetRoomPath, subAreaClumpEntrances, ref loopingPath);

            if (loopsBack == true)
            {
                // Add the current room clump and room entrance/door combo to loopingBackStorage
                loopingBackStorage.addRoomList(loopingPath);

                Debug.LogWarning("NEVER HAD LOOPING PATHS BEFORE, see if handleDeviatingPaths() in progressionGeneratorScript works");
            }
        }

        // If there are TWO SEPERATE (or more) looping back room clumps, then you need to handle both seperately
        //      Aka handle locking, if only have 1 end of the loop be unlocked (aplies to looping back with 2+ entrances)
        //      You do NOT need to handle pathing between seperate looping back room clumps
        List<List<SimplePath>> simliarLoopingPathsList = loopingBackStorage.getSimiliarPathList();
        if (simliarLoopingPathsList.Count != 0)
        {
            for (int i = 0; i < simliarLoopingPathsList.Count; i++)
            {
                List<SimplePath> similiarLoopPaths = simliarLoopingPathsList[i];

                // Lock the looping paths that are the closest to the start
                lockSimliarLoopingPathsInSubAreaClump(ref zoneOrderManager, subAreaClump, startingRoom, targetRoom, targetRoomPath, similiarLoopPaths);
            }
        }
    }

    // Provide the start room, will return the furthest room from it
    GameObject getFurthestRoomInSubAreaClump(SubAreaClump subAreaClump, GameObject firstRoom)
    {
        GameObject furthestRoom = new GameObject();
        List<List<GameObject>> listOfPaths = new List<List<GameObject>>();

        traverseSubAreaClumpTillDeadEndStart(subAreaClump, ref listOfPaths, firstRoom);

        List<GameObject> longestList = getLargestCountInList(listOfPaths);

        if (longestList[0].Equals(firstRoom))
            furthestRoom = longestList[longestList.Count - 1];
        else
            furthestRoom = longestList[0];

        return furthestRoom;
    }


    // Given a list of looping paths and the path that they all loop to (targetRoomPath) will lock appropriate rooms
    //      1. Rule for now is that it locks all rooms but the furthest one from the provided starting room
    void lockSimliarLoopingPathsInSubAreaClump(ref PathOrderManager zoneOrderManager, SubAreaClump subAreaClump, GameObject startingRoom, GameObject targetRoom,
                                    List<GameObject> targetRoomPath, List<SimplePath> similiarLoopPaths)
    {
        ZoneLockManager zoneLockManager = MapGeneratorScript.zoneManager.getLockManager();

        // Find the closest loop path start
        //                                          Room, Door, Path
        MinValue<ThreeValue<GameObject, Door, SimplePath>> closestLoopOrder 
                = new MinValue<ThreeValue<GameObject, Door, SimplePath>>(similiarLoopPaths.Count);

        for (int i = 0; i < similiarLoopPaths.Count; i++)
        {
            SimplePath currentLoopingPath = similiarLoopPaths[i];
            List<GameObject> endRooms = currentLoopingPath.getEndRoomList();
            GameObject loopPathStartRoom = new GameObject();

            // Find endroom that is in the target path. Will end up searching for this room
            for (int j = 0; j < endRooms.Count; j++)
            {
                if (targetRoomPath.Contains(endRooms[j]))
                {
                    loopPathStartRoom = endRooms[j];
                    break;
                }
            }

            List<GameObject> subareaClumpDestinations = new List<GameObject> { loopPathStartRoom };
            GameObject nearestRoom = new GameObject();
            List<GameObject> nearestPath = new List<GameObject>();

            // Get the closest path from the starting room, to the first room in the looping back path
            getClosestRoomInSubAreaClump(subAreaClump, startingRoom, subareaClumpDestinations, ref nearestRoom, ref nearestPath);
            SimplePath nearestSimplePath = new SimplePath(nearestPath);

            // Get the second room in the loop path, door connecting first and second. Add to min value class
            GameObject secondRoomInLoopPath = currentLoopingPath.getRoomInPathViaIndex(loopPathStartRoom, 1);
            Door doorConnectingFirstLoopRoomAndSecondLoopRoom = currentLoopingPath.getConnectingDoorInPath(loopPathStartRoom, secondRoomInLoopPath);
            ThreeValue<GameObject, Door, SimplePath> roomDoorPathCombo = new ThreeValue<GameObject, Door, SimplePath>(nearestRoom, doorConnectingFirstLoopRoomAndSecondLoopRoom, nearestSimplePath);
            closestLoopOrder.addValue(nearestSimplePath.getPathCount(), roomDoorPathCombo);
        }

        // Evaluate which room gets locked, only locking (list - 1) doors
        LinkedList<KeyValuePair<int, ThreeValue<GameObject, Door, SimplePath>>> minOrderedList 
                        = closestLoopOrder.getMinValues();
        LinkedList<KeyValuePair<int, ThreeValue<GameObject, Door, SimplePath>>>.Enumerator enumerator = minOrderedList.GetEnumerator();
        int count = 0;
        foreach (var index in minOrderedList)
        {
            // Only want to lock all but the last door
            if (count > minOrderedList.Count - 1)
                break;

            // Lock doors without question in min ordered list, if there's a tie then so what for now
            GameObject currentRoom = index.Value.getFirst();
            Door currentDoor = index.Value.getSecond();
            List<GameObject> path = index.Value.getThird().getPath();

            GameObject roomWithButton = currentDoor.adjacentRoom; // The door leading to the adjacent room should have the button
            KeyButton newButtonLock = new KeyButton(roomWithButton);
            zoneLockManager.addLockToZone(newButtonLock, ref currentRoom, currentDoor);

            enumerator.MoveNext();
            count++;
        }
    }

    // Will identify if a deviating path is a dead end, loops around, splits, or lead to other entrances
    bool doesDeviatingPathLoopBack(KeyValuePair<GameObject, Door> currentDeviation, SubAreaClump subAreaClump, List<GameObject> originalPath, 
                    List<GameObject> subAreaClumpEntrances, ref SimplePath loopingPath)
    {
        // First check if any paths touch other entrances
        bool loopsBack = false;
        List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
        GameObject startRoom = currentDeviation.Key;
        GameObject nextRoom = currentDeviation.Value.adjacentRoom;

        // Now check if it loops back to the original path
        listOfPaths = new List<List<GameObject>>();
        List<GameObject> endRooms = originalPath;

        traverseSubAreaClumpStartWithNextRoomDetermined(subAreaClump, ref listOfPaths, startRoom, nextRoom, endRooms);
        List<GameObject> shortestPathToOriginalPath = getSmallestCountInList(listOfPaths);


        if (listOfPaths.Count != 0)
        {
            loopsBack = true;
            loopingPath.setPath(shortestPathToOriginalPath);
            loopingPath.setEndRooms(shortestPathToOriginalPath[0], shortestPathToOriginalPath[shortestPathToOriginalPath.Count - 1]);
        }

        return loopsBack;
    }

    // Will travel along the given path and return any branches/deviations
    List<KeyValuePair<GameObject, Door>> getAllPathDeviations(SubAreaClump subAreaClump, GameObject startingRoom, GameObject targetRoom, List<GameObject> targetRoomPath)
    {
        List<KeyValuePair<GameObject, Door>> pathDeviations = new List<KeyValuePair<GameObject, Door>>();

        if (targetRoomPath[0].Equals(targetRoom))
            targetRoomPath.Reverse();

        for (int i = 0; i < targetRoomPath.Count; i++)
        {
            GameObject currentRoom = targetRoomPath[i];
            roomProperties currentRoomProperties = currentRoom.GetComponent<roomProperties>();
            List<Door> usedDoorsList = currentRoomProperties.getListOfUsedDoors();

            if (usedDoorsList.Count <= 2)
                continue;
            else
            {
                for (int j = 0; j < usedDoorsList.Count; j++)
                {
                    Door currentDoor = usedDoorsList[j];
                    if (targetRoomPath.Contains(currentDoor.adjacentRoom) == false && subAreaClump.roomIsInSubAreaClump(currentDoor.adjacentRoom))
                    {
                        pathDeviations.Add(new KeyValuePair<GameObject, Door>(currentRoom, currentDoor));
                    }
                }
            }
        }

        return pathDeviations;
    }

    // Provide the sub area clump, the list of room destinations and the the starting room. Will return the room with the shortest path
    void getClosestRoomInSubAreaClump(SubAreaClump subAreaClump, GameObject startingRoom,  
                            List<GameObject> subAreaClumpDestinations, ref GameObject nearestRoom, ref List<GameObject> nearestRoomPath)
    {
        KeyValuePair<GameObject, List<GameObject>> nearestRoomKeyPair = new KeyValuePair<GameObject, List<GameObject>>();
        MinValue<KeyValuePair<GameObject, List<GameObject>>> closestRoomOrder = new MinValue<KeyValuePair<GameObject, List<GameObject>>>(1);
        List<GameObject> listOfDestinations = subAreaClumpDestinations;

        for (int i = 0; i < listOfDestinations.Count; i++)
        {
            GameObject endRoom = listOfDestinations[i];
            List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
            traverseSubAreaClumpStart(subAreaClump, ref listOfPaths, startingRoom, endRoom);

            // Get the smallest/most direct path
            for (int j = 0; j < listOfPaths.Count; j++)
            {
                List<GameObject> currentPath = listOfPaths[j];
                KeyValuePair<GameObject, List<GameObject>> endRoomAndPathKeyPair = new KeyValuePair<GameObject, List<GameObject>>(endRoom, currentPath);
                closestRoomOrder.addValue(currentPath.Count, endRoomAndPathKeyPair);
            }
        }

        nearestRoomKeyPair = closestRoomOrder.randomlyChooseValue();
        nearestRoom = nearestRoomKeyPair.Key;
        nearestRoomPath = nearestRoomKeyPair.Value;
    }

    // Provide the sub area clump, the list of room destinations and the the starting room. Will return the room with the longest path
    void getFurthestRoomInSubAreaClump(SubAreaClump subAreaClump, GameObject startingRoom,
                            List<GameObject> subAreaClumpDestinations, ref GameObject futhestRoom, ref List<GameObject> futhestRoomPath)
    {
        KeyValuePair<GameObject, List<GameObject>> furthestRoomKeyPair = new KeyValuePair<GameObject, List<GameObject>>();
        MaxValue<KeyValuePair<GameObject, List<GameObject>>> furthestRoomOrder = new MaxValue<KeyValuePair<GameObject, List<GameObject>>>(1);
        List<GameObject> listOfDestinations = subAreaClumpDestinations;

        for (int i = 0; i < listOfDestinations.Count; i++)
        {
            GameObject endRoom = listOfDestinations[i];
            List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
            traverseSubAreaClumpStart(subAreaClump, ref listOfPaths, startingRoom, endRoom);

            // Get the smallest/most direct path
            for (int j = 0; j < listOfPaths.Count; j++)
            {
                List<GameObject> currentPath = listOfPaths[j];
                KeyValuePair<GameObject, List<GameObject>> endRoomAndPathKeyPair = new KeyValuePair<GameObject, List<GameObject>>(endRoom, currentPath);
                furthestRoomOrder.addValue(currentPath.Count, endRoomAndPathKeyPair);
            }
        }

        furthestRoomKeyPair = furthestRoomOrder.randomlyChooseValue();
        futhestRoom = furthestRoomKeyPair.Key;
        futhestRoomPath = furthestRoomKeyPair.Value;
    }

    void traverseSubAreaClumpStartWithNextRoomDetermined(SubAreaClump clump, ref List<List<GameObject>> listOfPaths, GameObject startRoom, 
                                        GameObject choosenNextRoom , List<GameObject> endRooms)
    {
        List<SubArea> subAreaList = clump.getListOfSubAreas();
        List<GameObject> currentPath = new List<GameObject>();

        currentPath.Add(startRoom);

        //                                                              prev,      startRoom
        traverseSubAreaClump(subAreaList, ref listOfPaths, currentPath, startRoom, choosenNextRoom, endRooms);
    }

    void traverseSubAreaClumpStart(SubAreaClump clump, ref List<List<GameObject>> listOfPaths, GameObject startRoom, GameObject endRoom)
    {
        List<SubArea> subAreaList = clump.getListOfSubAreas();
        List<GameObject> currentPath = new List<GameObject>();
        List<GameObject> endRooms = new List<GameObject> { endRoom };

        traverseSubAreaClump(subAreaList, ref listOfPaths, currentPath, startRoom, startRoom, endRooms);
    }

    void traverseSubAreaClumpTillDeadEndStart(SubAreaClump clump, ref List<List<GameObject>> listOfPaths, GameObject startRoom)
    {
        List<SubArea> subAreaList = clump.getListOfSubAreas();
        List<GameObject> currentPath = new List<GameObject>();

        traverseSubAreaClumpTillDeadEnd(subAreaList, ref listOfPaths, currentPath, startRoom, startRoom);
    }

    void traverseSubAreaClump(List<SubArea> subAreaList, ref List<List<GameObject>> listOfPaths, List<GameObject> currentPath, 
                                GameObject previousRoom, GameObject currentRoom, List<GameObject> endRooms)
    {
        roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

        currentPath.Add(currentRoom);

        // Basically check all adjacent rooms
        for (int i = 0; i < currentRoomProps.doorList.Count; i++)
        {
            // If door is used
            if (currentRoomProps.doorList[i].doorUsedBool == true)
            {
                // If door doesn't lead back to the previous room 
                if (currentRoomProps.doorList[i].adjacentRoom.name != previousRoom.name)
                {
                    GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProperties = adjacentRoom.GetComponent<roomProperties>();

                    // Check to see if the adjacent room is a part of the sub area list
                    bool partOfSubAreaList = false;
                    List<SubArea> adjacentRoomParentSubAreas = adjacentRoomProperties.getParentSubArea();

                    for (int j = 0; j < adjacentRoomParentSubAreas.Count; j++)
                    {
                        if (subAreaList.Contains(adjacentRoomParentSubAreas[j]) == true)
                        { 
                            partOfSubAreaList = true;
                            break;
                        }
                    }

                    if (currentPath.Contains(adjacentRoom) || partOfSubAreaList == false)
                    {
                        // It doesn't lead to a room within the sub area list
                        // If the adjacent room is already a part of the current path then stop the path here
                    }
                    // If the adjacent room is the end destination add it to the list of paths and stop searching
                    else if (endRooms.Contains(adjacentRoom))
                    {
                        //currentPath.Add(adjacentRoom);//  Can't add, it will screw up further searches
                        List<GameObject> tempPath = new List<GameObject>(currentPath); // Need to create a new list
                        tempPath.Add(adjacentRoom);
                        listOfPaths.Add(tempPath);
                    }
                    else
                    {
                        //                                                              prev room,   next room,
                        traverseSubAreaClump(subAreaList, ref listOfPaths, currentPath, currentRoom, adjacentRoom, endRooms);
                    }
                }
            }
        }

        // I think C# is retaining currentPath values across the recursion
        //     I have no idea WHY and nothing is useful online 
        //     So once you exit this function delete the current room
        currentPath.Remove(currentRoom);
    }

    // No destination is specifdied, will return all paths that lead to deadends
    void traverseSubAreaClumpTillDeadEnd(List<SubArea> subAreaList, ref List<List<GameObject>> listOfPaths, List<GameObject> currentPath,
                                GameObject previousRoom, GameObject currentRoom)
    {
        roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

        currentPath.Add(currentRoom);

        // Basically check all adjacent rooms
        for (int i = 0; i < currentRoomProps.doorList.Count; i++)
        {
            // If door is used
            if (currentRoomProps.doorList[i].doorUsedBool == true)
            {
                // If door doesn't lead back to the previous room 
                if (currentRoomProps.doorList[i].adjacentRoom.name != previousRoom.name)
                {
                    GameObject adjacentRoom = currentRoomProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProperties = adjacentRoom.GetComponent<roomProperties>();

                    // Check to see if the adjacent room is a part of the sub area list
                    bool partOfSubAreaList = false;
                    List<SubArea> adjacentRoomParentSubAreas = adjacentRoomProperties.getParentSubArea();

                    for (int j = 0; j < adjacentRoomParentSubAreas.Count; j++)
                    {
                        if (subAreaList.Contains(adjacentRoomParentSubAreas[j]) == true)
                        {
                            partOfSubAreaList = true;
                            break;
                        }
                    }

                    if (partOfSubAreaList == false)
                    {
                        // It doesn't lead to a room within the sub area list
                    }
                    // If the adjacent room is already a part of the current path then record this path
                    else if (currentPath.Contains(adjacentRoom))
                    {
                        List<GameObject> tempPath = new List<GameObject>(currentPath); // Need to create a new list
                        listOfPaths.Add(tempPath);
                    }
                    // If the adjacent room is a dead end, then add the room and record this path
                    else if (adjacentRoomProperties.getListOfUsedDoors().Count == 1)
                    {
                        //currentPath.Add(adjacentRoom);//  Can't add, it will screw up further searches
                        List<GameObject> tempPath = new List<GameObject>(currentPath); // Need to create a new list
                        tempPath.Add(adjacentRoom);
                        listOfPaths.Add(tempPath);
                    }
                    else
                    {
                        //                                                              prev room,   next room,
                        traverseSubAreaClumpTillDeadEnd(subAreaList, ref listOfPaths, currentPath, currentRoom, adjacentRoom);
                    }
                }
            }
        }

        // I think C# is retaining currentPath values across the recursion
        //     I have no idea WHY and nothing is useful online 
        //     So once you exit this function delete the current room
        currentPath.Remove(currentRoom);
    }

    List<GameObject> getAllRoomsInDictionary(Dictionary<SubArea, List<GameObject>> dict)
    {
        List<GameObject> listOfRooms = new List<GameObject>();

        foreach (var dictItem in dict)
        {
            for (int i = 0; i < dictItem.Value.Count; i++)
            {
                listOfRooms.Add(dictItem.Value[i]);
            }
        }

        return listOfRooms;
    }

    // Determine if a key or abilty should be placed in this sub area clump
    //      If there are no other sub area clumps then you have to place the ability
    //      Else you can randomly choose
    //      CURRENTLY ONLY DOES BUTTON AND ABILITY LOCKS
    List<LockType> determineSubAreaClumpLockOrder(List<SubAreaClump> listOfPotentialFirstSubAreaClumps)
    {
        List<LockType> subAreaClumpLockOrder = new List<LockType>();

        int rand = UnityEngine.Random.Range(0, listOfPotentialFirstSubAreaClumps.Count);
        for (int i = 0; i < listOfPotentialFirstSubAreaClumps.Count; i++) 
        {
            if (i == rand)
                subAreaClumpLockOrder.Add(LockType.Ability);
            else
                subAreaClumpLockOrder.Add(LockType.Button);
        }

        return subAreaClumpLockOrder;
    }

    // Will get all entrances to the sub area clumps
    List<GameObject> getSubAreaClumpEntrances(SubAreaClump subAreaClump, List<GameObject> commonPath)
    {
        List<GameObject> subAreaClumpEntranceList = new List<GameObject>();
        List<SubArea> subAreaList = subAreaClump.getListOfSubAreas();

        for (int i = 0; i < subAreaList.Count; i++)
        {
            SubArea currentSubArea = subAreaList[i];

            // Get each entrance connected to the common path
            if (currentSubArea.isConnectedToCommonPath() == true)
            {
                List<GameObject> commonPathEntrances = currentSubArea.getAllCommonPathEntrances(commonPath);
                for (int j = 0; j < commonPathEntrances.Count; j++)
                {
                    subAreaClumpEntranceList.Add(commonPathEntrances[i]);
                }
            }

            // if (sub area clump is boss related) {
                // Now check each entrance connected to a sub area
                //      If it's not in the clump then you have found a sub area clump entrance
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //}
        }

        return subAreaClumpEntranceList;
    }

    SubAreaClump getFirstSubAreaClumpAndGetFirstSubAreaClumpPathing(ref Zone zone, ref List<SubAreaClump> listOfPotentialFirstSubAreaClumps, ref GameObject firstRoomInSubAreaClump
                                    /*ref SubArea firstSubAreaInSubAreaClump, ref PathsClassOrder firstPathOrderInFirstSubArea*/)
    {
        //  Try to get a sub area that is equidistant to both primary entrances
        KeyValuePair<GameObject, SubAreaClump> firstSubAreaClumpKeyPair = new KeyValuePair<GameObject, SubAreaClump>();
        SubAreaClump firstSubAreaClump = new SubAreaClump();

        List<GameObject> primaryZoneEntrances = zone.getPrimaryZoneEntrances();

        // If theres only 1 primary zone entrance, then make the first subzone clump the furthest/2nd furthest one
        if (primaryZoneEntrances.Count == 1)
        {
            // Find the first sub area clump and the first room for that clump
            firstSubAreaClumpKeyPair = getFirstSubAreaClumpForSinglePrimaryEntrance(ref zone, ref listOfPotentialFirstSubAreaClumps);
            firstSubAreaClump = firstSubAreaClumpKeyPair.Value;
            firstRoomInSubAreaClump = firstSubAreaClumpKeyPair.Key;

            // Get the first sub area 
            //      And get the first path in the first subarea from the first room
            //firstSubAreaInSubAreaClump = determineSubAreaFromRoomInASubAreaClump(firstSubAreaClump, firstRoomInFirstSubArea);
            //Paths firstPathInFirstSubArea = determinePathFromRoomInASubArea(firstSubAreaInSubAreaClump, firstRoomInFirstSubArea);

            // Set up the firstPathOrder
            //firstPathOrderInFirstSubArea = new PathsClassOrder(firstPathInFirstSubArea, firstRoomInFirstSubArea);

            Debug.Log("1 PRIMARY ENTRANCE \n \n BIG ASS TEST: " + firstSubAreaClumpKeyPair.Key.name);
        }
        else
        {
            // Find the first sub area clump and the first room for that clump
            firstSubAreaClumpKeyPair = getFirstSubAreaClumpForDoublePrimaryEntrance(ref zone, ref listOfPotentialFirstSubAreaClumps);
            firstSubAreaClump = firstSubAreaClumpKeyPair.Value;
            firstRoomInSubAreaClump = firstSubAreaClumpKeyPair.Key;

            // Get the first sub area 
            //      And get the first path in the first subarea from the first room
            //firstSubAreaInSubAreaClump = determineSubAreaFromRoomInASubAreaClump(firstSubAreaClump, firstRoomInFirstSubArea);
            //Paths firstPathInFirstSubArea = determinePathFromRoomInASubArea(firstSubAreaInSubAreaClump, firstRoomInFirstSubArea);

            // Set up the firstPathOrder
            //firstPathOrderInFirstSubArea = new PathsClassOrder(firstPathInFirstSubArea, firstRoomInFirstSubArea);

            Debug.Log("2 PRIMARY ENTRANCES \n \n  BIG ASS TEST: " + firstSubAreaClumpKeyPair.Key.name);
        }

        return firstSubAreaClump;
    }

    // If a zone has 2 primary entrance, then use this function to determine which sub area clump should be the first clump to path for
    //      Protocol is to grab the 2 furthest most central sub area clumps and randomly choose one of them
    KeyValuePair<GameObject, SubAreaClump> getFirstSubAreaClumpForDoublePrimaryEntrance(ref Zone zone, ref List<SubAreaClump> listOfPotentialFirstSubAreaClumps)
    {
        KeyValuePair<GameObject, SubAreaClump> centralSubAreaClump = new KeyValuePair<GameObject, SubAreaClump>();

        //                   <CommonPath Room, SubArea Clump>
        List<KeyValuePair<GameObject, SubAreaClump>> mostCentralSubAreaClumps = new List<KeyValuePair<GameObject, SubAreaClump>>();

        // Get the shortest path between the primary entrances, aka common path
        List<GameObject> centralCommonRooms = getMostCentralRoomsInList(zone.getCommonPath());

        for (int i = 0; i < listOfPotentialFirstSubAreaClumps.Count; i++)
        {
            SubAreaClump currentSubAreaClump = listOfPotentialFirstSubAreaClumps[i];

            // Get the rooms that connect to the common path
            List<GameObject> commonPathRooms = getRoomsFromSubAreaClumpConnectedToCommonPath(zone, currentSubAreaClump);

            // Go through each common path room in a sub area clump and see if they are close to the central common path rooms
            for (int j = 0; j < commonPathRooms.Count; j++)
            {
                GameObject subAreaClumpCommonRoom = commonPathRooms[j];

                // If the current sub area clump room is in the acceptable central room list add it as a condidate to choose from
                if (centralCommonRooms.Contains(subAreaClumpCommonRoom))
                {
                    mostCentralSubAreaClumps.Add(new KeyValuePair<GameObject, SubAreaClump>(subAreaClumpCommonRoom, currentSubAreaClump));
                }
            }
        }

        if (mostCentralSubAreaClumps.Count == 0)
        {
            Debug.Log("NO CENTRAL SUBAREA FOUND FOR THE COMMON ROOM");
            // Lock the least main primary entrance and use getFirstSubAreaClumpForSinglePrimaryEntrance() instead
            MapGeneratorScript.zoneManager.lockTheSmallestPrimaryEntranceRoom(ref zone);

            centralSubAreaClump = getFirstSubAreaClumpForSinglePrimaryEntrance(ref zone, ref listOfPotentialFirstSubAreaClumps);
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, mostCentralSubAreaClumps.Count);
            centralSubAreaClump = mostCentralSubAreaClumps[rand];
        }


        return centralSubAreaClump;
    }

    // If a zone has 1 primary entrance, then use this function to determine which sub area clump should be the first clump to path for
    //      Protocol is to grab the 2 furthest sub area clumps and randomly choose one of them
    KeyValuePair<GameObject, SubAreaClump> getFirstSubAreaClumpForSinglePrimaryEntrance(ref Zone zone, ref List<SubAreaClump> listOfPotentialFirstSubAreaClumps)
    {
        KeyValuePair<GameObject, SubAreaClump> firstSubAreaClump = new KeyValuePair<GameObject, SubAreaClump>();

        //                   <CommonPath Room, SubArea Clump>
        MaxValue<KeyValuePair<GameObject, SubAreaClump>> farthestSubAreaClump = new MaxValue<KeyValuePair<GameObject, SubAreaClump>>(2);
        MaxValue<KeyValuePair<GameObject, SubAreaClump>> backupFarthestSubAreaClump = new MaxValue<KeyValuePair<GameObject, SubAreaClump>>(1);
        List<GameObject> primaryZoneEntrances = zone.getPrimaryZoneEntrances();

        List<GameObject> doNotTravelList = new List<GameObject>();
        List<GameObject> onlyTravelInList = new List<GameObject>();
        onlyTravelInList.AddRange(zone.getCommonPath());

        for (int i = 0; i < listOfPotentialFirstSubAreaClumps.Count; i++)
        {
            SubAreaClump currentSubAreaClump = listOfPotentialFirstSubAreaClumps[i];

            // Get the rooms that connect to the common path
            List<GameObject> commonPathRooms = getRoomsFromSubAreaClumpConnectedToCommonPath(zone, currentSubAreaClump);
            MaxValue<GameObject> fartestRoomInSubAreaClump = new MaxValue<GameObject>(1);

            // Go through each common path room in a sub area clump and get the furthest one
            for (int j = 0; j < commonPathRooms.Count; j++)
            {
                GameObject subAreaClumpCommonRoom = commonPathRooms[j];
                List<List<GameObject>> listOfPaths = new List<List<GameObject>>();

                zone.getPathingMapper().traverseZoneRoomListStart(zone.getZoneId(), primaryZoneEntrances[0], primaryZoneEntrances[0], subAreaClumpCommonRoom,
                                                    ref listOfPaths, doNotTravelList, onlyTravelInList);

                List<GameObject> largestList = zone.getPathingMapper().getLargestCountInList(listOfPaths);
                fartestRoomInSubAreaClump.addValue(largestList.Count, subAreaClumpCommonRoom);
            }

            LinkedList<KeyValuePair<int, GameObject>> farthestCommonPathRoomInCurrentSubArea = fartestRoomInSubAreaClump.getMaxValues();

            KeyValuePair<GameObject, SubAreaClump> commonRoomSubAreaClumpPair
                = new KeyValuePair<GameObject, SubAreaClump>(farthestCommonPathRoomInCurrentSubArea.First.Value.Value, currentSubAreaClump);
            int farthestCommonPathRoomInCurrentSubAreaDistance = farthestCommonPathRoomInCurrentSubArea.First.Value.Key;

            // Make sure that the sub area clump entrance is at least x rooms away from the start to the zone
            if (farthestCommonPathRoomInCurrentSubAreaDistance > minDistanceForSinglePrimaryEntranceZones)
            {
                farthestSubAreaClump.addValue(farthestCommonPathRoomInCurrentSubAreaDistance, commonRoomSubAreaClumpPair);
            }
            backupFarthestSubAreaClump.addValue(farthestCommonPathRoomInCurrentSubAreaDistance, commonRoomSubAreaClumpPair);
        }

        if (farthestSubAreaClump.getCount() == 0)
            firstSubAreaClump = backupFarthestSubAreaClump.randomlyChooseValue();
        else
            firstSubAreaClump = farthestSubAreaClump.randomlyChooseValue();


        return firstSubAreaClump;
    }


    List<GameObject> getMostCentralRoomsInList(List<GameObject> roomList)
    {
        // This function assumes that the room list is already organized
        List<GameObject> mostCentralRooms = new List<GameObject>();
        double middle = (double) roomList.Count /  (double) 2;
        
        // Even amount, means that 2 rooms are the most central
        if (roomList.Count % 2 == 0)
        {
            int middleIndexHigh = (int)middle;
            int middleIndexLow = (int)middle - 1; // Subtract one cause index starts at 0 

            for (int i = middleIndexLow - equalDistanceTolerance; i <= middleIndexHigh + equalDistanceTolerance; i++)
            {
                if (0 <= i && i < roomList.Count)
                    mostCentralRooms.Add(roomList[i]);
            }

        }
        // Odd amount of rooms
        else
        {
            int middleIndex = (int)Math.Ceiling(middle) - 1; // Subtract one cause index starts at 0

            for (int i = middleIndex - equalDistanceTolerance; i <= middleIndex + equalDistanceTolerance; i++)
            {
                if (0 <= i && i < roomList.Count)
                    mostCentralRooms.Add(roomList[i]);
            }
        }
            

        return mostCentralRooms;
    }

    // Will get all sub area clumps for subareas not related to the boss room and pre boss room sub areas
    void getAllPotentialFirstSubAreaClumps(ref PathMapper zonePathMapper, List<SubArea> listOfRemainingSubAreas, List<SubArea> listOfSubAreasLeadingToBossRoom,
                                            ref List<SubAreaClump> listOfPotentialFirstSubAreaClumps)
     {
        if (listOfRemainingSubAreas.Count != 0)
        {
            List<SubAreaClump> listOfNonBossSubAreaClumps = zonePathMapper.sortSubAreaListIntoClumps(listOfRemainingSubAreas);
            for (int i = 0; i < listOfNonBossSubAreaClumps.Count; i++)
            {
                SubAreaClump currentSubAreaClump = listOfNonBossSubAreaClumps[i];
                currentSubAreaClump.setClumpType(SubAreaClumpType.Normal);
                listOfPotentialFirstSubAreaClumps.Add(currentSubAreaClump);
            }
        }
        if (listOfSubAreasLeadingToBossRoom.Count != 0)
        {
            SubAreaClump preBossSubAreaClump = new SubAreaClump();
            preBossSubAreaClump.addSubAreaList(listOfSubAreasLeadingToBossRoom);
            preBossSubAreaClump.setClumpType(SubAreaClumpType.PreBoss);
            listOfPotentialFirstSubAreaClumps.Add(preBossSubAreaClump);
        }
     }

    // Provide a room to look for within a sub area clump to get the sub area associated with the room
    SubArea determineSubAreaFromRoomInASubAreaClump(SubAreaClump subAreaClump, GameObject room)
    {
        SubArea searchSubArea = null;

        List<SubArea> subAreaList = subAreaClump.getListOfSubAreas();

        for (int i = 0; i < subAreaList.Count; i++)
        {
            if (subAreaList[i].getAllRooms().Contains(room))
            {
                searchSubArea = subAreaList[i];
                break;
            }
        }

        return searchSubArea;
    }

    // Provide a room to look for within a sub area  to get the path associated with the sub area
    Paths determinePathFromRoomInASubArea(SubArea subArea, GameObject room)
    {
        Paths searchPaths = null;

        List<Paths> pathsList = subArea.getAllPaths();

        for (int i = 0; i < pathsList.Count; i++)
        {
            if (pathsList[i].getPathList().Contains(room))
            {
                searchPaths = pathsList[i];
                break;
            }
        }

        return searchPaths;
    }

    List<GameObject> getRoomsFromSubAreaClumpConnectedToCommonPath(Zone zone, SubAreaClump subAreaClump)
    {
        List<GameObject> roomsConnectedToCommonPath = new List<GameObject>();
        List<SubArea> subAreasConnectingToCommonPath = subAreaClump.getCommonPathSubAreas();

        // Go through all subareas touching the common path
        for (int i = 0; i < subAreasConnectingToCommonPath.Count; i++)
        {
            List<Paths> pathsConnectingToCommonPath = subAreasConnectingToCommonPath[i].getPathConnectedToCommonPath();

            // Go through all paths in subareas that are touching the common path
            for (int j = 0; j < pathsConnectingToCommonPath.Count; j++)
            {
                List<GameObject> path = pathsConnectingToCommonPath[j].getPathList();

                // For each room in the path, see if it's a part of the common path
                for (int k = 0; k < path.Count; k++)
                {
                    if (zone.getCommonPath().Contains(path[k]))
                        roomsConnectedToCommonPath.Add(path[k]);
                }
            }
        }

        return roomsConnectedToCommonPath;
    }

    //------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------



    List<ProgressionType> profileZone(int zoneIndex)
    {
        // This function will profile the zone and select possible progression types

        // Randomly decide if the boss room is going to be locked via zone ability or normal progression
        //    Make it so that it's 66.666% normal progression and 33.333% locked via zone ability

        // Get some stats about the zone room placement
        //  0. Need to also randomly select the a zone to start placements with (in case zones cross over)
        //  1. Get the shortest path to the boss room
        //      a. From all initial zone entrances
        //  2. Block off the shortest path with a story lock
        //  3. Search for another path to the boss room

        List<ProgressionType> possibleProgressionTypes = new List<ProgressionType>();
        return possibleProgressionTypes;
    }

    // Depricated
    void lockNonStartingEntrances()
    {
        // Go through all zone entrances for the starting zone and lock them via ability if they aren't in sendOffRoomDungeonEntrance
        //      AKA if they aren't coming from the sendoff room
        for (int i = 0; i < MapGeneratorScript.zoneManager.zoneList.Count; i++)
        {
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[i];
            if (currentZone.getIsEarlyGameZone())
            {
                for (int j = 0; j < currentZone.roomsList.Count; j++)
                {
                    GameObject currentRoom = currentZone.roomsList[j];
                    roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

                    // If the room is a zone entrance, then we need to check if we already locked it
                    if (currentRoomProps.isZoneEntrance == true && MapGeneratorScript.zoneManager.roomIsInANonVeinZone(currentRoom))
                    {
                        if (sendOffRoomDungeonEntrance.Contains(currentRoom) == true)
                        { }// If the zone entrance is from the send off room then do nothing
                        else if (currentRoomProps.roomIsLocked() == false)
                        {
                            abilities zoneAbility = abilities.None;
                            int adjacentDoorId = 0;
                            List<int> lockedDoorIds = new List<int>();
                            bool adjacentRoomIsInAnotherZone = checkAdjacentRoomForDifferentNonVeinZone(currentRoom, ref adjacentDoorId);
                            // Check if any adjacent rooms leads to another non vein zone
                            //    What if a room leads to multiple zones
                            if (adjacentRoomIsInAnotherZone)
                            {
                                if (currentRoomProps.doorList[adjacentDoorId].adjacentRoom.GetComponent<roomProperties>().roomIsLocked() == false)
                                {
                                    // Randomly choose one of the rooms to lock
                                    int randNum = UnityEngine.Random.Range(0, 2);
                                    GameObject randLockedRoom = null;
                                    GameObject notLockedAdjacentRoom = null;
                                    if (randNum == 0)
                                    {
                                        // Select the current room
                                        notLockedAdjacentRoom = currentRoomProps.doorList[adjacentDoorId].adjacentRoom;
                                        randLockedRoom = currentRoom;
                                    }
                                    else
                                    {
                                        // Select the adjacent room
                                        randLockedRoom = currentRoomProps.doorList[adjacentDoorId].adjacentRoom;
                                        notLockedAdjacentRoom = currentRoom;
                                    }

                                    // Lock the room with the selected room zone ability
                                    lockedDoorIds = MapGeneratorScript.zoneManager.getDoorIdsThatLeadsToADifferentNonVeinZone(randLockedRoom);

                                    for (int k = 0; k < lockedDoorIds.Count; k++)
                                    {
                                        zoneAbility = MapGeneratorScript.zoneManager.getAbilityFromZoneTypeId(randLockedRoom.GetComponent<roomProperties>().getZoneTypeId());
                                        lockRoomWithAbility(ref randLockedRoom, zoneAbility, lockedDoorIds[k]);
                                    }


                                    // Make sure to record the other side of the door that the room can be a new possible zone entrance
                                    //MapGeneratorScript.zoneManager.addPrimaryRoomEntrance(notLockedAdjacentRoom.GetComponent<roomProperties>().getZoneId(), notLockedAdjacentRoom);
                                }
                            }
                            else
                            {
                                // Lock the room with the current zone ability
                                lockedDoorIds = MapGeneratorScript.zoneManager.getDoorIdsThatLeadsToADifferentNonVeinZone(currentRoom);
                                for (int k = 0; k < lockedDoorIds.Count; k++)
                                {
                                    zoneAbility = MapGeneratorScript.zoneManager.getAbilityFromZoneTypeId(currentRoomProps.getZoneTypeId());
                                    lockRoomWithAbility(ref currentRoom, zoneAbility, lockedDoorIds[k]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    void setupMidAndLateGameZones()
    {
        // Randomly select a progression type for a zone
        //      If the progression type is locked to the zone theme and ability set it here
        for (int i = 0; i < MapGeneratorScript.zoneManager.zoneList.Count; i++)
        {
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[i];
            int zoneId = MapGeneratorScript.zoneManager.zoneIdList[i];

            // We first profile all starting zones, decide which progress types can work
            //      Then we implement them
            if (currentZone.getIsEarlyGameZone() == false && zoneIsAThemedZone(i))
            {
                List<ProgressionType> possibleProgressTypes = new List<ProgressionType>();
                possibleProgressTypes = profileZone(i);

                setupProgressionType(possibleProgressTypes);
            }
        }
    }

    void setupProgressionType(List<ProgressionType> possibleProgressionTypes)
    {
        int randNum = UnityEngine.Random.Range(0, possibleProgressionTypes.Count);
        ProgressionType randomProgressType = ProgressionType.Type1;// (ProgressionType)possibleProgressionTypes[randNum];

        switch (randomProgressType)
        {
            case ProgressionType.Type1:
                // code block
                break;
            case ProgressionType.Type2:
                // code block
                break;
            case ProgressionType.None:
                Debug.LogError("setupStartingZones() - No progression type selected");
                break;
        }
    }

    List<GameObject> findShortestPathToBossRoom(ref Zone zone, GameObject startRoom)
    {
        //List<GameObject> roomsInZone = zone.roomsList;
        List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
        List<GameObject> doNotTravelList = new List<GameObject>();
        List<GameObject> endRooms = new List<GameObject>();
        endRooms.Add(zone.bossRoom);

        zone.pathMapper.traverseZoneRoomListStart(zone.getZoneId(), startRoom, startRoom, endRooms, ref listOfPaths, doNotTravelList);

        List<GameObject> minPath = new List<GameObject>();
        int minLength = int.MaxValue;
        // Find the shortest path
        for (int i = 0; i < listOfPaths.Count; i++)
        {
            if (listOfPaths[i].Count < minLength)
            {
                minPath = listOfPaths[i];
                minLength = listOfPaths[i].Count;
            }
        }

        // Print 
        //Debug.Log("======== MIN PATH FOR ZONE" + zone.getZoneId + " ========");
        //for (int i = 0; i < minPath.Count; i++)
        //{
        //    Debug.Log(minPath[i].name);
        //}

        return minPath;
    }

    void lockRoomWithAbility(ref GameObject currentRoom, abilities ability, int doorId)
    {
        currentRoom.GetComponent<roomProperties>().addRoomLock(new RoomLockAbility(ability, doorId));
        // Might need to check if you can fit an ability against the door
        // If the locked door is too close to another door, what do you do?
        //      Find a different room that will serve the same purpose?
    }

    void findEntranceRoomsToEarlyGameZones(GameObject inputRoom, GameObject previousRoom, ref List<GameObject> zoneEntrances)
    {
        roomProperties inputRoomProps = inputRoom.GetComponent<roomProperties>();
        List<GameObject> allZoneEntraces = MapGeneratorScript.zoneManager.getAllZoneEntracesRooms();

        // Basically check all adjacent rooms if its an entrance to the zone
        for (int i = 0; i < inputRoomProps.doorList.Count; i++)
        {
            // If door is used
            if (inputRoomProps.doorList[i].doorUsedBool == true)
            {
                // If door doesn't lead back to the previous room
                if (inputRoomProps.doorList[i].adjacentRoom.name != previousRoom.name)
                {
                    roomProperties adjacentRoomProperties = inputRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>();

                    for (int j = 0; j < allZoneEntraces.Count; j++)
                    {
                        // Check if the adjacent room is a zone entrance and that the zone id is not a vein zone or no intended zone
                        if (adjacentRoomProperties.name == allZoneEntraces[j].name &&
                            adjacentRoomProperties.getZoneTypeId() != GlobalDefines.veinIntendedId && adjacentRoomProperties.getZoneTypeId() != GlobalDefines.startZoneId)
                        {
                            zoneEntrances.Add(allZoneEntraces[j]);
                        }
                    }

                    // If adjacent room is has a a vein zone id or no intended id, then continue looking for entrances
                    if (adjacentRoomProperties.getZoneTypeId() == GlobalDefines.veinIntendedId || adjacentRoomProperties.getZoneTypeId() == GlobalDefines.startZoneId)
                    {
                        findEntranceRoomsToEarlyGameZones(inputRoomProps.doorList[i].adjacentRoom, inputRoom, ref zoneEntrances);
                    }
                }
            }
        }
    }

    int findRoomIndex(string roomName)
    {
        int index = -99;

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].name == roomName)
            {
                index = i;
                break;
            }
        }

        if (index == -99)
            Debug.LogError("findRoomIndex() - Room Name not found: " + roomName);

        return index;
    }

    void organizeRoomsIntoZonesByZoneType()
    {
        int typeId = GlobalDefines.defaultId;
        int xAccess = 0;
        int yAccess = 0;

        //Debug.Log("START");
        //int i = 0; 

        // Puts all of the rooms generated into an orgainzed zone list
        //    AT THIS POINT HYBRID ZONES SHOULD HAVE BEEN ALREADY PUT IN PLACE
        foreach (GameObject room in roomList)
        {
            //Debug.Log("Room: " + room.name);
            //i++;
            //if (i > 2)
            //    break;

            if (room.name == GlobalDefines.sendOffRoomName || room.name == GlobalDefines.startRoomName)
            {
                typeId = GlobalDefines.startZoneId;
                room.GetComponent<roomProperties>().setZoneTypeId(typeId);

                MapGeneratorScript.zoneManager.addRoomViaTypeZoneId(typeId, room);
            }
            else
            {
                Dictionary<int, List<intendedUnitType>> gridZoneDistribution = new Dictionary<int, List<intendedUnitType>>();
                Dictionary<int, List<int>> gridZoneAreaDistribution = new Dictionary<int, List<int>>();
                roomProperties roomProps = room.GetComponent<roomProperties>();

                for (int x = 0; x < roomProps.grids2.Count; x++)
                {
                    for (int y = 0; y < roomProps.grids2[x].Count; y++)
                    {
                        // Go through all grids that the room is in and determine which zone it belongs in
                        if (roomProps.grids2[x][y] == 1)
                        {
                            xAccess = roomProps.gridCoords.x + x;
                            yAccess = roomProps.gridCoords.y + y;

                            gridUnitScript currentGrid = grid.GetComponent<gridManagerScript>().grid[xAccess, yAccess].GetComponent<gridUnitScript>();

                            // If intended type is just a vein
                            if (currentGrid.intendedType == intendedUnitType.Vein)
                            {
                                typeId = GlobalDefines.veinIntendedId;
                            }
                            // Else if it's part of a zone
                            else
                            {
                                typeId = currentGrid.intendedZoneId;
                            }

                            if (gridZoneDistribution.ContainsKey(typeId) == false)
                            {
                                List<intendedUnitType> tempList = new List<intendedUnitType>();
                                tempList.Add(currentGrid.intendedType);
                                gridZoneDistribution.Add(typeId, tempList);
                            }
                            else
                            {
                                gridZoneDistribution[typeId].Add(currentGrid.intendedType);
                            }

                            if (currentGrid.zoneArea.Count != 0)
                            {
                                if (gridZoneAreaDistribution.ContainsKey(typeId) == false)
                                {
                                    List<int> tempList2 = new List<int>();
                                    tempList2.Add(currentGrid.zoneArea[0]);
                                    gridZoneAreaDistribution.Add(typeId, tempList2);
                                }
                                else
                                {
                                    gridZoneAreaDistribution[typeId].Add(currentGrid.zoneArea[0]);
                                }
                            }
                            
                        }
                    }
                }


                // !!!Need to take Zone Area into acount!!!!


                // Once we have accumulated all grids intended type, count which one is the most prevalent and add it to the zone list
                int mostCountId = GlobalDefines.defaultId;
                int mostCount = -99;
                foreach (var key in gridZoneDistribution.Keys)
                {
                    if (gridZoneDistribution[key].Count > mostCount)
                    {
                        mostCountId = key;
                        mostCount = gridZoneDistribution[key].Count;
                    }
                }

                // If it's a inended to be a vein, see if it is also in a zone
                int mostCountAreaId = GlobalDefines.defaultId;
                int mostAreaCount = -99;

                if (mostCountId == GlobalDefines.veinIntendedId && gridZoneAreaDistribution.Count == 0)
                {
                    // Do Nothing
                }
                else if (mostCountId == GlobalDefines.veinIntendedId)
                {
                    foreach (var key in gridZoneAreaDistribution.Keys)
                    {
                        if (gridZoneAreaDistribution[key].Count > mostAreaCount)
                        {
                            mostCountAreaId = key;
                            mostAreaCount = gridZoneAreaDistribution[key].Count;
                        }
                    }
                    mostCountId = gridZoneAreaDistribution[mostCountAreaId][0];
                }

                room.GetComponent<roomProperties>().setZoneTypeId(mostCountId);
                MapGeneratorScript.zoneManager.addRoomViaTypeZoneId(mostCountId, room);
            }
        }

        
    }

    void assignUniqueZoneIds()
    {
        ZoneManager newZoneList = new ZoneManager();
        int uniqueZoneId = 0; 

        for (int index = 0; index < MapGeneratorScript.zoneManager.zoneList.Count; index++)
        {
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[index];
            List<GameObject> roomList = currentZone.getRoomList();

            // If the zone is not of the vein type, then add all rooms to the new zone list
            if (currentZone.getZoneTypeId() != GlobalDefines.veinIntendedId)
            {
                for (int i = 0; i < roomList.Count; i++)
                {
                    GameObject currentRoom = roomList[i];
                    roomProperties currentRoomProps = currentRoom.GetComponent<roomProperties>();

                    currentRoomProps.setZoneId(uniqueZoneId);
                    newZoneList.addRoomViaUniqueZoneId(uniqueZoneId, currentZone.getZoneTypeId(), currentZone.getZoneAbility(), currentZone.getZoneTheme(), currentRoom);
                }
                uniqueZoneId++;
            }
            else
            {
                // Else we need to check and figure out which rooms are actually touching and can create a zone
                List<GameObject> alreadyCheckedRooms = new List<GameObject>();

                for (int i = 0; i < roomList.Count; i++)
                {
                    List<GameObject> touchingVeinRooms = new List<GameObject>();
                    GameObject currentRoom = roomList[i];

                    if (alreadyCheckedRooms.Contains(currentRoom) == false)
                    {
                        touchingVeinRooms = currentZone.getPathingMapper().getAllTouchingRoomsWithTheSameZoneTypeIdStart(currentRoom, ref currentZone);
                        alreadyCheckedRooms.AddRange(touchingVeinRooms);

                        // Add all rooms to the new zone list
                        for (int j = 0; j < touchingVeinRooms.Count; j++)
                        {
                            GameObject touchingRoom = touchingVeinRooms[j];
                            roomProperties touchingRoomProps = touchingRoom.GetComponent<roomProperties>();

                            touchingRoomProps.setZoneId(uniqueZoneId);
                            newZoneList.addRoomViaUniqueZoneId(uniqueZoneId, currentZone.getZoneTypeId(), currentZone.getZoneAbility(), currentZone.getZoneTheme(), touchingRoom);
                        }
                        uniqueZoneId++;
                    }
                }
            }
        }

        MapGeneratorScript.zoneManager = newZoneList;
    }

    void findZoneEntrances()
    {
        for (int index = 0; index < MapGeneratorScript.zoneManager.zoneList.Count; index++)
        {
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[index];
            int currentIdType = currentZone.getZoneTypeId();

            foreach (GameObject room in currentZone.roomsList)
            {
                checkAdjacentRoomsForDifferentZoneViaZoneTypeId(room, currentIdType, ref currentZone);
            }
        }
    }

    // Will connect adjacent zones
    void linkZonesStart()
    {
        Zone startingZone = MapGeneratorScript.zoneManager.getStartZone();
        List<int> alreadyTraveledToZoneIds = new List<int>();

        linkZones(startingZone, ref alreadyTraveledToZoneIds);
    }

    void linkZones(Zone currentZone, ref List<int> alreadyTraveledToZoneIds)
    {
        alreadyTraveledToZoneIds.Add(currentZone.getZoneId());
        List<GameObject> entranceRooms = currentZone.getEntranceRooms();

        // Go through the current zones entrances and see if we have travled to the connecting zones
        for (int i = 0; i < entranceRooms.Count; i++)
        {
            List<int> doorIdsToDifferentZones = MapGeneratorScript.zoneManager.getDoorIdsThatLeadToAnyDifferentZone(entranceRooms[i]);

            foreach (int currentDoorId in doorIdsToDifferentZones)
            {
                roomProperties currentEntranceRoomProps = entranceRooms[i].GetComponent<roomProperties>();
                roomProperties adjacentRoomRoomProps = currentEntranceRoomProps.doorList[currentDoorId].adjacentRoom.GetComponent<roomProperties>();
                int adjacentZoneId = adjacentRoomRoomProps.getZoneId();

                if (alreadyTraveledToZoneIds.Contains(adjacentZoneId) == false)
                {
                    // Get the zone id of the other zone
                    bool adjacentZoneAdded = MapGeneratorScript.zoneManager.linkAdjacentZones(currentZone.getZoneId(), adjacentZoneId);

                    // If we succesfully added it, travel to the next zone
                    if (adjacentZoneAdded)
                        linkZones(MapGeneratorScript.zoneManager.getZone(adjacentZoneId), ref alreadyTraveledToZoneIds);
                }
            }
        }
    }
    
    bool checkAdjacentRoomForDifferentNonVeinZone(GameObject room, ref int doorId)
    {
        bool foundAdjacentRoomInAnotherZone = false;
        roomProperties roomProps = room.GetComponent<roomProperties>();

        for (int i = 0; i < roomProps.doorList.Count; i++)
        {
            if (roomProps.doorList[i].doorUsedBool == true)
            {
                GameObject adjacentRoom = roomProps.doorList[i].adjacentRoom;
                roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

                if (adjacentRoomProps.getZoneId() != roomProps.getZoneId() &&
                    MapGeneratorScript.zoneManager.roomIsInANonVeinZone(adjacentRoom) == true)
                {
                    doorId = i;
                    foundAdjacentRoomInAnotherZone = true;
                    break;
                }
            }
        }

        return foundAdjacentRoomInAnotherZone;
    }

    void checkAdjacentRoomsForDifferentZoneViaZoneTypeId(GameObject room, int currentIdType, ref Zone zone)
    {
        roomProperties roomProps = room.GetComponent<roomProperties>();

        for (int i = 0; i < roomProps.doorList.Count; i++)
        {

            if (roomProps.doorList[i].doorUsedBool == true)

            {
                roomProperties adjacentRoomProps = roomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>();

                if (adjacentRoomProps.getZoneTypeId() != currentIdType)
                {
                    roomProps.isZoneEntrance = true;
                    MapGeneratorScript.zoneManager.addRoomEntrance(zone.getZoneId(), room);

                    break;
                }
            }
        }
    }

    void getAdjacentRoom(GameObject room, int doorNum, GameObject adjacentRoom)
    {
        adjacentRoom = room.GetComponent<roomProperties>().doorList[doorNum].adjacentRoom;
    }

    void identifyBossRoomsInEachZone()
    {
        for (int i = 0; i < MapGeneratorScript.zoneManager.zoneList.Count; i++)
        {
            int currentZoneId = MapGeneratorScript.zoneManager.zoneList[i].getZoneId();
            Zone currentZone = MapGeneratorScript.zoneManager.zoneList[i];

            for (int j = 0; j < currentZone.roomsList.Count; j++)
            {
                if (currentZone.roomsList[j].name.Contains(GlobalDefines.bossRoomNamePrefix))
                {
                    MapGeneratorScript.zoneManager.addBossRoomToZone(currentZoneId, currentZone.roomsList[j]);
                    //Debug.Log("BOSS ROOM: " + currentZone.roomsList[j].name);
                }
            }
        }
    }

    bool zoneIsVein(int zoneIndex)
    {
        if (MapGeneratorScript.zoneManager.zoneIdList[zoneIndex] == GlobalDefines.veinIntendedId)
            return true;

        return false;
    }

    bool zoneIsAThemedZone(int zoneIndex)
    {
        if (MapGeneratorScript.zoneManager.zoneIdList[zoneIndex] == GlobalDefines.veinIntendedId || 
            MapGeneratorScript.zoneManager.zoneIdList[zoneIndex] == GlobalDefines.startZoneId ||
            MapGeneratorScript.zoneManager.zoneIdList[zoneIndex] == GlobalDefines.defaultId)
            return false;

        return true;
    }

    void filterSubAreaFromList(ref List<SubArea> listOfSubAreas, SubArea subAreaToFilter)
    {
        bool foundFilteredSubArea = false;
        List<SubArea> newFilteredList = new List<SubArea>();

        for (int i = 0; i < listOfSubAreas.Count; i++)
        {
            if (listOfSubAreas[i].Equals(subAreaToFilter))
                foundFilteredSubArea = true;
            else
                newFilteredList.Add(listOfSubAreas[i]);
        }

        if (foundFilteredSubArea == false)
            Debug.LogError("Progression Generator Script - filterSubAreaFromList(): Did not find a sub area in this list");

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
}
