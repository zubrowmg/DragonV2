using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;

namespace CommonlyUsedClasses
{
    public class Coords<T>
    {
        protected T x;
        protected T y;

        public Coords(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public virtual Coords<T> deepCopy()
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

        public void set(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public void print(string msg)
        {
            Debug.Log(msg + x + "," + y);
        }
        
    }

    public class CoordsInt : Coords<int>
    {
        public CoordsInt(int x, int y) : base (x, y)
        {}

        public CoordsInt deepCopyInt()
        {
            return new CoordsInt(this.x, this.y);
        }

        public void incX(int num)
        {
            this.x = this.x + num;
        }

        public void incY(int num)
        {
            this.y = this.y + num;
        }

        public void decX(int num)
        {
            this.x = this.x - num;
        }

        public void decY(int num)
        {
            this.y = this.y - num;
        }

        public void incX()
        {
            incX(1);
        }

        public void incY()
        {
            incY(1);
        }

        public void decX()
        {
            decX(1);
        }

        public void decY()
        {
            decY(1);
        }
    }

    public class TwoDList<T>
    {
        List<List<Double<CoordsInt, T>>> array;
        Double<CoordsInt, T> selectedItem = new Double<CoordsInt, T>(default(CoordsInt), default(T));
        T selectedTItem = default(T);

        public TwoDList()
        {
            this.array = new List<List<Double<CoordsInt, T>>>();
        }

        public TwoDList(List<List<Double<CoordsInt, T>>> newArray)
        {
            this.array = newArray;
        }

        public void addElement(CoordsInt index, T item)
        {
            addRefElement(index, ref item);
        }

        public void addRefElement(CoordsInt index, ref T item)
        {
            Double<CoordsInt, T> newItem = new Double<CoordsInt, T>(index, item);

            // If the x index is greater than x axis need a new x row
            if (array.Count - 1 < index.getX())
                array.Add(new List<Double<CoordsInt, T>> { newItem });
            else
                array[index.getX()].Add(newItem);
        }


        public ref T getElement(CoordsInt index)
        {
            selectElement(index);
            return ref selectedTItem;
        }

        public void selectElement(CoordsInt index)
        {
            this.selectedItem = array[index.getX()][index.getY()];
            this.selectedTItem = this.selectedItem.getTwo();
        }

        public CoordsInt goLeft(int amount, out bool rejected, out T selectedTItem)
        {
            rejected = false;
            CoordsInt currentCoords = selectedItem.getOne().deepCopyInt();

            if (currentCoords.getX() - amount < 0)
                rejected = true;
            else
                currentCoords.decX(amount);

            selectedTItem = getElement(currentCoords);

            return currentCoords;
        }

        public CoordsInt goDown(int amount, out bool rejected, out T selectedTItem)
        {
            rejected = false;
            CoordsInt currentCoords = selectedItem.getOne().deepCopyInt();

            if (currentCoords.getY() - amount < 0)
                rejected = true;
            else
                currentCoords.decY(amount);

            selectedTItem = getElement(currentCoords);

            return currentCoords;
        }

        public CoordsInt goRight(int amount, out bool rejected, out T selectedTItem)
        {
            rejected = false;
            CoordsInt currentCoords = selectedItem.getOne().deepCopyInt();

            if (currentCoords.getX() + amount >= getXCount())
                rejected = true;
            else
                currentCoords.incX(amount);

            selectedTItem = getElement(currentCoords);

            return currentCoords;
        }

        public CoordsInt goUp(int amount, out bool rejected, out T selectedTItem)
        {
            rejected = false;
            CoordsInt currentCoords = selectedItem.getOne().deepCopyInt();

            if (currentCoords.getY() + amount >= getYCount())
                rejected = true;
            else
                currentCoords.incY(amount);

            selectedTItem = getElement(currentCoords);

            return currentCoords;
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

    public class Double<T1, T2>
    {
        protected T1 one;
        protected T2 two;

        public Double(T1 one, T2 two)
        {
            this.one = one;
            this.two = two;
        }

        public Double<T1, T2> deepCopy()
        {
            return new Double<T1, T2>(this.one, this.two);
        }

        public T1 getOne()
        {
            return this.one;
        }

        public T2 getTwo()
        {
            return this.two;
        }

    }

    public class Triple<T1, T2, T3> : Double<T1, T2>
    {
        protected T3 three;

        public Triple(T1 one, T2 two, T3 three) : base (one, two)
        {
            this.one = one;
            this.two = two;
        }

        public T3 getThree()
        {
            return this.three;
        }
    }

    public class DirectionBias
    {
        Direction horizontalDir;
        Direction verticalDir;

        public DirectionBias(Direction horizontalDir, Direction verticalDir)
        {
            if (horizontalDir == Direction.South || horizontalDir == Direction.North)
                Debug.LogError("DirectionBias - Attempted to set horizontal bias to either North or South");
            if (verticalDir == Direction.East || verticalDir == Direction.West)
                Debug.LogError("DirectionBias - Attempted to set vertical bias to either East or West");

            this.horizontalDir = horizontalDir;
            this.verticalDir = verticalDir;
        }

        public Direction getHorizontalDir()
        {
            return this.horizontalDir;
        }

        public Direction getVerticalDir()
        {
            return this.verticalDir;
        }

        public void print()
        {
            Debug.Log("Horizontal: " + this.horizontalDir +
                   "\nVertical: " + this.verticalDir);
        }
    }
}
