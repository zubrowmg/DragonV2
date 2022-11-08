using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using CommonlyUsedFunctions;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class SetCoordsVein : SimpleVein
    {
        Queue<CoordsInt> setCoords = new Queue<CoordsInt>();

        public SetCoordsVein(ref GeneratorContainer contInst,
                          int id,
                          CoordsInt startCoords,
                          CoordsInt endCoords,
                          bool varyWidth,
                          bool varyLength,
                          bool varySlope,
                          int width)
                   : base(ref contInst,
                          id,
                          Direction.None, // General Direction gets manually calculated
                          startCoords,
                          endCoords,
                          varyWidth,
                          varyLength,
                          varySlope,
                          width)
        {
            this.setVeinType(VeinType.Set_Coords);
        }

        void init()
        {
            this.currentDistance = 0f;
        }

        protected void initCoords(CoordsInt startCoords, CoordsInt endCoords, Direction generalDirection)
        {
            this.generalVeinDirection = generalDirection;
            this.intendedVeinDirection = this.currentVeinDirection;
            this.currentVeinDirection = calculateCurrentVeinDirection(startCoords, endCoords);

            this.startCoords = startCoords;
            this.prevCoords = startCoords.deepCopyInt();
            this.currentCoords = startCoords.deepCopyInt();
            this.endCoords = endCoords;

            this.veinSlope = new Slope(startCoords, endCoords);
            this.intendedVeinSlope = new Slope(startCoords, endCoords);

            this.slopeTargetManager = new TargetProbabilityManager(.1f,
                                                                   veinSlope.getSlope(),
                                                                   veinSlope.getSlope(),
                                                                   new List<float> { .3f, .4f, .3f },
                                                                   new List<int> { -1, 0, 1 },
                                                                   1,
                                                                   .5f);

            float distance = CommonFunctions.calculateCoordsDistance(startCoords, endCoords);
            bool varyLength = false;
            this.approxDistance = initVaryLength(varyLength, distance);
        }

        public void addSetCoord(CoordsInt coords)
        {
            setCoords.Enqueue(coords);
        }

        public void addSetCoord(List<CoordsInt> coords)
        {
            foreach (var item in coords)
            {
                setCoords.Enqueue(item);
            }
        }

        public void triggerSetCoordsVeinGeneration()
        {
            if (setCoords.Count < 2)
                Debug.LogError("SetCoordsVein - triggerSetCoordsVeinGeneration: Less than 2 coords added");


            bool firstLoop = true;

            CoordsInt prevWorldCoords = new CoordsInt(0, 0);
            CoordsInt currentWorldCoords = new CoordsInt(0, 0);

            while (setCoords.Count > 0)
            {
                if (firstLoop == true)
                {
                    prevWorldCoords = setCoords.Dequeue();
                    currentWorldCoords = setCoords.Dequeue();
                    firstLoop = false;
                }
                else
                {
                    prevWorldCoords = currentWorldCoords.deepCopyInt();
                    currentWorldCoords = setCoords.Dequeue();
                }

                Direction generalDirection = Direction.East;

                //prevWorldCoords.print("PREV: ");
                //currentWorldCoords.print("Current: ");

                if (prevWorldCoords.getX() > currentWorldCoords.getX())
                    generalDirection = Direction.West;

                initCoords(prevWorldCoords, currentWorldCoords, generalDirection);
                init();
                triggerVeinGeneration();
            }
        }

        

        public override VeinConnection getFurthestVeinConnectorFromStart()
        {
            // PROBABLY NOT CORRECT!!!!
            return listOfVeinConnections[listOfVeinConnections.Count - 1];
        }

    }

}

