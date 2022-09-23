using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// THIS IS JUST A TEMPORARY CLASS, RENAME DELETE DO WHATEVER ONCE YOU GET HERE
//      Just wanted to show how the container and wrapper work
public class VeinManager : ContainerAccessor
{

    public VeinManager(ref GeneratorContainer contInst) : base(ref contInst)
    {
    }

    public void test1()
    {
        List<int> test1 = getTileMap();

        foreach (var i in test1)
        {
            Debug.Log(i);
        }

        test1 = new List<int> { 9, 8, 6 };

        setTileMap(test1);
    }
}
