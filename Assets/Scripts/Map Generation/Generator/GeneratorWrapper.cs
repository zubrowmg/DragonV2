using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class GeneratorWrapper : IntfGridManager, IntfVeinManager
{
    GameObject main;
    GridManager gridManager;
    VeinManager veinManager;

    public GeneratorWrapper(bool generateGridManagerTile)
    {
        gridManager = new GridManager(generateGridManagerTile);
        veinManager = new VeinManager();
    }
    ~GeneratorWrapper() { }

}

// ----------------------------- Grid Manager -----------------------------
public partial class GeneratorWrapper : IntfGridManager, IntfVeinManager
{
    public void accessGridManagerGrid()
    {
        gridManager.accessGrid();
    }
}

// ----------------------------- Vein Manager Access -----------------------------
public partial class GeneratorWrapper : IntfGridManager, IntfVeinManager
{
    public void veinTest()
    {
        veinManager.test();
    }

    public void accessGridManagerGrid___ForVeinMananger()
    {
        gridManager.accessGrid();
    }
}
