using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SaveMenu.SetActive(true);

        string fullPath = "Assets/GameSaves/";

        // Get the amount of save files
        DirectoryInfo dir = new DirectoryInfo(fullPath);
        FileInfo[] saveFiles = dir.GetFiles("*");

        GameObject saves = SaveMenu.transform.Find("Saves").transform.gameObject;

        for (int i = 0; i < saveFiles.Length; i++)
        {
            // Create new Save Button and move it to correct location
            GameObject SaveSelect = Instantiate(SaveFileButton, new Vector3(0, 0, 0), Quaternion.identity);
            SaveSelect.transform.SetParent(saves.transform);
            SaveSelect.transform.localPosition = new Vector3(0, 0 - (i * 50), 0);

            // Change the Text of the save button
            SaveSelect.GetComponentInChildren<Text>().text = "Save " + System.Convert.ToString(i + 1);
            SaveSelect.GetComponent<SaveFileScript>().saveId = i + 1;
        }
    }

    public void OpenOptions()
    {
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    // ----------------------------------------------------------------------------------------------
    //     Select Save Menu
    // ----------------------------------------------------------------------------------------------

    public GameObject SaveMenu;
    public GameObject SaveFileButton;

    public static int selectedSaveFile = 1;

    public void OpenWorld()
    {
        //print(selectedSaveFile);
        SceneManager.LoadScene("Assets/Scenes/Game.unity");
    }



    public void CreateNewWorld()
    {
        SceneManager.LoadScene("Assets/Scenes/MapGenerator.unity");
    }

    public void SaveMenuQuit()
    {
        SaveMenu.SetActive(false);
    }

}
