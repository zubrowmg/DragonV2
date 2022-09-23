using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;


// Tile Manager will create the tile map
public partial class TileManager : ContainerAccessor
{
    GameObject grid;
    GameObject exampleTile = Singleton.instantiateTile();

    bool generateTileGameObjects;

    // Grid Properties
    static Coords<float> startCoords = new Coords<float>(-.3f, 0f);
    static Coords<int> minDim = new Coords<int>(0, 0);
    static Coords<int> maxDim = new Coords<int>(147*2*2, 72*2*2);

    static Dimensions tileMapDimensions = new Dimensions(minDim, maxDim);

    ~TileManager() { }

    public TileManager(bool generateTiles, ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.generateTileGameObjects = generateTiles;
    }


    public void createTileMap()
    {
        float tileHeight = exampleTile.GetComponent<SpriteRenderer>().bounds.size.x;
        float tileWidth = exampleTile.GetComponent<SpriteRenderer>().bounds.size.y;

        for (int x = 0; x < tileMapDimensions.getMaxX(); x++)
        {
            for (int y = 0; y < tileMapDimensions.getMaxY(); y++)
            {
                // Get new position
                Coords<float> newPos = 
                    new Coords<float> (((startCoords.getX() + x) * tileWidth) + (tileWidth / 2),
                                       ((startCoords.getY() + y) * tileHeight) + (tileHeight / 2));

                // Create tile
            }
        }


    }

    public void test0()
    {
        List<int> test0 = new List<int> { 1, 2, 5 };
        setTileMap(test0);
    }

    public void test2()
    {
        List<int> test2 = getTileMap();

        foreach (var i in test2)
        {
            Debug.Log(i);
        }
    }
}


public partial class TileManager : ContainerAccessor
{
    // ------------------ Getters ------------------
    // ------------------ Setters ------------------
}
