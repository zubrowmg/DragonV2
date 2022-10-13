using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;
using CommonlyUsedClasses;

public class DimRoomCreator : DimCreator
{
    float notVeinPercentage = .50f;

    float squareAreaMaxArea = 120;

    // Room Creator Variables
    int roomMinSideLength = 5;
    int roomMaxArea = 160;

    // Room Fill Creator Variables
    int roomFillMinSideLength = 3;
    int roomFillMaxArea = 250;

    public DimRoomCreator(ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.wiggleDisplacementRange = 3;
        this.maxSqaureAreaArea = 120f;
        this.maxAdjacentSearchDisplacement = 11;
    }

    // =======================================================================================
    //                                  Abstract Functions
    // =======================================================================================

    protected override bool tileCheck(CoordsInt coords)
    {
        return tileIsOccupiedByRoom(coords);
    }

    protected override void expandAroundPoint(ref CoordsInt minCoords, ref CoordsInt maxCoords)
    {
        float area = 0f;

        int increment = 1;

        bool xMinLocked = false;
        bool xMaxLocked = false;
        bool yMinLocked = false;
        bool yMaxLocked = false;

        // Every so often try to unlock the locked boundries
        //   Helps in very odd cases, where the start point is close to non veins
        int unlockTime = 3;
        int unlockMax = 2;
        int xMinLockedTimer = 0; int xMaxLockedTimer = 0;
        int yMinLockedTimer = 0; int yMaxLockedTimer = 0;
        int xMinUnlockCount = 0; int xMaxUnlockCount = 0;
        int yMinUnlockCount = 0; int yMaxUnlockCount = 0;

        while (area < maxSqaureAreaArea)
        {

            // Unlock dimensions code
            if (xMinLocked && xMinLockedTimer > unlockTime && xMinUnlockCount < unlockMax)
            {
                xMinLocked = false;
                xMinUnlockCount++;
            }
            if (xMaxLocked && xMaxLockedTimer > unlockTime && xMaxUnlockCount < unlockMax)
            {
                xMaxLocked = false;
                xMaxUnlockCount++;
            }
            if (yMinLocked && yMinLockedTimer > unlockTime && yMinUnlockCount < unlockMax)
            {
                yMinLocked = false;
                yMinUnlockCount++;
            }
            if (yMaxLocked && yMaxLockedTimer > unlockTime && yMaxUnlockCount < unlockMax)
            {
                yMaxLocked = false;
                yMaxUnlockCount++;
            }

            // Increment y dimensions that aren't locked
            if (!yMinLocked)
            {
                minCoords.decY(increment);
                yMinLockedTimer = 0;
            }
            else
            {
                yMinLockedTimer++;
            }
            if (!yMaxLocked)
            {
                maxCoords.incY(increment);
                yMaxLockedTimer = 0;
            }
            else
            {
                yMaxLockedTimer++;
            }

            int minNotVeinCount = 0;
            int maxNotVeinCount = 0;

            // Because we are checking the top and bottom first we can't take the changed x axis into account
            // The top and bottoms might be fine to expand, but xMin or xMax might be in an occupied room
            // Check the top/bottom perimeter
            for (int x = minCoords.getX(); x <= maxCoords.getX(); x++) // +1 and -1 ARE NEEDED!!!! READ ABOVE
            {
                if (yMinLocked && yMaxLocked) break;

                if (!yMinLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(x, minCoords.getY());

                    // If it's occupied then don't expand the bounds
                    if (tileIsOccupiedByRoom(coordsToCheck) == true)
                    {
                        yMinLocked = true;
                        minCoords.incY();
                    }
                    // If it's not a vein then check the total non vein count
                    else if (tileIsVein(coordsToCheck) == false)
                    {
                        minNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)minNotVeinCount / Mathf.Abs(maxCoords.getX() - minCoords.getX())) > notVeinPercentage)
                        {
                            yMinLocked = true;
                            minCoords.incY();
                        }
                    }
                }

                if (!yMaxLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(x, maxCoords.getY());

                    if (tileIsOccupiedByRoom(coordsToCheck) == true)
                    {
                        yMaxLocked = true;
                        maxCoords.decY();
                        //break;
                    }
                    // If it's not a vein then check the total non vein count
                    else if (tileIsVein(coordsToCheck) == false)
                    {
                        maxNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)maxNotVeinCount / Mathf.Abs(maxCoords.getX() - minCoords.getX())) > notVeinPercentage)
                        {
                            yMaxLocked = true;
                            maxCoords.decY();
                        }
                    }
                }
            }

            // Increment dimensions that aren't locked
            if (!xMinLocked)
            {
                minCoords.decX(increment);
                xMinLockedTimer = 0;
            }
            else
            {
                xMinLockedTimer++;
            }
            if (!xMaxLocked)
            {
                maxCoords.incX(increment);
                xMaxLockedTimer = 0;
            }
            else
            {
                xMaxLockedTimer++;
            }

            // If all 4 dimensions are locked then break
            if (xMinLocked && xMaxLocked && yMinLocked && yMaxLocked) break;

            minNotVeinCount = 0;
            maxNotVeinCount = 0;

            // Check the left/right perimeter
            for (int y = minCoords.getY(); y <= maxCoords.getY(); y++)
            {
                if (xMinLocked && xMaxLocked) break;


                //print("test  " + ((float)minNotVeinCount / Mathf.Abs(yMax - yMin)));
                if (!xMinLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(minCoords.getX(), y);

                    // If it's occupied then don't expand the bounds
                    if (tileIsOccupiedByRoom(coordsToCheck) == true)
                    {
                        xMinLocked = true;
                        minCoords.incX();
                    }
                    // If it's not a vein then check the total non vein count
                    else if (tileIsVein(coordsToCheck) == false)
                    {
                        minNotVeinCount++;

                        // If there are too many non vein grids then don't expand
                        if ((float)((float)minNotVeinCount / Mathf.Abs(maxCoords.getY() - minCoords.getY())) > notVeinPercentage)
                        {
                            xMinLocked = true;
                            minCoords.incX();
                        }
                    }
                }

                if (!xMaxLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(maxCoords.getX(), y);


                    if (tileIsOccupiedByRoom(coordsToCheck) == true)
                    {
                        xMaxLocked = true;
                        maxCoords.decX();
                    }
                    // If it's not a vein then check the total non vein count
                    else if (tileIsVein(coordsToCheck) == false)
                    {
                        maxNotVeinCount++;
                        // If there are too many non vein grids then don't expand
                        if ((float)((float)maxNotVeinCount / Mathf.Abs(maxCoords.getY() - minCoords.getY())) > notVeinPercentage)
                        {
                            xMaxLocked = true;
                            maxCoords.decX();
                        }
                    }
                }
            }

            area = ((Mathf.Abs(maxCoords.getX() - minCoords.getX()) + 1) * (Mathf.Abs(maxCoords.getY() - minCoords.getY()) + 1));
        }
    }

    protected override bool wiggleConditions(CoordsInt wiggledCoords)
    {
        // For Dim Room Creator we check if the tile is a vein and if it's not a room
        bool isVeinAndNotARoom = tileIsVein(wiggledCoords) == true && tileIsOccupiedByRoom(wiggledCoords) == false;
        return isVeinAndNotARoom;
    }

    protected override void setDimensionVariables(int minSideLength, int maxArea, float individualMaxSquareArea)
    {
        this.minSideLength = minSideLength;
        this.maxSqaureAreaArea = individualMaxSquareArea;
        this.maxArea = maxArea;
    }

    // =======================================================================================
    //                                  Start Functions
    // =======================================================================================

    public DimensionList getDimensionsForRoom(CoordsInt startCoords)
    {
        setDimensionVariables(roomMinSideLength, roomMaxArea, squareAreaMaxArea);
        return getDimensions(startCoords);
    }

    public DimensionList getDimensionsToFillRoomCracks(CoordsInt startCoords)
    {
        setDimensionVariables(roomFillMinSideLength, roomFillMaxArea, squareAreaMaxArea);
        return getDimensions(startCoords);
    }

}
