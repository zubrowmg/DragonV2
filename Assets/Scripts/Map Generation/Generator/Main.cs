using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    const bool generateTileGameObject = true;
    GeneratorWrapper generatorManager;

    public GameObject tileMapGameObject;
    public GameObject garbage;

    void Start()
    {
        int seed = 1550803247;// (int)System.DateTime.Now.Ticks;
        seed = seed < 0 ? seed * -1 : seed;
        Random.InitState(seed);
        Debug.Log("Seed: " + seed);

        Singleton.Init();
        generatorManager = new GeneratorWrapper(generateTileGameObject, tileMapGameObject, garbage);

        generatorManager.startGeneration();

    }


}
