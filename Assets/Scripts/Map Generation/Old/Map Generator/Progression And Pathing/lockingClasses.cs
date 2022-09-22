using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;


// ZoneLockManager
//      Has a dictionary with keys that unlock locks
//      Keys are defined by lock type
namespace LockingClasses
{
    // Story - Side mission, find missing thing
    // Ability - Need ability to traverse an area
    // Boss - Need to kill a boss to unlock a door
    // Button - Button will open door/path
    // Key - Need a key to open door/path
    public enum LockType { None, Story, Ability, Boss, Button, Key };

    public class ZoneLockManager
    {
        Dictionary<Key, List<ZoneLock>> lookUpLocksWithKey;
        Dictionary<GameObject, List<ZoneLock>> lookUpLocksWithRoom;
        Dictionary<int, List<ZoneLock>> lookUpLocksWithZoneId;

        Dictionary<int, List<int>> zoneIsLockedByZoneId;

        public ZoneLockManager()
        {
            lookUpLocksWithKey = new Dictionary<Key, List<ZoneLock>>();
            lookUpLocksWithRoom = new Dictionary<GameObject, List<ZoneLock>>();
            lookUpLocksWithZoneId = new Dictionary<int, List<ZoneLock>>();

            zoneIsLockedByZoneId = new Dictionary<int, List<int>>();
        }


        public bool addLockToZone(Key newKey, ref GameObject room, Door door)
        {
            int doorIndex = 0;
            List<Door> doorList = room.GetComponent<roomProperties>().getListOfUsedDoors();
            for (int i = 0; i < doorList.Count; i++)
            {
                if (doorList[i].Equals(door))
                    doorIndex = i;
            }
            return addLockToZone(newKey, ref room, doorIndex);
        }

        public bool addLockToZone(Key newKey, ref GameObject room, int doorIndex)
        {
            // Covert the lock to a room lock and add it to the room
            RoomLock roomLock = null;
            switch (newKey.getLockType())
            {
                case LockType.Ability:
                    roomLock = new RoomLockAbility(newKey.getAbility(), doorIndex);
                    roomLock.setIsUsed(true);
                    room.GetComponent<roomProperties>().addRoomLock(roomLock);
                    break;

                case LockType.Button:
                    roomLock = new RoomLockButton(doorIndex);
                    roomLock.setIsUsed(true);
                    room.GetComponent<roomProperties>().addRoomLock(roomLock);
                    break;
            }
            

            // Create a new ZoneLock
            ZoneLock newLock = new ZoneLock(newKey, room, doorIndex);
            bool doorIndexHasBeenLocked = checkIfRoomAlreadyLocksDoorIndex(room, doorIndex);
            bool lockSucceded = !doorIndexHasBeenLocked;

            // Do same to lookUpLocksWithRoom
            if (lookUpLocksWithRoom.ContainsKey(room) == true && doorIndexHasBeenLocked == false)
                lookUpLocksWithRoom[room].Add(newLock);
            else
            {
                List<ZoneLock> newLockList = new List<ZoneLock> { newLock };
                lookUpLocksWithRoom.Add(room, newLockList);
            }

            // If lookUpLocksWithKey doesn't contain key then add key and new ZoneLock
            if (lookUpLocksWithKey.ContainsKey(newKey) == true && doorIndexHasBeenLocked == false)
                lookUpLocksWithKey[newKey].Add(newLock);
            else
            {
                List<ZoneLock> newLockList = new List<ZoneLock> { newLock };
                lookUpLocksWithKey.Add(newKey, newLockList);
            }

            roomProperties roomProps = room.GetComponent<roomProperties>();
            int currentZoneId = roomProps.getZoneId();

            // Do same to lookUpLocksWithZoneId
            if (lookUpLocksWithZoneId.ContainsKey(currentZoneId) == true && doorIndexHasBeenLocked == false)
                lookUpLocksWithZoneId[currentZoneId].Add(newLock);
            else
            {
                List<ZoneLock> newLockList = new List<ZoneLock> { newLock };
                lookUpLocksWithZoneId.Add(currentZoneId, newLockList);
            }

            int adjacentLockedRoomZoneId = roomProps.doorList[doorIndex].adjacentRoom.GetComponent<roomProperties>().getZoneId();

            // If the room that is getting locked off is not in the same zone, save it in zoneIsLockedByZoneId
            if (currentZoneId != adjacentLockedRoomZoneId && doorIndexHasBeenLocked == false)
            {
                if (zoneIsLockedByZoneId.ContainsKey(roomProps.getZoneId()) == true)
                    zoneIsLockedByZoneId[adjacentLockedRoomZoneId].Add(currentZoneId);
                else
                {
                    List<int> newList = new List<int> { currentZoneId };
                    zoneIsLockedByZoneId.Add(adjacentLockedRoomZoneId, newList);
                }
            }

            return lockSucceded;
        }

        bool checkIfRoomAlreadyLocksDoorIndex(GameObject room, int doorIndex)
        {
            bool doorIsAlreadyLocked = false;

            if (lookUpLocksWithRoom.ContainsKey(room) == true)
            { 
                List<ZoneLock> locksLockedInRoom = lookUpLocksWithRoom[room];
                for (int i = 0; i < locksLockedInRoom.Count; i++)
                {
                    if (locksLockedInRoom[i].getDoorNum() == doorIndex)
                    {
                        doorIsAlreadyLocked = true;
                        break;
                    }
                }
            }

            return doorIsAlreadyLocked;
        }

        // Ideally you want to lock a room with only 2 doors, makes it easy
        //      It also depends on the ability how easy it would be to lock a room
        public bool findIdealRoomToLock()
        {
            bool foundIdealRoomToLock = true;

            return foundIdealRoomToLock;
        }

        public List<ZoneLock> getLocksWithZoneId(int zoneId)
        {
            List<ZoneLock> locks = new List<ZoneLock>();

            if (lookUpLocksWithZoneId.ContainsKey(zoneId) == true)
                locks = lookUpLocksWithZoneId[zoneId];

            return locks;
        }

    }
}

// More Broad Zone Locks
//      Zone locks are used in the progression generator
//      Later the progression generator will take the Zone Lock and apply it to a room via Room Lock
namespace LockingClasses
{
    public class ZoneLock
    {
        // Key needed for lock
        Key keyToLock;

        // Lock details
        GameObject room;
        int doorNumber;

        // DO NOT CREATE ZONE LOCK TYPES, only keys need the typing
        public ZoneLock()
        {
            this.keyToLock = new Key();

            this.room = null;
            this.doorNumber = -1;
        }

        public ZoneLock(Key key, GameObject room, int doorIndex)
        {
            this.keyToLock = key;

            this.room = room;
            this.doorNumber = doorIndex;
        }

        public GameObject getRoom()
        {
            return room;
        }

        public int getDoorNum()
        {
            return doorNumber;
        }

        public Key getKey()
        {
            return keyToLock;
        }
    }
}

// Key for each lock type
namespace LockingClasses
{
    public class Key
    {
        public LockType lockType;

        public Key()
        {
            this.lockType = LockType.None;
        }

        public Key(LockType lockType)
        {
            this.lockType = lockType;
        }

        public LockType getLockType()
        {
            return lockType;
        }

        public virtual abilities getAbility()
        {
            return abilities.None;
        }
    }

    public class KeyAbility : Key
    {
        abilities ability;

        public KeyAbility(abilities ability) : base(LockType.Ability)
        {
            this.ability = ability;
        }

        public override abilities getAbility()
        {
            return ability;
        }

    }

    public class KeyButton : Key
    {
        GameObject buttonLocation;

        public KeyButton(GameObject roomWithButton) : base(LockType.Button)
        {
            this.buttonLocation = roomWithButton;
        }
    }
}

// Room Locks
namespace LockingClasses
{
    public class RoomLock
    {
        // Room is locked from progression, either by story or powerups
        public bool isUsed;

        public bool currentlyLocked;
        public LockType lockType;
        public int doorLockedId;

        public RoomLock(LockType type)
        {
            this.isUsed = true;
            this.currentlyLocked = true;
            this.lockType = type;
        }

        public RoomLock()
        {
            this.isUsed = false;
            this.currentlyLocked = false;
            this.lockType = LockType.None;
            this.doorLockedId = -99;
        }

        public void setIsUsed(bool isUsed)
        {
            this.isUsed = isUsed;
        }

        public int getDoorId()
        {
            return doorLockedId;
        }
    }

    public class RoomLockAbility : RoomLock
    {
        abilities ability;

        public RoomLockAbility(abilities ability, int doorId) : base(LockType.Ability)
        {
            this.ability = ability;
            this.doorLockedId = doorId;
        }
    }

    public class RoomLockButton : RoomLock
    {

        public RoomLockButton(int doorId) : base(LockType.Button)
        {
            this.doorLockedId = doorId;
        }
    }

    public class RoomLockStory : RoomLock
    {
        public RoomLockStory() : base(LockType.Story)
        {
        }
    }
}