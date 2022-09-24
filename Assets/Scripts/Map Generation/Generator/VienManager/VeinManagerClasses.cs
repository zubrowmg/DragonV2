using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class VeinContainer
    {
        Dictionary<Tile, Vein> tileToVeinLookUp;
    }

    public class Vein
    {
        // Direction
        Direction generalDirection;
        Slope veinSlope;
        Coords<int> startCoords;

        // Width and Distance
        int approxWidth = 6;
        int approxDistance = 6;

        // Vein varying properties
        bool varyVeinWidth = false;
        bool varyVeinLength = false;
        bool varyVeinSlope = false;

        // Vein properties used during vein creation time
        VeinDirection veinDirection = VeinDirection.None_Set;


        // List of Tiles
        // List of Tile Bookmarks, Bookmarks are meant for future vein expansion

        public Vein(Direction generalDirection, Coords<int> startCoords, Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope)
        {
            this.generalDirection = generalDirection;
            this.startCoords = startCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;
        }

        public Vein(Direction generalDirection, Coords<int> startCoords, Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope, int width, int distance)
        {
            this.generalDirection = generalDirection;
            this.startCoords = startCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.approxWidth = width;
            this.approxDistance = distance;
        }


        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================
        public void setCurrentVeinDirection(VeinDirection currentVeinDirection)
        {
            this.veinDirection = currentVeinDirection;
        }

        public VeinDirection getCurrentVeinDirection()
        {
            return this.veinDirection;
        }
    }

    public class Slope
    {
        int xChange;
        int yChange;

        float slopeFloat;

        public Slope()
        {
            this.xChange = 1;
            this.yChange = 1;
            this.slopeFloat = (float)this.yChange / (float)this.xChange;
        }

        public Slope(int xChange, int yChange)
        {
            this.xChange = xChange;
            this.yChange = yChange;
            this.slopeFloat = (float)this.yChange / (float)this.xChange;
        }



        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================
        public int getXChange()
        {
            return xChange;
        }

        public int getYChange()
        {
            return yChange;
        }

        public float getSlope()
        {
            return slopeFloat;
        }
    }
}
