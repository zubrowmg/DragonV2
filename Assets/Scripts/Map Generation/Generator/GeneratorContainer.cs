using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;

// Just holds, no method implementation in this class
public class GeneratorContainer
{
    // ONLY the accessor should directly touch these variables
    public TileMap tileMap;

    public GeneratorContainer()
    {
        this.tileMap = new TileMap();
    }


}
