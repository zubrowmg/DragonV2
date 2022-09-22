using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Class will grab objects from Assets/PreFabs/
public class PreFabManager
{
    public PreFabManager() { }


    // ----------------------- Tile -----------------------
    GameObject tile = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/PreFabs/Grid Manager/Tile.prefab", typeof(GameObject));

    public GameObject getNewTile()
    {
        return tile;
    }

    // ----------------------- Starting Rooms -----------------------



    // ----------------------- Old PreFab Example -----------------------
    GameObject gridControllerProgressionButton = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/PreFabs/GridController/DefaultRoomLock.prefab", typeof(GameObject));

    public GameObject getNewGridControllerProgressionButton()
    {
        return gridControllerProgressionButton;
    }
}
