using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;
using CommonlyUsedClasses;

public class DimVeinZoneCreator : DimCreator
{

    // Defualt Vein Zone Creator Variables
    int squareAreaFillMinSideLength = 6;
    float squareAreaMaxArea = 75;

    int veinZoneMaxArea = 2000;
    bool topOffDimList = true;

    public DimVeinZoneCreator(ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.wiggleDisplacementRange = 0;
        this.maxAdjacentSearchDisplacement = squareAreaFillMinSideLength + 3;
    }

    protected override bool tileCheck(CoordsInt coords)
    {
        return tileIsVein(coords);
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
        int unlockMax = 0;
        int xMinLockedTimer = 0; int xMaxLockedTimer = 0;
        int yMinLockedTimer = 0; int yMaxLockedTimer = 0;
        int xMinUnlockCount = 0; int xMaxUnlockCount = 0;
        int yMinUnlockCount = 0; int yMaxUnlockCount = 0;

        //Debug.Log("MAX AREA: " + maxSqaureAreaArea);

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

            // Because we are checking the top and bottom first we can't take the changed x axis into account
            // The top and bottoms might be fine to expand, but xMin or xMax might be in an occupied room
            // Check the top/bottom perimeter
            for (int x = minCoords.getX(); x <= maxCoords.getX(); x++) // +1 and -1 ARE NEEDED!!!! READ ABOVE
            {
                if (yMinLocked && yMaxLocked) break;

                if (!yMinLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(x, minCoords.getY());

                    // If there's a vein then don't expand the bounds
                    if (tileIsVein(coordsToCheck) == true)
                    {
                        yMinLocked = true;
                        minCoords.incY();
                    }
                }

                if (!yMaxLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(x, maxCoords.getY());
                    
                    // If there's a vein then don't expand the bounds
                    if (tileIsVein(coordsToCheck) == true)
                    {
                        yMaxLocked = true;
                        maxCoords.decY();

                        //break;
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

            // Check the left/right perimeter
            for (int y = minCoords.getY(); y <= maxCoords.getY(); y++)
            {
                if (xMinLocked && xMaxLocked) break;

                if (!xMinLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(minCoords.getX(), y);

                    // If there's a vein then don't expand the bounds
                    if (tileIsVein(coordsToCheck) == true)
                    {
                        xMinLocked = true;
                        minCoords.incX();

                    }
                }

                if (!xMaxLocked)
                {
                    CoordsInt coordsToCheck = new CoordsInt(maxCoords.getX(), y);
                    
                    // If there's a vein then don't expand the bounds
                    if (tileIsVein(coordsToCheck) == true)
                    {
                        xMaxLocked = true;
                        maxCoords.decX();

                    }
                }
            }

            area = ((Mathf.Abs(maxCoords.getX() - minCoords.getX()) + 1) * (Mathf.Abs(maxCoords.getY() - minCoords.getY()) + 1));
        }

    }

    protected override bool wiggleConditions(CoordsInt wiggledCoords)
    {
        // For Dim Vein Zone Creator we check if the tile is not a vein
        bool isNotVein = tileIsVein(wiggledCoords) == false;
        return isNotVein;
    }

    // =======================================================================================
    //                                  Start Functions
    // =======================================================================================

    public DimensionList getDimensionsForVeinZone(CoordsInt startCoords, bool debugMode, DirectionBias directionBias, out TwoDList<Tile> tileMapRef)
    {
        setDimensionVariables(squareAreaFillMinSideLength, veinZoneMaxArea, squareAreaMaxArea, directionBias, this.topOffDimList);

        //directionBias.print();

        DimensionList newDimList = getDimensions(startCoords);
        tileMapRef = this.tileMapRef;

        if (debugMode)
            markSelectedGridForDebug(newDimList);

        return newDimList;
    }
}
