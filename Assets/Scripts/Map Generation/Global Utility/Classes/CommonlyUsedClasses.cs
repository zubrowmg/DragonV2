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
        
        public string getPrintString()
        {
            return "(" + x.ToString() + "," + y.ToString() + ")";
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

        public int getXCount()
        {
            return array.Count;
        }

        public int getYCount()
        {
            return array[0].Count;
        }

        public int getYCount(int index)
        {
            return array[index].Count;
        }

        public T getZeroZero()
        {
            return this.array[0][0].getTwo();
        }

        public bool isInsideBounds(CoordsInt coords)
        {
            bool isInside = true;

            // Boundry check
            if (0 <= coords.getX() && coords.getX() < getXCount() &&
                0 <= coords.getY() && coords.getY() < getYCount())
            {
                // Do nothing, it's within bounds
            }
            else
            {
                isInside = false;
            }

            return isInside;
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

        public ref T1 getOne()
        {
            return ref this.one;
        }

        public ref T2 getTwo()
        {
            return ref this.two;
        }

        public void setOne(T1 item)
        {
            this.one = item;
        }

        public void setTwo(T2 item)
        {
            this.two = item;
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


    public abstract class OrderValuesBase<T1, T2>
    {
        // First value is the priority
        //      If using Min first value is the smallest
        //      If using Max first value is the largest
        protected LinkedList<KeyValuePair<T1, T2>> queue;
        protected int queueSize;

        public OrderValuesBase(int size)
        {
            queue = new LinkedList<KeyValuePair<T1, T2>>();
            queueSize = size;
        }

        public int getCount()
        {
            return queue.Count;
        }

        public KeyValuePair<T1, T2> randomlyChooseKeyValuePair()
        {
            KeyValuePair<T1, T2> randomPair = new KeyValuePair<T1, T2>();

            int randNumber = Random.Range(0, queue.Count);
            int count = 0;
            for (LinkedListNode<KeyValuePair<T1, T2>> pair = queue.First; pair != null; pair = pair.Next)
            {
                if (count == randNumber)
                {
                    randomPair = pair.Value;
                    break;
                }

                count++;
            }

            return randomPair;
        }

        public void addValueToQueue(T1 keyInt, T2 value)
        {
            KeyValuePair<T1, T2> newValue = new KeyValuePair<T1, T2>(keyInt, value);

            if (queue.Count == 0)
                queue.AddFirst(newValue);
            else
            {
                for (LinkedListNode<KeyValuePair<T1, T2>> pair = queue.First; pair != null; pair = pair.Next)
                {
                    if (compareCheck(keyInt, pair))
                    {
                        queue.AddBefore(pair, newValue);

                        if (queue.Count > queueSize)
                            queue.RemoveLast();

                        break;
                    }
                }
            }
        }

        protected abstract bool compareCheck(T1 keyInt, LinkedListNode<KeyValuePair<T1, T2>> pair);
    }

    public class MaxValue<T1, T2> : OrderValuesBase<T1, T2>
    {
        public MaxValue(int size) : base(size) {}

        protected override bool compareCheck(T1 keyInt, LinkedListNode<KeyValuePair<T1, T2>> pair)
        {
            bool check = false;
            int c = Comparer<T1>.Default.Compare(keyInt, pair.Value.Key);

            // keyInt > pair.Value.Key
            if (c > 0)
            {
                check = true;
            }

            return check;
        }

        public LinkedList<KeyValuePair<T1, T2>> getMaxValues()
        {
            return queue;
        }

        public KeyValuePair<T1, T2> getMaxVal()
        {
            KeyValuePair<T1, T2> pair = queue.First.Value;
            return pair;
        }
    }

    public class MinValue<T1, T2> : OrderValuesBase<T1, T2>
    {
        public MinValue(int size) : base (size) {}


        protected override bool compareCheck(T1 keyInt, LinkedListNode<KeyValuePair<T1, T2>> pair)
        {
            bool check = false;
            int c = Comparer<T1>.Default.Compare(keyInt, pair.Value.Key);

            // keyInt < pair.Value.Key
            if (c < 0)
            {
                check = true;
            }

            return check;
        }


        public LinkedList<KeyValuePair<T1, T2>> getMinValues()
        {
            return queue;
        }

        public KeyValuePair<T1, T2> getMinVal()
        {
            KeyValuePair<T1, T2> pair = queue.First.Value; 
            return pair;
        }
        
    }

    public class QueueWrapper<T>
    {
        List<T> queue = new List<T>();
        int maxSize;

        public QueueWrapper()
        {
            this.maxSize = System.Int32.MaxValue;
        }

        public QueueWrapper(int maxSize)
        {
            this.maxSize = maxSize;
        }

        public void enqueue(T item)
        {
            queue.Add(item);

            if (queue.Count > maxSize)
                deque();
        }

        public void deque()
        {
            queue.RemoveAt(0);
        }

        public T enqueueGetRemovedItem(T item, out bool itemOverflow)
        {
            itemOverflow = false;
            queue.Add(item);
            T removedItem = default(T);

            if (queue.Count > maxSize)
            {
                itemOverflow = true;
                removedItem = dequeGetRemovedItem();
            }

            return removedItem;
        }

        public T dequeGetRemovedItem()
        {
            T removedItem = queue[0];
            queue.RemoveAt(0);
            return removedItem;
        }

        public T dequeLastAdded(out bool queueEmpty)
        {
            int index = queue.Count - 1;
            queueEmpty = false;

            if (index < 0)
            {
                queueEmpty = true;
                return default(T);
            }
            

            T item = queue[index];
            queue.RemoveAt(index);

            return item;
        }

        public int getCount()
        {
            return queue.Count;
        }

        public T getElement(int index)
        {
            return queue[index];
        }

        public ref List<T> getRawQueueList()
        {
            return ref this.queue;
        }
    }
}
