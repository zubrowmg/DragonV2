using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

namespace TileManagerClasses
{
    // Tile map is used in the generation proccess to place room pieces and veins on
    public class TileMap
    {
        List<int> tileMap;

        public TileMap()
        {
            tileMap = new List<int>();
        }

        public void setTileMap(List<int> test)
        {
            tileMap = test;
        }

        public List<int> getTileMap()
        {
            return tileMap;
        }
    }

    public class Tile
    {
        GameObject gameObject = new GameObject();
        bool gameObjectExists;

        float tileHeight;
        float tileWidth;



        public Tile(bool gameObjectExists, GameObject tile, float height, float width)
        {
            this.gameObject = tile;
            this.gameObjectExists = gameObjectExists;

            this.tileHeight = height;
            this.tileWidth = width;
        }
    }
}

