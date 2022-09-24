using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

namespace TileManagerClasses
{
    // Tile map is used in the generation proccess to place room pieces and veins on
    public class TileMap
    {
        TwoDList<Tile> tileMap;
        GameObject tileMapGameObject;

        public TileMap(GameObject tileMapGameObject)
        {
            this.tileMap = new TwoDList<Tile>();
            this.tileMapGameObject = tileMapGameObject;
        }

        public void addTile(Coords<int> index, Tile item)
        {
            tileMap.addElement(index, item);
        }

        public GameObject getTileMapGameObject()
        {
            return tileMapGameObject;
        }

        public void countTileDims()
        {
            Debug.Log("TileMap X Count: " + tileMap.getXCount() + 
                    "\n        Y Count: " + tileMap.getYCount());
        }
    }

    public class Tile
    {
        GameObject gameObject;
        bool gameObjectTilesAreOn;

        float tileHeight;
        float tileWidth;

        Coords<int> tileMapCoords;
        Coords<float> worldCoords;

        string name;

        public Tile()
        {
            this.gameObject = null;
            this.gameObjectTilesAreOn = false;

            this.tileHeight = 0f;
            this.tileWidth = 0f;

            this.tileMapCoords = null;
            this.worldCoords = null;

            this.name = "NULL";
        }

        public Tile(bool gameObjectTilesAreOn, GameObject tile, float height, float width, 
                    Coords<float> worldCoords, Coords<int> tileMapCoords, ref GameObject tileMapGameObject)
        {
            this.gameObject = tile;
            this.gameObjectTilesAreOn = gameObjectTilesAreOn;

            this.tileHeight = height;
            this.tileWidth = width;

            this.tileMapCoords = tileMapCoords;
            this.worldCoords = worldCoords;

            this.name = "Tile";

            // If gameobject tils are on, then we need to set additional gameobject setting
            if (gameObjectTilesAreOn == true)
            {
                this.gameObject.transform.position = new Vector3(worldCoords.getX(), worldCoords.getY(), 0);
                this.gameObject.transform.SetParent(tileMapGameObject.transform);
                this.gameObject.name = this.name;
            }
        }
    }
}

