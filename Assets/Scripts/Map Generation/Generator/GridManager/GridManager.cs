using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;




// If a future class needs to access any grid manager fields, then inherit this interface
//      Classes can inherit from multiple interfaces
interface IntfGridManager
{
    void accessGridManagerGrid();
}

public partial class GridManager
{
    GameObject grid;
    GameObject exampleTile = Singleton.instantiateTile();

    bool generateTileGameObjects;

    // Grid Properties
    static Coords<float> startCoords = new Coords<float>(-.3f, 0f);
    static Coords<int> minDim = new Coords<int>(0, 0);
    static Coords<int> maxDim = new Coords<int>(147*2*2, 72*2*2);

    static Dimensions gridDimensions = new Dimensions(minDim, maxDim);

    ~GridManager() { }

    public GridManager(bool generateTiles)
    {
        this.generateTileGameObjects = generateTiles;
    }

    public void createGrid()
    {
        float tileHeight = exampleTile.GetComponent<SpriteRenderer>().bounds.size.x;
        float tileWidth = exampleTile.GetComponent<SpriteRenderer>().bounds.size.y;

        for (int x = 0; x < gridDimensions.getMaxX(); x++)
        {
            for (int y = 0; y < gridDimensions.getMaxY(); y++)
            {
                // Get new position
                Coords<float> newPos = 
                    new Coords<float> (((startCoords.getX() + x) * tileWidth) + (tileWidth / 2),
                                       ((startCoords.getY() + y) * tileHeight) + (tileHeight / 2));

                // Create tile
            }
        }
    }
}

// Wrapper function
public partial class GridManager
{
    public void accessGrid()
    {
        Debug.Log("accessGrid");
    }
}


public partial class GridManager
{
    // ------------------ Getters ------------------
    // ------------------ Setters ------------------
}
