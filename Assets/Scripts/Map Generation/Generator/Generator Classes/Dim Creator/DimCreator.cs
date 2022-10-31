using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System; // For using Enum (capital E)
using CommonlyUsedFunctions;
using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedDefinesAndEnums;

public abstract class DimCreator : TileAccessor
{
    // Class is meant to be used for capturing a collection of square areas

    // Top off dim list is a way to fill in the rest of the dim list, without further expanding the dim list
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

    public DimCreator(ref GeneratorContainer contInst) : base (ref contInst)
    {
    }

    // =======================================================================================
    //                                  Abstract Functions
    // =======================================================================================

    protected abstract bool tileCheck(CoordsInt coords);

    protected bool tileIsOccupiedByRoom(CoordsInt coords)
    {
        bool tileContainsRoom = false;
        bool accessSuccessful = false;
        Tile selectedTile = getTile(coords, ref accessSuccessful);

        if (accessSuccessful)
            tileContainsRoom = selectedTile.getIsOccupiedByRoom();

        return tileContainsRoom;
    }

    protected bool tileIsVein(CoordsInt coords)
    {
        bool tileIsVein = false;
        bool accessSuccessful = false;
        Tile selectedTile = getTile(coords, ref accessSuccessful);

        if (accessSuccessful)
            tileIsVein = selectedTile.getIsVein();

        return tileIsVein;
    }

    protected abstract void expandAroundPoint(ref CoordsInt minCoords, ref CoordsInt maxCoords);

    protected abstract bool wiggleConditions(CoordsInt wiggledCoords);


    // =======================================================================================
    //                                  Main Functions
    // =======================================================================================

    // Should not be calling this function directly, look at getDimensionsForRoom() as an example
    protected DimensionList getDimensions(CoordsInt startCoords)
    {
        int topOffCount = 0;
        int topOffMax = 10;

        this.tileMapRef = new TwoDList<Tile>();
        DimensionList dimensionList = new DimensionList(startCoords);
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

        while (done == false)
        {
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

            expandAroundPoint(ref minCoords, ref maxCoords);

            // If any of the sides a shorter than min length then reject the square
            if (maxCoords.getX() - minCoords.getX() < minSideLength - 1 || maxCoords.getY() - minCoords.getY() < minSideLength - 1)
            {
                // Don't add anything
            }
            else
            {
                if (getDimsToTopOffDimList == false)
                {
                    dimensionRejected = dimensionList.addDimension(new SquareArea(minCoords, maxCoords, center));
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
            if (dimensionList.area >= maxArea)
            {
                

                if (this.topOffDimList == true)
                {
                    if (coordsToCheck.Count % 50 == 0)
                    {
                        Debug.Log("COUNT: " + coordsToCheck.Count + "\n" +
                              "TOP OFF COUNT: " + topOffCount);
                    }
                    

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

        // Need to do a final check to make sure that there aren't any square areas in the dim list that are touching by a 2 wide unit
        bool dimensionListIsAcceptable = dimensionList.finalCheck();
        if (dimensionListIsAcceptable == true)
            fillTileMap(dimensionList);



        return dimensionList;
    }

    void fillTileMap(DimensionList dimList)
    {
        List<List<int>> grid;
        Coords<int> startCoords;
        dimList.getGrid(out grid, out startCoords);
        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[0].Count; y++)
            {
                bool accessSuccesful = false;
                CoordsInt tileCoords = new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                Tile currentTile = getTile(tileCoords, ref accessSuccesful);

                CoordsInt tileRefCoords = new CoordsInt(x, y);

                if (accessSuccesful == true)
                {
                    //currentTile.instantiateTileGameObject();
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

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            foundNewPoint = false;
            int displacement = startDisplacement;


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


            while (!foundNewPoint)
            {
                //print("DISPLACEMENT: " + x + "," + y);
                bool tooCloseToPreviouslyAttemptedSquareCore = false;

                CoordsInt wiggledCoords = new CoordsInt(x, y);
                foundNewPoint = checkDisplacentAndWiggle(ref tooCloseToPreviouslyAttemptedSquareCore, 
                                    dimensionList, ref coordsToCheck, wiggledCoords, startCoords, getDimsToTopOffDimList);
                if (tooCloseToPreviouslyAttemptedSquareCore)
                    break;

                

                // If not change the displacement so that it's closer to the original point
                if (foundNewPoint == false)
                {
                    displacement--;

                    if (dir == Direction.East)
                    {
                        x = center.getX() + displacement;
                    }
                    else if (dir == Direction.West)
                    {
                        x = center.getX() - displacement;
                    }
                    else if (dir == Direction.North)
                    {
                        y = center.getY() + displacement;
                    }
                    else if (dir == Direction.South)
                    {
                        y = center.getY() - displacement;
                    }

                    if (displacement <= 6)
                    {
                        // Don't get too close or else it's just the same square
                        break;
                    }
                }
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


    bool checkDisplacentAndWiggle(ref bool tooCloseToPreviouslyAttemptedSquareCore, DimensionList dimensionList, ref LinkedList<CoordsInt> coordsToCheck, 
                        CoordsInt wiggledCoords, CoordsInt startCoords, bool getDimsToTopOffDimList)
    {
        bool foundNewPoint = false;

        // If the grid is a vein and is not occupied and the point is not already added then add the point
        //    pointAlreadyChecked() is needed to avoid an infinite loop when 2 "gaps" between squares is deemed addable
        //        The dimension list will reject the square gaps, but 
        if (wiggleConditions(wiggledCoords) == true)
        {
            if (dimensionList.pointTooCloseToPreviouslyAttemptedSquareCore(wiggledCoords, wiggleDisplacementRange) == true)
            {
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
                    addCoordsToList(ref coordsToCheck, wiggledCoords, startCoords);
                    foundNewPoint = true;
                }
            }
        }

        return foundNewPoint;
    }

    void addCoordsToList(ref LinkedList<CoordsInt> coordsToCheck, CoordsInt wiggledCoords, CoordsInt startCoords)
    {
        // Need to add the coords into the list ordered by how far they are from the starting coords

        float newDistance = Mathf.Sqrt(
                                Mathf.Pow(startCoords.getX() - wiggledCoords.getX(), 2) + 
                                Mathf.Pow(startCoords.getY() - wiggledCoords.getY(), 2));

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
        List<List<int>> grid;
        Coords<int> startCoords;
        dimList.getGrid(out grid, out startCoords);

        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[0].Count; y++)
            {
                bool accessSuccesful = false;
                CoordsInt tileCoords = new CoordsInt(x + startCoords.getX(), y + startCoords.getY());
                Tile currentTile = getTile(tileCoords, ref accessSuccesful);

                if (accessSuccesful == true)
                {
                    currentTile.instantiateTileGameObject();
                }
            }
        }
    }

    // =======================================================================================
    //                                  Setters/Getters
    // =======================================================================================

    protected void setDimensionVariables(int minSideLength, int maxArea, float individualMaxSquareArea, DirectionBias directionBias, bool topOffDimList)
    {
        this.minSideLength = minSideLength;
        this.maxSqaureAreaArea = individualMaxSquareArea;
        this.maxArea = maxArea;
        this.directionBias = directionBias;

        this.topOffDimList = topOffDimList;
    }
}
