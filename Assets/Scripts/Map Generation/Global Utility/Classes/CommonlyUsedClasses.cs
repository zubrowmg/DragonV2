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

        public Coords<T> deepCopy()
        {
            return new Coords<T>(this.x, this.y);
        }

        public T getX()
        {
            return x;
        }

        public T getY()
        {
            return y;
        }

        public void setX(T x)
        {
            this.x = x;
        }

        public void setY(T y)
        {
            this.y = y;
        }

        public void print(string msg)
        {
            Debug.Log(msg + x + "," + y);
        }
    }

    public class TwoDList<T>
    {
        List<List<T>> array;
        T selectedItem = default(T);

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

        public ref T getElement(Coords<int> index)
        {
            selectedItem = array[index.getX()][index.getY()];
            return ref selectedItem;
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
        public static int getIntBasedOnPercentage(params RandomSelection[] selections)
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

    // Meant to be used for variables that are randomly changed during generation
    //      But you want some control of the randomization. Example, used for controlling Vein Slope for simple Veins
    public class TargetProbabilityManager
    {
        float allocatedUnitOfChange;
        float target;
        float currentValue;

        //int degreesOfPossibleChange; // { Inc, Dec, keep } = 3 degrees of possible change
        List<float> percentagesList; // Passed in from Dec*2, Dec, Keep, Inc, Inc*2 etc
        List<float> originalPercentagesList; // Passed in from Dec*2, Dec, Keep, Inc, Inc*2 etc
        List<int> valuesList; // Passed in from Dec*2, Dec, Keep, Inc, Inc*2 etc
        int keepIndex; // Keep index in the list
        //bool keepNewPercentages; // When you inc/dec the percentage, do you store the new values in the percentages list?

        // Calculation variables
        float bias; // .5f is no bias
        int amountOfDec;
        int amountOfInc;

        public TargetProbabilityManager(float allocatedUnitOfChange, float target, float currentValue,
                                    List<float> percentages, List<int> values, int keepIndex, float bias)
        {
            this.allocatedUnitOfChange = allocatedUnitOfChange;
            this.target = target;
            this.currentValue = currentValue;
            //this.degreesOfPossibleChange = degreesOfPossibleChange;
            this.percentagesList = percentages;
            this.originalPercentagesList = percentages;
            //this.keepNewPercentages = keepNewPercentages;
            this.valuesList = values;
            this.keepIndex = keepIndex;
            this.bias = bias;

            for (int i = 0; i < valuesList.Count; i++)
            {
                if (i < keepIndex)
                    amountOfDec++;
                else if (i > keepIndex)
                    amountOfInc++;
            }
        }

        public int getControledRandomizedValue()
        {
            // If our current value is too high, make decrease percentage greater
            if (currentValue > target)
            {
                float allocatedIncUnits = allocatedUnitOfChange * (1 - bias);
                float allocatedDecUnits = allocatedUnitOfChange * bias;
                calculateNewPercentages(allocatedIncUnits, allocatedDecUnits);
            }
            // If our current value is too low, make increase percentage greater
            else if (currentValue < target)
            {
                float allocatedIncUnits = allocatedUnitOfChange * bias;
                float allocatedDecUnits = allocatedUnitOfChange * (1 - bias);
                calculateNewPercentages(allocatedIncUnits, allocatedDecUnits);
            }
            else
            {
                percentagesList = originalPercentagesList;
            }

            RandomProbability.RandomSelection[] selections = new RandomProbability.RandomSelection[valuesList.Count];
            for (int i = 0; i < valuesList.Count; i++)
            {
                selections[i] = new RandomProbability.RandomSelection(valuesList[i], valuesList[i], percentagesList[i]);
            }

            int randValue = RandomProbability.getIntBasedOnPercentage(selections);

            return randValue;
        }

        void calculateNewPercentages(float allocatedIncUnits, float allocatedDecUnits)
        {
            for (int i = 0; i < percentagesList.Count; i++)
            {
                if (i < keepIndex)
                    percentagesList[i] = allocatedDecUnits / (float)amountOfDec;
                else if (i > keepIndex)
                    percentagesList[i] = allocatedIncUnits / (float)amountOfInc;
            }
        }

    }

    public class RangeLimit
    {
        int min;
        int max; 

        public RangeLimit(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool valueIsInBetweenRange(int val)
        {
            bool valIsGood = false;
            if (min <= val && val <= max)
                valIsGood = true;
            return valIsGood;
        }

        public int getMin()
        {
            return this.min;
        }
    }
}
