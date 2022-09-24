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
        Singleton.Init();
        generatorManager = new GeneratorWrapper(generateTileGameObject, tileMapGameObject, garbage);

        generatorManager.startGeneration();

    }


}
