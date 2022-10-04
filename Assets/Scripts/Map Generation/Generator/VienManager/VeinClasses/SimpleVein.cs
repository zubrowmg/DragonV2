using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class SimpleVein : Vein
    {
        public SimpleVein(ref GeneratorContainer contInst,
                          Direction generalDirection,
                          Coords<int> startCoords,
                          Coords<int> endCoords,
                          bool varyWidth,
                          bool varyLength,
                          bool varySlope,
                          int width,
                          int distance)
                   : base(ref contInst,
                          generalDirection,
                          startCoords,
                          endCoords,
                          varyWidth,
                          varyLength,
                          varySlope,
                          width,
                          distance)
        {
            this.setVeinType(VeinType.Simple);
        }

        public override void triggerVeinGeneration()
        {
            DistanceStateTracker distanceTracker = new DistanceStateTracker(getDistanceGoal());
            bool distanceStateChanged = false;

            bool hitDistanceGoal = false;
            int currentSlopeIndex = 0; // Keeps count of how many (Vein Main) points were ploted with the current slope. Resets to 0 when slope changes
            Coords<int> currentSlopeStartCoords = currentCoords.deepCopy(); // Resets to current coords when slope changes
            Coords<int> nextCoords = currentCoords.deepCopy();

            while (hitDistanceGoal == false)
            {
                distanceTracker.updateState(ref distanceStateChanged, getCurrentDistance());

                // Change slope first
                if (distanceStateChanged == true && varyVeinSlope == true)
                {
                    changeSlopeEveryXDistance(ref currentSlopeIndex, ref currentSlopeStartCoords, this.currentCoords, distanceTracker.getCurrentState());
                }

                // Next handle width changes
                if (varyVeinWidth == true)
                    handleWidthChanges();

                // Calculate distance that next coord would put the vein at
                //      If it goes over the distance goal end while loop here
                float newCurrentDistance = calculateNewPosition(nextCoords, ref this.currentCoords, ref this.prevCoords, getCurrentDistance());
                setCurrentDistance(newCurrentDistance);
                if (getCurrentDistance() > (float)getDistanceGoal())
                    break;

                // Mark a strip of tiles as veins
                createVeinStrip(currentCoords);

                // Calculate next coords
                nextCoords = calculateIndexToCoords(currentSlopeStartCoords, currentSlopeIndex, veinSlope.getSlope(), getCurrentVeinDirection());


                currentSlopeIndex++;
            }
        }

    }

}

