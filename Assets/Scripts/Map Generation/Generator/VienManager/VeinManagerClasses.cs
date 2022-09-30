using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class VeinContainer
    {
        Dictionary<Tile, Vein> tileToVeinLookUp;
    }

    public class Slope
    {
        // Hard coded limits
        float maxSlope = 8f; // Old value
        float maxNonVerticalSlope = 7.9f; // New value

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

        // TEMPORARY FUNCTION, NEEDS TO BE UPDATED
        public void changeSlope(float newValue)
        {
            this.slopeFloat = newValue;
        }

        public void changeSlope(SlopeChange slopeChange)
        {
            
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

        public float getMaxSlope()
        {
            return maxSlope;
        }

        public float getMaxNonVerticalSlope()
        {
            return maxNonVerticalSlope;
        }
    }

    public class DistanceStateTracker
    {
        VeinDistanceTraveled state = VeinDistanceTraveled.None;
        bool justChangedStates = false;
        float distanceGoal;

        public DistanceStateTracker(float distanceGoal)
        {
            this.distanceGoal = distanceGoal;
        }

        public void updateState(ref bool stateChange, float currentDistance)
        {
            if (state == VeinDistanceTraveled.None)
            {
                if (currentDistance >= (distanceGoal / 6))
                {
                    state = VeinDistanceTraveled.One_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.One_Sixths)
            {
                if (currentDistance >= (2 * (distanceGoal / 6)))
                {
                    state = VeinDistanceTraveled.Two_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Two_Sixths)
            {
                if (currentDistance >= (3 * (distanceGoal / 6)))
                {
                    state = VeinDistanceTraveled.Three_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Three_Sixths)
            {
                if (currentDistance >= (4 * (distanceGoal / 6)))
                {
                    state = VeinDistanceTraveled.Four_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Four_Sixths)
            {
                if (currentDistance >= (5 * (distanceGoal / 6)))
                {
                    state = VeinDistanceTraveled.Five_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Five_Sixths)
            {
                justChangedStates = false;
            }

            stateChange = justChangedStates;
        }

        public VeinDistanceTraveled getCurrentState()
        {
            return state;
        }
    }
}
