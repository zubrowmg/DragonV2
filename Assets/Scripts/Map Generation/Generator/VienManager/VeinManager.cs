using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IntfVeinManager
{
    void accessGridManagerGrid___ForVeinMananger();
}

// THIS IS JUST A TEMPORARY CLASS, RENAME DELETE DO WHATEVER ONCE YOU GET HERE
//      Just wanted to show an interface example
public class VeinManager : IntfVeinManager
{
    public void test()
    {
        Debug.Log("VeinManager Accessing Grid Manager TEST");
        accessGridManagerGrid___ForVeinMananger();
    }

    public void accessGridManagerGrid___ForVeinMananger()
    {
        // Dummy function, doesn't do anything
    }
}
