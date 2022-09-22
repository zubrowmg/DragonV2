using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PathingClasses;
using Classes;

namespace ProgGeneratorClasses
{

    // Class meant to hold all of the entrances to a clump of rooms
    public class RoomClumpEntranceStorage
    {
        Dictionary<RoomClump, List<KeyValuePair<GameObject, Door>>> dictOfAListOfRoomClumpEntrances;

        public RoomClumpEntranceStorage()
        {
            dictOfAListOfRoomClumpEntrances = new Dictionary<RoomClump, List<KeyValuePair<GameObject, Door>>>();
        }
    }
    

    public class SimplePathStorage
    {
        List<SimplePath> listOfPaths;
        List<List<SimplePath>> pathsWithSimiliarRooms; // Room lists with similiar rooms 

        bool linkListWithSimiliarRooms;

        public SimplePathStorage(bool compareLists)
        {
            listOfPaths = new List<SimplePath>();
            pathsWithSimiliarRooms = new List<List<SimplePath>>();

            linkListWithSimiliarRooms = compareLists;
        }

        public void addRoomList(SimplePath newPath)
        {
            listOfPaths.Add(newPath);

            if (linkListWithSimiliarRooms == true)
            {
                refreshSimiliarPathList();
            }
        }

        bool doespathsWithSimiliarRoomsContainPath(SimplePath path)
        {
            bool similiarRooms = false;

            for (int i = 0; i < pathsWithSimiliarRooms.Count; i++)
            {
                if (pathsWithSimiliarRooms[i].Contains(path) == true)
                {
                    similiarRooms = true;
                    break;
                }
            }

            return similiarRooms;
        }

        // Will refresh the similiar paths list, check if any new connections have been made
        void refreshSimiliarPathList()
        {
            pathsWithSimiliarRooms = new List<List<SimplePath>>();

            // First pass
            // Go through each similiar path list
            for (int i = 0; i < listOfPaths.Count; i++)
            {
                SimplePath currentPath = listOfPaths[i];
                bool foundSimliarPath = false;
                
                if (pathsWithSimiliarRooms.Count == 0)
                {
                    List<SimplePath> newList = new List<SimplePath> { currentPath };
                    pathsWithSimiliarRooms.Add(newList);
                }
                else
                {
                    for (int j = 0; j < pathsWithSimiliarRooms.Count; j++)
                    {
                        List<SimplePath> similarPathList = pathsWithSimiliarRooms[j];

                        for (int k = 0; k < similarPathList.Count; k++)
                        {
                            List<GameObject> currentSimilarPath = similarPathList[k].getPath();
                            
                            for (int l = 0; l < currentSimilarPath.Count; l++)
                            {
                                if (currentPath.getPath().Contains(currentSimilarPath[l]))
                                {
                                    pathsWithSimiliarRooms[j].Add(currentPath);
                                    //similarPathList.Add(currentPath); same difference as line above
                                    foundSimliarPath = true;
                                    break;
                                }
                            }
                            if (foundSimliarPath) break;
                        }
                        if (foundSimliarPath) break;
                    }
                }
            }

            // Second pass, now you need to see if any similiar paths match with other clumped paths
            //      Example, first and second path don't match. They both get inserted into similiar paths list in different entries
            //          Path three links first and second path
            bool updated = false;
            while (updated == true)
            {
                updated = false;

                for (int i = 0; i < pathsWithSimiliarRooms.Count; i++)
                {
                    List<SimplePath> compareList1 = pathsWithSimiliarRooms[i];

                    for (int j = 0; j < pathsWithSimiliarRooms.Count; j++)
                    {
                        List<SimplePath> compareList2 = pathsWithSimiliarRooms[j];

                        if (compareList1.Equals(compareList2) == false)
                        {
                            List<GameObject> compareRoomList1 = getAllRoomsFromPathList(compareList1);
                            List<GameObject> compareRoomList2 = getAllRoomsFromPathList(compareList2);

                            for (int k = 0; k < compareRoomList1.Count; k++){
                                if (compareRoomList2.Contains(compareRoomList1[k]))
                                {
                                    // If there's a match update compare list 1 with contents from compare list 2
                                    //      Update pathsWithSimiliarRooms
                                    //      Break out because you updated pathsWithSimiliarRooms
                                    //      Restart at the top of while loop
                                    updated = true;
                                    compareList1.AddRange(compareList2);
                                    pathsWithSimiliarRooms.Remove(compareList2);
                                    break;
                                }
                            }
                            if (updated) break;
                        }
                    }
                    if (updated) break;
                }
            }

            List<List<SimplePath>> newSimiliarPathList = new List<List<SimplePath>>();
            // Trim the list, only have simliar paths
            for (int i = 0; i < pathsWithSimiliarRooms.Count; i++)
            {
                if (pathsWithSimiliarRooms[i].Count > 1)
                    newSimiliarPathList.Add(pathsWithSimiliarRooms[i]);
            }
            pathsWithSimiliarRooms = newSimiliarPathList;
        }

        List<GameObject> getAllRoomsFromPathList(List<SimplePath> pathList)
        {
            List<GameObject> roomList = new List<GameObject>();

            for (int i = 0; i < pathList.Count; i++)
            {
                roomList.AddRange(pathList[i].getPath());
            }

            return roomList;
        }

        void linkListWithSimiliarPaths(SimplePath newPath)
        {
            bool foundSimiliarity = false;
            List<GameObject> newRoomList = newPath.getPath();

            // Go through each similiar path list
            for (int i = 0; i < pathsWithSimiliarRooms.Count; i++)
            {
                List<SimplePath> listOfPathsWithOverlappingRooms = pathsWithSimiliarRooms[i];

                for (int j = 0; j < listOfPathsWithOverlappingRooms.Count; j++)
                {
                    SimplePath currentPath = listOfPathsWithOverlappingRooms[j];
                    List<GameObject> currentRoomList = currentPath.getPath();

                    for (int k = 0; k < currentRoomList.Count; k++)
                    {
                        GameObject currentRoom = currentRoomList[k];
                        if (newRoomList.Contains(currentRoom) == true)
                        {
                            listOfPathsWithOverlappingRooms.Add(newPath);
                            foundSimiliarity = true;
                        }

                        if (foundSimiliarity)
                            break;
                    }
                    if (foundSimiliarity)
                        break;
                }
                if (foundSimiliarity)
                    break;
            }

            if (foundSimiliarity == false)
                addNewEntryToSimiliarList(newPath);
            else
            {
                // There's a chance that the newly added path links to multiple paths, but earlier we only linked it to one
                //      Need to re-assess each in the similiar path list and see if they connect
                refreshSimiliarPathList();
            }
        }

        void addNewEntryToSimiliarList(SimplePath newPath)
        {
            List<SimplePath> newPathList = new List<SimplePath> { newPath };
            pathsWithSimiliarRooms.Add(newPathList);
        }

        public List<List<SimplePath>> getSimiliarPathList()
        {
            return pathsWithSimiliarRooms;
        }

        public List<SimplePath> getContainerViaIndex(int index)
        {
            return pathsWithSimiliarRooms[index];
        }
    }

    public class RoomListStorage
    {
        List<RoomContainer> listOfRoomLists;
        List<List<RoomContainer>> roomListsWithSimiliarRooms; // Room lists with similiar rooms 

        bool linkListWithSimiliarRooms;

        public RoomListStorage(bool compareLists)
        {
            listOfRoomLists = new List<RoomContainer>();
            roomListsWithSimiliarRooms = new List<List<RoomContainer>>();

            linkListWithSimiliarRooms = compareLists;
        }

        public void addRoomList(RoomContainer newRoomContainer)
        {
            listOfRoomLists.Add(newRoomContainer);

            if (linkListWithSimiliarRooms == true)
            {
                bool breakBool = false;
                List<GameObject> newRoomList = newRoomContainer.getRoomList();

                for (int i = 0; i < roomListsWithSimiliarRooms.Count; i++)
                {
                    List<RoomContainer> listOfRoomsWithOverlappingRooms = roomListsWithSimiliarRooms[i];

                    for (int j = 0; j < listOfRoomsWithOverlappingRooms.Count; j++)
                    {
                        RoomContainer currentRoomContainer = listOfRoomsWithOverlappingRooms[j];
                        List<GameObject> currentRoomList = currentRoomContainer.getRoomList();

                        for (int k = 0; k < currentRoomList.Count; k++)
                        {
                            GameObject currentRoom = currentRoomList[k];
                            if (newRoomList.Contains(currentRoom) == true)
                            {
                                listOfRoomsWithOverlappingRooms.Add(newRoomContainer);
                                breakBool = true;
                            }

                            if (breakBool)
                                break;
                        }
                        if (breakBool)
                            break;
                    }
                    if (breakBool)
                        break;
                }
            }
        }

        public int getSimiliarRoomListCount()
        {
            return roomListsWithSimiliarRooms.Count;
        }

        public List<RoomContainer> getContainerViaIndex(int index)
        {
            return roomListsWithSimiliarRooms[index];
        }
    }

}
