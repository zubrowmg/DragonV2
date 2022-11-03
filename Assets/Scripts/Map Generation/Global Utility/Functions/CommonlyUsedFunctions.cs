using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace CommonlyUsedFunctions
{

    public static class CommonFunctions
    {

        public static List<T> Shuffle<T>(ref List<T> list)
        {
            List<T> shuffledList = new List<T>();

            while (list.Count > 0)
            {
                int randIndex = Random.Range(0, list.Count);

                shuffledList.Add(list[randIndex]);
                list.RemoveAt(randIndex);
            }

            return shuffledList;
        }

        public static float calculateCoordsDistance(Coords<int> oneCoord, Coords<int> twoCoords)
        {
            float xChange = calculateDifference(oneCoord.getX(), twoCoords.getX());
            float yChange = calculateDifference(oneCoord.getY(), twoCoords.getY());
            return Mathf.Sqrt((xChange * xChange) + (yChange * yChange));
        }

        public static float calculateDifference(float numOne, float numTwo)
        {
            return (float)Mathf.Abs(numOne - numTwo);
        }

        public static T randomlySelectFromList<T>(List<T> list)
        {
            int randInt = Random.Range(0, list.Count);

            return list[randInt];
        }

        public static Direction getOppositeDir(Direction dir)
        {
            Direction oppositeDir = Direction.None;

            switch (dir)
            {
                case Direction.North:
                    oppositeDir = Direction.South;
                    break;
                case Direction.East:
                    oppositeDir = Direction.West;
                    break;
                case Direction.South:
                    oppositeDir = Direction.North;
                    break;
                case Direction.West:
                    oppositeDir = Direction.East;
                    break;
                case Direction.None:
                    Debug.LogError("CommonlyUsedFuntions - getOppositeDir(): There's no opposite direction for Direction.None");
                    break;
            }

            return oppositeDir;
        }
    }
}

