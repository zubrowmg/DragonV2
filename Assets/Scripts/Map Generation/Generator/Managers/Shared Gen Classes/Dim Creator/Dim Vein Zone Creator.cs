using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;
using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;
namespace DimCreatorClasses
{
    public class DimVeinZoneCreator : DimCreator
    {
        // ===============================================
        // Defualt Vein Zone Creator Variables
        // ===============================================
        int defualtSquareAreaFillMinSideLength = 6;
        float defualtSquareAreaMaxArea = 75;

        int defaultVeinZoneMaxArea = 4000;
        bool defaultTopOffDimList = true;

        int defaultMaxDistanceFromCenter = System.Int32.MaxValue;


        // ===============================================
        // Restricted Free Space Creator Variables
        // ===============================================
        int restrictedSquareAreaFillMinSideLength = 8;
        float restrictedIndividualSquareMax = 100;

        //int restrictedMaxArea = 150;
        bool restrictedTopOffDimList = false;

        int restrictedMaxDistanceFromCenter = 20;


        public DimVeinZoneCreator(ref GeneratorContainer contInst) : base(ref contInst)
        {
            this.historyWiggleDisplacementRange = this.wiggleDisplacementRange;
        }

        void init(float squareAreaMaxArea, int squareAreaFillMinSideLength)
        {
            this.wiggleDisplacementRange = (int)Mathf.Sqrt(squareAreaMaxArea) - 3;
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

            //Debug.Log

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
                        if (expandAxisFailed(coordsToCheck) == true)
                        {
                            yMinLocked = true;
                            minCoords.incY();
                        }
                    }

                    if (!yMaxLocked)
                    {
                        CoordsInt coordsToCheck = new CoordsInt(x, maxCoords.getY());

                        // If there's a vein then don't expand the bounds
                        if (expandAxisFailed(coordsToCheck) == true)
                        {
                            yMaxLocked = true;
                             maxCoords.decY();
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
                        if (expandAxisFailed(coordsToCheck) == true)
                        {
                            xMinLocked = true;
                            minCoords.incX();
                        }
                    }

                    if (!xMaxLocked)
                    {
                        CoordsInt coordsToCheck = new CoordsInt(maxCoords.getX(), y);

                        // If there's a vein then don't expand the bounds
                        if (expandAxisFailed(coordsToCheck) == true)
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

        protected override bool expandAxisFailed(CoordsInt coordsToCheck)
        {
            bool axisCheckFailed = false;

            if (coordAreInsideAllocatedBounds(coordsToCheck) == false)
                axisCheckFailed = true;
            else
            {
                if (tileIsVein(coordsToCheck) == true || coordDistanceToCenterCheck(coordsToCheck) == false)
                    axisCheckFailed = true;
            }

            return axisCheckFailed;
        }

        // =======================================================================================
        //                                  Start Functions
        // =======================================================================================

        public DimensionList getDimensionsForVeinZone(CoordsInt startCoords, bool debugMode, DirectionBias directionBias, out TwoDList<Tile> tileMapRef)
        {
            init(defualtSquareAreaMaxArea, defualtSquareAreaFillMinSideLength);
            setDimensionVariables(defualtSquareAreaFillMinSideLength, defaultVeinZoneMaxArea, defualtSquareAreaMaxArea, directionBias, this.defaultTopOffDimList, getTileMapDims(), defaultMaxDistanceFromCenter);

            //directionBias.print();

            DimensionList newDimList = getDimensions(startCoords);
            tileMapRef = this.tileMapRef;

            if (debugMode)
                markSelectedGridForDebug(newDimList);

            return newDimList;
        }

        // Will return a dimlist within a restricted dimension
        //      Originally meant to be used for finding free space (non vein space) for zone vein generation
        public DimensionList getDimensionsInRestrictedTileArea(CoordsInt startCoords, bool debugMode, DirectionBias directionBias, Dimensions restrictedDims, int maxTotalArea)
        {
            init(restrictedIndividualSquareMax, restrictedSquareAreaFillMinSideLength);
            setDimensionVariables(restrictedSquareAreaFillMinSideLength, maxTotalArea, restrictedIndividualSquareMax, directionBias, this.restrictedTopOffDimList, restrictedDims, restrictedMaxDistanceFromCenter);

            //directionBias.print();

            DimensionList newDimList = getDimensions(startCoords);

            if (debugMode)
                markSelectedGridForDebug(newDimList);

            return newDimList;
        }

        // New free space finder
        public DimensionList getDimensionsUsingAlternateTileMap(CoordsInt startCoords, bool debugMode, DirectionBias directionBias, Dimensions restrictedDims, int maxTotalArea, ref TwoDList<Tile> refTileMap)
        {
            int individualSquareMax = 4;
            int minSideLength = 4;
            int tempMaxTotalArea = 4;
            int maxDistanceFromCenter = 10;

            init(individualSquareMax, minSideLength);
            setDimensionVariables(minSideLength, tempMaxTotalArea, individualSquareMax, directionBias, this.restrictedTopOffDimList, restrictedDims, maxDistanceFromCenter);
            setAlternateTileMap(ref refTileMap);

            //directionBias.print();

            DimensionList newDimList = getDimensions(startCoords);

            if (debugMode)
                markSelectedGridForDebug(newDimList);

            return newDimList;
        }
    }
}
