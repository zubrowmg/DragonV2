using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
using CommonlyUsedClasses;

namespace VeinManagerClasses
{
    public class VeinContainer
    {

    }

    public class Vein
    {
        Direction generalDirection;
        Coords<int> startCoords;
        Slope veinSlope;

        // List of Tiles
        // List of Tile Bookmarks, Bookmarks are meant for future vein expansion

        public Vein(Direction generalDirection, Coords<int> startCoords, Slope veinSlope)
        {
            this.generalDirection = generalDirection;
            this.startCoords = startCoords;
            this.veinSlope = veinSlope;
        }

    }

    public class Slope
    {
        int xChange;
        int yChange;

        public Slope()
        {
            this.xChange = 1;
            this.yChange = 1;
        }

        public Slope(int xChange, int yChange)
        {
            this.xChange = xChange;
            this.yChange = yChange;
        }

        public int getXChange()
        {
            return xChange;
        }

        public int getYChange()
        {
            return yChange;
        }
    }
}
