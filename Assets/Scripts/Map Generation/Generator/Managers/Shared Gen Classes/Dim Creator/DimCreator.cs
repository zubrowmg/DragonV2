using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using System; // For using Enum (capital E)
using CommonlyUsedFunctions;
using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedDefinesAndEnums;

namespace DimCreatorClasses
{
    public abstract class DimCreator : TileAccessor
    {
        protected bool debuging = false; // Temporary, eventually to be removed

        // Class is meant to be used for capturing a collection of square areas
        DimensionList dimensionList;

        // Top off dim list is a way to fill in gaps in the dim list, without further expanding the dim list
        //      Pretty much only used for zone vein generation
        bool topOffDimList = false;

        // Square Area control variables
        protected float maxSqaureAreaArea;
        protected int minSideLength;
        protected int maxArea;

        // Adjacent coords search variables
        //      Don't make this too large, will lead to spread apart square areas and barely touching square areas
        protected int maxAdjacentSearchDisplacement;

        // Wiggle Conditions
        protected int wiggleDisplacementRange;

        // Tile Map Reference
        protected TwoDList<Tile> tileMapRef = new TwoDList<Tile>();

        // Dim Direction Bias
        protected DirectionBias directionBias = new DirectionBias(Direction.None, Direction.None);

        // History coords queue
        QueueWrapper<CoordsInt> recentlyAddedCoordsToCheck;
        int recentlyAddedCoordsQueueSize = 40;
        protected int historyWiggleDisplacementRange;

        // Allocated tile map dims and distance control
        protected Dimensions allocatedTileMapDims;
        protected int maxDistanceFromCenter = System.Int32.MaxValue;

        // Alternate coords system
        //      Needs a 2d list of Tiles
        TwoDList<Tile> alternateTileMap = new TwoDList<Tile>();
        bool alternateTileMapIsUsed = false;

        // Granularity just means that individual square areas can only be touching by a 1 wide section
        //      Not meant to be overly used, meant for small search areas 20x20. Else it will take a while
        bool isGranular = false;

        public DimCreator(ref GeneratorContainer contInst) : base(ref contInst)
        {
            resetDimCreator();   
        }

        // =======================================================================================
        //                                  Abstract Functions
        // =======================================================================================

        protected void resetDimCreator()
        {
            this.tileMapRef = new TwoDList<Tile>();
            this.directionBias = new DirectionBias(Direction.None, Direction.None);
            this.recentlyAddedCoordsToCheck = new QueueWrapper<CoordsInt>(recentlyAddedCoordsQueueSize);

            this.maxDistanceFromCenter = System.Int32.MaxValue;

            this.alternateTileMap = new TwoDList<Tile>();
            this.alternateTileMapIsUsed = false;
            this.isGranular = false;
        }

        protected abstract bool tileCheck(CoordsInt coords);

        protected bool tileIsOccupiedByRoom(CoordsInt coords)
        {
            bool tileContainsRoom = false;
            Tile selectedTile = getTileTranslated(coords);

            if (selectedTile != null)
                tileContainsRoom = selectedTile.getIsOccupiedByRoom();

            return tileContainsRoom;
        }

        protected bool tileIsVein(CoordsInt coords)
        {
            bool tileIsVein = false;
            Tile selectedTile = getTileTranslated(coords);

            if (selectedTile != null)
                tileIsVein = selectedTile.getIsVein();

            return tileIsVein;
        }

        protected abstract void expandAroundPoint(ref CoordsInt minCoords, ref CoordsInt maxCoords);

        protected abstract bool wiggleConditions(CoordsInt wiggledCoords);

        protected void setAlternateTileMap(ref TwoDList<Tile> altTileMap)
        {
            this.alternateTileMapIsUsed = true;
            this.alternateTileMap = altTileMap;

            CoordsInt minCoords = new CoordsInt(0, 0);
            CoordsInt maxCoords = new CoordsInt(altTileMap.getXCount() - 1, altTileMap.getYCount() - 1);
            this.allocatedTileMapDims = new Dimensions(minCoords, maxCoords);
        }

        protected abstract bool expandAxisFailed(CoordsInt coordsToCheck);

        // =======================================================================================
        //                                  Main Functions
        // =======================================================================================

        // Should not be calling this function directly, look at getDimensionsForRoom() as an example
        protected DimensionList getDimensions(CoordsInt startCoords)
        {
            int topOffCount = 0;
            int topOffMax = 10;

            this.tileMapRef = new TwoDList<Tile>();
            this.dimensionList = new DimensionList(startCoords);
            bool dimensionRejected = false;

            CoordsInt minCoords = startCoords.deepCopyInt();
            CoordsInt maxCoords = startCoords.deepCopyInt();
            CoordsInt center = startCoords.deepCopyInt();

            // To speed up generation,
            //      For vein dim creator this checks is the tile already has a room placed in it
            //      For vein zone dim creator this checks if the tile is already a vein
            if (tileCheck(startCoords) == true)
                return dimensionList;


            LinkedList<CoordsInt> coordsToCheck = new LinkedList<CoordsInt>();
            coordsToCheck.AddFirst(startCoords.deepCopyInt());
            CoordsInt currentCoords = coordsToCheck.First.Value;


            bool done = false;
            bool getDimsToTopOffDimList = false;

            //startCoords.print("===========================================\nSTART COORDS: ");

            while (done == false)
            {
                //Debug.Log("TOP___WHILE");

                if (coordsToCheck.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCoords = coordsToCheck.First.Value;
                    coordsToCheck.RemoveFirst();
                }

                minCoords = currentCoords.deepCopyInt();
                maxCoords = currentCoords.deepCopyInt();
                center = currentCoords.deepCopyInt();

                //if (debuging)
                //    currentCoords.print("==================================================\nCURRRENT COORDS: ");

                expandAroundPoint(ref minCoords, ref maxCoords);

                //if (debuging)
                //{
                //    minCoords.print("MIN COORDS: ");
                //    maxCoords.print("MAX COORDS: ");
                //    center.print("CENTER COORDS: ");
                //}

                // If any of the sides a shorter than min length then reject the square
                if (sideIsShorterThanMinLength(Axis.x_Axis, minCoords, maxCoords) || sideIsShorterThanMinLength(Axis.y_Axis, minCoords, maxCoords))
                {
                    // Don't add anything
                }
                else
                {
                    if (getDimsToTopOffDimList == false)
                    {
                        dimensionRejected = dimensionList.addDimension(new SquareArea(minCoords, maxCoords, center));
                        //if (debuging && dimensionRejected)
                        //    Debug.Log("DIM REJECTED");
                    }
                    else
                    {
                        //dimensionRejected = dimensionList.addDimension(new SquareArea(minCoords, maxCoords, center));
                        dimensionRejected = dimensionList.addDimensionWithOutExpandingDims(new SquareArea(minCoords, maxCoords, center));
                    }

                    if (dimensionRejected == false)
                    {
                        // Search for more dimensions and add them to coordsToCheck
                        findAdjacentStartPoints(dimensionList, center, ref coordsToCheck, startCoords, getDimsToTopOffDimList);
                    }
                }

                // Break out conditions
                if (dimensionList.getArea() >= maxArea)
                {
                    // Top off dim list, this is used to make dim areas that are not rectangular into a rectangular shape
                    //      Ex: 
                    //          ---------       ---------
                    //          | xxxxx |       | xxxxx |
                    //          |    xx |   =>  | xxxxx |
                    //          |    xx |       | xxxxx |
                    //          ---------       ---------
                    if (this.topOffDimList == true)
                    {
                        if (coordsToCheck.Count == 0)
                            topOffCount++;
                        else
                            topOffCount = 0;

                        if (topOffCount >= topOffMax)
                            done = true;

                        getDimsToTopOffDimList = true;
                    }
                    else
                    {
                        //Debug.Log("\nDONE FOR REAL\n");
                        done = true;
                    }
                }
            }

            // Need to do a final check to make sure that there aren't any square areas in the dim list that are NOT touching by a 2 wide unit
            //      Does not apply to granular (1 wide obviously), there should be no gaps as addDimension() checks for that
            bool dimensionListIsAcceptable = true;
            if (isGranular == false)
                dimensionListIsAcceptable = dimensionList.finalCheck();

            if (dimensionListIsAcceptable == true)
                fillTileMap(dimensionList);

            return dimensionList;
        }

        void fillTileMap(DimensionList dimList)
        {
            dimList.getGrid(out TwoDList<int> grid, out CoordsInt startCoords);
            for (int x = 0; x < grid.getXCount(); x++)
            {
                for (int y = 0; y < grid.getYCount(); y++)
                {
                    CoordsInt tileRefCoords = new CoordsInt(x, y);

                    CoordsInt tileCoords = null;
                    if (this.alternateTileMapIsUsed == false)
                        tileCoords = new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                    else
                        tileCoords = new CoordsInt(x, y);

                    Tile currentTile = getTileTranslated(tileCoords);

                    if (currentTile != null)
                    {
                        this.tileMapRef.addRefElement(tileRefCoords, ref currentTile);
                    }

                }
            }

        }

        bool findAdjacentStartPoints(DimensionList dimensionList, CoordsInt center, ref LinkedList<CoordsInt> coordsToCheck,
                                     CoordsInt startCoords, bool getDimsToTopOffDimList)
        {
            int startDisplacement = this.maxAdjacentSearchDisplacement;

            int x = 0;
            int y = 0;

            bool foundNewPoint = false;

            // Four for each direction

            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                foundNewPoint = false;
                int displacement = startDisplacement;

                //if (debuging)
                //    Debug.Log("======== DIRECTION: " + dir + " ========");

                if (dir == Direction.North)
                {
                    x = center.getX();
                    y = center.getY() + displacement;
                }
                else if (dir == Direction.East)
                {
                    x = center.getX() + displacement;
                    y = center.getY();
                }
                else if (dir == Direction.South)
                {
                    x = center.getX();
                    y = center.getY() - displacement;
                }
                else if (dir == Direction.West)
                {
                    x = center.getX() - displacement;
                    y = center.getY();
                }
                else
                    continue;

                // Control the direction based on the Direction Bias passed in
                if (directionCheck(startCoords, dir) == false)
                    continue;

                int extraGranularJiggleCount = -1;
                while (!foundNewPoint)
                {
                    // If not change the displacement so that it's closer to the original point
                    if (foundNewPoint == false)
                    {
                        // Need to do some additional coord jiggling for very granular reads
                        if (this.isGranular == true)
                        {
                            extraGranularJiggleCount++;
                            if (extraGranularJiggleCount >= 3)
                            {
                                extraGranularJiggleCount = 0;
                                displacement--;
                            }

                            if (dir == Direction.East || dir == Direction.West)
                            {
                                if (extraGranularJiggleCount == 0)
                                    y = center.getY();
                                else if(extraGranularJiggleCount == 1)
                                    y = center.getY() + 1;
                                else if (extraGranularJiggleCount == 2)
                                    y = center.getY() - 1;
                            }
                            else if (dir == Direction.North || dir == Direction.South)
                            {
                                if (extraGranularJiggleCount == 0)
                                    x = center.getX();
                                else if(extraGranularJiggleCount == 1)
                                    x = center.getX() + 1;
                                else if (extraGranularJiggleCount == 2)
                                    x = center.getX() - 1;
                            }

                            //if (debuging)
                            //    Debug.Log("JIGGLE COUNT: " + extraGranularJiggleCount);

                            
                        }
                        else
                            displacement--;

                        // Don't get too close or else it's just the same square
                        if (displacement <= 6 && this.isGranular == false)
                            break;
                        else if (displacement <= 1 && this.isGranular == true)
                            break;

                        if (dir == Direction.East)
                            x = center.getX() + displacement;
                        else if (dir == Direction.West)
                            x = center.getX() - displacement;
                        else if (dir == Direction.North)
                            y = center.getY() + displacement;
                        else if (dir == Direction.South)
                            y = center.getY() - displacement;

                        

                    }

                    CoordsInt wiggledCoords = new CoordsInt(x, y);

                    foundNewPoint = checkDisplacentAndWiggle(out bool tooCloseToPreviouslyAttemptedSquareCore,
                                        dimensionList, ref coordsToCheck, wiggledCoords, startCoords, getDimsToTopOffDimList);
                    if (tooCloseToPreviouslyAttemptedSquareCore)
                        break;
                }
            }

            return foundNewPoint;
        }

        bool directionCheck(CoordsInt startCoords, Direction attemptedDir)
        {
            bool allowDirectionExpansion = false;

            if ((directionBias.getHorizontalDir() == Direction.None && attemptedDir == Direction.East) ||
                (directionBias.getHorizontalDir() == Direction.None && attemptedDir == Direction.West))
            {
                allowDirectionExpansion = true;
            }

            else if ((directionBias.getVerticalDir() == Direction.None && attemptedDir == Direction.North) ||
                     (directionBias.getVerticalDir() == Direction.None && attemptedDir == Direction.South))
            {
                allowDirectionExpansion = true;

            }

            else if (attemptedDir == directionBias.getHorizontalDir() ||
                attemptedDir == directionBias.getVerticalDir())
            {
                allowDirectionExpansion = true;
            }

            return allowDirectionExpansion;
        }


        bool checkDisplacentAndWiggle(out bool tooCloseToPreviouslyAttemptedSquareCore, DimensionList dimensionList,
                            ref LinkedList<CoordsInt> coordsToCheck, CoordsInt wiggledCoords, CoordsInt startCoords, bool getDimsToTopOffDimList)
        {
            tooCloseToPreviouslyAttemptedSquareCore = false;
            bool foundNewPoint = false;

            //if (debuging)
            //    wiggledCoords.print("WIGGLED COORDS: ");

            // If the grid is a vein and is not occupied and the point is not already added then add the point
            //    pointAlreadyChecked() is needed to avoid an infinite loop when 2 "gaps" between squares is deemed addable
            //        The dimension list will reject the square gaps, but 

            if (coordAreInsideAllocatedBounds(wiggledCoords) == false)
                foundNewPoint = false;
            else if (wiggleConditions(wiggledCoords) == true)
            {
                if (coordDistanceToCenterCheck(wiggledCoords) == false)
                    foundNewPoint = false;
                if (dimensionList.pointTooCloseToPreviouslyAttemptedSquareCore(wiggledCoords, wiggleDisplacementRange) == true)
                {
                    //if (debuging)
                    //    Debug.Log("REJECT__0");
                    tooCloseToPreviouslyAttemptedSquareCore = true;
                }
                else if (coordsTooCloseToListHistoryBuffer(wiggledCoords) == true)
                {
                    //if (debuging)
                    //    Debug.Log("REJECT__1");
                    tooCloseToPreviouslyAttemptedSquareCore = true;
                }
                else
                {
                    if (getDimsToTopOffDimList == true &&
                         dimensionList.startCoordsAreOutsideOfCurrentDimList(wiggledCoords) == true)
                    {
                        //startCoordsAreOutsideOfCurrentDimList = true;
                        // Don't add the coords as a point of interest
                    }
                    else
                    {
                        //if (debuging)
                        //    Debug.Log("\tADDED");
                        addCoordsToList(ref coordsToCheck, wiggledCoords, startCoords);
                        foundNewPoint = true;
                    }
                }
            }

            return foundNewPoint;
        }

        // Will check the coords list history buffer to make sure that the wiggled coords are not too close to any in the list
        bool coordsTooCloseToListHistoryBuffer(CoordsInt wiggledCoords)
        {
            bool tooClose = false;

            for (int i = 0; i < recentlyAddedCoordsToCheck.getCount(); i++)
            {
                CoordsInt currentCoords = recentlyAddedCoordsToCheck.getElement(i);

                //if (debuging)
                //    currentCoords.print("\tALREADY ADDED COORD: ");

                if (currentCoords.getX() - historyWiggleDisplacementRange <= wiggledCoords.getX() && wiggledCoords.getX() <= currentCoords.getX() + historyWiggleDisplacementRange &&
                    currentCoords.getY() - historyWiggleDisplacementRange <= wiggledCoords.getY() && wiggledCoords.getY() <= currentCoords.getY() + historyWiggleDisplacementRange)
                {
                    tooClose = true;
                    break;
                }
            }

            return tooClose;
        }

        void addCoordsToList(ref LinkedList<CoordsInt> coordsToCheck, CoordsInt wiggledCoords, CoordsInt startCoords)
        {
            // Need to add the coords into the list ordered by how far they are from the starting coords

            float newDistance = Mathf.Sqrt(
                                    Mathf.Pow(startCoords.getX() - wiggledCoords.getX(), 2) +
                                    Mathf.Pow(startCoords.getY() - wiggledCoords.getY(), 2));

            recentlyAddedCoordsToCheck.enqueue(wiggledCoords);

            if (coordsToCheck.Count == 0)
                coordsToCheck.AddLast(wiggledCoords.deepCopyInt());
            else
            {
                for (LinkedListNode<CoordsInt> node = coordsToCheck.First; node != null; node = node.Next)
                {
                    float currentDistance = CommonFunctions.calculateCoordsDistance(wiggledCoords, startCoords);// Mathf.Sqrt(Mathf.Pow(xStart - node.Value.x, 2) + Mathf.Pow(yStart - node.Value.y, 2));

                    // If we get to the end of the list
                    if (node.Equals(coordsToCheck.Last))
                    {
                        if (newDistance < currentDistance)
                            coordsToCheck.AddBefore(node, wiggledCoords.deepCopyInt());
                        else
                            coordsToCheck.AddAfter(node, wiggledCoords.deepCopyInt());

                        break;
                    }
                    // Add it to the list ordered by distance
                    else if (newDistance < currentDistance)
                    {
                        coordsToCheck.AddBefore(node, wiggledCoords.deepCopyInt());

                        break;
                    }
                }
            }

            //coordsToCheck.AddLast(new Coords(x, y));
        }

        protected void markSelectedGridForDebug(DimensionList dimList)
        {
            dimList.getGrid(out TwoDList<int> grid, out CoordsInt startCoords);

            for (int x = 0; x < grid.getXCount(); x++)
            {
                for (int y = 0; y < grid.getYCount(); y++)
                {
                    CoordsInt tileCoords = null; // new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                    if (this.alternateTileMapIsUsed == false)
                        tileCoords = new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                    else
                        tileCoords = new CoordsInt(x, y);


                    Tile currentTile = getTileTranslated(tileCoords);

                    if (tileCoords != null)
                    {
                        currentTile.instantiateTileGameObject();
                    }
                }
            }
        }

        // Normally the boundries are going to be the entire tile map
        //      But in the case that we want to look for space in a restricted area we need to check
        protected bool coordAreInsideAllocatedBounds(CoordsInt coords)
        {
            bool isInsideBounds = false;
            if (allocatedTileMapDims.getMinX() <= coords.getX() && coords.getX() <= allocatedTileMapDims.getMaxX() &&
                allocatedTileMapDims.getMinY() <= coords.getY() && coords.getY() <= allocatedTileMapDims.getMaxY())
                isInsideBounds = true;
            return isInsideBounds;
        }

        protected bool coordDistanceToCenterCheck(CoordsInt coords)
        {
            bool isInsideRadius = false;

            // If the dim list is empty then the dim list hasn't defined a center yet
            if (this.dimensionList.getDimCount() == 0)
                isInsideRadius = true;
            else if (CommonFunctions.calculateCoordsDistance(this.dimensionList.getCenterCoord(), coords) < (float)maxDistanceFromCenter)
                isInsideRadius = true;
            return isInsideRadius;
        }

        protected Tile getTileTranslated(CoordsInt coords)
        {
            // Use this function when grabbing any tile inside of this DimCreator class
            //      Use default will use the coords as they are aka maps directly to world coords
            //      Non default means that you supplied a two d list and the coords provided map to that two d list
            //          Therefore you need to translate the coords before grabbing them
            Tile currentTile = null;

            if (this.alternateTileMapIsUsed == false)
            {
                CoordsInt tileCoords = new CoordsInt(coords.getX(), coords.getY());
                currentTile = getTile(tileCoords, out bool accessSuccesful);
            }
            else
            {
                currentTile = this.alternateTileMap.getElement(coords);
            }

            return currentTile;
        }

        protected bool sideIsShorterThanMinLength(Axis axis, CoordsInt minCoords, CoordsInt maxCoords)
        {
            bool sideIsShorter = false;
            int diff = 0;

            switch (axis)
            {
                case Axis.x_Axis:
                    diff = maxCoords.getX() - minCoords.getX() + 1;
                    break;

                case Axis.y_Axis:
                    diff = maxCoords.getY() - minCoords.getY() + 1;
                    break;
            }

            if (diff < minSideLength)
                sideIsShorter = true;

            return sideIsShorter;
        }

        // =======================================================================================
        //                                  Setters/Getters
        // =======================================================================================

        protected void setDimensionVariables(int minSideLength, int maxArea, float individualMaxSquareArea, DirectionBias directionBias, bool topOffDimList, Dimensions allocatedTileMapDims, int maxDistanceFromCenter)
        {
            this.minSideLength = minSideLength;

            if (this.minSideLength == 1)
                this.isGranular = true;

            this.maxSqaureAreaArea = individualMaxSquareArea;
            this.maxArea = maxArea;
            this.directionBias = directionBias;

            this.topOffDimList = topOffDimList;
            this.allocatedTileMapDims = allocatedTileMapDims;
            this.maxDistanceFromCenter = maxDistanceFromCenter;

            this.alternateTileMapIsUsed = false;
        }
    }
}
