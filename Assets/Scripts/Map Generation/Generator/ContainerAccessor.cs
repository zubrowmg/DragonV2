using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Common class to access common variables in GeneratorContainer
//      Each manager that needs access to anything in the GeneratorContainer needs to inherit this class
public partial class ContainerAccessor
{
    GeneratorContainer contInst;

    public ContainerAccessor(ref GeneratorContainer contInst)
    {
        this.contInst = contInst;
    }
}


// ==========================================================
//              Tile Manager Accessors
// ==========================================================
public partial class ContainerAccessor
{
    public void setTileMap(List<int> test)
    {
        contInst.tileMap.setTileMap(test);
    }

    public List<int> getTileMap()
    {
        return contInst.tileMap.getTileMap();
    }
}
