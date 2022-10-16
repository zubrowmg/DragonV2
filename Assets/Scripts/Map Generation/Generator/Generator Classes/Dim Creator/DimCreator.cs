using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;
using CommonlyUsedClasses;
using TileManagerClasses;


public abstract class DimCreator : TileAccessor
{
    // Class is meant to be used for capturing a collection of square areas

    // Square Area control variables
    protected float maxSqaureAreaArea;
    protected int minSideLength;
    protected int maxArea;

    // Adjacent coords search variables
    //      Don't make this too large, will lead to spread apart square areas and barely touching square areas
    protected int maxAdjacentSearchDisplacement;

    // Wiggle Conditions
    protected int wiggleDisplacementRange;

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

    protected abstract void setDimensionVariables(int minSideLength, int maxArea, float individualMaxSquareArea);

    protected abstract void expandAroundPoint(ref CoordsInt minCoords, ref CoordsInt maxCoords);

    protected abstract bool wiggleConditions(CoordsInt wiggledCoords);

    // =======================================================================================
    //                                  Main Functions
    // =======================================================================================

    // Should not be calling this function directly, look at getDimensionsForRoom() as an example
    protected DimensionList getDimensions(CoordsInt startCoords)
    {
        DimensionList dimensionList = new DimensionList();
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


        while (dimensionList.area < maxArea)
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
                dimensionRejected = dimensionList.addDimension(new SquareArea(minCoords, maxCoords, center));

                if (dimensionRejected == false)
                {
                    // Search for more dimensions and add them to coordsToCheck
                    findAdjacentStartPoints(dimensionList, center, ref coordsToCheck, startCoords);

                }
            }
        }

        // Need to do a final check to make sure that there aren't any square areas in the dim list that are touching by a 2 wide unit

        dimensionList.finalCheck();

        return dimensionList;
    }

    void findAdjacentStartPoints(DimensionList dimensionList, CoordsInt center, ref LinkedList<CoordsInt> coordsToCheck, CoordsInt startCoords)
    {
        int startDisplacement = this.maxAdjacentSearchDisplacement;

        int x = 0;
        int y = 0;

        bool foundNewPoint = false;

        // Four for each direction
        for (int i = 0; i < 4; i++)
        {
            foundNewPoint = false;
            int displacement = startDisplacement;

            if (i == 0)
            {
                x = center.getX() + displacement;
                y = center.getY();
            }
            else if (i == 1)
            {
                x = center.getX() - displacement;
                y = center.getY();
            }
            else if (i == 2)
            {
                x = center.getX();
                y = center.getY() + displacement;
            }
            else if (i == 3)
            {
                x = center.getX();
                y = center.getY() - displacement;
            }

            while (!foundNewPoint)
            {
                //print("DISPLACEMENT: " + x + "," + y);
                bool tooCloseToPreviouslyAttemptedSquareCore = false;
                CoordsInt wiggledCoords = new CoordsInt(x, y);
                foundNewPoint = checkDisplacentAndWiggle(ref tooCloseToPreviouslyAttemptedSquareCore, dimensionList, ref coordsToCheck, wiggledCoords, startCoords);
                if (tooCloseToPreviouslyAttemptedSquareCore)
                    break;

                // If not change the displacement so that it's closer to the original point
                if (foundNewPoint == false)
                {
                    displacement--;

                    if (i == 0)
                    {
                        x = center.getX() + displacement;
                    }
                    else if (i == 1)
                    {
                        x = center.getX() - displacement;
                    }
                    else if (i == 2)
                    {
                        y = center.getY() + displacement;
                    }
                    else if (i == 3)
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
    }

    bool checkDisplacentAndWiggle(ref bool tooCloseToPreviouslyAttemptedSquareCore, DimensionList dimensionList, ref LinkedList<CoordsInt> coordsToCheck, CoordsInt wiggledCoords, CoordsInt startCoords)
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
                addCoordsToList(ref coordsToCheck, wiggledCoords, startCoords);
                foundNewPoint = true;
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
                Coords<int> tileCoords = new Coords<int>(x + startCoords.getX(), y + startCoords.getY());
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

}
