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
        // Type
        VeinType veinType = VeinType.None_Set;

        // Direction
        Direction generalDirection; // Not really used in calculations, only to help for debug
        VeinDirection intendedVeinDirection = VeinDirection.None_Set;
        Slope veinSlope;
        Coords<int> startCoords;
        Coords<int> endCoords;

        // Width and Distance
        int approxWidth = 6;
        int approxDistance = 6;

        // Vein varying properties
        bool varyVeinWidth = false;
        bool varyVeinLength = false;
        bool varyVeinSlope = false;

        // Vein properties used during vein creation time
        int currentWidth;
        int currentDistance = 0;
        VeinDirection currentVeinDirection = VeinDirection.None_Set;


        // List of Tiles
        // List of Tile Bookmarks, Bookmarks are meant for future vein expansion

        public Vein(VeinType type, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords, Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope)
        {
            this.veinType = type;

            this.generalDirection = generalDirection;
            this.startCoords = startCoords;
            this.endCoords = endCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection();
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxDistance = initVaryLength(varyLength, approxDistance);
        }

        public Vein(VeinType type, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords, Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope, int width, int distance)
        {
            this.veinType = type;

            this.generalDirection = generalDirection;
            this.startCoords = startCoords;
            this.endCoords = endCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection();
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxWidth = width;
            this.approxDistance = initVaryLength(varyLength, distance);
        }

        int initVaryLength(bool varyLength, int distance)
        {
            if (varyLength == false)
            {
                return distance;
            }
            else
            {
                return distance + RandomProbability.getIntBasedOnPercentage(
                                    new RandomProbability.RandomSelection(0, 25, .25f),
                                    new RandomProbability.RandomSelection(26, 45, .75f),
                                    new RandomProbability.RandomSelection(46, 65, .0f));
            }
        }

        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================
        public VeinDirection getIntendedVeinDirection()
        {
            return this.intendedVeinDirection;
        }

        public VeinDirection getCurrentVeinDirection()
        {
            return this.currentVeinDirection;
        }

        public VeinType getVeinType()
        {
            return this.veinType;
        }

        public VeinDirection calculateCurrentVeinDirection()
        {
            VeinDirection veinDirection = VeinDirection.None_Set;

            if (startCoords.getX() < endCoords.getX() || startCoords.getX() == endCoords.getX())
            {
                veinDirection = VeinDirection.Right;
            }
            else
            {
                veinDirection = VeinDirection.Left;
            }

            return veinDirection;
        }

        public Coords<int> getStartCoords()
        {
            return startCoords;
        }

        public Coords<int> getEndCoords()
        {
            return endCoords;
        }

        public int getCurrentWidth()
        {
            return currentWidth;
        }

        public int getCurrentDistance()
        {
            return this.currentDistance;
        }

        public void setCurrentDistance(int newDistance)
        {
            this.currentDistance = newDistance;
        }

        public int getDistanceGoal()
        {
            return approxDistance;
        }
    }

    public class Slope
    {
        // Hard coded limits
        float maxSlope = 5f;
        float maxNonVerticalSlope = 4.9f;

        int xChange;
        int yChange;

        float slopeFloat;

        public Slope()
        {
            this.xChange = 1;
            this.yChange = 1;
            this.slopeFloat = 1f;
        }

        public Slope(Coords<int> startDestination, Coords<int> endDestination)
        {
            this.xChange = endDestination.getX() - startDestination.getX();
            this.yChange = endDestination.getY() - startDestination.getY();
            this.slopeFloat = calculateSlope(xChange, yChange, startDestination, endDestination);
        }

        // Calculates slope, also if slope is beyond limits this will fix that
        public float calculateSlope(int xChange, int yChange, Coords<int> startDestination, Coords<int> endDestination)
        {
            float slope = (float)this.yChange / (float)this.xChange;

            if (slope > maxSlope)
            {
                if (Mathf.Abs(startDestination.getX() - endDestination.getX()) > 30)
                {
                    slope = maxNonVerticalSlope - .05f;
                }
                else
                {
                    slope = maxSlope;
                }
            }
            else if (slope < -maxSlope)
            {
                if (Mathf.Abs(startDestination.getX() - endDestination.getX()) > 30)
                {
                    slope = -maxNonVerticalSlope + .05f;
                }
                else
                {
                    slope = -maxSlope;
                }
            }
            else if (-.05f < slope && slope < .05f)
            {
                slope = .01f;
            }

            return slope;
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
