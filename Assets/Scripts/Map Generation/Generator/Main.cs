using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Main : MonoBehaviour
{
    bool debugMode = true;
    bool generateTileGameObject = false;
    bool enabledGameObjectIfTouched = true;
    GeneratorWrapper generatorManager;

    // Game Objects
    public GameObject tileMapGameObject;
    public GameObject garbage;
    public GameObject debugManager;

    void Start()
    {
        System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();

        int seed = 1550803247;// (int)System.DateTime.Now.Ticks;
        seed = seed < 0 ? seed * -1 : seed;
        Random.InitState(seed);
        Debug.Log("Seed: " + seed);

        Singleton.Init();
        generatorManager = new GeneratorWrapper(debugMode, generateTileGameObject, enabledGameObjectIfTouched, tileMapGameObject, garbage);

        generatorManager.startGeneration();

        stopWatch.Stop();
        Debug.Log("Generation Time:  " + (stopWatch.ElapsedMilliseconds * .001) + "s");
        Debug.LogError("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\nWHENEVER YOU ARE DONE WITH A FEATURE CHECK THE ENHANCEMENT TAB IN BINDER\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Debug.LogError("CENTER COORD IN DIM LIST CLASS NEEDS TO BE DECIDED BY A ROUGH MEDIAN (X MEDIAN, Y MEDIAN) OF ALL 1 GRID TILES");


        // Give the debug manager access to generation manager
        debugManager.GetComponent<DebugControllerManager>().init(ref generatorManager);
    }


}
