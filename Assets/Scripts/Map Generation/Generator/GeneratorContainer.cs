using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;

// Just holds, no method implementation in this class
public class GeneratorContainer
{
    // ONLY the accessor should directly touch these variables
    public TileMap tileMap;
    public GameObject garbage;

    public GeneratorContainer(GameObject tileMapGameObject, GameObject garbage)
    {
        this.tileMap = new TileMap(tileMapGameObject);
        this.garbage = garbage;
    }

}
