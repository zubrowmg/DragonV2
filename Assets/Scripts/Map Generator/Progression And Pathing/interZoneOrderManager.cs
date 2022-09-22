using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathingClasses
{

    public class PathOrderManager
    {
        public LinkedList<SubAreaClump> subAreaClumpOrder;
        public Dictionary<SubAreaClump, List<RoomClump>> roomClumpOrderInsideSubAreaClump;

        public RoomClump tempRoomClump; // Meant to be loaded up with rooms, then trigered to add to roomClumpOrderInsideSubAreaClump

        // ------- Depricated -------
        public Dictionary<SubAreaClump, LinkedList<SubArea>> subAreaOrderInsideSubAreaClump;
        public Dictionary<SubArea, LinkedList<PathsClassOrder>> pathsOrderInsideSubArea;
        // --------------------------

        public PathOrderManager()
        {
            this.subAreaClumpOrder = new LinkedList<SubAreaClump>();
            this.roomClumpOrderInsideSubAreaClump = new Dictionary<SubAreaClump, List<RoomClump>>();
            this.tempRoomClump = new RoomClump();

            this.subAreaOrderInsideSubAreaClump = new Dictionary<SubAreaClump, LinkedList<SubArea>>();
            this.pathsOrderInsideSubArea = new Dictionary<SubArea, LinkedList<PathsClassOrder>>();
        }

        public void setRoomClumpInSubAreaClump(RoomClump newRoomClump, SubAreaClump subAreaClump)
        {
            if (roomClumpOrderInsideSubAreaClump.ContainsKey(subAreaClump) == false)
            {
                roomClumpOrderInsideSubAreaClump.Add(subAreaClump, new List<RoomClump> { newRoomClump });
            }
            else
            {
                roomClumpOrderInsideSubAreaClump[subAreaClump].Add(newRoomClump);
            }
        }

        public void queueSubAreaClumpOrder(SubAreaClump nextSubAreaClump)
        {
            subAreaClumpOrder.AddLast(nextSubAreaClump);
        }

        public void queueSubAreaOrder(SubAreaClump subAreaClump, SubArea nextSubArea)
        {
            if (subAreaOrderInsideSubAreaClump.ContainsKey(subAreaClump) == false)
                subAreaOrderInsideSubAreaClump.Add(subAreaClump, new LinkedList<SubArea>());
            
            subAreaOrderInsideSubAreaClump[subAreaClump].AddLast(nextSubArea);
        }

        public void queuePathsOrder(SubArea subArea, PathsClassOrder nextPath)
        {
            if (pathsOrderInsideSubArea.ContainsKey(subArea) == false)
                pathsOrderInsideSubArea.Add(subArea, new LinkedList<PathsClassOrder>());

            pathsOrderInsideSubArea[subArea].AddLast(nextPath);
        }

        public SubAreaClump getFirstSubAreaClump()
        {
            return subAreaClumpOrder.First.Value;
        }

        public GameObject getFirstEntranceToSubAreaClump(SubAreaClump clump)
        {
            SubArea firstSubArea = subAreaOrderInsideSubAreaClump[clump].First.Value;
            return pathsOrderInsideSubArea[firstSubArea].First.Value.getFirstRoom(); ;
        }

        public SubArea getFirstSubAreaToSubAreaClump(SubAreaClump clump)
        {
            return subAreaOrderInsideSubAreaClump[clump].First.Value;
        }

        public GameObject getFirstEntranceToSubAreaClump(SubArea subArea)
        {
            return pathsOrderInsideSubArea[subArea].First.Value.getFirstRoom(); ;
        }

        public void addRoomsToTempRoomClump(List<GameObject> roomList)
        {
            tempRoomClump.addRoomsWithDuplicateCheck(roomList);
        }

    }


    public class RoomClump
    {
        List<GameObject> rooms;
        GameObject firstRoom;

        public RoomClump()
        {
            rooms = new List<GameObject>();
            firstRoom = new GameObject();
        }

        public RoomClump(List<GameObject> newRooms, GameObject newFirstRooms)
        {
            this.rooms = newRooms;
            this.firstRoom = newFirstRooms;
        }

        public void addRoom(GameObject newRoom)
        {
            rooms.Add(newRoom);
        }

        public void addRooms(List<GameObject> newRooms)
        {
            rooms.AddRange(newRooms);
        }

        public void addRoomsWithDuplicateCheck(List<GameObject> newRooms)
        {
            for (int i = 0; i < newRooms.Count; i++)
            {
                if (rooms.Contains(newRooms[i]) == false)
                    addRoom(newRooms[i]);
            }
        }

        public void setFirstRoom(GameObject newFirstRooms)
        {
            firstRoom = newFirstRooms;
        }

        public bool roomIsInRoomClump(GameObject room)
        {
            return rooms.Contains(room);
        }
    }

    public class PathsClassOrder
    {
        Paths path;
        GameObject firstRoom;
        GameObject lastRoom;

        public PathsClassOrder()
        {
            path = new Paths();
            firstRoom = new GameObject();
            lastRoom = new GameObject();
        }

        public PathsClassOrder(Paths newPath, GameObject newFirstRoom)
        {
            path = newPath;
            firstRoom = newFirstRoom;

            if (newPath.getEndRoomList()[0].Equals(newFirstRoom))
                lastRoom = newPath.getEndRoomList()[1];
            else
                lastRoom = newPath.getEndRoomList()[0];
        }

        public void setPaths(Paths newPath)
        {
            path = newPath;
        }

        public Paths getPath()
        {
            return path;
        }

        public void setFirstRoom(GameObject newFirstRoom)
        {
            firstRoom = newFirstRoom;
        }

        public GameObject getFirstRoom()
        {
            return firstRoom;
        }

        public void setLastRoom(GameObject newLastRoom)
        {
            lastRoom = newLastRoom;
        }

        public GameObject getLastRoom()
        {
            return lastRoom;
        }
    }
}
