using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class UVein : Vein
    {
        bool isUpDown = Random.Range(0, 1 + 1) == 1 ? true : false; // Determines which way the u will go, up/down or left/right
        int shiftStart = 2;
        int totalUParts = 4;
        int totalSections = 8;

        List<int> partWidths = new List<int> { 0, 0, 0, 0 };
        List<int> lowerBoundries = new List<int> { 0, 0, 0, 0 };
        List<int> upperBoundries = new List<int> { 0, 0, 0, 0 };

        public UVein(ref GeneratorContainer contInst,
                     int id,
                     Direction generalDirection,
                     Coords<int> startCoords,
                     Coords<int> endCoords,
                     bool varyWidth,
                     bool varyLength,
                     int width,
                     int distance)
              : base(ref contInst,
                     id,
                     generalDirection,
                     startCoords,
                     endCoords,
                     varyWidth,
                     varyLength,
                     false,
                     width,
                     distance)
        {
            this.setVeinType(VeinType.U);

            configureUVein();
        }

        void configureUVein()
        {

            int credits = totalSections - 5;
            int partWidth = 0;
            int part = 0;

            UVeinType uType = (UVeinType)Random.Range(0, 2 + 1);
            if (uType == UVeinType.Shift)
            {
                // Can also shift 0 times aka None
                // Can only shift up to 3
                shiftStart += Random.Range(0, 3 + 1);
            }
            else if (uType == UVeinType.Widen)
            {
                while (credits > 0)
                {
                    // Choose a random width
                    partWidth = Random.Range(1, credits + 1);
                    credits -= partWidth;

                    // Give it to a random part
                    part = Random.Range(0, partWidths.Count);
                    for (int i = 0; i < partWidths.Count; i++)
                    {
                        if (part == i)
                        {
                            partWidths[part] += partWidth;
                        }
                    }
                }
            }
            else if (uType == UVeinType.Shift_Widen)
            {
                // Can only shift up to 2, need credits for widening
                int shiftStartChange = 1;// Random.Range(1, 2+1);
                shiftStart += shiftStartChange;
                credits -= shiftStartChange;

                while (credits > 0)
                {
                    // Choose a random width
                    partWidth = Random.Range(1, credits + 1);
                    credits -= partWidth;

                    // Give it to a random part
                    part = Random.Range(0, partWidths.Count);
                    for (int i = 0; i < partWidths.Count; i++)
                    {
                        if (part == i)
                        {
                            partWidths[part] += partWidth;
                        }
                    }
                }
            }

            configurePartBoundries();
        }

        void configurePartBoundries()
        {
            int combinedWidths = 0;
            for (int i = 0; i < partWidths.Count; i++)
            {
                lowerBoundries[i] = (shiftStart + combinedWidths + i - 2) * (getDistanceGoal() / totalSections);
                combinedWidths += partWidths[i];
                upperBoundries[i] = (shiftStart + combinedWidths + i) * (getDistanceGoal() / totalSections);
            }
        }

        void handleUVeinSlope()
        {
            if (getCurrentDistance() < 1 * (getDistanceGoal() / totalSections))
            {
                // DON'T EVER START TURNING IN HERE
            }
            else if (lowerBoundries[0] <= getCurrentDistance() && getCurrentDistance() < upperBoundries[0])
            {
                handleUtype(UVeinStage.Part1);
            }
            else if (lowerBoundries[1] <= getCurrentDistance() && getCurrentDistance() < upperBoundries[1])
            {
                handleUtype(UVeinStage.Part2);
            }
            else if (lowerBoundries[2] <= getCurrentDistance() && getCurrentDistance() < upperBoundries[2])
            {
                handleUtype( UVeinStage.Part3);
            }
            else if (lowerBoundries[3] <= getCurrentDistance() && getCurrentDistance() < upperBoundries[3])
            {
                handleUtype(UVeinStage.Part4);
            }
        }

        void handleUtype(UVeinStage stage)
        {
            VeinDirection newDir = getCurrentVeinDirection();
            float slope = veinSlope.getSlope();
            float newSlope = 0f;
            float intendedSlope = intendedVeinSlope.getSlope();

            if (stage == UVeinStage.Part1)
            {
                if (isUpDown)
                {
                    newSlope = veinSlope.calculateAngleChange(intendedVeinDirection, intendedSlope, currentVeinDirection, slope, 45f, ref newDir);
                    if (isMovingAngleLessThanTargetAngle(slope, currentVeinDirection, newSlope, newDir))
                    {
                        veinSlope.changeSlope(SlopeChange.Inc, ref currentVeinDirection);
                    }
                }
                else
                {
                    newSlope = veinSlope.calculateAngleChange(intendedVeinDirection, intendedSlope, currentVeinDirection, slope, -45f, ref newDir);
                    if (isMovingAngleGreaterThanTargetAngle(slope, currentVeinDirection, newSlope, newDir))
                    {
                        veinSlope.changeSlope(SlopeChange.Dec, ref currentVeinDirection);
                    }
                }
            }
            else if (stage == UVeinStage.Part2)
            {
                if (isUpDown)
                {
                    if (isMovingAngleGreaterThanTargetAngle(slope, currentVeinDirection, intendedSlope, intendedVeinDirection))
                    {
                        veinSlope.changeSlope(SlopeChange.Dec, ref currentVeinDirection);
                    }
                }
                else
                {
                    if (isMovingAngleLessThanTargetAngle(slope, currentVeinDirection, intendedSlope, intendedVeinDirection))
                    {
                        veinSlope.changeSlope(SlopeChange.Inc, ref currentVeinDirection);
                    }
                }
            }
            else if (stage == UVeinStage.Part3)
            {
                if (isUpDown)
                {
                    newSlope = veinSlope.calculateAngleChange(intendedVeinDirection, intendedSlope, currentVeinDirection, slope, -45f, ref newDir);
                    if (isMovingAngleGreaterThanTargetAngle(slope, currentVeinDirection, newSlope, newDir))
                    {
                        veinSlope.changeSlope(SlopeChange.Dec, ref currentVeinDirection);
                    }
                }
                else
                {
                    newSlope = veinSlope.calculateAngleChange(intendedVeinDirection, intendedSlope, currentVeinDirection, slope, 45f, ref newDir);
                    if (isMovingAngleLessThanTargetAngle(slope, currentVeinDirection, newSlope, newDir))
                    {
                        veinSlope.changeSlope(SlopeChange.Inc, ref currentVeinDirection);
                    }
                }
            }
            else if (stage == UVeinStage.Part4)
            {
                if (isUpDown)
                {
                    if (isMovingAngleLessThanTargetAngle(slope, currentVeinDirection, intendedSlope, intendedVeinDirection))
                    {
                        veinSlope.changeSlope(SlopeChange.Inc, ref currentVeinDirection);
                    }
                }
                else
                {
                    if (isMovingAngleGreaterThanTargetAngle(slope, currentVeinDirection, intendedSlope, intendedVeinDirection))
                    {
                        veinSlope.changeSlope(SlopeChange.Dec, ref currentVeinDirection);
                    }
                }
            }
        }

        bool isMovingAngleGreaterThanTargetAngle(float slope, VeinDirection dir, float newSlope, VeinDirection newDir)
        {
            float movingAngle = getAngleFromSlope(slope, dir);
            float targetAngle = getAngleFromSlope(newSlope, newDir);

            bool isGreater = false;
            //Debug.Log("Moving Dir: " + dir + "    New Dir: " + newDir);
            //Debug.Log("Moving Ang: " + movingAngle + "    Target Ang: " + targetAngle);

            if (dir == VeinDirection.Right && newDir == VeinDirection.Right)
            {
                if (0f < movingAngle && movingAngle < 90f && 270f < targetAngle && targetAngle < 360f)
                {
                    isGreater = true;
                }
                else if (270f < movingAngle && movingAngle < 360f && 0f < targetAngle && targetAngle < 90f)
                {
                    isGreater = false;
                }
                else if (targetAngle < movingAngle)
                {
                    isGreater = true;
                }
            }
            else if (targetAngle < movingAngle)
            {
                isGreater = true;
            }

            return isGreater;
        }


        bool isMovingAngleLessThanTargetAngle(float slope, VeinDirection dir, float newSlope, VeinDirection newDir)
        {
            float movingAngle = getAngleFromSlope(slope, dir);
            float targetAngle = getAngleFromSlope(newSlope, newDir);

            bool isLess = false;

            if (dir == VeinDirection.Right && newDir == VeinDirection.Right)
            {
                // This is to handle the 360 degrees to 0 degrees transition
                if (0f < targetAngle && targetAngle < 90f && 270f < movingAngle && movingAngle < 360f)
                {
                    isLess = true;
                }
                else if (270f < targetAngle && targetAngle < 360f && 0f < movingAngle && movingAngle < 90f)
                {
                    isLess = false;
                }
                else if (movingAngle < targetAngle)
                {
                    isLess = true;
                }
            }
            else if (movingAngle < targetAngle)
            {
                isLess = true;
            }

            return isLess;
        }

        float getAngleFromSlope(float slope, VeinDirection dir)
        {
            float newAngle = 0f;

            if (dir == VeinDirection.Right)
            {
                newAngle = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Abs(slope), 1);
                if (veinSlope.slopeIsNegative(slope) == true)
                {
                    newAngle = 360f - newAngle;
                }
            }
            else
            {
                newAngle = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Abs(slope), 1);
                if (veinSlope.slopeIsNegative(slope) == true)
                {
                    newAngle = 180f - newAngle;
                }
                else
                {
                    newAngle += 180f;
                }
            }

            return newAngle;
        }


        public override void triggerVeinGeneration()
        {
            Slope prevSlope = veinSlope.deepCopy();
            DistanceStateTracker distanceTracker = new DistanceStateTracker(getDistanceGoal());
            bool distanceStateChanged = false;

            bool hitDistanceGoal = false;
            int currentSlopeIndex = 0; // Keeps count of how many (Vein Main) points were ploted with the current slope. Resets to 0 when slope changes
            Coords<int> currentSlopeStartCoords = currentCoords.deepCopy(); // Resets to current coords when slope changes
            Coords<int> nextCoords = currentCoords.deepCopy();

            int delaySlopeChangeCount = 0; // Slope changes immediatly, to try and smoothen it out delay slope change so that every other slope change takes effect

            while (hitDistanceGoal == false)
            {
                distanceTracker.updateState(ref distanceStateChanged, getCurrentDistance());

                // Change slope to look like a U
                prevSlope = veinSlope.deepCopy();
                if (delaySlopeChangeCount >= 3)
                    handleUVeinSlope();

                if (veinSlope.getSlope() != prevSlope.getSlope())
                {
                    currentSlopeIndex = 0;
                    currentSlopeStartCoords = currentCoords.deepCopy();
                    delaySlopeChangeCount = 0;
                }
                else
                    delaySlopeChangeCount++;

                // Next handle width changes
                if (varyVeinWidth == true)
                    handleWidthChanges();

                // Calculate distance that next coord would put the vein at
                //      If it goes over the distance goal end while loop here
                float newCurrentDistance = calculateNewPosition(nextCoords, ref this.currentCoords, ref this.prevCoords, getCurrentDistance());
                handleMiddleVeinConnections(this.currentDistance, newCurrentDistance);
                setCurrentDistance(newCurrentDistance);

                if (getCurrentDistance() > (float)getDistanceGoal())
                {
                    placeVeinConnection(this.currentCoords);
                    break;
                };

                // Mark a strip of tiles as veins
                createVeinStrip(currentCoords);

                // Calculate next coords
                nextCoords = calculateIndexToCoords(currentSlopeStartCoords, currentSlopeIndex, veinSlope.getSlope(), getCurrentVeinDirection());


                currentSlopeIndex++;
            }
        }

        public override VeinConnection getFurthestVeinConnectorFromStart()
        {
            // Should be the last vein connection added to the list, look at triggerVeinGeneration() in simple and U veins
            return listOfVeinConnections[listOfVeinConnections.Count - 1];
        }
        }
}
