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

        public void print(string str)
        {
            Debug.Log(str);
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
        CoordsInt centerCoord;

        int squareCount;

        int area;

        List<SquareArea> squareArealist;
        List<SquareArea> listHistory;
        TwoDList<int> grid;

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
            this.centerCoord = new CoordsInt(0, 0);

            this.area = 0;

            this.squareArealist = new List<SquareArea>();
            this.listHistory = new List<SquareArea>();
            //this.grid = new List<List<int>>();
            this.grid = new TwoDList<int>();
        }

        public bool addDimension(SquareArea newArea)
        {
            CoordsInt prevMin = minCoords.deepCopyInt();
            CoordsInt prevMax = maxCoords.deepCopyInt();

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
            bool dimensionRejected = false;

            Coords<int> newMin = new Coords<int>(newArea.xMin(), newArea.yMin());
            Coords<int> newMax = new Coords<int>(newArea.xMax(), newArea.yMax());

            // If new min x coord is smaller than current grid x min, reduce min x to grid x min
            if (newArea.xMin() < minCoords.getX())
            {
                newMin.setX(minCoords.getX());

                // If new min x value is greater than max x value, then it's out of range of the current grid
                if (newMin.getX() > newArea.xMax())
                    dimensionRejected = true;
            }

            // If new min y coord is smaller than current grid y min, reduce min y to grid y min
            if (newArea.yMin() < minCoords.getY())
            {
                newMin.setY(minCoords.getY());

                // If new min y value is greater than max y value, then it's out of range of the current grid
                if (newMin.getY() > newArea.yMax())
                    dimensionRejected = true;
            }

            // If new max x coord is greater than current grid x max, reduce max x to grid x max
            if (newArea.xMax() > maxCoords.getX())
            {
                newMax.setX(maxCoords.getX());

                // If new max x value is smaller than min x value, then it's out of range of the current grid
                if (newMax.getX() < newArea.xMin())
                    dimensionRejected = true;
            }

            // If new max y coord is greater than current grid y max, reduce max y to grid y max
            if (newArea.yMax() > maxCoords.getY())
            {
                newMax.setY(maxCoords.getY());

                // If new max y value is smaller than min y value, then it's out of range of the current grid
                if (newMax.getY() < newArea.yMin())
                    dimensionRejected = true;
            }

            SquareArea trimmedSquareArea = new SquareArea(newMin, newMax, newArea.getStartCoords());


            if (dimensionRejected == false)
                return addDimension(trimmedSquareArea);
            else
                return dimensionRejected;
        }

        private bool checkForGaps(SquareArea newArea)
        {
            bool gapsExist = true;
            CoordsInt checkCoords = new CoordsInt(0, 0);

            for (int x = (newArea.xMin() - minCoords.getX()); x <= (newArea.xMax() - minCoords.getX()); x++)
            {
                int yMinCheck = newArea.yMin() - minCoords.getY() - 1;
                int yMaxCheck = newArea.yMax() - minCoords.getY() + 1;

                if (yMinCheck >= 0)
                {
                    checkCoords = new CoordsInt(x, yMinCheck);
                    if (grid.getElement(checkCoords) == 1)
                    {
                        gapsExist = false;
                        break;
                    }
                }
                if (yMaxCheck < grid.getYCount())
                {
                    checkCoords = new CoordsInt(x, yMaxCheck);
                    if (grid.getElement(checkCoords) == 1)
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
                        CoordsInt minCheckCoord = new CoordsInt(xMinCheck, y);
                        if (grid.getElement(minCheckCoord) == 1)
                        {
                            gapsExist = false;
                            break;
                        }
                    }
                    if (xMaxCheck < grid.getXCount())
                    {
                        CoordsInt maxCheckCoord = new CoordsInt(xMaxCheck, y);
                        if (grid.getElement(maxCheckCoord) == 1)
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
            grid = new TwoDList<int>();
            //List<int> temp = new List<int>();
            CoordsInt newCoord = new CoordsInt(0, 0);

            // Create a blank grid
            for (int x = 0; x < (maxCoords.getX() - minCoords.getX() + 1); x++)
            {
                //temp = new List<int>();

                for (int y = 0; y < (maxCoords.getY() - minCoords.getY() + 1); y++)
                {
                    //temp.Add(0);
                    newCoord = new CoordsInt(x, y);
                    grid.addElement(newCoord, 0);
                }


                //grid.Add(temp);
            }

            // Stamp all of the recorded dimensions into the blank grid
            for (int i = 0; i < squareArealist.Count; i++)
            {
                //temp = new List<int>();
                SquareArea currentSquare = squareArealist[i];
                int xAccess = currentSquare.xMin() - minCoords.getX();
                int yAccess = currentSquare.yMin() - minCoords.getY();

                for (int x = 0; x < (currentSquare.xMax() - currentSquare.xMin() + 1); x++)
                {
                    yAccess = currentSquare.yMin() - minCoords.getY();
                    for (int y = 0; y < (currentSquare.yMax() - currentSquare.yMin() + 1); y++)
                    {
                        CoordsInt editCoord = new CoordsInt(xAccess, yAccess);
                        int one = 1;
                        grid.setElement(editCoord, ref one);

                        //grid[xAccess][yAccess] = 1;
                        yAccess++;
                    }
                    xAccess++;
                }
            }

            // Must update the area and center coord afterwards
            updateArea();
            updateCenterCoord();
        }

        // Rough calculation, don't check all coords. Just a few
        public void updateCenterCoord()
        {
            int diffX = maxCoords.getX() - minCoords.getX();
            int diffY = maxCoords.getY() - minCoords.getY();

            int centerX = Mathf.FloorToInt(diffX / 2) + minCoords.getX();
            int centerY = Mathf.FloorToInt(diffY / 2) + minCoords.getY();

            this.centerCoord = new CoordsInt(centerX, centerY);


            // This function couldn't handle outliars and wasn't accurate
            /*if (squareArealist.Count == 1)
            {
                int diffX = maxCoords.getX() - minCoords.getX();
                int diffY = maxCoords.getY() - minCoords.getY();

                int centerX = Mathf.FloorToInt(diffX / 2) + minCoords.getX();
                int centerY = Mathf.FloorToInt(diffY / 2) + minCoords.getY();

                this.centerCoord = new CoordsInt(centerX + minCoords.getX(), centerY + minCoords.getY());
            }
            else
            {
                List<CoordsInt> reducedCoordsList = grid.getReducedCoordsList();
                CoordsInt currentAverage = null;
                bool firstCalc = true;
                Debug.Log("\tX AXIS: " + grid.getXCount() + "\n\tY AXIS: " + grid.getYCount());

                //for (int x = 0; x < grid.getXCount(); x = x + 3)

                foreach (var coordCheck in reducedCoordsList)
                {
                    //for (int y = 0; y < grid.getYCount(); y = y + 3)
                    //{
                        //CoordsInt coordCheck = new CoordsInt(x, y);
                        coordCheck.print("\t\tCOORD CHECK: ");
                        if (grid.getElement(coordCheck) == 1)
                        {
                            if (firstCalc)
                            {
                                currentAverage = coordCheck;
                                firstCalc = false;
                            }
                            else
                                currentAverage = CommonFunctions.calculateCoordsAverage(currentAverage, coordCheck);
                            currentAverage.print("\t\tAVERAGE: ");

                        }
                    //}
                }
                this.centerCoord = currentAverage;
                this.centerCoord.incX(minCoords.getX());
                this.centerCoord.incY(minCoords.getY());
                this.centerCoord.print("\t\tFINAL AVERAGE: ");

            }
            */
        }

        private void updateArea()
        {
            area = 0;
            for (int x = 0; x < grid.getXCount(); x++)
            {
                for (int y = 0; y < grid.getYCount(); y++)
                {
                    CoordsInt checkCoord = new CoordsInt(x, y);

                    if (grid.getElement(checkCoord) == 1)
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

        // Takes another dim list as input and calculates the percentage of overlaping coords
        //      Percentage is based on how many coords overlap to THIS dim list
        public float checkOverlapPercent(DimensionList compareDimList)
        {
            bool bothDimsOverlap = false;
            float overlapPercentage = 0f;

            CoordsInt compareMinCoords = compareDimList.getMinCoords();
            CoordsInt compareMaxCoords = compareDimList.getMaxCoords();

            CoordsInt commonMinCoord = new CoordsInt(0, 0);
            CoordsInt commonMaxCoord = new CoordsInt(0, 0);

            bool xOverlap = false;
            bool yOverlap = false;

            // First start with a basic min/max coord check
            //      Find a common min and max coord between the two

            if (this.minCoords.getX() <= compareMinCoords.getX() && compareMinCoords.getX() <= this.maxCoords.getX())
            {
                xOverlap = true;
                commonMinCoord.setX(compareMinCoords.getX());
            }
            else
                commonMinCoord.setX(this.minCoords.getX());

            if (this.minCoords.getX() <= compareMaxCoords.getX() && compareMaxCoords.getX() <= this.maxCoords.getX())
            {
                xOverlap = true;
                commonMaxCoord.setX(compareMaxCoords.getX());
            }
            else
                commonMaxCoord.setX(this.maxCoords.getX());

            if (this.minCoords.getY() <= compareMinCoords.getY() && compareMinCoords.getY() <= this.maxCoords.getY())
            {
                yOverlap = true;
                commonMinCoord.setY(compareMinCoords.getY());
            }
            else
                commonMinCoord.setY(this.minCoords.getY());

            if (this.minCoords.getY() <= compareMaxCoords.getY() && compareMaxCoords.getY() <= this.maxCoords.getY())
            {
                yOverlap = true;
                commonMaxCoord.setY(compareMaxCoords.getY());
            }
            else
                commonMaxCoord.setY(this.maxCoords.getY());

            bothDimsOverlap = xOverlap && yOverlap;

            //this.printMinMax("DIM 1: ");
            //compareDimList.printMinMax("DIM 2: ");

            //commonMinCoord.print("COMMON MIN COORDS: ");
            //commonMaxCoord.print("COMMON MAX COORDS: ");

            //Debug.Log("BOTH OVERLAP BOOL: " + bothDimsOverlap);

            // Start a brute force compare if both dims can overlap
            if (bothDimsOverlap == true)
            {
                int overlapCount = 0;
                compareDimList.getGrid(out TwoDList<int> compareGrid, out CoordsInt compareStartCoords);

                for (int x = commonMinCoord.getX(); x < commonMaxCoord.getX(); x++)
                {
                    for (int y = commonMinCoord.getY(); y < commonMaxCoord.getY(); y++)
                    {
                        //Debug.Log("LOOP: " + x + ", " + y);
                        int xAccess = x - this.minCoords.getX();
                        int yAccess = y - this.minCoords.getY();

                        int xCompareAccess = x - compareMinCoords.getX();
                        int yCompareAccess = y - compareMinCoords.getY();

                        CoordsInt checkCoord = new CoordsInt(xAccess, yAccess);
                        CoordsInt compareCheckCoord = new CoordsInt(xCompareAccess, yCompareAccess);
                        if (this.grid.getElement(checkCoord) == 1 && compareGrid.getElement(compareCheckCoord) == 1)
                            overlapCount++;
                    }
                }
                //Debug.Log("COUNT: " + overlapCount);
                //Debug.Log("AREA: " + this.area);
                overlapPercentage = (float)overlapCount / (float)this.area;
            }

            return overlapPercentage;
        }

        // =======================================================================
        //                        Final Check Functions
        // =======================================================================

        // THIS FUNCTION IS NEEDED
        //      Checks if there is a body of sqaure areas that are connected by a 2 wide Tile. This is not good enough to be considered "touching"
        public bool finalCheck()
        {
            int minTouchingWidth = 2; // I do not recommend changing this
            bool dimensionListIsAcceptable = true;

            if (squareArealist.Count == 1)
                return dimensionListIsAcceptable;

            CoordsInt checkCoords = new CoordsInt(0, 0);

            //Debug.Log("=========================================================");

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
                bool topPerimeterIsOnGridEdge = true;
                if (yMaxCheck < grid.getYCount())
                    topPerimeterIsOnGridEdge = false;

                int yMinCheck = square.yMin() - minCoords.getY() - 1;
                bool botPerimeterIsOnGridEdge = true;
                if (yMinCheck >= 0)
                    botPerimeterIsOnGridEdge = false;

                int xMinCheck = square.xMin() - minCoords.getX() - 1;
                bool leftPerimeterIsOnGridEdge = true;
                if (xMinCheck >= 0)
                    leftPerimeterIsOnGridEdge = false;

                int xMaxCheck = square.xMax() - minCoords.getX() + 1;
                bool rightPerimeterIsOnGridEdge = true;
                if (xMaxCheck < grid.getXCount())
                    rightPerimeterIsOnGridEdge = false;

                //Debug.Log("CHECK MIN: " + xMinCheck + ", " + yMinCheck + "\nCHECK MAX:" + xMaxCheck + ", " + yMaxCheck);
                //Debug.Log("SQUARE MIN: " + square.xMin() + ", " + square.yMin() + "\nSQAURE MAX: " + square.xMax() + ", " + square.yMax());

                // Check top perimeter
                if (topPerimeterIsOnGridEdge == false)
                {
                    for (int x = (square.xMin() - minCoords.getX()); x <= (square.xMax() - minCoords.getX()); x++)
                    {
                        checkCoords = new CoordsInt(x, yMaxCheck);
                        // Top
                        if (grid.getElement(checkCoords) == 1)
                        {
                            yMaxCount++;
                            tempCoord = new CoordsInt(x, yMaxCheck);

                            if (yMaxCount >= minTouchingWidth)
                                break;
                        }
                    }

                    if (0 < yMaxCount && yMaxCount < minTouchingWidth)
                    {
                        if (topPerimeterCheck(tempCoord, rightPerimeterIsOnGridEdge, leftPerimeterIsOnGridEdge) == false)
                        {
                            Debug.LogError("Rejected top perimeter. Width: " + yMaxCount);
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check bot perimeter
                if (botPerimeterIsOnGridEdge == false)
                {
                    for (int x = (square.xMin() - minCoords.getX()); x <= (square.xMax() - minCoords.getX()); x++)
                    {
                        checkCoords = new CoordsInt(x, yMinCheck);

                        if (grid.getElement(checkCoords) == 1)
                        {
                            yMinCount++;
                            tempCoord = new CoordsInt(x, yMinCheck);

                            if (yMinCount >= minTouchingWidth)
                                break;
                        }
                    }
                    if (0 < yMinCount && yMinCount < minTouchingWidth)
                    {
                        if (bottomPerimeterCheck(tempCoord, rightPerimeterIsOnGridEdge, leftPerimeterIsOnGridEdge) == false)
                        {
                            Debug.LogError("Rejected bottom perimeter");
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check right perimeter
                if (rightPerimeterIsOnGridEdge == false)
                {
                    for (int y = (square.yMin() - minCoords.getY()); y <= (square.yMax() - minCoords.getY()); y++)
                    {
                        checkCoords = new CoordsInt(xMaxCheck, y);
                        if (grid.getElement(checkCoords) == 1)
                        {
                            xMaxCount++;
                            tempCoord = new CoordsInt(xMaxCheck, y);

                            if (xMaxCount >= minTouchingWidth)
                                break;
                        }
                    }

                    if (0 < xMaxCount && xMaxCount < minTouchingWidth)
                    {
                        if (rightPerimeterCheck(tempCoord, topPerimeterIsOnGridEdge, botPerimeterIsOnGridEdge) == false)
                        {
                            Debug.LogError("Rejected right perimeter");
                            dimensionListIsAcceptable = false;
                            break;
                        }
                    }
                }

                // Check left perimeter
                if (leftPerimeterIsOnGridEdge == false)
                {
                    for (int y = (square.yMin() - minCoords.getY()); y <= (square.yMax() - minCoords.getY()); y++)
                    {
                        checkCoords = new CoordsInt(xMinCheck, y);
                        if (grid.getElement(checkCoords) == 1)
                        {
                            xMinCount++;
                            tempCoord = new CoordsInt(xMinCheck, y);

                            if (xMinCount >= minTouchingWidth)
                                break;
                        }
                    }

                    if (0 < xMinCount && xMinCount < minTouchingWidth)
                    {
                        if (leftPerimeterCheck(tempCoord, topPerimeterIsOnGridEdge, botPerimeterIsOnGridEdge) == false)
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

        private bool topPerimeterCheck(CoordsInt coord, bool rightPerimeterIsOnGridEdge, bool leftPerimeterIsOnGridEdge)
        {
            bool checkPass = false;

            // If the square area is NOT on the right most edge, then check the top right corner
            if (rightPerimeterIsOnGridEdge == false)
            {
                if (topRightCheckForTopCheck(coord) == true)
                    checkPass = true;
            }

            // If the square area is NOT on the left most edge, then check the top left corner
            if (leftPerimeterIsOnGridEdge == false && checkPass == false)
            {
                if (topLeftCheckForTopCheck(coord) == true)
                    checkPass = true;
            }

            return checkPass;
        }
        private bool topRightCheckForTopCheck(CoordsInt coord)
        {
            CoordsInt rightCheck = coord.deepCopyInt();
            rightCheck.incX();
            CoordsInt rightDownCheck = rightCheck.deepCopyInt();
            rightDownCheck.decY();

            // FOR CHECKING THE TOP EDGE PERIMETER OF A SQUARE AREA
            // Check top right corner, spaces represent square areas
            //      01 11
            //
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 11
            bool pass = false;
            if (grid.getElement(rightCheck) == 1 && grid.getElement(rightDownCheck) == 1)
                pass = true;
            return pass;
        }
        private bool topLeftCheckForTopCheck(CoordsInt coord)
        {
            CoordsInt leftCheck = coord.deepCopyInt();
            leftCheck.decX();
            CoordsInt leftDownCheck = leftCheck.deepCopyInt();
            leftDownCheck.decY();

            // FOR CHECKING THE TOP EDGE PERIMETER OF A SQUARE AREA
            // Check top left corner, spaces represent square areas
            //      11 10
            //
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 11
            bool pass = false;
            if (grid.getElement(leftCheck) == 1 && grid.getElement(leftDownCheck) == 1)
                pass = true;
            return pass;
        }

        private bool bottomPerimeterCheck(CoordsInt coord, bool rightPerimeterIsOnGridEdge, bool leftPerimeterIsOnGridEdge)
        {
            bool checkPass = false;

            // If the square area is NOT on the right most edge, then check the bottom right corner
            if (rightPerimeterIsOnGridEdge == false)
            {
                if (botRightCheckForBottomCheck(coord) == true)
                    checkPass = true;
            }
            // If the square area is NOT on the left most edge, then check the bottom left corner
            if (leftPerimeterIsOnGridEdge == false && checkPass == false)
            {
                if (botLeftCheckForBottomCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool botRightCheckForBottomCheck(CoordsInt coord)
        {
            CoordsInt rightCheck = coord.deepCopyInt();
            rightCheck.incX();
            CoordsInt rightUpCheck = rightCheck.deepCopyInt();
            rightUpCheck.incY();

            // FOR CHECKING THE BOTTOM EDGE PERIMETER OF A SQUARE AREA
            // Check bottom right corner, spaces represent square areas
            //      11 11
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap between top and bottom dims
            //      
            //      01 11
            bool pass = false;
            if (grid.getElement(rightCheck) == 1 && grid.getElement(rightUpCheck) == 1)
                pass = true;
            return pass;
        }
        private bool botLeftCheckForBottomCheck(CoordsInt coord)
        {
            CoordsInt leftCheck = coord.deepCopyInt();
            leftCheck.decX();
            CoordsInt leftUpCheck = leftCheck.deepCopyInt();
            leftUpCheck.incY();

            // FOR CHECKING THE BOTTOM EDGE PERIMETER OF A SQUARE AREA
            // Check bottom left corner, spaces represent square areas
            //      11 11
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap between top and bottom dims
            //      
            //      11 10
            bool pass = false;
            if (grid.getElement(leftCheck) == 1 && grid.getElement(leftUpCheck) == 1)
                pass = true;
            return pass;
        }

        private bool rightPerimeterCheck(CoordsInt coord, bool topPerimeterIsOnGridEdge, bool botPerimeterIsOnGridEdge)
        {
            bool checkPass = false;

            // If the square area is NOT on the top most edge, then check the top right corner
            if (topPerimeterIsOnGridEdge == false)
            {
                if (topRightCheckForRightCheck(coord) == true)
                    checkPass = true;

            }
            // If the square area is NOT on the bottom most edge, then check the bottom right corner
            if (botPerimeterIsOnGridEdge == false && checkPass == false)
            {
                if (botRightCheckForRightCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool botRightCheckForRightCheck(CoordsInt coord)
        {
            CoordsInt downCheck = coord.deepCopyInt();
            downCheck.decY();
            CoordsInt downLeftCheck = downCheck.deepCopyInt();
            downLeftCheck.decX();
            
            // FOR CHECKING THE RIGHT EDGE PERIMETER OF A SQUARE AREA
            // Check bottom right corner, spaces represent square areas
            //      11 00
            //      11 11      Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      
            //      11 11
            bool pass = false;
            if (grid.getElement(downCheck) == 1 && grid.getElement(downLeftCheck) == 1)
                pass = true;
            return pass;
        }
        private bool topRightCheckForRightCheck(CoordsInt coord)
        {
            CoordsInt upCheck = coord.deepCopyInt();
            upCheck.incY();
            CoordsInt upLeftCheck = upCheck.deepCopyInt();
            upLeftCheck.decX();
            
            // FOR CHECKING THE RIGHT EDGE PERIMETER OF A SQUARE AREA
            // Check top right corner, spaces represent square areas
            //      11 11     
            //
            //      11 11       Second 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      11 00
            bool pass = false;
            if (grid.getElement(upCheck) == 1 && grid.getElement(upLeftCheck) == 1)
                pass = true;
            return pass;
        }

        private bool leftPerimeterCheck(CoordsInt coord, bool topPerimeterIsOnGridEdge, bool botPerimeterIsOnGridEdge)
        {
            bool checkPass = false;

            // If the square area is NOT on the top most edge, then check the top left corner
            if (topPerimeterIsOnGridEdge == false)
            {
                if (topLeftCheckForLeftCheck(coord) == true)
                    checkPass = true;

            }
            // If the square area is NOT on the bottom most edge, then check the bottom left corner
            if (botPerimeterIsOnGridEdge == false && checkPass == false)
            {
                if (botLeftCheckForLeftCheck(coord) == true)
                    checkPass = true;
            }
            return checkPass;
        }
        private bool botLeftCheckForLeftCheck(CoordsInt coord)
        {
            CoordsInt downCheck = coord.deepCopyInt();
            downCheck.decY();
            CoordsInt downRightCheck = downCheck.deepCopyInt();
            downRightCheck.incX();
            
            // FOR CHECKING THE LEFT EDGE PERIMETER OF A SQUARE AREA
            // Check bottom left corner, spaces represent square areas
            //      00 11
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      
            //      11 11
            bool pass = false;
            if (grid.getElement(downCheck) == 1 && grid.getElement(downRightCheck) == 1)
                pass = true;
            return pass;
        }
        private bool topLeftCheckForLeftCheck(CoordsInt coord)
        {
            CoordsInt upCheck = coord.deepCopyInt();
            upCheck.incY();
            CoordsInt upRightCheck = upCheck.deepCopyInt();
            upRightCheck.incX();
            
            // FOR CHECKING THE LEFT EDGE PERIMETER OF A SQUARE AREA
            // Check top left corner, spaces represent square areas
            //      11 11
            //
            //      11 11      Third 1 in this row will reject the entire dim, even though there is a 2 wide gap
            //      00 11
            bool pass = false;
            if (grid.getElement(upCheck) == 1 && grid.getElement(upRightCheck) == 1)
                pass = true;
            return pass;
        }

        // =======================================================================
        //                              Setter/Getters
        // =======================================================================

        public void getGrid(out TwoDList<int> grid, out CoordsInt startCoords)
        {
            grid = this.grid;
            startCoords = this.minCoords;
        }

        public List<CoordsInt> getAllSelectedGridCoords()
        {
            List<CoordsInt> listOfCoords = new List<CoordsInt>();

            for (int y = grid.getYCount() - 1; y >= 0; y--)
            {
                for (int x = 0; x < grid.getXCount(); x++)
                {
                    CoordsInt accessCoord = new CoordsInt(x, y);
                    if (grid.getElement(accessCoord) == 1)
                        listOfCoords.Add(new CoordsInt(x + minCoords.getX(), y + minCoords.getY()));
                }
            }

            return listOfCoords;
        }

        public int getGridVal(CoordsInt coords)
        {
            return this.grid.getElement(coords);
        }

        public void printGrid(bool printCoords)
        {
            printMinMax("\t");

            for (int y = grid.getYCount() - 1; y >= 0; y--)
            {
                string printStr = "\t[";
                for (int x = 0; x < grid.getXCount(); x++)
                {
                    CoordsInt accessCoords = new CoordsInt(x, y);
                    if (printCoords)
                    {
                        string xStr = "";
                        string yStr = "";

                        if (x < 10)
                            xStr = "0" + x.ToString();
                        else
                            xStr = x.ToString();

                        if (y < 10)
                            yStr = "0" + y.ToString();
                        else
                            yStr = y.ToString();

                        if (grid.getElement(accessCoords) == 1)
                            printStr = printStr + " " + xStr + "," + yStr;
                        else
                            printStr = printStr + " " + "-----";
                    }
                    else
                        printStr = printStr + " " + grid.getElement(accessCoords);
                }
                printStr = printStr + " ]";
                Debug.Log(printStr);
            }


        }

        public void printMinMax(string str)
        {
            Debug.Log(str);
            minCoords.print("\tGRID DIM MIN: ");
            maxCoords.print("\tGRID DIM MAX: ");
        }

        public CoordsInt getStartCoords()
        {
            return this.startCoords;
        }

        public CoordsInt getMinCoords()
        {
            return this.minCoords;
        }

        public CoordsInt getMaxCoords()
        {
            return this.maxCoords;
        }

        public int getArea()
        {
            return area;
        }

        public CoordsInt getCenterCoord()
        {
            return this.centerCoord;
        }
    }
}
