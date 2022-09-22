using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    static PreFabManager preFabManager;

    public static void Init()
    {
        Singleton.preFabManager = new PreFabManager();
    }

    public static void test()
    {

    }

    // ------------------ Getters ------------------
    public static GameObject instantiateTile()
    {
        return Instantiate(Singleton.preFabManager.getNewTile(), new Vector3(0, 0, 0), Quaternion.identity);
    }

    // ------------------ Setters ------------------
}
