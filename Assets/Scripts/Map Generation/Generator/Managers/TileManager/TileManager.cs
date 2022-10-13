using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;


// ==========================================================
//              Tile Manager Accessors
// ==========================================================
public class TileAccessor
{
    GeneratorContainer contInst;

    public TileAccessor(ref GeneratorContainer contInst)
    {
        this.contInst = contInst;
    }

    public void addTileToTileMap(Coords<int> index, Tile item)
    {
        contInst.tileMap.addTile(index, item);
    }

    public ref GameObject getTileManagerGameObject()
    {
        return ref contInst.tileMap.getTileMapGameObject();
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

    public ref Tile getTile(Coords<int> coords, ref bool accessSuccessful)
    {
        return ref contInst.tileMap.getTile(coords, ref accessSuccessful);
    }
}

// Tile Manager will create the tile map
public partial class TileManager : ContainerAccessor
{
    GameObject exampleTile;

    bool generateTileGameObjects;
    bool enabledGameObjectIfTouched;

    // Tile Map Properties
    Coords<float> tileMapStartCoords = new Coords<float>(-.3f, 0f);
    
    public TileManager(bool generateTiles, bool enabledGameObjectIfTouched, ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.generateTileGameObjects = generateTiles;
        this.enabledGameObjectIfTouched = enabledGameObjectIfTouched;

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

        for (int x = 0; x < tileAccessor.getTileMapDims().getMaxX(); x++)
        {
            for (int y = 0; y < tileAccessor.getTileMapDims().getMaxY(); y++)
            {
                // Get new position
                Coords<float> newWorldCoords = 
                    new Coords<float> (((tileMapStartCoords.getX() + x) * tileWidth) + (tileWidth / 2),
                                       ((tileMapStartCoords.getY() + y) * tileHeight) + (tileHeight / 2));
                CoordsInt newTileMapCoords = new CoordsInt(x, y);

                Tile newTile = new Tile();
                GameObject tileManagerGameObject = tileAccessor.getTileManagerGameObject();
                // Create tile
                if (generateTileGameObjects == true)
                {
                    newTile = new Tile(generateTileGameObjects, enabledGameObjectIfTouched, Singleton.instantiateTile(), tileHeight, tileWidth,
                                        newWorldCoords, newTileMapCoords, ref tileManagerGameObject);
                }
                else
                {
                    newTile = new Tile(generateTileGameObjects, enabledGameObjectIfTouched, null, tileHeight, tileWidth,
                                        newWorldCoords, newTileMapCoords, ref tileManagerGameObject);
                }

                tileAccessor.addTileToTileMap(newTileMapCoords, newTile);
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


