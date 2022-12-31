using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedDefinesAndEnums;

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

    public void addTileToTileMap(CoordsInt index, Tile item)
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
        return contInst.tileMap.getTileMapDims();
    }

    public Coords<int> getTileMapCenter()
    {
        return contInst.tileMap.getTileMapCenter();
    }

    public ref Tile getTile(CoordsInt coords, out bool accessSuccessful)
    {
        return ref contInst.tileMap.getTile(coords, out accessSuccessful);
    }

    public List<Tile> getBlockOfTiles(CoordsInt centerCoord, int blockLength)
    {
        // Block length starts from the center, 1 length is a 3x3 black
        List<Tile> blockOfTiles = new List<Tile>();
        CoordsInt startCoord = centerCoord.deepCopyInt();
        startCoord.decX(blockLength);
        startCoord.decY(blockLength);

        int sideLength = (blockLength * 2) + 1;

        for (int x = 0; x < sideLength; x++)
        {
            for (int y = 0; y < sideLength; y++)
            {
                Tile tile = getTile(new CoordsInt(startCoord.getX() + x, startCoord.getY() + y), out bool accessSuccessful);
                if (accessSuccessful)
                    blockOfTiles.Add(tile);
            }
        }
        return blockOfTiles;
    }

    public Depth getTileDepth(CoordsInt coords)
    {
        return contInst.tileMap.getTileDepth(coords);
    }

    public HorizontalDisplacement getTileHorizontalDisplacement(CoordsInt coords)
    {
        return contInst.tileMap.getTileHorizontalDisplacement(coords);
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

                // Create tile
                GameObject tileManagerGameObject = tileAccessor.getTileManagerGameObject();
                Tile newTile = new Tile(generateTileGameObjects, enabledGameObjectIfTouched, tileHeight, tileWidth,
                                        newWorldCoords, newTileMapCoords, ref tileManagerGameObject);

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


