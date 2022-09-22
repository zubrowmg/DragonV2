using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using LockingClasses;
using System;

using Classes;

namespace PathingClasses
{
    public class Zone
    {
        public int typeId;
        public int id;
        public List<GameObject> roomsList;

        public List<GameObject> zoneEntranceRooms;
        public List<GameObject> primaryZoneEntranceRooms;  // Meant to be used as the players first encounter with a zone, should be 1-2 per zone
        public List<GameObject> secondaryZoneEntranceRooms;

        bool primaryZoneEntrancesAreCloseToEachOther;
        int shortRoomDistance = 6;

        public GameObject bossRoom;

        public List<Zone> adjacentZones;
        public bool isEarlyGameZone;

        // Progression Paths
        public PathMapper pathMapper;
        public PathOrderManager pathOrderManager;

        // Abilities and Themes
        public abilities ability = abilities.None;
        public zoneThemes theme = zoneThemes.None;

        // Zone Ordering
        int zoneOrder; // Only meant to be used for non vein zones

        public Zone()
        {
            this.id = GlobalDefines.defaultId;
            this.typeId = GlobalDefines.defaultId;
            this.roomsList = new List<GameObject>();

            this.zoneEntranceRooms = new List<GameObject>();
            this.primaryZoneEntranceRooms = new List<GameObject>();
            this.secondaryZoneEntranceRooms = new List<GameObject>();

            this.primaryZoneEntrancesAreCloseToEachOther = false;

            this.bossRoom = null;

            this.adjacentZones = new List<Zone>();
            this.isEarlyGameZone = false;

            this.pathMapper = new PathMapper();
            this.pathOrderManager = new PathOrderManager();

            this.ability = abilities.None;
            this.theme = zoneThemes.None;

            this.zoneOrder = GlobalDefines.defaultOrder;
        }

        // Meant for first pass zone creation, when zone type id is all we have
        public Zone(int typeId, GameObject room)
        {
            this.id = GlobalDefines.defaultId;
            this.typeId = typeId;
            this.roomsList = new List<GameObject>();

            this.zoneEntranceRooms = new List<GameObject>();
            this.primaryZoneEntranceRooms = new List<GameObject>();
            this.secondaryZoneEntranceRooms = new List<GameObject>();

            this.primaryZoneEntrancesAreCloseToEachOther = false;

            this.bossRoom = null;

            this.adjacentZones = new List<Zone>();
            this.isEarlyGameZone = false;

            this.pathMapper = new PathMapper();
            this.pathOrderManager = new PathOrderManager();

            this.roomsList.Add(room);
            this.theme = GlobalDefines.themeAndAbilityManager.getZoneTheme(typeId);
            this.ability = GlobalDefines.themeAndAbilityManager.getZoneAbility(typeId);

            this.zoneOrder = GlobalDefines.defaultOrder;

        }

        // Meant for second pass zone creation, when we are trying to assign a unique zone id
        public Zone(int uniqueId, int typeId, GameObject room, abilities ability, zoneThemes theme)
        {
            this.id = uniqueId;
            this.typeId = typeId;
            this.roomsList = new List<GameObject>();

            this.zoneEntranceRooms = new List<GameObject>();
            this.primaryZoneEntranceRooms = new List<GameObject>();
            this.secondaryZoneEntranceRooms = new List<GameObject>();

            this.primaryZoneEntrancesAreCloseToEachOther = false;

            this.bossRoom = null;

            this.adjacentZones = new List<Zone>();
            this.isEarlyGameZone = false;

            this.pathMapper = new PathMapper();
            this.pathOrderManager = new PathOrderManager();

            this.roomsList.Add(room);
            this.theme = theme;
            this.ability = ability;

            this.zoneOrder = GlobalDefines.defaultOrder;
        }

        public void setBossRoom(GameObject room)
        {
            bossRoom = room;
        }

        public void addRoom(GameObject room)
        {
            roomsList.Add(room);
        }

        public void addRoomEntrance(GameObject room)
        {
            zoneEntranceRooms.Add(room);
        }

        public void addPrimaryZoneEntranceRoom(GameObject room)
        {
            if (primaryZoneEntranceRooms.Count == 0)
                primaryZoneEntranceRooms.Add(room);
            else
            {
                primaryZoneEntranceRooms.Add(room);

                // Need to calculate if the zone entrances are relatively close to one another
                List<List<GameObject>> listOfPaths = new List<List<GameObject>>();
                List<GameObject> doNotTravelList = new List<GameObject>();

                pathMapper.traverseZoneRoomListStart(getZoneId(), primaryZoneEntranceRooms[0], primaryZoneEntranceRooms[0],
                    new List<GameObject> { primaryZoneEntranceRooms[1] }, ref listOfPaths, doNotTravelList);

                int roomDistance = pathMapper.getSmallestCountInList(listOfPaths).Count;

                if (roomDistance < shortRoomDistance)
                    primaryZoneEntrancesAreCloseToEachOther = true;
            }
        }

        public void movePrimaryEntranceToSecondary(GameObject entrance)
        {
            if (primaryZoneEntranceRooms.Contains(entrance) == true)
            {
                secondaryZoneEntranceRooms.Add(entrance);
                primaryZoneEntranceRooms.Remove(entrance);

                primaryZoneEntrancesAreCloseToEachOther = false;
            }
            else
            {
                Debug.LogError("movePrimaryEntranceToSecondary() in Zone. Room is not a primary entrance: " + entrance.name);
            }
        }

        public void printAllLockedEntranceRooms()
        {
            foreach (GameObject room in zoneEntranceRooms)
            {
                if (room.GetComponent<roomProperties>().roomIsLocked() == true)
                {
                    Debug.Log("Zone: " + id + "  Ability: " + ability + " Room: " + room.name);
                }
            }
        }

        public void addAdjacentZone(Zone adjacentZone)
        {
            if (adjacentZones.Contains(adjacentZone) == false)
                adjacentZones.Add(adjacentZone);
        }

        public List<Zone> getAdjacentZones()
        {
            return this.adjacentZones;
        }

        public List<int> getAdjacentZoneIds()
        {
            List<int> adjacentZoneIds = new List<int>();
            for (int i = 0; i < adjacentZones.Count; i++)
            {
                adjacentZoneIds.Add(adjacentZones[i].getZoneId());
            }
            return adjacentZoneIds;
        }

        public void setZoneTypeId(int zoneTypeId)
        {
            this.typeId = zoneTypeId;
        }

        public List<GameObject> getRoomList()
        {
            return this.roomsList;
        }

        public int getZoneTypeId()
        {
            return typeId;
        }

        public int getZoneId()
        {
            return id;
        }

        public abilities getZoneAbility()
        {
            return ability;
        }

        public zoneThemes getZoneTheme()
        {
            return theme;
        }

        public PathMapper getPathingMapper()
        {
            return pathMapper;
        }

        public bool getIsEarlyGameZone()
        {
            return this.isEarlyGameZone;
        }

        public List<GameObject> getEntranceRooms()
        {
            return zoneEntranceRooms;
        }

        public void setZoneOrder(int zoneOrder)
        {
            this.zoneOrder = zoneOrder;
        }

        public int getZoneOrder()
        {
            return this.zoneOrder;
        }

        public List<GameObject> getZoneEntrances()
        {
            return zoneEntranceRooms;
        }

        public List<GameObject> getPrimaryZoneEntrances()
        {
            return primaryZoneEntranceRooms;
        }

        public void addPrimaryZoneEntrances(List<GameObject> primaryEntrances)
        {
            this.primaryZoneEntranceRooms.AddRange(primaryEntrances);
        }

        public List<GameObject> getSecondaryZoneEntrances()
        {
            return secondaryZoneEntranceRooms;
        }

        public void addSecondaryZoneEntrances(List<GameObject> primaryEntrances)
        {
            this.secondaryZoneEntranceRooms.AddRange(primaryEntrances);
        }

        public bool arePrimaryZoneEntrancesAreCloseToEachOther()
        {
            return primaryZoneEntrancesAreCloseToEachOther;
        }

        public PathOrderManager getZonePathOrderManager()
        {
            return pathOrderManager;
        }

        public List<GameObject> getCommonPath()
        {
            return pathMapper.getCommonPath();
        }
    }
    
    public partial class ZoneManager
    {
        public List<Zone> zoneList;
        public List<int> zoneIdList; // Get rid of this

        Dictionary<int, Zone> zoneDictionary; // Eventually replace zonelist with this, should be way faster

        public ZoneManager()
        {
            this.zoneList = new List<Zone>();
            this.zoneIdList = new List<int>();
            this.zoneDictionary = new Dictionary<int, Zone>(); // Only for unique ids

            // Zone Ordering 
            this.nextOrder = 0;
            this.lockManager = new ZoneLockManager();
        }

        public Zone getZone(int id)
        {
            Zone zone = null;
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == id)
                {
                    zone = zoneList[i];
                    break;
                }
            }
            if (zone == null)
                Debug.LogError("ZoneManager getZone() - No Zone Found For Id=" + id);
            return zone;
        }

        public void addBossRoomToZone(int id, GameObject room)
        {
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == id)
                {
                    zoneList[i].setBossRoom(room);

                    break;
                }
            }
        }

        // Only used for the first pass at creating zones
        public void addRoomViaTypeZoneId(int typeId, GameObject room)
        {
            bool foundZone = false;

            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneTypeId() == typeId)
                {
                    zoneList[i].addRoom(room);

                    foundZone = true;
                    break;
                }
            }



            if (foundZone == false)
            {
                zoneList.Add(new Zone(typeId, room));
                zoneIdList.Add(typeId);
            }
        }

        // Only used for the first pass at creating zones
        public void addRoomViaUniqueZoneId(int uniqueId, int typeId, abilities ability, zoneThemes theme, GameObject room)
        {
            bool foundZone = false;

            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == uniqueId)
                {
                    zoneList[i].addRoom(room);

                    foundZone = true;
                    break;
                }
            }

            if (foundZone == false)
            {
                zoneList.Add(new Zone(uniqueId, typeId, room, ability, theme));
                zoneIdList.Add(uniqueId);
            }
        }

        public void addRoomEntrance(int id, GameObject room)
        {
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == id)
                {
                    zoneList[i].addRoomEntrance(room);

                    break;
                }
            }
        }

        public void addPrimaryRoomEntrance(int id, GameObject room)
        {
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == id)
                {
                    zoneList[i].addPrimaryZoneEntranceRoom(room);

                    break;
                }
            }
        }

        public abilities getAbilityFromZoneTypeId(int typeId)
        {
            abilities zoneAbility = abilities.None;

            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneTypeId() == typeId)
                {
                    zoneAbility = zoneList[i].getZoneAbility();
                    break;
                }
            }

            if (zoneAbility == abilities.None)
                Debug.LogError("ZoneManager getAbilityFromZoneTypeId() - No Ability Found From Type Id=" + typeId);

            return zoneAbility;
        }

        public List<GameObject> getAllZoneEntracesRooms()
        {
            List<GameObject> entraceRooms = new List<GameObject>();
            for (int i = 0; i < zoneList.Count; i++)
            {
                for (int j = 0; j < zoneList[i].zoneEntranceRooms.Count; j++)
                {
                    entraceRooms.Add(zoneList[i].zoneEntranceRooms[j]);
                }
            }

            return entraceRooms;
        }

        public bool linkAdjacentZones(int zoneIdOne, int zoneIdTwo)
        {
            bool adjacentZoneAdded = false;
            Zone zoneOne = getZone(zoneIdOne);
            Zone zoneTwo = getZone(zoneIdTwo);

            if (zoneOne.getAdjacentZones().Contains(zoneTwo) == false)
            {
                adjacentZoneAdded = true;
                zoneOne.addAdjacentZone(zoneTwo);
                zoneTwo.addAdjacentZone(zoneOne);
            }
            return adjacentZoneAdded;
        }

        public List<Zone> getAllEarlyGameZones()
        {
            List<Zone> earlyGameZones = new List<Zone>();
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getIsEarlyGameZone())
                {
                    earlyGameZones.Add(zoneList[i]);
                }
            }
            return earlyGameZones;
        }

        public void getAllEarlyGameZonesByReference(ref List<Zone> earlyGameZones)
        {
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getIsEarlyGameZone())
                {
                    earlyGameZones.Add(zoneList[i]);
                }
            }
        }

        public Zone getStartZone()
        {
            Zone startingZone = null;
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneTypeId() == GlobalDefines.startZoneId)
                {
                    startingZone = zoneList[i];
                    break;
                }
            }
            return startingZone;
        }

        public void printAllRooms()
        {
            foreach (Zone zone in zoneList)
            {
                if (zone.getZoneId() == -2)
                {
                    Debug.Log("=== Zone ID: " + zone.getZoneId() + " ===");
                    Debug.Log("=== Zone Type ID: " + zone.getZoneTypeId() + " ===");
                    if (zone.roomsList.Count != 0)
                    {
                        foreach (GameObject room in zone.roomsList)
                        {
                            Debug.Log(room.name);
                        }
                    }
                    else
                    {
                        Debug.Log("NO ROOMS IN ZONE");
                    }
                }
            }
        }

        public void printAllLockedAbilityRooms()
        {
            foreach (Zone zone in zoneList)
            {
                zone.printAllLockedEntranceRooms();
            }
        }

        public void printAllThemesAndAbilities()
        {
            foreach (Zone zone in zoneList)
            {

                Debug.Log("================================================ \n" +
                    "\t\t ====== Zone ID: " + zone.getZoneId() + " ======\n" +
                    "\t\t    Zone Type ID: " + zone.getZoneTypeId());

                Debug.Log("\t\t    Zone Order: " + zone.getZoneOrder() + "\n" +
                    "\t\t    Theme: " + zone.theme + "   Ability: " + zone.ability + "\n" +
                    "================================================");
            }
        }

        public void setEarlyGameZoneBool(int id)
        {
            for (int i = 0; i < zoneList.Count; i++)
            {
                if (zoneList[i].getZoneId() == id)
                {
                    zoneList[i].isEarlyGameZone = true;

                    break;
                }
            }
        }
        
    }

    // -----------------------------------------------------------------------------------------------------------
    // Zone Order Manager
    //      Will create an order that the zones will have to loosely follow
    //      And will set any locks that are needed
    public partial class ZoneManager
    {
        // Zone Order Rules:
        //      1. If you are in a zone with order 5, you should be able to access zone 1-4 wihout those abilities
        //          - This is just a default rule, we can change the flow order to restrict this
        //      2. Must make sure that any zone does not connect to 3 or more lower ordered zones
        //          - If it is try to re-order
        //          - (Not implemented) You can also artificially restrict access to one of the lower zones, but it might complicate rule 1
        //          - If a zone has 2 entrances to another zone, only use 1 of them. You want 2 primary zone entrances from different zones
        //      3.
        //      4.

        // Future enhancement - Travel Lock - details logged in binder


        int nextOrder;
        public ZoneLockManager lockManager; 

       

        public void setUpZoneOrder()
        {
            setUpEarlyGameZoneOrdering();
            setUpEarlyGameZoneLocks();
            // setUpMidAndLateZoneOrdering();

            // createZoneFlow();
            //      Not needed for setting up early game zones
            //      But the idea is that zone order is a psuedo order that the player should play in
            //      Can't allow the player to just blindly go from zone to zone with ease
            //      There should be moments in the game where the player needs to go back to an oder area to progress, or else it's not a metriodvania
            //      Let the player blindly traverse 1-2 zones before throwing a curveball (aka a lock) that needs to be completed by a different zone
            //      This will create a more natural metriodvania "flow"
        }

        // Set up early zone locks
        void setUpEarlyGameZoneLocks()
        {
            // Basically just follow rule 2 for zone order
            //      Lock all secondary entrances with Ability Locks (Zone to Zone links)
            //      Rule 1 will apply ONLY to early game zones, below comment is for mid game zones
            //      !!! For now the early game zones will lock the entrances to the mid game zones with the early zone ability
            //          In the future, you should zone off 2/3 early zones with a mid zone ability
            //          The 1/3 zone should not be able to proceed to the mid zone until it has 1 (or maybe 2) of the 2/3 early zone abilities
            Debug.Log("ZoneManager - setUpEarlyGameZoneLocks(): When implementing mid game zones, refer to the comment above this log message");
            
            List<Zone> earlyGameZones = this.getAllEarlyGameZones();
            bool lockForEarlyZones = true;

            for (int i = 0; i < earlyGameZones.Count; i++)
            {
                Zone currentZone = earlyGameZones[i];

                // Set up the primary and secondary zone entrances
                setPrimaryEntrances(ref currentZone);
                setSecondaryEntrances(ref currentZone);
                lockAllEntrancesWithAbilityLock(ref currentZone, lockForEarlyZones);
            }
        }

        // Will lock the smallest primary entrance and make it a secondary one
        public void lockTheSmallestPrimaryEntranceRoom(ref Zone currentZone)
        {
            List<GameObject> primaryZoneEntrances = currentZone.getPrimaryZoneEntrances();
            MaxValue<GameObject> primaryEntranceWithLargestAdjacentZoneOrder = new MaxValue<GameObject>(1);

            // Go through each primary zone entrance, lock the doors that lead to lower zone orders
            for (int i = 0; i < primaryZoneEntrances.Count; i++)
            {
                GameObject currentZoneEntrance = primaryZoneEntrances[i];
                roomProperties currentZoneEntranceProps = currentZoneEntrance.GetComponent<roomProperties>();
                List<int> doorsToDifferentZones = getDoorIdsThatLeadToAnyDifferentZone(currentZoneEntrance);
                MinValue<GameObject> smallestAdjacentZoneOrder = new MinValue<GameObject>(1);

                // Go through each adjacent room in a different zone
                for (int j = 0; j < doorsToDifferentZones.Count; j++)
                {
                    int currentDoor = doorsToDifferentZones[j];
                    roomProperties adjacentZoneRoomProps = currentZoneEntranceProps.doorList[currentDoor].adjacentRoom.GetComponent<roomProperties>();
                    Zone adjacentZone = getZone(adjacentZoneRoomProps.getZoneId());

                    // If the adjacent zone is a vein zone, then try to find the order of an adjacent non vein zone
                    if (adjacentZone.getZoneTypeId() == GlobalDefines.veinIntendedId)
                        adjacentZone = getSmallestOrderAdjacentZoneWithNonDefualtOrdering(currentZone, adjacentZone);

                    smallestAdjacentZoneOrder.addValue(adjacentZone.getZoneOrder(), currentZoneEntrance);
                }


                primaryEntranceWithLargestAdjacentZoneOrder.addValue(smallestAdjacentZoneOrder.randomlyChooseKeyValuePair().Key, currentZoneEntrance);
            }

            GameObject choosenPrimaryEntranceToLock = primaryEntranceWithLargestAdjacentZoneOrder.randomlyChooseValue();

            bool roomIsGoodToLock = lockManager.findIdealRoomToLock(); // Not currently implemented

            if (roomIsGoodToLock)
            {
                // Create a new key and add it to the lock manager
                abilities currentZoneAbility = currentZone.getZoneAbility();
                KeyAbility newAbilityKey = new KeyAbility(currentZoneAbility);

                currentZone.movePrimaryEntranceToSecondary(choosenPrimaryEntranceToLock);

                List<int> doorsToDifferentZones = getDoorIdsThatLeadToAnyDifferentZone(choosenPrimaryEntranceToLock);
                for (int i = 0; i < doorsToDifferentZones.Count; i++)
                {
                    bool lockSucceded = lockManager.addLockToZone(newAbilityKey, ref choosenPrimaryEntranceToLock, doorsToDifferentZones[i]);
                    if (lockSucceded == false)
                        Debug.LogError("Zone Manager - lockTheSmallestPrimaryEntranceRoom(): Second addLockToZone() function failed");
                }
            }
        }

        // Will Lock all entrances if they follow rule 1
        void lockAllEntrancesWithAbilityLock(ref Zone currentZone, bool lockForEarlyZones)
        {
            Debug.Log("Zone Manager - lockAllEntrancesWithAbilityLock(): findIdealRoomToLock() is not implemented in this function");

            List<GameObject> secondaryZoneEntrances = currentZone.getSecondaryZoneEntrances();

            // Always lock each secondary zone entrance, at this point they have already been choosen to be locked
            for (int i = 0; i < secondaryZoneEntrances.Count; i++)
            {
                GameObject currentZoneEntrance = secondaryZoneEntrances[i];
                roomProperties currentZoneEntranceProps = currentZoneEntrance.GetComponent<roomProperties>();
                List<int> doorsToDifferentZones = getDoorIdsThatLeadToAnyDifferentZone(currentZoneEntrance);

                // Go through each adjacent room in a different zone
                for (int j = 0; j < doorsToDifferentZones.Count; j++)
                {
                    int currentDoor = doorsToDifferentZones[j];
                    roomProperties adjacentZoneRoomProps = currentZoneEntranceProps.doorList[currentDoor].adjacentRoom.GetComponent<roomProperties>();
                    Zone adjacentZone = getZone(adjacentZoneRoomProps.getZoneId());

                    bool roomIsGoodToLock = lockManager.findIdealRoomToLock(); // Not currently implemented

                    if (roomIsGoodToLock)
                    {
                        // Create a new key and add it to the lock manager
                        abilities currentZoneAbility = currentZone.getZoneAbility();
                        KeyAbility newAbilityKey = new KeyAbility(currentZoneAbility);

                        bool lockSucceded = lockManager.addLockToZone(newAbilityKey, ref currentZoneEntrance, currentDoor);
                        if (lockSucceded == false)
                            Debug.LogError("Zone Manager - lockAllEntrancesWithAbilityLock(): First addLockToZone() function failed");
                    }
                }
            }

            List<GameObject> primaryZoneEntrances = currentZone.getPrimaryZoneEntrances();

            // Go through each primary zone entrance, lock the doors that lead to lower zone orders
            for (int i = 0; i < primaryZoneEntrances.Count; i++)
            {
                GameObject currentZoneEntrance = primaryZoneEntrances[i];
                roomProperties currentZoneEntranceProps = currentZoneEntrance.GetComponent<roomProperties>();
                List<int> doorsToDifferentZones = getDoorIdsThatLeadToAnyDifferentZone(currentZoneEntrance);

                // Go through each adjacent room in a different zone
                for (int j = 0; j < doorsToDifferentZones.Count; j++)
                {
                    int currentDoor = doorsToDifferentZones[j];
                    roomProperties adjacentZoneRoomProps = currentZoneEntranceProps.doorList[currentDoor].adjacentRoom.GetComponent<roomProperties>();
                    Zone adjacentZone = getZone(adjacentZoneRoomProps.getZoneId());

                    // If the adjacent zone is a vein zone, then try to find the order of an adjacent non vein zone
                    if (adjacentZone.getZoneTypeId() == GlobalDefines.veinIntendedId)
                        adjacentZone = getSmallestOrderAdjacentZoneWithNonDefualtOrdering(currentZone, adjacentZone);


                    // If we still have a vein zone, then that means that we hit a dead end. No need to lock
                    if (adjacentZone.getZoneTypeId() == GlobalDefines.veinIntendedId)
                        continue;
                    // Check to see the current zone ordering is smaller than the adjacent zone ordering
                    else if (currentZone.getZoneOrder() < adjacentZone.getZoneOrder())
                    {
                        bool roomIsGoodToLock = lockManager.findIdealRoomToLock(); // Not currently implemented

                        if (roomIsGoodToLock)
                        {
                            // Create a new key and add it to the lock manager
                            abilities currentZoneAbility = currentZone.getZoneAbility();
                            KeyAbility newAbilityKey = new KeyAbility(currentZoneAbility);

                            bool lockSucceded = lockManager.addLockToZone(newAbilityKey, ref currentZoneEntrance, currentDoor);
                            if (lockSucceded == false)
                                Debug.LogError("Zone Manager - lockAllEntrancesWithAbilityLock(): Second addLockToZone() function failed");
                        }
                    }
                }
            }
        }

        // Based on the main zone, this function will return the smallest order that adjacentVeinZone is connected to
        //      Excluding the main zone, if none is found it will return the adjacentVeinZone. Meaning that the path is a dead end
        Zone getSmallestOrderAdjacentZoneWithNonDefualtOrdering(Zone mainZone, Zone adjacentVeinZone)
        {
            Zone minOrderAdjacentZone = adjacentVeinZone;
            List<Zone> adjacentNonVeinZones = getAdjacentZonesWithNonDefualtOrdering(adjacentVeinZone);

            for (int i = 0; i < adjacentNonVeinZones.Count; i++)
            {
                if (adjacentNonVeinZones[i].Equals(mainZone))
                    continue;
                else if (adjacentNonVeinZones[i].getZoneOrder() < minOrderAdjacentZone.getZoneOrder())
                    minOrderAdjacentZone = adjacentNonVeinZones[i];
            }

            return minOrderAdjacentZone;
        }

        // Starts the pseudo order for the early game
        void setUpEarlyGameZoneOrdering()
        {
            // Set starting zone to order 0
            Zone startingZone = this.getStartZone();
            setZoneOrder(ref startingZone, true, nextOrder);

            // Set early game zone orders, shuffle the order
            List<Zone> earlyGameZones = this.getAllEarlyGameZones();
            earlyGameZones = GlobalDefines.globalRandomClass.Shuffle<Zone>(earlyGameZones);

            for (int i = 0; i < earlyGameZones.Count; i++)
            {
                Zone currentEarlyGameZone = earlyGameZones[i];
                bool assignedOrderIsValid = setZoneOrder(ref currentEarlyGameZone, true, nextOrder);

                // If rule 2 is broken, then attempt an order swap between an already ordered zone
                if (assignedOrderIsValid == false)
                {
                    swapZoneOrderWithAdjacentZone(ref currentEarlyGameZone, nextOrder);
                }
            }
        }

        // This function will:
        //      1. Check all adjacent zone orderings and get the highest adjacent order
        //      2. Swap those and check rule 2 again
        bool swapZoneOrderWithAdjacentZone(ref Zone zone, int attemptedNewOrder)
        {
            bool swapWasSuccessful = true;
            List<Zone> adjacentZonesWithNonDefualtOrder = getAdjacentZonesWithNonDefualtOrdering(zone);

            // Get the closest order to the attemped new order, idealy you will get the max order possible
            Zone highestZoneOrder = adjacentZonesWithNonDefualtOrder[0];
            for (int i = 1; i < adjacentZonesWithNonDefualtOrder.Count; i++)
            {
                if (adjacentZonesWithNonDefualtOrder[i].getZoneOrder() > highestZoneOrder.getZoneOrder())
                    highestZoneOrder = adjacentZonesWithNonDefualtOrder[i];
            }

            // Assign the attempted order to the highest adjacent zone order
            int stolenZoneOrder = highestZoneOrder.getZoneOrder();
            bool assignedOrderIsValid = setZoneOrder(ref highestZoneOrder, false, attemptedNewOrder);

            // Theoretically this error should never happen
            if (assignedOrderIsValid == false)
            {
                Debug.LogError("Zone Manager - swapZoneOrderWithAdjacentZone(): First attempted swap failed");
                swapWasSuccessful = false;
            }

            // Assign the stolen highest adjacent order to the passed in zone
            assignedOrderIsValid = setZoneOrder(ref zone, false, stolenZoneOrder);

            // Theoretically this error should never happen
            if (assignedOrderIsValid == false)
            {
                Debug.LogError("Zone Manager - swapZoneOrderWithAdjacentZone(): Second attempted swap failed");
                swapWasSuccessful = false;
            }

            if (attemptedNewOrder == nextOrder)
                nextOrder++;

            return swapWasSuccessful;
        }

        // Will set the primary entrances, at this point all zone entrances should have been identified
        void setPrimaryEntrances(ref Zone currentZone)
        {
            List<GameObject> zoneEntrances = currentZone.getZoneEntrances();

            // If there are just two then you can add them with out checking
            if (zoneEntrances.Count <= 2)
                currentZone.addPrimaryZoneEntrances(zoneEntrances);
            // Else you must check their pseudo zone order, get the lowest 2 numbers
            else
            {
                int minZoneOrderOne = int.MaxValue;
                int minZoneOrderTwo = int.MaxValue;
                GameObject minZoneRoomOne = null;
                GameObject minZoneRoomTwo = null;

                for (int i = 0; i < zoneEntrances.Count; i++)
                {
                    int minZoneOrder = getAdjacentZoneMinOrderFromAnEntrance(currentZone, zoneEntrances[i]);

                    // Do not want a zones primary entrances to connect to the same zone
                    if (minZoneOrder == minZoneOrderOne || minZoneOrder == minZoneOrderTwo)
                        continue;
                    else if (minZoneOrder < minZoneOrderOne || minZoneOrder < minZoneOrderTwo)
                    {
                        // If it's only smaller than order one
                        if (minZoneOrder < minZoneOrderOne && minZoneOrder > minZoneOrderTwo)
                        {
                            minZoneOrderOne = minZoneOrder;
                            minZoneRoomOne = zoneEntrances[i];
                        }
                        // If it's only smaller than order two
                        else if (minZoneOrder > minZoneOrderOne && minZoneOrder < minZoneOrderTwo)
                        {
                            minZoneOrderTwo = minZoneOrder;
                            minZoneRoomTwo = zoneEntrances[i];
                        }
                        // Need to replace the largest order from order one and order two
                        else
                        {
                            if (minZoneOrderOne > minZoneOrderTwo)
                            {
                                minZoneOrderOne = minZoneOrder;
                                minZoneRoomOne = zoneEntrances[i];
                            }
                            else
                            {
                                minZoneOrderTwo = minZoneOrder;
                                minZoneRoomTwo = zoneEntrances[i];
                            }
                        }

                    }
                }

                // Lastly add to zone entrances as long as they aren't null
                List<GameObject> newPrimaryEntrances = new List<GameObject>();
                if (minZoneRoomOne != null)
                    newPrimaryEntrances.Add(minZoneRoomOne);
                if (minZoneRoomTwo != null)
                    newPrimaryEntrances.Add(minZoneRoomTwo);

                currentZone.addPrimaryZoneEntrances(newPrimaryEntrances);

            }
        }

        // Will return the min zone order from the supplied entrance, will not return vein zone order
        int getAdjacentZoneMinOrderFromAnEntrance(Zone currentZone, GameObject zoneEntrance)
        {
            List<Zone> adjacentZones = getAdjacentZoneFromEntranceRoom(currentZone.getZoneId(), zoneEntrance);
            int minZoneOrder = int.MaxValue;

            // If a zone entrance connects to more than one zone, get the lowest zone order
            for (int j = 0; j < adjacentZones.Count; j++)
            {
                // If it's a vein zone, then you need to get the lowest adjacent zone from the vein zone
                if (adjacentZones[j].getZoneTypeId() == GlobalDefines.veinIntendedId)
                {
                    // Vein zone orders should be int.MaxValue - 1
                    //      Needed because in the rare case that an entrance leads to a vein that has no other adjacent zones, we need to defualt to the vein order
                    //      And if there are other entrances in the zone, this vein zone won't take priority over legitimate adjacent zones
                    int minNonVeinZone = currentZone.getZoneOrder();

                    List<Zone> adjacentVeinZones = adjacentZones[j].getAdjacentZones();
                    for (int k = 0; k < adjacentVeinZones.Count; k++)
                    {
                        if (adjacentVeinZones[k].getZoneId() != currentZone.getZoneId() &&
                            adjacentVeinZones[k].getZoneOrder() < minNonVeinZone)
                        {
                            minNonVeinZone = adjacentVeinZones[k].getZoneOrder();
                        }
                        minZoneOrder = minNonVeinZone;
                    }
                }
                else if (adjacentZones[j].getZoneOrder() < minZoneOrder)
                    minZoneOrder = adjacentZones[j].getZoneOrder();
            }

            return minZoneOrder;
        }

        void setSecondaryEntrances(ref Zone currentZone)
        {
            List<GameObject> zoneEntrances = currentZone.getZoneEntrances();
            List<GameObject> primaryZoneEntrances = currentZone.getPrimaryZoneEntrances();
            List<GameObject> secondaryZoneEntrances = new List<GameObject>();

            for (int i = 0; i < zoneEntrances.Count; i++)
            {
                if (primaryZoneEntrances.Contains(zoneEntrances[i]) == false)
                    secondaryZoneEntrances.Add(zoneEntrances[i]);
            }

            currentZone.addSecondaryZoneEntrances(secondaryZoneEntrances);
        }

        // Will set the zone order and make sure that it doesn't break any of the ordering rules
        bool setZoneOrder(ref Zone zone, bool useNextOrder, int atemptedNewOrder)
        {
            int zonesPotentialOrder = GlobalDefines.defaultOrder;

            if (useNextOrder == true)
                zonesPotentialOrder = nextOrder;
            else
                zonesPotentialOrder = atemptedNewOrder;

            // Check to see if rule 2 is broken for the current zone
            bool newOrderIsFine = checkAdjacentZoneOrdering(zone, zonesPotentialOrder);

            // Do not need to check rule 2 for adjacent zones, theoretically you are assigned a higher order than the adjacent zones
            // *** Check to see if rule 2 is broken for adjacent zones that have had already been assigned

            if (newOrderIsFine == true)
            {
                zone.setZoneOrder(nextOrder);

                if (useNextOrder == true)
                    nextOrder++;
            }

            return newOrderIsFine;
        }

        List<Zone> getAdjacentZoneFromEntranceRoom(int currentZoneId, GameObject entranceRoom)
        {
            List<Zone> adjacentZones = new List<Zone>();
            roomProperties entranceRoomProps = entranceRoom.GetComponent<roomProperties>();

            // Basically check all adjacent rooms if its an entrance to the zone
            for (int i = 0; i < entranceRoomProps.doorList.Count; i++)
            {
                // If door is used
                if (entranceRoomProps.doorList[i].doorUsedBool == true)
                {
                    roomProperties adjacentRoomProps = entranceRoomProps.doorList[i].adjacentRoom.GetComponent<roomProperties>();

                    // If the adjacent room leads to another zone
                    if (adjacentRoomProps.getZoneId() != currentZoneId)
                    {
                        adjacentZones.Add(this.getZone(adjacentRoomProps.getZoneId()));
                    }
                }
            }

            return adjacentZones;
        }

        // Will return a list of adjacent zones that have non defualt ordering
        List<Zone> getAdjacentZonesWithNonDefualtOrdering(Zone zone)
        {
            List<Zone> adjacentZones = zone.getAdjacentZones();
            List<Zone> adjacentZonesWithNonDefualtOrder = new List<Zone>();

            for (int i = 0; i < adjacentZones.Count; i++)
            {
                Zone adjacentZone = adjacentZones[i];
                int adjacentZoneOrder = GlobalDefines.defaultOrder;
                int adjacentZoneTypeId = adjacentZone.getZoneTypeId();

                // If the adjacent zone is a vein, then we need to get the lowest ordering from it's adjacent zones
                if (adjacentZoneTypeId == GlobalDefines.veinIntendedId)
                {
                    int minOrder = GlobalDefines.defaultOrder;
                    Zone minZone = null;
                    List<Zone> adjacentZonesNonVeinType = adjacentZone.getAdjacentZones();

                    for (int j = 0; j < adjacentZonesNonVeinType.Count; j++)
                    {
                        // Make sure it's not the same zone as the one passed in
                        //      And that it's not a vein zone
                        if (adjacentZonesNonVeinType[j].getZoneId() != adjacentZone.getZoneId() &&
                            adjacentZonesNonVeinType[j].getZoneTypeId() != GlobalDefines.veinIntendedId &&
                            adjacentZonesNonVeinType[j].getZoneOrder() != GlobalDefines.defaultOrder)
                        {
                            // Update min order if it's still the default order value
                            if (minOrder == GlobalDefines.defaultOrder)
                            {
                                minOrder = adjacentZonesNonVeinType[j].getZoneOrder();
                                minZone = adjacentZonesNonVeinType[j];
                            }
                            // Else if the zone order is smaller, then update the min order
                            else if (adjacentZonesNonVeinType[j].getZoneOrder() < minOrder)
                            {
                                minOrder = adjacentZonesNonVeinType[j].getZoneOrder();
                                minZone = adjacentZonesNonVeinType[j];
                            }
                        }
                    }

                    adjacentZoneOrder = minOrder;
                    adjacentZone = minZone;
                }
                else
                    adjacentZoneOrder = adjacentZone.getZoneOrder();

                // Only add the adjacent zone order if it's been assigned. Aka not the defualt order
                if (adjacentZoneOrder != GlobalDefines.defaultOrder)
                    adjacentZonesWithNonDefualtOrder.Add(adjacentZone);
            }

            return adjacentZonesWithNonDefualtOrder;
        }

        // Will get all adjacent zone ordering and make sure they follow rule 2
        bool checkAdjacentZoneOrdering(Zone zone, int zonesNewOrdering)
        {
            bool newOrderIsFine = false;

            List<Zone> adjacentZonesWithNonDefualtOrder = getAdjacentZonesWithNonDefualtOrdering(zone);

            // Now check if the accuired adjacent zone orders abide rule 2
            if (adjacentZonesWithNonDefualtOrder.Count == 0)
                newOrderIsFine = true;
            else
            {
                int adjacentOrdersAreLowerCount = 0;

                for (int i = 0; i < adjacentZonesWithNonDefualtOrder.Count; i++)
                {
                    if (adjacentZonesWithNonDefualtOrder[i].getZoneOrder() < zonesNewOrdering)
                        adjacentOrdersAreLowerCount++;
                    else if (adjacentZonesWithNonDefualtOrder[i].getZoneOrder() == zonesNewOrdering)
                        Debug.LogError("Zone Manager - checkAdjacentZoneOrdering(): Found two zones with the same zone ordering");
                }

                if (adjacentOrdersAreLowerCount <= 2)
                    newOrderIsFine = true;
            }

            return newOrderIsFine;
        }

        public List<int> getDoorIdsThatLeadToAnyDifferentZone(GameObject room)
        {
            List<int> doorIds = new List<int>();
            //bool foundAdjacentRoomInAnotherZone = false;
            roomProperties roomProps = room.GetComponent<roomProperties>();

            for (int i = 0; i < roomProps.doorList.Count; i++)
            {
                if (roomProps.doorList[i].doorUsedBool == true)
                {
                    GameObject adjacentRoom = roomProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

                    if (adjacentRoomProps.getZoneId() != roomProps.getZoneId())
                    {
                        doorIds.Add(i);

                        //if (foundAdjacentRoomInAnotherZone)
                        //    Debug.LogError("getDoorIdsThatLeadToAnyDifferentZone() - Found multiple doors to other zones in room: " + room.name);
                        //foundAdjacentRoomInAnotherZone = true;
                    }
                }
            }

            return doorIds;
        }

        public List<int> getDoorIdsThatLeadsToADifferentNonVeinZone(GameObject room)
        {
            List<int> doorIds = new List<int>();
            //bool foundAdjacentRoomInAnotherZone = false;
            roomProperties roomProps = room.GetComponent<roomProperties>();

            for (int i = 0; i < roomProps.doorList.Count; i++)
            {
                if (roomProps.doorList[i].doorUsedBool == true)
                {
                    GameObject adjacentRoom = roomProps.doorList[i].adjacentRoom;
                    roomProperties adjacentRoomProps = adjacentRoom.GetComponent<roomProperties>();

                    if (adjacentRoomProps.getZoneId() != roomProps.getZoneId() &&
                        roomIsInANonVeinZone(adjacentRoom) == true)
                    {
                        doorIds.Add(i);

                        //if (foundAdjacentRoomInAnotherZone)
                        //    Debug.LogError("getDoorIdsThatLeadsToADifferentNonVeinZone() - Found multiple doors to other zones in room: " + room.name);
                        //foundAdjacentRoomInAnotherZone = true;
                    }
                }
            }

            return doorIds;
        }

        public bool roomIsInANonVeinZone(GameObject room)
        {
            bool roomIsInAZone = true;

            if (room.GetComponent<roomProperties>().getZoneTypeId() == GlobalDefines.startZoneId ||
                room.GetComponent<roomProperties>().getZoneTypeId() == GlobalDefines.veinIntendedId)
            {
                roomIsInAZone = false;
            }

            return roomIsInAZone;
        }

        public ZoneLockManager getLockManager()
        {
            return lockManager;
        }
    }
}