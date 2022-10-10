using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

public class DimCreator
{
    // Class is meant to be used for capturing a collection of square areas

    // Square Area control variables
    int minSideLength;
    int maxArea;

    Coords<int> startCoords;


    public DimCreator()
    {

    }


    // =======================================================================================
    //                                  Main Functions
    // =======================================================================================

    void getDimensions(Coords<int> startCoords, ref DimensionList dimensionList)
    {
        bool dimensionRejected = false;

        Coords<int> minCoords = startCoords.deepCopy();
        Coords<int> maxCoords = startCoords.deepCopy();
        Coords<int> center = startCoords.deepCopy();

        if (grid.GetComponent<gridManagerScript>().grid[xStart, yStart].GetComponent<gridUnitScript>().isOccupied == true)
        {
            // To speed up generation, if the bookmark isOccupied then return immediatly
            return;
        }


        LinkedList<Coords<int>> coordsToCheck = new LinkedList<Coords<int>>();
        coordsToCheck.AddFirst(startCoords.deepCopy());
        Coords<int> currentCoords = coordsToCheck.First.Value;

        while (dimensionList.area < maxArea)
        {
            //print("TOP OF WHILE");
            if (coordsToCheck.Count == 0)
            {
                break;
            }
            else
            {
                currentCoords = coordsToCheck.First.Value;
                coordsToCheck.RemoveFirst();
            }

            minCoords = currentCoords.deepCopy();
            maxCoords = currentCoords.deepCopy();
            center = currentCoords.deepCopy();

            expandAroundPoint(ref xMin, ref yMin, ref xMax, ref yMax);

            // If any of the sides a shorter than 5 then reject the square
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
                    findAdjacentStartPoints(dimensionList, center, ref coordsToCheck, xStart, yStart);
                }
            }
        }

        // Need to do a final check to make sure that there aren't any square areas in the dim list that are touching by only 1 unit
        dimensionList.finalCheck();
    }

    // =======================================================================================
    //                                  Setters/Getters
    // =======================================================================================
}
