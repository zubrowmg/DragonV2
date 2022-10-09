using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonlyUsedClasses
{
    public class SquareArea
    {
        Coords<int> minCoords;
        Coords<int> maxCoords;

        Coords<int> startCoords;

        public SquareArea(Coords<int> minCoords, Coords<int> maxCoords, Coords<int> startCoords)
        {
            this.minCoords = minCoords;
            this.maxCoords = maxCoords;
            this.startCoords = startCoords;
        }

        public Coords<int> getMin()
        {
            return this.minCoords;
        }

        public Coords<int> getMax()
        {
            return this.maxCoords;
        }

        public int xMin()
        {
            return this.minCoords.getX();
        }

        public int yMin()
        {
            return this.minCoords.getY();
        }

        public int xMax()
        {
            return this.maxCoords.getX();
        }

        public int yMax()
        {
            return this.maxCoords.getY();
        }
    }


    public class Dimensions
    {
        Coords<int> minCoords;
        Coords<int> maxCoords;

        public Dimensions(Coords<int> minCoords, Coords<int> maxCoords)
        {
            this.minCoords = minCoords;
            this.maxCoords = maxCoords;
        }

        public int getMinX()
        {
            return minCoords.getX();
        }

        public int getMinY()
        {
            return minCoords.getY();
        }

        public int getMaxX()
        {
            return maxCoords.getX();
        }

        public int getMaxY()
        {
            return maxCoords.getY();
        }
    }


    public class DimensionList
    {
        Coords<int> minCoords;
        Coords<int> maxCoords;
        int squareCount;

        public int area;

        public List<SquareArea> squareArealist;
        public List<SquareArea> listHistory;
        public List<List<int>> grid;

        public DimensionList()
        {
            resetDimensionList();
        }

        private void resetDimensionList()
        {
            this.squareCount = 0;

            this.minCoords = new Coords<int>(System.Int32.MaxValue, System.Int32.MaxValue);
            this.maxCoords = new Coords<int>(0, 0);

            this.area = 0;

            this.squareArealist = new List<SquareArea>();
            this.listHistory = new List<SquareArea>();
            this.grid = new List<List<int>>();
        }

        public bool addDimension(SquareArea newArea)
        {
            Coords<int> prevMin = minCoords.deepCopy();
            Coords<int> prevMax = maxCoords.deepCopy();

            if (newArea.xMin() < minCoords.getX()) minCoords.setX(newArea.xMin());
            if (newArea.yMin() < minCoords.getY()) minCoords.setY(newArea.yMin());
            if (newArea.xMax() > maxCoords.getX()) maxCoords.setX(newArea.xMax());
            if (newArea.yMax() > maxCoords.getY()) maxCoords.setY(newArea.yMax());

            squareArealist.Add(newArea);
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

                squareArealist.Remove(newArea);
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
            foreach (SquareArea square in squareArealist)
            {
                int yMaxCount = 0;
                int yMinCount = 0;
                int xMaxCount = 0;
                int xMinCount = 0;

                int yMaxCheck = square.yMax() - yMin + 1;
                int yMinCheck = square.yMin() - yMin - 1;
                int xMinCheck = square.xMin() - xMin - 1;
                int xMaxCheck = square.xMax() - xMin + 1;

                // Check top perimeter
                if (yMaxCheck < grid[0].Count)
                {
                    for (int x = (square.xMin() - xMin); x <= (square.xMax() - xMin); x++)
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
                    for (int x = (square.xMin() - xMin); x <= (square.xMax() - xMin); x++)
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
                    for (int y = (square.yMin() - yMin); y <= (square.yMax() - yMin); y++)
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
                    for (int y = (square.yMin() - yMin); y <= (square.yMax() - yMin); y++)
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
                        Debug.Log((x + minCoords.getX()) + "," + (y + minCoords.getY()));
                    }
                }
            }
        }

        

        private bool checkForGaps(SquareArea newArea)
        {
            bool gapsExist = true;

            for (int x = (newArea.xMin() - xMin); x <= (newArea.xMax() - xMin); x++)
            {
                int yMinCheck = newArea.yMin() - yMin - 1;
                int yMaxCheck = newArea.yMax() - yMin + 1;

                if (yMinCheck >= 0)
                {
                    if (grid[x][yMinCheck] == 1)
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

            // Check left/right perimeter
            if (gapsExist != false)
            {
                int xMinCheck = newArea.xMin() - xMin - 1;
                int xMaxCheck = newArea.xMax() - xMin + 1;
                for (int y = (newArea.yMin() - yMin); y <= (newArea.yMax() - yMin); y++)
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
            8
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
            for (int i = 0; i < squareArealist.Count; i++)
            {
                temp = new List<int>();
                SquareArea currentSquare = list[i];
                int xAccess = currentSquare.xMin() - xMin;
                int yAccess = currentSquare.yMin() - yMin;

                for (int x = 0; x < (currentSquare.xMax() - currentSquare.xMin() + 1); x++)
                {
                    yAccess = currentSquare.yMin() - yMin;
                    for (int y = 0; y < (currentSquare.yMax() - currentSquare.yMin() + 1); y++)
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

                if (listHistory[i].xStart() - displacement <= x && x <= listHistory[i].xStart() + displacement &&
                    listHistory[i].yStart() - displacement <= y && y <= listHistory[i].yStart() + displacement)
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

            for (int i = 0; i < squareArealist.Count; i++)
            {
                if (squareArealist[i].xMin() <= x && x <= squareArealist[i].xMax() &&
                    squareArealist[i].yMin() <= y && y <= squareArealist[i].yMax())
                {
                    pointUsed = true;
                }
            }

            return pointUsed;
        }

    }
}
