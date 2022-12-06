using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

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

        public int xStart()
        {
            return this.startCoords.getX();
        }

        public int yStart()
        {
            return this.startCoords.getY();
        }

        public Coords<int> getStartCoords()
        {
            return this.startCoords;
        }

        public void print()
        {
            minCoords.print("\tSQUARE AREA MIN: ");
            maxCoords.print("\tSQUARE AREA MAX: ");
        }
    }


    public class Dimensions
    {
        CoordsInt minCoords;
        CoordsInt maxCoords;
        int area;

        public Dimensions(CoordsInt minCoords, CoordsInt maxCoords)
        {
            this.minCoords = minCoords;
            this.maxCoords = maxCoords;
            this.area = CommonFunctions.calculateArea(minCoords, maxCoords);
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

        public int getArea()
        {
            return this.area;
        }
    }


    public class DimensionList
    {
        CoordsInt startCoords;

        CoordsInt minCoords;
        CoordsInt maxCoords;
        int squareCount;

        public int area;

        public List<SquareArea> squareArealist;
        public List<SquareArea> listHistory;
        public List<List<int>> grid;

        public DimensionList(CoordsInt startCoords)
        {
            this.startCoords = startCoords;
            resetDimensionList();
        }

        private void resetDimensionList()
        {
            this.squareCount = 0;

            this.minCoords = new CoordsInt(System.Int32.MaxValue, System.Int32.MaxValue);
            this.maxCoords = new CoordsInt(0, 0);

            this.area = 0;

            this.squareArealist = new List<SquareArea>();
            this.listHistory = new List<SquareArea>();
            this.grid = new List<List<int>>();
        }

        public bool addDimension(SquareArea newArea)
        {
            CoordsInt prevMin = minCoords.deepCopyInt();
            CoordsInt prevMax = maxCoords.deepCopyInt();


            //if (newArea.xMin() == 600 && newArea.yMin() == 205 &&
            //    newArea.xMax() == 600 && newArea.yMax() == 211)
            //{
            //    Debug.Log("FOUND IT");
            //}



            if (newArea.xMin() < minCoords.getX())
                minCoords.setX(newArea.xMin());
            if (newArea.yMin() < minCoords.getY())
                minCoords.setY(newArea.yMin());
            if (newArea.xMax() > maxCoords.getX())
                maxCoords.setX(newArea.xMax());
            if (newArea.yMax() > maxCoords.getY())
                maxCoords.setY(newArea.yMax());

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
                minCoords = prevMin.deepCopyInt();
                maxCoords = prevMax.deepCopyInt();

                squareArealist.Remove(newArea);
                updateGrid();
            }
            else
            {
                squareCount++;
            }

            return isIsolated;
        }

        public bool addDimensionWithOutExpandingDims(SquareArea newArea)
        {
            Coords<int> newMin = new Coords<int>(newArea.xMin(), newArea.yMin());
            Coords<int> newMax = new Coords<int>(newArea.xMax(), newArea.yMax());

            //Debug.Log("WITHOUT EXPANDING");
            //newArea.print();

            if (newArea.xMin() < minCoords.getX())
                newMin.setX(minCoords.getX());
            if (newArea.yMin() < minCoords.getY())
                newMin.setY(minCoords.getY());

            if (newArea.xMax() > maxCoords.getX())
                newMax.setX(maxCoords.getX());
            if (newArea.yMax() > maxCoords.getY())
                newMax.setY(maxCoords.getY());

            SquareArea trimmedSquareArea = new SquareArea(newMin, newMax, newArea.getStartCoords());

            return addDimension(trimmedSquareArea);
        }

        // THIS FUNCTION IS NEEDED
        //      Checks if there is a body of sqaure areas that are connected by a 2 wide Tile. This is not good enough to be considered "touching"
        public bool finalCheck()
        {
            int minTouchingWidth = 2; // I do not recommend changing this
            bool dimensionListIsAcceptable = true;

            if (squareArealist.Count == 1)
                return dimensionListIsAcceptable;

            //Debug.Log("=========================================================");
            minCoords.print("\tMIN COORDS: ");
            maxCoords.print("\tMAX COORDS: ");

            // Checks all square areas to see if any are touching by a single unit, if they are then delete everything
            //      Checks the next row/columns for touching square areas
            foreach (SquareArea square in squareArealist)
            {
                CoordsInt tempCoord = new CoordsInt(0, 0);

                int yMaxCount = 0;
                int yMinCount = 0;
                int xMaxCount = 0;
                int xMinCount = 0;

                // If any of these checks are negative that means that the current square area is on the edge
                //      No need to check these 
                int yMaxCheck = square.yMax() - minCoords.getY() + 1;
                bool checkingTopPerimeter = false;
                if (yMaxCheck < grid[0].Count)
                    checkingTopPerimeter = true;

                int yMinCheck = square.yMin() - minCoords.getY() - 1;
                bool checkingBotPerimeter = false;
                if (yMinCheck >= 0)
                    checkingBotPerimeter = true;

                int xMinCheck = square.xMin() - minCoords.getX() - 1;
                bool checkingLeftPerimeter = false;
                if (xMinCheck >= 0)
                    checkingLeftPerimeter = true;

                int xMaxCheck = square.xMax() - minCoords.getX() + 1;
                bool checkingRightPerimeter = false;
                if (xMaxCheck < grid.Count)
                    checkingRightPerimeter = true;

                //Debug.Log("CHECK MIN: " + xMinCheck + ", " + yMinCheck + "\nCHECK MAX:" + xMaxCheck + ", " + yMaxCheck);
                Debug.Log("SQUARE MIN: " + square.xMin() + ", " + square.yMin() + "\nSQAURE MAX: " + square.xMax() + ", " + square.yMax());

                // Check top perimeter
                if (checkingTopPerimeter == true)
                {
                    for (int x = (square.xMin() - minCoords.getX()); x <= (square.xMax() - minCoords.getX()); x++)
                    {
                        // Top
                        if (grid[x][yMaxCheck] == 1)
                        {
                            yMaxCount++;
                            tempCoord = new CoordsInt(x, yMaxCheck);

                            if (yMaxCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < yMaxCount && yMaxCount < minTouchingWidth)
                    {
                        
                        if (topRightCheckForTopCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        else if (topLeftCheckForTopCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        else
                        {
                            Debug.LogError("Rejected top perimeter. Width: " + yMaxCount);
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check bot perimeter
                if (checkingBotPerimeter == true)
                {
                    for (int x = (square.xMin() - minCoords.getX()); x <= (square.xMax() - minCoords.getX()); x++)
                    {
                        if (grid[x][yMinCheck] == 1)
                        {
                            yMinCount++;
                            tempCoord = new CoordsInt(x, yMinCheck);

                            if (yMinCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < yMinCount && yMinCount < minTouchingWidth)
                    {
                        //tempCoord.print("TEMP COORDS: ");
                        //Debug.Log("GRID DIMS: " + grid.Count + ", " + grid[0].Count);
                        

                        if (bottomPerimeterCheck(tempCoord, checkingRightPerimeter, checkingLeftPerimeter) == false)
                        {
                            Debug.LogError("Rejected bottom perimeter");
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check right perimeter
                if (checkingRightPerimeter == true)
                {
                    for (int y = (square.yMin() - minCoords.getY()); y <= (square.yMax() - minCoords.getY()); y++)
                    {
                        if (grid[xMaxCheck][y] == 1)
                        {
                            xMaxCount++;
                            tempCoord = new CoordsInt(xMaxCheck, y);

                            //Debug.Log(xMaxCheck + ", " + y);

                            if (xMaxCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < xMaxCount && xMaxCount < minTouchingWidth)
                    {
                       
                        if (botRightCheckForRightCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        else if (topRightCheckForRightCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        else
                        {
                            Debug.LogError("Rejected right perimeter");
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check left perimeter
                if (checkingLeftPerimeter == true)
                {
                    for (int y = (square.yMin() - minCoords.getY()); y <= (square.yMax() - minCoords.getY()); y++)
                    {
                        if (grid[xMinCheck][y] == 1)
                        {
                            xMinCount++;
                            tempCoord = new CoordsInt(xMinCheck, y);

                            if (xMinCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < xMinCount && xMinCount < minTouchingWidth)
                    {
                        /*
                        if (botLeftCheckForLeftCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        
                        else if (topLeftCheckForLeftCheck(tempCoord) == true)
                        {
                            // Nothing
                        }
                        */
                        if (leftPerimeterCheck(tempCoord, checkingTopPerimeter, checkingBotPerimeter) == false)
                        //else
                        {
                            Debug.LogError("Rejected left perimeter");
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }
            }

            if (dimensionListIsAcceptable == false)
            {
                resetDimensionList();
            }

            return dimensionListIsAcceptable;
        }

        private bool topPerimeterCheck(CoordsInt coord, bool checkingRightPerimeter, bool checkingLeftPerimeter)
        {
            bool checkPass = false;

            // If the square area is on the right most edge, then only check for top left corner
            if (checkingRightPerimeter == false)
            {
                if (topLeftCheckForTopCheck(coord) == true)
                    checkPass = true;
            }
            // If the square area is on the left most edge, then only check for top right corner
            if (checkingLeftPerimeter == false && checkPass == false)
            {
                if (topRightCheckForTopCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool topRightCheckForTopCheck(CoordsInt coord)
        {
            // FOR CHECKING THE TOP EDGE PERIMETER OF A SQUARE AREA
            // Check top right corner, spaces represent square areas
            //      01 11
            //
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 11
            bool pass = false;
            if (grid[coord.getX() + 1][coord.getY()] == 1 && grid[coord.getX() + 1][coord.getY() - 1] == 1)
                pass = true;
            return pass;
        }
        private bool topLeftCheckForTopCheck(CoordsInt coord)
        {
            // FOR CHECKING THE TOP EDGE PERIMETER OF A SQUARE AREA
            // Check top left corner, spaces represent square areas
            //      11 10
            //
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 11
            bool pass = false;
            if (grid[coord.getX() - 1][coord.getY()] == 1 && grid[coord.getX() - 1][coord.getY() - 1] == 1)
                pass = true;
            return pass;
        }

        private bool bottomPerimeterCheck(CoordsInt coord, bool checkingRightPerimeter, bool checkingLeftPerimeter)
        {
            bool checkPass = false;

            // If the square area is on the right most edge, then only check for bottom left corner
            if (checkingRightPerimeter == false)
            {
                if (botLeftCheckForBottomCheck(coord) == true)
                    checkPass = true;
            }
            // If the square area is on the left most edge, then only check for bottom right corner
            if (checkingLeftPerimeter == false && checkPass == false)
            {
                if (botRightCheckForBottomCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool botRightCheckForBottomCheck(CoordsInt coord)
        {
            // FOR CHECKING THE BOTTOM EDGE PERIMETER OF A SQUARE AREA
            // Check bottom right corner, spaces represent square areas
            //      11 11
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap between top and bottom dims
            //      
            //      01 11
            bool pass = false;
            if (grid[coord.getX() + 1][coord.getY()] == 1 && grid[coord.getX() + 1][coord.getY() + 1] == 1)
                pass = true;
            return pass;
        }
        private bool botLeftCheckForBottomCheck(CoordsInt coord)
        {
            // FOR CHECKING THE BOTTOM EDGE PERIMETER OF A SQUARE AREA
            // Check bottom left corner, spaces represent square areas
            //      11 11
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap between top and bottom dims
            //      
            //      11 10
            bool pass = false;
            if (grid[coord.getX() - 1][coord.getY()] == 1 && grid[coord.getX() - 1][coord.getY() + 1] == 1)
                pass = true;
            return pass;
        }

        private bool rightPerimeterCheck(CoordsInt coord, bool checkingTopPerimeter, bool checkingBottomPerimeter)
        {
            bool checkPass = false;

            // If the square area is on the top most edge, then only check for bottom right corner
            if (checkingTopPerimeter == false)
            {
                if (botRightCheckForRightCheck(coord) == true)
                    checkPass = true;
            }
            // If the square area is on the bottom most edge, then only check for top right corner
            if (checkingBottomPerimeter == false && checkPass == false)
            {
                if (topRightCheckForRightCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool botRightCheckForRightCheck(CoordsInt coord)
        {
            // FOR CHECKING THE RIGHT EDGE PERIMETER OF A SQUARE AREA
            // Check bottom right corner, spaces represent square areas
            //      11 00
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      
            //      11 11
            bool pass = false;
            if (grid[coord.getX() - 1][coord.getY() - 1] == 1 && grid[coord.getX()][coord.getY() - 1] == 1)
                pass = true;
            return pass;
        }
        private bool topRightCheckForRightCheck(CoordsInt coord)
        {
            // FOR CHECKING THE RIGHT EDGE PERIMETER OF A SQUARE AREA
            // Check top right corner, spaces represent square areas
            //      11 11     
            //
            //      11 11       Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 00
            bool pass = false;
            if (grid[coord.getX() - 1][coord.getY() + 1] == 1 && grid[coord.getX()][coord.getY() + 1] == 1)
                pass = true;
            return pass;
        }

        private bool leftPerimeterCheck(CoordsInt coord, bool checkingTopPerimeter, bool checkingBottomPerimeter)
        {
            bool checkPass = false;

            // If the square area is on the top most edge, then only check for bottom left corner
            if (checkingTopPerimeter == false)
            {
                if (botLeftCheckForLeftCheck(coord) == true)
                    checkPass = true;
            }
            // If the square area is on the bottom most edge, then only check for top left corner
            //if (checkingBottomPerimeter == false && checkPass == false)
            //{
                if (topLeftCheckForLeftCheck(coord) == true)
                    checkPass = true;
            //}
            return checkPass;
        }
        private bool botLeftCheckForLeftCheck(CoordsInt coord)
        {
            // FOR CHECKING THE LEFT EDGE PERIMETER OF A SQUARE AREA
            // Check bottom left corner, spaces represent square areas
            //      00 11
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      
            //      11 11
            bool pass = false;
            if (grid[coord.getX()][coord.getY() - 1] == 1 && grid[coord.getX() + 1][coord.getY() - 1] == 1)
                pass = true;
            return pass;
        }
        private bool topLeftCheckForLeftCheck(CoordsInt coord)
        {
            // FOR CHECKING THE LEFT EDGE PERIMETER OF A SQUARE AREA
            // Check top left corner, spaces represent square areas
            //      11 11
            //
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      00 11
            bool pass = false;
            if (grid[coord.getX()][coord.getY() + 1] == 1 && grid[coord.getX() + 1][coord.getY() + 1] == 1)
                pass = true;
            return pass;
        }

        private bool checkForGaps(SquareArea newArea)
        {
            bool gapsExist = true;

            for (int x = (newArea.xMin() - minCoords.getX()); x <= (newArea.xMax() - minCoords.getX()); x++)
            {
                int yMinCheck = newArea.yMin() - minCoords.getY() - 1;
                int yMaxCheck = newArea.yMax() - minCoords.getY() + 1;

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
                int xMinCheck = newArea.xMin() - minCoords.getX() - 1;
                int xMaxCheck = newArea.xMax() - minCoords.getX() + 1;
                for (int y = (newArea.yMin() - minCoords.getY()); y <= (newArea.yMax() - minCoords.getY()); y++)
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
            for (int x = 0; x < (maxCoords.getX() - minCoords.getX() + 1); x++)
            {
                temp = new List<int>();

                for (int y = 0; y < (maxCoords.getY() - minCoords.getY() + 1); y++)
                {
                    temp.Add(0);
                }

                grid.Add(temp);
            }

            // Stamp all of the dimensions into the blank grid
            for (int i = 0; i < squareArealist.Count; i++)
            {
                temp = new List<int>();
                SquareArea currentSquare = squareArealist[i];
                int xAccess = currentSquare.xMin() - minCoords.getX();
                int yAccess = currentSquare.yMin() - minCoords.getY();

                for (int x = 0; x < (currentSquare.xMax() - currentSquare.xMin() + 1); x++)
                {
                    yAccess = currentSquare.yMin() - minCoords.getY();
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

        public bool pointTooCloseToPreviouslyAttemptedSquareCore(CoordsInt coords, int displacemntRange)
        {
            int x = coords.getX();
            int y = coords.getY();
            bool pointRejected = false;
            int displacement = displacemntRange;

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

        public bool pointAlreadyAdded(CoordsInt coords)
        {
            int x = coords.getX();
            int y = coords.getY();
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

        public bool startCoordsAreOutsideOfCurrentDimList(Coords<int> startCoords)
        {
            bool startCoordAreOutside = false;

            if (minCoords.getX() > startCoords.getX() || startCoords.getX() > maxCoords.getX())
                startCoordAreOutside = true;

            if (minCoords.getY() > startCoords.getY() || startCoords.getY() > maxCoords.getY())
                startCoordAreOutside = true;

            return startCoordAreOutside;
        }

        // =======================================================================
        //                              Setter/Getters
        // =======================================================================

        public void getGrid(out List<List<int>> grid, out Coords<int> startCoords)
        {
            grid = this.grid;
            startCoords = this.minCoords;
        }

        public int getGridVal(Coords<int> coords)
        {
            return this.grid[coords.getX()][coords.getY()];
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

        public void printMinMax(string str)
        {
            minCoords.print(str + "DIM MIN: ");
            maxCoords.print(str + "DIM MAX: ");
        }

        public CoordsInt getStartCoords()
        {
            return this.startCoords;
        }

        public CoordsInt getMinCoords()
        {
            return this.minCoords;
        }
    }
}
