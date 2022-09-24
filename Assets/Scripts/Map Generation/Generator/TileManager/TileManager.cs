using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;


// ==========================================================
//              Tile Manager Accessors
// ==========================================================
public partial class ContainerAccessor
{
    public void addTileToTileMap(Coords<int> index, Tile item)
    {
        contInst.tileMap.addTile(index, item);
    }

    public GameObject getTileManagerGameObject()
    {
        return contInst.tileMap.getTileMapGameObject();
    }

    public void countTileDims()
    {
        contInst.tileMap.countTileDims();
    }

    public Dimensions getTileMapDims()
    {
        return contInst.tileMap.tileMapDimensions;
    }

    public Coords<int> getTileMapCenter()
    {
        return contInst.tileMap.tileMapCenter;
    }
}

// Tile Manager will create the tile map
public partial class TileManager : ContainerAccessor
{
    GameObject exampleTile;

    bool generateTileGameObjects;

    // Tile Map Properties
    Coords<float> tileMapStartCoords = new Coords<float>(-.3f, 0f);
    
    public TileManager(bool generateTiles, ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.generateTileGameObjects = generateTiles;

        this.exampleTile = Singleton.instantiateTile();
        this.exampleTile.transform.position = getGarbageGameObject().transform.position;
        this.exampleTile.transform.SetParent(getGarbageGameObject().transform);
        this.exampleTile.name = "ExampleTile";
    }

    ~TileManager() { }




    public void createTileMap()
    {
        float tileHeight = exampleTile.GetComponent<SpriteRenderer>().bounds.size.x;
        float tileWidth = exampleTile.GetComponent<SpriteRenderer>().bounds.size.y;

        for (int x = 0; x < getTileMapDims().getMaxX(); x++)
        {
            for (int y = 0; y < getTileMapDims().getMaxY(); y++)
            {
                // Get new position
                Coords<float> newWorldCoords = 
                    new Coords<float> (((tileMapStartCoords.getX() + x) * tileWidth) + (tileWidth / 2),
                                       ((tileMapStartCoords.getY() + y) * tileHeight) + (tileHeight / 2));
                Coords<int> newTileMapCoords = new Coords<int>(x, y);

                Tile newTile = new Tile();
                GameObject tileManagerGameObject = getTileManagerGameObject();
                // Create tile
                if (generateTileGameObjects == true)
                {
                    newTile = new Tile(generateTileGameObjects, Singleton.instantiateTile(), tileHeight, tileWidth,
                                        newWorldCoords, newTileMapCoords, ref tileManagerGameObject);
                }
                else
                {
                    newTile = new Tile(generateTileGameObjects, null, tileHeight, tileWidth,
                                        newWorldCoords, newTileMapCoords, ref tileManagerGameObject);
                }

                addTileToTileMap(newTileMapCoords, newTile);
            }
        }
        //countTileDims();
    }
}


public partial class TileManager : ContainerAccessor
{
    // ------------------ Getters ------------------
    // ------------------ Setters ------------------
}


