using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    const bool generateTileGameObject = false;
    const bool enabledGameObjectIfTouched = true;
    GeneratorWrapper generatorManager;

    // Game Objects
    public GameObject tileMapGameObject;
    public GameObject garbage;
    public GameObject debugManager;

    void Start()
    {
        int seed = 1550803247;// (int)System.DateTime.Now.Ticks;
        seed = seed < 0 ? seed * -1 : seed;
        Random.InitState(seed);
        Debug.Log("Seed: " + seed);

        Singleton.Init();
        generatorManager = new GeneratorWrapper(generateTileGameObject, enabledGameObjectIfTouched, tileMapGameObject, garbage);

        generatorManager.startGeneration();

        // Give the debug manager access to generation manager
        debugManager.GetComponent<DebugControllerManager>().init(ref generatorManager);
    }


}
