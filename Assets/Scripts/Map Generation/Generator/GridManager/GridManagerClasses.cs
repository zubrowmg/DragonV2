using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

namespace GridManagerClasses
{

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

