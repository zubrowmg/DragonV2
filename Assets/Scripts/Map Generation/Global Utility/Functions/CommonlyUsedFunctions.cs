using System.Collections;
using System.Collections.Generic;
using UnityEngine;




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
    }
}

