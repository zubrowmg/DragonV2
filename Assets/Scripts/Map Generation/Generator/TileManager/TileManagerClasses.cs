using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using VeinManagerClasses;
using CommonlyUsedEnums;

namespace TileManagerClasses
{
    // Tile map is used in the generation proccess to place room pieces and veins on
    public class TileMap
    {
        TwoDList<Tile> tileMap;
        GameObject tileMapGameObject;

        Tile selectedTile = null;

        // Properties
        public Coords<int> minDim = new Coords<int>(0, 0);
        public Coords<int> maxDim = new Coords<int>(147 * 2 * 2, 72 * 2 * 2);

        public Dimensions tileMapDimensions;

        public Coords<int> tileMapCenter;

        public TileMap(GameObject tileMapGameObject)
        {
            this.tileMap = new TwoDList<Tile>();
            this.tileMapGameObject = tileMapGameObject;
            this.tileMapDimensions = new Dimensions(minDim, maxDim);
            this.tileMapCenter = new Coords<int>(tileMapDimensions.getMaxX()/2, 
                                                (tileMapDimensions.getMaxY()/2) + 50);
        }

        public void addTile(Coords<int> index, Tile item)
        {
            tileMap.addElement(index, item);
        }

        public ref GameObject getTileMapGameObject()
        {
            return ref tileMapGameObject;
        }

        public void countTileDims()
        {
            Debug.Log("TileMap X Count: " + tileMap.getXCount() + 
                    "\n        Y Count: " + tileMap.getYCount());
        }

        public ref Tile getTile(Coords<int> coords, ref bool accessSuccessful)
        {
            if (tileMapDimensions.getMinX() <= coords.getX() && coords.getX() < tileMapDimensions.getMaxX() &&
                tileMapDimensions.getMinY() <= coords.getY() && coords.getY() < tileMapDimensions.getMaxY())
            {
                selectedTile = tileMap.getElement(coords);
                accessSuccessful = true;
            }
            else
            {
                Debug.Log("Tile Class - getTile() attempted to access out of bounds tile: " + coords.getX() + ", " + coords.getY());
                accessSuccessful = false;
            }

            return ref selectedTile;
        }
    }

    public class Tile
    {
        GameObject gameObject;
        GameObject tileMapGameObject;
        bool gameObjectTilesAreOn;
        bool enabledGameObjectIfTouched;

        // Coords
        Coords<int> tileMapIndex;
        Coords<float> worldCoords;

        // Properties
        string name;
        float tileHeight;
        float tileWidth;

        // Debug and Generation
        bool isTouched = false;
        TileRoomType intendedRoomType = TileRoomType.None_Set; // Zone specific room, normal vein room, a vein biome room (GreatTunnel)

        bool isVein = false;
        bool isVeinMain = false;
        Vein associatedVein = null;

        public Tile()
        {
            this.gameObject = null;
            this.tileMapGameObject = null;
            this.gameObjectTilesAreOn = false;

            this.tileHeight = 0f;
            this.tileWidth = 0f;

            this.tileMapIndex = null;
            this.worldCoords = null;

            this.name = "NULL";
        }

        public Tile(bool gameObjectTilesAreOn, bool enabledGameObjectIfTouched, GameObject tile, float height, float width, 
                    Coords<float> worldCoords, Coords<int> tileMapCoords, ref GameObject tileMapGameObject)
        {
            this.gameObjectTilesAreOn = gameObjectTilesAreOn;
            this.tileMapGameObject = tileMapGameObject;
            this.enabledGameObjectIfTouched = enabledGameObjectIfTouched;

            this.tileHeight = height;
            this.tileWidth = width;

            this.tileMapIndex = tileMapCoords;
            this.worldCoords = worldCoords;

            this.name = "Tile__(" + tileMapIndex.getX().ToString() + "," + tileMapIndex.getY().ToString() + ")";

            // If gameobject tils are on, then we need to set additional gameobject setting
            if (gameObjectTilesAreOn == true)
            {
                instantiateTileGameObject(tile);
            }
        }

        public void instantiateTileGameObject(GameObject tile)
        {
            this.gameObject = tile;
            this.gameObject.transform.position = new Vector3(worldCoords.getX(), worldCoords.getY(), 0);
            this.gameObject.transform.SetParent(tileMapGameObject.transform);
            this.gameObject.name = this.name;
        }

        public void setTileAsVein(Vein veinInst)
        {
            if (this.isVein == false)
            {
                this.isVein = true;
                this.isTouched = true;
                this.associatedVein = veinInst;
                this.intendedRoomType = TileRoomType.Vein;

                if (gameObjectTilesAreOn == false)
                {
                    instantiateTileGameObject(Singleton.instantiateTile());
                }
            }
        }

        public void setTileAsVeinMain(Vein veinInst)
        {
            if (this.isVeinMain == false)
            {
                this.isVeinMain = true;
                setTileAsVein(veinInst);
            }
        }

        public ref GameObject getTileGameObject()
        {
            return ref gameObject;
        }

        // Test function, not used in generation
        public void changeVeinName()
        {
            associatedVein.name = "TEST_______111";
        }

        public bool getIsVeinMain()
        {
            return this.isVeinMain;
        }
    }
}

