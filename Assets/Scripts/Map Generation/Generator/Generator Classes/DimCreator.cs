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

    void getDimensions(int xStart, int yStart, ref DimensionList dimensionList)
    {
        bool dimensionRejected = false;

        int xMin = xStart;
        int yMin = yStart;
        int xMax = xStart;
        int yMax = yStart;

        if (grid.GetComponent<gridManagerScript>().grid[xStart, yStart].GetComponent<gridUnitScript>().isOccupied == true)
        {
            // To speed up generation, if the bookmark isOccupied then return immediatly
            return;
        }

        Coords center = new Coords(xStart, yStart);

        LinkedList<Coords> coordsToCheck = new LinkedList<Coords>();
        coordsToCheck.AddFirst(new Coords(xStart, yStart));
        Coords currentCoords = coordsToCheck.First.Value;

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

            xMin = currentCoords.x;
            yMin = currentCoords.y;
            xMax = currentCoords.x;
            yMax = currentCoords.y;

            //    print("CHECKING:     " + xMin + "," + yMin);

            center.x = currentCoords.x;
            center.y = currentCoords.y;

            expandAroundPoint(ref xMin, ref yMin, ref xMax, ref yMax);

            // If any of the sides a shorter than 5 then reject the square
            if (xMax - xMin < minSideLength - 1 || yMax - yMin < minSideLength - 1)
            {
                // Don't add anything
            }
            else
            {
                dimensionRejected = dimensionList.addDimension(new SquareArea(xMin, yMin, xMax, yMax, center.x, center.y));

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
