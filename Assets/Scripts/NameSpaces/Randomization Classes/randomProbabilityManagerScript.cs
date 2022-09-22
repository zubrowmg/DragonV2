using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Randomization
{
    public class RandomClass
    {
        public RandomClass()
        {
        }

        public List<T> Shuffle<T>(List<T> list)
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
    }
}

// Maybe at some point add this to the randomization namespace
public class randomProbabilityManagerScript : MonoBehaviour
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
