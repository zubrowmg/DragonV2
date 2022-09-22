using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using PathingClasses;

// Rename to Generic Classes
//      Utility classes? Min/Max classes could be thrown in there
//      Can split up to xScriptClasses if x script uses some classes the most
namespace Classes
{
    public class ZoneUnitProperties
    {
        public zoneThemes zoneTheme;
        public abilities zoneAbility;
        public int zoneAreaId;

        public ZoneUnitProperties()
        {
            this.zoneTheme = zoneThemes.None;
            this.zoneAbility = abilities.None;
            this.zoneAreaId = GlobalDefines.defaultId;
        }

        public ZoneUnitProperties(zoneThemes theme, abilities ability, int id)
        {
            this.zoneTheme = theme;
            this.zoneAbility = ability;
            this.zoneAreaId = id;
        }
    }

    public class Themes
    {
        public List<zoneThemes> list;

        public Themes(gameTiming timing)
        {
            switch (timing)
            {
                case gameTiming.Early:
                    this.list = new List<zoneThemes>() {
                        zoneThemes.Rock,
                        zoneThemes.Fire,
                        zoneThemes.Forest,
                        zoneThemes.Wind
                    };

                    break;

                case gameTiming.Mid:
                    this.list = new List<zoneThemes>() {
                        zoneThemes.Lake
                    };

                    break;

                case gameTiming.Late:

                    break;

                case gameTiming.Post:

                    break;
            }
        }
    }

    public class Abilities
    {
        public List<abilities> list;

        public Abilities(gameTiming timing)
        {
            
            switch (timing)
            {
                case gameTiming.Early:
                    this.list = new List<abilities>() {
                        abilities.DoubleJump,
                        abilities.Dash,
                        abilities.WallJump,
                        abilities.MagicBullet
                    };

                    break;

                case gameTiming.Mid:
                    this.list = new List<abilities>(){
                        abilities.AirMask
                    };
                    break;

                case gameTiming.Late:

                    break;

                case gameTiming.Post:

                    break;
            }
        }
    }

    public class Coords
    {
        public int x;
        public int y;

        public Coords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class BookMark
    {
        public int x;
        public int y;

        public GameObject room;
        public int doorNum;
        public generalDirection orientation;

        public BookMark(int doorNum, generalDirection orientation, GameObject room, int x, int y)
        {
            this.x = x;
            this.y = y;

            this.room = room;
            this.doorNum = doorNum;
            this.orientation = orientation;
        }
    }

    public class SquareArea
    {
        public int xMin;
        public int xMax;
        public int yMin;
        public int yMax;

        public int xStart;
        public int yStart;

        public SquareArea(int xMin, int yMin, int xMax, int yMax, int xStart, int yStart)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;

            this.xStart = xStart;
            this.yStart = yStart;
        }
    }

    public class DimensionList
    {
        public int xMin;
        public int yMin;
        public int xMax;
        public int yMax;
        public int squareCount;

        public int area;

        public List<SquareArea> list;
        public List<SquareArea> listHistory;
        public List<List<int>> grid;

        public DimensionList()
        {
            squareCount = 0;

            xMin = System.Int32.MaxValue;
            yMin = System.Int32.MaxValue;
            xMax = 0;
            yMax = 0;

            area = 0;

            list = new List<SquareArea>();
            listHistory = new List<SquareArea>();
            grid = new List<List<int>>();
        }

        public bool addDimension(SquareArea newArea)
        {
            int prevXMin = xMin;
            int prevXMax = xMax;
            int prevYMin = yMin;
            int prevYMax = yMax;

            if (newArea.xMin < xMin) xMin = newArea.xMin;
            if (newArea.yMin < yMin) yMin = newArea.yMin;
            if (newArea.xMax > xMax) xMax = newArea.xMax;
            if (newArea.yMax > yMax) yMax = newArea.yMax;

            

            list.Add(newArea);
            listHistory.Add(newArea);

            updateGrid();

            bool isIsolated = false;

            // Only Check for gaps after you've added a square
            if (squareCount != 0)
            {
                isIsolated = checkForGaps(newArea);
            }
            
            if (isIsolated == true)
            {
                // Remove the last added square
                xMin = prevXMin;
                yMin = prevYMin;
                xMax = prevXMax;
                yMax = prevYMax;

                list.Remove(newArea);
                updateGrid();
            }
            else
            {
                squareCount++;
            }

            return isIsolated;
        }

        public void finalCheck()
        {
            int minTouchingWidth = 2;
            bool dimensionListRejected = false;

            // Checks all square areas to see if any are touching by a single unit, if they are then delete everything
            foreach (SquareArea square in list)
            {
                int yMaxCount = 0;
                int yMinCount = 0;
                int xMaxCount = 0;
                int xMinCount = 0;

                int yMaxCheck = square.yMax - yMin + 1;
                int yMinCheck = square.yMin - yMin - 1;
                int xMinCheck = square.xMin - xMin - 1;
                int xMaxCheck = square.xMax - xMin + 1;

                // Check top perimeter
                if (yMaxCheck < grid[0].Count)
                {
                    for (int x = (square.xMin - xMin); x <= (square.xMax - xMin); x++)
                    {
                        // Top
                        if (grid[x][yMaxCheck] == 1)
                        {
                            yMaxCount++;

                            if (yMaxCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < yMaxCount && yMaxCount < minTouchingWidth)
                    {
                        Debug.Log("Rejected top perimeter");
                        dimensionListRejected = true;
                        break;
                    }
                }

                // Check bot perimeter
                if (yMinCheck >= 0)
                {
                    for (int x = (square.xMin - xMin); x <= (square.xMax - xMin); x++)
                    {
                        if (grid[x][yMinCheck] == 1)
                        {
                            yMinCount++;

                            if (yMinCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < yMinCount && yMinCount < minTouchingWidth)
                    {
                        Debug.Log("Rejected bottom perimeter");
                        dimensionListRejected = true;
                        break;
                    }
                }

                // Check right perimeter
                if (xMaxCheck < grid.Count)
                {
                    for (int y = (square.yMin - yMin); y <= (square.yMax - yMin); y++)
                    {
                        if (grid[xMaxCheck][y] == 1)
                        {
                            xMaxCount++;

                            if (xMaxCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < xMaxCount && xMaxCount < minTouchingWidth)
                    {
                        Debug.Log("Rejected right perimeter");
                        dimensionListRejected = true;
                        break;
                    }
                }

                // Check left perimeter
                if (xMinCheck >= 0)
                {
                    for (int y = (square.yMin - yMin); y <= (square.yMax - yMin); y++)
                    {
                        if (grid[xMinCheck][y] == 1)
                        {
                            xMinCount++;

                            if (xMinCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < xMinCount && xMinCount < minTouchingWidth)
                    {
                        Debug.Log("Rejected left perimeter");
                        dimensionListRejected = true;
                        break;
                    }
                }
                
            }

            if (dimensionListRejected == true)
            {
                resetDimensionList();
            }
        }

        public void printGrid()
        {
            for (int x = 0; x < grid.Count; x++)
            {
                for (int y = 0; y < grid[x].Count; y++)
                {
                    if (grid[x][y] == 1)
                    {
                        Debug.Log((x + xMin) + "," + (y + yMin));
                    }
                }
            }
        }

        private void resetDimensionList()
        {
            squareCount = 0;

            xMin = System.Int32.MaxValue;
            yMin = System.Int32.MaxValue;
            xMax = 0;
            yMax = 0;

            area = 0;

            list = new List<SquareArea>();
            listHistory = new List<SquareArea>();
            grid = new List<List<int>>();
        }

        private bool checkForGaps(SquareArea newArea)
        {
            bool gapsExist = true;

            // Check the last added square for gaps
            // Basically check the immediate perimeter

            //Debug.Log("DIM MIN: " + xMin + "," + yMin);
            //Debug.Log("DIM MAX: " + xMax + "," + yMax);

            //Debug.Log("newArea MIN: " + newArea.xMin + "," + newArea.yMin);
            //Debug.Log("newArea MAX: " + newArea.xMax + "," + newArea.yMax);

            //Debug.Log("x Range: " + (newArea.xMin - xMin) + "," + (newArea.xMax - xMin));

            // Check top/bot perimeter
            //Debug.Log(newArea.xMax - newArea.xMin);
            for (int x = (newArea.xMin - xMin); x <= (newArea.xMax - xMin); x++)
            {
                int yMinCheck = newArea.yMin - yMin - 1;
                int yMaxCheck = newArea.yMax - yMin + 1;

                //Debug.Log(yMaxCheck);

                if (yMinCheck >= 0)
                {
                    if (grid[x][yMinCheck] == 1 )
                    {
                        gapsExist = false;
                        break;
                    }
                }
                if (yMaxCheck < grid[0].Count)
                {
                    if (grid[x][yMaxCheck] == 1)
                    {
                        gapsExist = false;
                        break;
                    }
                }
            }

            //Debug.Log("GAPS_0: " + gapsExist);

            // Check left/right perimeter
            if (gapsExist != false)
            {
                int xMinCheck = newArea.xMin - xMin - 1;
                int xMaxCheck = newArea.xMax - xMin + 1;
                for (int y = (newArea.yMin - yMin); y <= (newArea.yMax - yMin); y++)
                {
                    if (xMinCheck >= 0)
                    {
                        if (grid[xMinCheck][y] == 1)
                        {
                            gapsExist = false;
                            break;
                        }
                    }
                    if (xMaxCheck < grid.Count)
                    {
                        if (grid[xMaxCheck][y] == 1)
                        {
                            gapsExist = false;
                            break;
                        }
                    }
                }
            }



            return gapsExist;
        }

        private void updateGrid()
        {
            grid = new List<List<int>>();
            List<int> temp = new List<int>();

            // Create a blank grid
            for (int x = 0; x < (xMax - xMin + 1); x++)
            {
                temp = new List<int>();

                for (int y = 0; y < (yMax - yMin + 1); y++)
                {
                    temp.Add(0);
                }

                grid.Add(temp);
            }

            // Stamp all of the dimensions into the blank grid
            for (int i = 0; i < list.Count; i++)
            {
                temp = new List<int>();
                SquareArea currentSquare = list[i];
                int xAccess = currentSquare.xMin - xMin;
                int yAccess = currentSquare.yMin - yMin;

                for (int x = 0; x < (currentSquare.xMax - currentSquare.xMin + 1); x++)
                {
                    yAccess = currentSquare.yMin - yMin;
                    for (int y = 0; y < (currentSquare.yMax - currentSquare.yMin + 1); y++)
                    {
                        //print(xAccess + "," + yAccess);

                        grid[xAccess][yAccess] = 1;
                        yAccess++;
                    }
                    xAccess++;
                }
            }

            // Must update the area afterwards
            updateArea();
        }

        private void updateArea()
        {
            area = 0;
            for (int x = 0; x < grid.Count; x++)
            {
                for (int y = 0; y < grid[0].Count; y++)
                {
                    if (grid[x][y] == 1)
                    {
                        area++;
                    }
                }
            }

        }

        public bool pointTooCloseToPreviouslyAttemptedSquareCore(int x, int y)
        {
            bool pointRejected = false;
            int displacement = 3;

            //Debug.Log("INPUT: " + x + "," + y);

            for (int i = 0; i < listHistory.Count; i++)
            {
                //Debug.Log("POINT CHECK X: " + (listHistory[i].xStart - displacement) + "," + (listHistory[i].xStart + displacement));
                //Debug.Log("POINT CHECK Y: " + (listHistory[i].yStart - displacement) + "," + (listHistory[i].yStart + displacement));

                if (listHistory[i].xStart - displacement <= x && x <= listHistory[i].xStart + displacement &&
                    listHistory[i].yStart - displacement <= y && y <= listHistory[i].yStart + displacement)
                {
                    pointRejected = true;
                    break;
                }
            }

            //if (pointRejected) Debug.Log("REJECTED: " + x + "," + y);
            //if (!pointRejected) Debug.Log("ACCEPTED: " + x + "," + y);

            return pointRejected;
        }

        public bool pointAlreadyAdded(int x, int y)
        {
            bool pointUsed = false;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].xMin <= x && x <= list[i].xMax &&
                    list[i].yMin <= y && y <= list[i].yMax)
                {
                    pointUsed = true;
                }
            }

            return pointUsed;
        }

    }

    public class WallPiece
    {
        public Coords coords;
        public string wallType;

        public WallPiece(int x, int y, string wall)
        {
            this.coords = new Coords(x, y);
            this.wallType = wall;
        }
    }

    public class Door
    {
        public int doorId;

        public GameObject adjacentRoom = null;
        public int linkedDoorId; // The door id of the other side of the door

        public bool doorUsedBool; 
        public bool doorLocked; 
        public generalDirection doorsOrient;
        public Coords doorCoords;

        public Door(int doorId, Coords coords, generalDirection orient)
        {
            this.doorId = doorId;
            this.adjacentRoom = null;
            this.doorUsedBool = false;
            this.doorLocked = false;
            this.doorsOrient = orient;
            this.linkedDoorId = -1;
            this.doorCoords = coords;
        }

        public Door(int doorId, int x, int y, generalDirection orient)
        {
            this.doorId = doorId;
            this.adjacentRoom = null;
            this.doorUsedBool = false;
            this.doorLocked = false;
            this.doorsOrient = orient;
            this.linkedDoorId = -1;
            this.doorCoords = new Coords(x, y);
        }

        public Door(int doorId, GameObject adjacentRoom, bool doorUsedBool, bool doorLocked, generalDirection doorsOrient,
                    int linkedDoorId, int x, int y)
        {
            this.doorId = doorId;
            this.adjacentRoom = adjacentRoom;
            this.doorUsedBool = doorUsedBool;
            this.doorLocked = doorLocked;
            this.doorsOrient = doorsOrient;
            this.linkedDoorId = linkedDoorId;
            this.doorCoords = new Coords(x, y);
        }

        public Door(int doorId, GameObject adjacentRoom, bool doorUsedBool, bool doorLocked, generalDirection doorsOrient,
                    int linkedDoorId, Coords coords)
        {
            this.doorId = doorId;
            this.adjacentRoom = adjacentRoom;
            this.doorUsedBool = doorUsedBool;
            this.doorLocked = doorLocked;
            this.doorsOrient = doorsOrient;
            this.linkedDoorId = linkedDoorId;
            this.doorCoords = coords;
        }
    }

    public class RoomPreset
    {
        public Coords coords;
        public string bossRoomId;
        public generalDirectionTwo presetDirection;

        public RoomPreset(int x, int y, string id)
        {
            this.coords = new Coords(x, y);
            this.bossRoomId = id;
            this.presetDirection = generalDirectionTwo.Right;
        }

        public RoomPreset(int x, int y, string id, generalDirectionTwo dir)
        {
            this.coords = new Coords(x, y);
            this.bossRoomId = id;
            this.presetDirection = dir;
        }
    }

    public class MaxValue<T>
    {
        // First value is the largest, last value is the smallest
        LinkedList<KeyValuePair<int, T>> maxQueue;
        int queueSize;

        public MaxValue(int size)
        {
            maxQueue = new LinkedList<KeyValuePair<int, T>>();
            queueSize = size;
        }

        // KeyInt is what is used to determine if
        public void addValue(int keyInt, T value)
        {
            KeyValuePair<int, T> newValue = new KeyValuePair<int, T>(keyInt, value);

            if (maxQueue.Count == 0)
                maxQueue.AddFirst(newValue);
            else
            {
                for (LinkedListNode<KeyValuePair<int, T>> pair = maxQueue.First; pair != null; pair = pair.Next)
                {
                    if (keyInt > pair.Value.Key)
                    {
                        maxQueue.AddBefore(pair, newValue);
                        if (maxQueue.Count > queueSize)
                            maxQueue.RemoveLast();
                    }
                    break;
                }
            }
        }

        public T randomlyChooseValue()
        {
            T randomValue = default(T);

            int randNumber = Random.Range(0, maxQueue.Count);
            int count = 0;
            for (LinkedListNode<KeyValuePair<int, T>> pair = maxQueue.First; pair != null; pair = pair.Next)
            {
                if (count == randNumber)
                {
                    randomValue = pair.Value.Value;
                    break;
                }
                
                count++;
            }

            return randomValue;
        }

        public int getCount()
        {
            return maxQueue.Count;
        }

        public LinkedList<KeyValuePair<int, T>> getMaxValues()
        {
            return maxQueue;
        }
    }

    public class MinValue<T>
    {
        // First value is the largest, last value is the smallest
        LinkedList<KeyValuePair<int, T>> minQueue;
        int queueSize;

        public MinValue(int size)
        {
            minQueue = new LinkedList<KeyValuePair<int, T>>();
            queueSize = size;
        }

        // KeyInt is what is used to determine if
        public void addValue(int keyInt, T value)
        {
            KeyValuePair<int, T> newValue = new KeyValuePair<int, T>(keyInt, value);

            if (minQueue.Count == 0)
                minQueue.AddFirst(newValue);
            else
            {
                for (LinkedListNode<KeyValuePair<int, T>> pair = minQueue.First; pair != null; pair = pair.Next)
                {
                    if (keyInt < pair.Value.Key)
                    {
                        minQueue.AddBefore(pair, newValue);
                        if (minQueue.Count > queueSize)
                            minQueue.RemoveLast();
                    }
                    break;
                }
            }
        }

        public T randomlyChooseValue()
        {
            T randomValue = default(T);

            int randNumber = Random.Range(0, minQueue.Count);
            int count = 0;
            for (LinkedListNode<KeyValuePair<int, T>> pair = minQueue.First; pair != null; pair = pair.Next)
            {
                if (count == randNumber)
                {
                    randomValue = pair.Value.Value;
                    break;
                }

                count++;
            }

            return randomValue;
        }

        public KeyValuePair<int, T> randomlyChooseKeyValuePair()
        {
            KeyValuePair<int, T> randomPair = new KeyValuePair<int, T>();

            int randNumber = Random.Range(0, minQueue.Count);
            int count = 0;
            for (LinkedListNode<KeyValuePair<int, T>> pair = minQueue.First; pair != null; pair = pair.Next)
            {
                if (count == randNumber)
                {
                    randomPair = pair.Value;
                    break;
                }

                count++;
            }

            return randomPair;
        }

        public int getCount()
        {
            return minQueue.Count;
        }

        public LinkedList<KeyValuePair<int, T>> getMinValues()
        {
            return minQueue;
        }
    }

    public class TwoValue<TOne, TTwo>
    {
        TOne firstVal;
        TTwo secondVal;

        public TwoValue(TOne valOne, TTwo valTwo)
        {
            firstVal = valOne;
            secondVal = valTwo;
        }

        public TOne getFirstVal()
        {
            return firstVal;
        }

        public TTwo getSecondVal()
        {
            return secondVal;
        }
    }

    public class ThreeValue<TOne, TTwo, TThree>
    {
        TOne firstVal;
        TTwo secondVal;
        TThree thirdVal;

        public ThreeValue(TOne valOne, TTwo valTwo, TThree valThree)
        {
            firstVal = valOne;
            secondVal = valTwo;
            thirdVal = valThree;
        }

        public TOne getFirst()
        {
            return firstVal;
        }

        public TTwo getSecond()
        {
            return secondVal;
        }

        public TThree getThird()
        {
            return thirdVal;
        }
    }

    public class RoomContainer
    {
        List<GameObject> roomList;
        public RoomContainer()
        {
            roomList = new List<GameObject>();
        }

        public void addRooms(List<GameObject> newRooms)
        {
            roomList.AddRange(newRooms);
        }

        public List<GameObject> getRoomList()
        {
            return roomList;
        }
    }


}

