using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameObjectCollection
{
    public class Blah : MonoBehaviour
    {
        // PreFabAccessor
        static GameObject gridControllerProgressionButton = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/PreFabs/GridController/DefaultRoomLock.prefab", typeof(GameObject));
        
        public static GameObject getNewGridControllerProgressionButton()
        {
            return Instantiate(gridControllerProgressionButton, Vector3.zero, Quaternion.identity) as GameObject;
        }
    }
}
