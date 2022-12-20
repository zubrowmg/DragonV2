using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace CommonlyUsedFunctions
{

    public static class CommonFunctions
    {

        public static T randomlySelectInList<T>(ref List<T> list)
        {
            int rand = Random.Range(0, list.Count);
            T selectedItem = list[rand];
            return selectedItem;
        } 

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


        public static CoordsInt calculateCoordsAverage(Coords<int> oneCoord, Coords<int> twoCoord)
        {
            int xAverage = Mathf.FloorToInt(((float)oneCoord.getX() + (float)twoCoord.getX()) / (float)2);
            int yAverage = Mathf.FloorToInt(((float)oneCoord.getY() + (float)twoCoord.getY()) / (float)2);
            return new CoordsInt(xAverage, yAverage);
        }

        public static CoordsInt calculateCoordsAverage(List<CoordsInt> coordList)
        {
            int xSum = 0;
            int ySum = 0;

            foreach (var coords in coordList)
            {
                xSum = xSum + coords.getX();
                ySum = ySum + coords.getY();
            }
            
            int xAverage = Mathf.FloorToInt(((float)xSum) / (float)coordList.Count);
            int yAverage = Mathf.FloorToInt(((float)ySum) / (float)coordList.Count);
            return new CoordsInt(xAverage, yAverage);
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

        public static void addIfItemDoesntExist<T>(ref List<T> list, T item)
        {
            if (list.Contains(item) == false)
                list.Add(item);
        }

        public static void addIfItemDoesntExist<T>(ref List<T> list, List<T> itemList)
        {
            foreach (var currentItem in itemList)
                addIfItemDoesntExist(ref list, currentItem);
        }

        public static List<T> getSmallestListCount<T>(List<List<T>> list)
        {
            List<T> smallestCount = list[0];
            foreach (var indexList in list)
            {
                if (indexList.Count < smallestCount.Count)
                    smallestCount = indexList;
            }
            return smallestCount;
        }

        public static int calculateArea(CoordsInt minCoords, CoordsInt maxCoords)
        {
            int xDiff = (int)CommonFunctions.calculateDifference(minCoords.getX(), maxCoords.getX());
            int yDiff = (int)CommonFunctions.calculateDifference(minCoords.getY(), maxCoords.getY());

            int area = (xDiff + 1) * (yDiff + 1);
            return area;
        }

        public static DirectionBias calculatePrimaryDirection(CoordsInt startCoords, CoordsInt endCoords)
        {
            int displacementNeeded = 10;
            int xDiff = endCoords.getX() - startCoords.getX();
            int yDiff = endCoords.getY() - startCoords.getY();

            Direction primaryHorizontalDir = Direction.None;
            Direction primaryVerticalDir = Direction.None;

            if (xDiff >= displacementNeeded)
                primaryHorizontalDir = Direction.East;
            else if (xDiff <= displacementNeeded)
                primaryHorizontalDir = Direction.West;

            if (yDiff >= displacementNeeded)
                primaryVerticalDir = Direction.None;
            else if (yDiff <= displacementNeeded)
                primaryVerticalDir = Direction.South;

            DirectionBias newDirectionBias = new DirectionBias(primaryHorizontalDir, primaryVerticalDir);
            return newDirectionBias;
        }
    }
}

