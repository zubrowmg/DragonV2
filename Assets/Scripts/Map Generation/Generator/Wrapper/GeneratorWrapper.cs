using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GeneratorWrapper
{
    GameObject main;

    // Generation managers, each take care of a specific part of the generation process
    TileManager tileManager;
    VeinManager veinManager;

    // Contains any classes that map generator managers will need to share, don't abuse this by placing everything and anything in here
    GeneratorContainer commonContainer;

    public GeneratorWrapper(bool generateGridManagerTile, GameObject tileMapGameObject, GameObject garbage)
    {
        commonContainer = new GeneratorContainer(tileMapGameObject, garbage);
        tileManager = new TileManager(generateGridManagerTile, ref commonContainer);
        veinManager = new VeinManager(ref commonContainer);
    }
    ~GeneratorWrapper() { }


    public void startGeneration()
    {
        // First we need to create the tile map that's the base for all generation
        tileManager.createTileMap();
        veinManager.startVeinGeneration();
    }
}


