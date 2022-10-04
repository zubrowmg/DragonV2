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
        float maxSlope = 5f; 
        float maxNonVerticalSlope = 4.9f; 

        // Degrees that the slope can change at
        List<float> fivePointSevenDegrees = new List<float> { .1f, .17f, .2f, .49f, .78f, 1.17f, 1.71f, 2.4f };

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

        public Slope(int xChange, int yChange, float slopeFloat)
        {
            this.xChange = xChange;
            this.yChange = yChange;
            this.slopeFloat = slopeFloat;
        }

        public Slope deepCopy()
        {
            return new Slope(this.xChange, this.yChange, this.slopeFloat);
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

        public void changeSlope(SlopeChange slopeChange, ref VeinDirection currentVeinDirection)
        {
            //float prevSope = slopeFloat;
            
            // Change slopeChange into a value for increasing or decreasing slope
            float incOrDec = 1f;
            if (slopeChange == SlopeChange.Dec)
                incOrDec = -1f;

            if (Mathf.Abs(slopeFloat) < .5f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[0] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 1f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[1] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 1.5f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[2] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 2f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[3] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 2.5f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[4] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 3f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[5] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 3.5f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[6] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < 4f)
                slopeFloat = slopeFloat + (fivePointSevenDegrees[7] * (incOrDec));
            else if (Mathf.Abs(slopeFloat) < maxSlope)
            {
                if (slopeIsPositive(slopeFloat) == true)
                {
                    if (incOrDec > 0)
                        slopeFloat = maxSlope;
                    else
                        slopeFloat = slopeFloat + (fivePointSevenDegrees[7] * incOrDec);
                }
                else
                {
                    if (incOrDec < 0)
                        slopeFloat = -maxSlope;
                    else
                        slopeFloat = slopeFloat + (fivePointSevenDegrees[7] * incOrDec);
                }
            }
            else
            {
                if (slopeIsNegative(slopeFloat) == true)
                    slopeFloat = slopeFloat + (fivePointSevenDegrees[6] * incOrDec);
                else if (slopeFloat < 0)
                    slopeFloat = slopeFloat + (fivePointSevenDegrees[6] * incOrDec);
                else
                    slopeFloat = slopeFloat + (fivePointSevenDegrees[6] * incOrDec);

            }

            // Check if the new slope needs to be corrected, aka can't be greater than the max slope
            if (slopeIsPositive(slopeFloat) == true)
            {
                if (slopeFloat > maxSlope)
                {
                    slopeFloat = -maxSlope + Mathf.Abs(slopeFloat - maxSlope);
                    if (currentVeinDirection == VeinDirection.Right)
                        currentVeinDirection = VeinDirection.Left;
                    else
                        currentVeinDirection = VeinDirection.Right;
                }
            }
            else if (slopeIsNegative(slopeFloat) == true)
            {
                if (slopeFloat < -maxSlope)
                {
                    slopeFloat = maxSlope - Mathf.Abs(slopeFloat + maxSlope);
                    if (currentVeinDirection == VeinDirection.Right)
                        currentVeinDirection = VeinDirection.Left;
                    else
                        currentVeinDirection = VeinDirection.Right;
                }
            }

            //Debug.Log("SLOPE IN : " + prevSope + "\n" +
            //          "SLOPE OUT: " + slopeFloat);
        }

        public bool slopeIsNegative(float slope)
        {
            bool slopeIsNegative = false;
            if (slope < 0f)
                slopeIsNegative = true;
            return slopeIsNegative;
        }

        public bool slopeIsPositive(float slope)
        {
            bool slopeIsPositive = false;
            if (slope > 0f)
                slopeIsPositive = true;
            return slopeIsPositive;
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

        public float calculateAngleChange(VeinDirection intendedDir, float intendedSlope, VeinDirection dir, float slope, float angleDiff, ref VeinDirection newDir)
        {
            float oldAngle = Mathf.Rad2Deg * Mathf.Atan2(intendedSlope, 1);
            float currentAngle = Mathf.Rad2Deg * Mathf.Atan2(slope, 1);
            float newAngle = (angleDiff + (Mathf.Rad2Deg * Mathf.Atan2(intendedSlope, 1)));

            float tempCurrentAngle = currentAngle;
            float tempNewAngle = newAngle;



            if (dir == VeinDirection.Left)
            {
                if (tempCurrentAngle < 0f)
                {
                    tempCurrentAngle = 180f - Mathf.Abs(currentAngle);
                    currentAngle = 180f - Mathf.Abs(currentAngle);
                    if (0f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 90f)
                    {
                        dir = VeinDirection.Right;
                        //Debug.Log("      Right");
                    }
                }
                else
                {
                    tempCurrentAngle = Mathf.Abs(currentAngle) + 180f;
                    currentAngle = 180f + Mathf.Abs(currentAngle);
                    if (270f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 360f)
                    {
                        dir = VeinDirection.Right;
                        //Debug.Log("      Right");
                    }
                }
            }
            else if (dir == VeinDirection.Right)
            {
                if (currentAngle < 0f)
                {
                    tempCurrentAngle = 360 - Mathf.Abs(currentAngle);
                    currentAngle = 360 - Mathf.Abs(currentAngle);
                    if (180f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 270f)
                    {
                        dir = VeinDirection.Left;
                        //Debug.Log("      LEFT");
                    }
                }
                else
                {
                    tempCurrentAngle = currentAngle;
                    //currentAngle = currentAngle;
                    if (90f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 180f)
                    {
                        dir = VeinDirection.Left;
                        //Debug.Log("      Left");
                    }
                }
            }

            if (intendedDir == VeinDirection.Left)
            {
                if (oldAngle < 0f)
                {
                    oldAngle = 180f - Mathf.Abs(oldAngle);
                    newAngle = oldAngle + angleDiff;
                    if (0f < newAngle && newAngle < 90f)
                    {
                        newDir = VeinDirection.Right;
                        //Debug.Log("      Right");
                    }
                    else
                    {
                        newDir = VeinDirection.Left;
                    }
                }
                else
                {
                    oldAngle = Mathf.Abs(oldAngle) + 180f;
                    newAngle = oldAngle + angleDiff;
                    if (270f < newAngle && newAngle < 360f)
                    {
                        newDir = VeinDirection.Right;
                        //Debug.Log("      Right");
                    }
                    else
                    {
                        newDir = VeinDirection.Left;
                    }
                }
            }
            else if (intendedDir == VeinDirection.Right)
            {
                if (oldAngle < 0f)
                {
                    oldAngle = 360f - Mathf.Abs(oldAngle);
                    newAngle = oldAngle + angleDiff;
                    if (newAngle > 360f)
                    {
                        newAngle = newAngle - 360f;
                    }
                    if (180f < newAngle && newAngle < 270f)
                    {
                        newDir = VeinDirection.Left;
                        //Debug.Log("      LEFT");
                    }
                    else
                    {
                        newDir = VeinDirection.Right;
                    }
                }
                else
                {
                    //oldAngle = oldAngle;
                    newAngle = oldAngle + angleDiff;
                    if (newAngle < 0f)
                    {
                        newAngle = 360f + newAngle;
                    }
                    if (90f < newAngle && newAngle < 180f)
                    {
                        newDir = VeinDirection.Left;
                        //Debug.Log("      Left");
                    }
                    else
                    {
                        newDir = VeinDirection.Right;
                    }
                }
            }

            float newSlope = Mathf.Tan(Mathf.Deg2Rad * newAngle);


            if (intendedSlope > -maxSlope && newSlope < -maxSlope)
            {
                newSlope = -maxSlope;
            }

            if (intendedSlope < maxSlope && newSlope > maxSlope)
            {
                newSlope = maxSlope;
            }


            return newSlope;
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
