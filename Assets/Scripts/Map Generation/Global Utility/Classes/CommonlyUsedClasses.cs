using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonlyUsedClasses
{
    public class Coords<T>
    {
        T x;
        T y;

        public Coords(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public T getX()
        {
            return x;
        }

        public T getY()
        {
            return y;
        }
    }

    public class Dimensions
    {
        Coords<int> minCoords;
        Coords<int> maxCoords;

        public Dimensions(Coords<int> minCoords, Coords<int> maxCoords)
        {
            this.minCoords = minCoords;
            this.maxCoords = maxCoords;
        }

        public int getMinX()
        {
            return minCoords.getX();
        }

        public int getMinY()
        {
            return minCoords.getY();
        }

        public int getMaxX()
        {
            return maxCoords.getX();
        }

        public int getMaxY()
        {
            return maxCoords.getY();
        }
    }

    public class TwoDList<T>
    {
        List<List<T>> array;

        public TwoDList()
        {
            this.array = new List<List<T>>();
        }

        public TwoDList(List<List<T>> newArray)
        {
            this.array = newArray;
        }

        public void addElement(Coords<int> index, T item)
        {
            // If the x index is greater than x axis need a new x row
            if (array.Count - 1 < index.getX())
                array.Add(new List<T> { item });
            else
                array[index.getX()].Add(item);
        }

        public int getXCount()
        {
            return array.Count;
        }

        public int getYCount()
        {
            return array[0].Count;
        }
    }

    public class RandomProbability
    {
        public struct RandomSelection
        {
            private int minValue;
            private int maxValue;
            public float probability;

            public RandomSelection(int minValue, int maxValue, float probability)
            {
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.probability = probability;
            }

            public int GetValue() { return Random.Range(minValue, maxValue + 1); }
        }


        // Probabilities need to add up to 1
        public int getIntBasedOnPercentage(params RandomSelection[] selections)
        {
            float rand = Random.value;
            float currentProb = 0;
            foreach (var selection in selections)
            {
                currentProb += selection.probability;
                if (rand <= currentProb)
                    return selection.GetValue();
            }

            return -1;
        }
    }
}
