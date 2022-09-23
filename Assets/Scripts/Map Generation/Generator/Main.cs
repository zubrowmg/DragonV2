using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    const bool generateTileGameObject = true;
    GeneratorWrapper generatorManager;

    void Start()
    {
        Singleton.Init();
        generatorManager = new GeneratorWrapper(generateTileGameObject);

        generatorManager.startGeneration();

    }


}
