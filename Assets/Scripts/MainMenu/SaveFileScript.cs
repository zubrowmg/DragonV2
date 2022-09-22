using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaveFileScript : MonoBehaviour
{
    public int saveId;

    public void SelectWorld()
    {
        MainMenu.selectedSaveFile = saveId;
    }
}
