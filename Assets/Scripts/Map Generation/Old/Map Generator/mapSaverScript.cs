using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

using jsonFileFormatter;

public class mapSaverScript : MonoBehaviour
{
    public GameObject loadedMapObject;

    // Needed to save as JSON file
    public void saveMap(List<GameObject> roomList)
    {
        // Go through each room, rooms at this point should have these defined:
        //    Wall, Door, Platform positions
        //    Loot, mini bosses, encounter positions
        //    Zone type

        int saveFile = 1;

        string savePath = Application.dataPath + "/GameSaves/Save";
        savePath = savePath + saveFile.ToString() + "/MapSave.json";

        // Clear output file
        File.WriteAllText(savePath, "");

        jsonRoomList jsonList = new jsonRoomList();

        for (int i = 0; i < roomList.Count; i++)
        {
            GameObject currentRoom = roomList[i];

            //installDoorsToInnerGrid(ref currentRoom);
            jsonReadyRoom saveReady = convertRoomGameObjToClass(currentRoom, i);
            jsonList.list.Add(saveReady);
        }

        saveRooms(jsonList, savePath);

    }

   


    void saveRooms(jsonRoomList rooms, string savePath)
    {
        // Need to create a JSON compatible class
        //jsonReadyRoom saveReady = convertRoomGameObjToClass(rooms, roomId);

        string output = JsonConvert.SerializeObject(rooms, Formatting.Indented);
        File.AppendAllText(savePath, output);
    }

    jsonReadyRoom convertRoomGameObjToClass(GameObject room, int roomId)
    {
        int zone = 0;
        int gridXPos = room.GetComponent<roomProperties>().gridCoords.x;
        int gridYPos = room.GetComponent<roomProperties>().gridCoords.y;

        List<List<string>> innerGrid = room.GetComponent<roomProperties>().innerGrid;

        jsonReadyRoom saveRoom = new jsonReadyRoom(roomId, zone, gridXPos, gridYPos, innerGrid);

        return saveRoom;
    }
}
