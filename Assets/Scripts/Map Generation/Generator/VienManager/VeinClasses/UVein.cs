using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
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

        public UVein(ref GeneratorContainer contInst,
                     Direction generalDirection,
                     Coords<int> startCoords,
                     Coords<int> endCoords,
                     bool varyWidth,
                     bool varyLength,
                     int width,
                     int distance)
              : base(ref contInst,
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
        }

        public override void triggerVeinGeneration2()
        {
            //int[] partWidths = new int[totalUParts];
            //int[] lowerBoundries = new int[totalUParts];
            //int[] upperBoundries = new int[totalUParts];

            //if (vein == veinType.U)
            //{
            //    configureUVein(ref shiftStart, ref partWidths, totalUParts, totalSections, ref newDistance, ref lowerBoundries, ref upperBoundries);
            //}

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
                if (distanceStateChanged == true)
                {
                    //changeSlopeEveryXDistance(ref currentSlopeIndex, ref currentSlopeStartCoords, this.currentCoords, distanceTracker.getCurrentState());
                    //!!!!!!!!!!
                }
                else if (veinType == VeinType.U)
                {

                }

                // Next handle width changes
                if (varyVeinWidth == true)
                    handleWidthChanges();

                // Calculate distance that next coord would put the vein at
                //      If it goes over the distance goal end while loop here
                updatePosition(nextCoords, ref this.currentCoords, ref this.prevCoords, ref this.currentDistance);
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
