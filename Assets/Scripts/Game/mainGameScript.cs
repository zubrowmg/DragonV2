using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

using jsonFileFormatter;

public class mainGameScript : MonoBehaviour
{
    public GameObject loadedMapObject;
    public GameObject MainCamera;
    public GameObject Player;

    public GameObject InnerCorner;
    public GameObject OuterCorner;
    public GameObject HorizontalWall;
    public GameObject VerticalWall;
    public GameObject ThreeQuater_HorizontalWall;
    public GameObject ThreeQuater_VerticalWall;



    // Start is called before the first frame update
    void Start()
    {

        // Load the Save File
        Dictionary<int, Dictionary<int, List<GameObject>>> roomList = loadMap(MainMenu.selectedSaveFile);

        // Change the camera and player to the start room
        setup(roomList);
    }

    public void setup(Dictionary<int, Dictionary<int, List<GameObject>>>  roomList)
    {
        Vector3 pos = roomList[0][0][0].transform.position;

        MainCamera.transform.position = new Vector3(pos.x, pos.y, MainCamera.transform.position.z);
        Player.transform.position = new Vector3(pos.x + 1, pos.y + 1, pos.z);
    }

    public Dictionary<int, Dictionary<int, List<GameObject>>> loadMap(int selectedSaveFile)
    {
        int saveFile = selectedSaveFile;
        string savePath = Application.dataPath + "/GameSaves/Save";
        savePath = savePath + saveFile.ToString() + "/MapSave.json";

        jsonRoomList roomList = JsonConvert.DeserializeObject<jsonRoomList>(File.ReadAllText(savePath)) as jsonRoomList;


        Dictionary<int, Dictionary<int, List<GameObject>>> renderedZoneList = new Dictionary<int, Dictionary<int, List<GameObject>>>();
        Dictionary<int, List<GameObject>> renderedRoomList = new Dictionary<int, List<GameObject>>();
        List<GameObject> renderedPieces = new List<GameObject>();

        float unit = InnerCorner.GetComponent<SpriteRenderer>().bounds.size.y;
        //print(unit);

        // Go through each room
        for (int i = 0; i < roomList.list.Count; i++)
        {
            jsonReadyRoom currentRoom = roomList.list[i];

            int roomId = currentRoom.roomId;
            int zone = currentRoom.zone;

            float xStart = currentRoom.gridXPos * unit;
            float yStart = currentRoom.gridYPos * unit;

            renderedPieces = new List<GameObject>();

            // Go through all inner grid attributes and spawn the pieces in
            for (int x = 0; x < currentRoom.innerGrid.Count; x++)
            {
                for (int y = 0; y < currentRoom.innerGrid[0].Count; y++)
                {
                    GameObject instantiatedPiece = null;
                    float newXPos = xStart + (x * unit);
                    float newYPos = yStart + (y * unit);

                    if (currentRoom.innerGrid[x][y] == "WN")
                    {
                        newYPos = (newYPos + unit / 2) - (HorizontalWall.GetComponent<SpriteRenderer>().bounds.size.y / 2);
                        instantiatedPiece = Instantiate(HorizontalWall, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "WS")
                    {
                        newYPos = (newYPos - unit / 2) + (HorizontalWall.GetComponent<SpriteRenderer>().bounds.size.y / 2);
                        instantiatedPiece = Instantiate(HorizontalWall, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "WE")
                    {
                        newXPos = (newXPos + unit / 2) - VerticalWall.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        instantiatedPiece = Instantiate(VerticalWall, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "WW")
                    {
                        newXPos = (newXPos - unit / 2) + VerticalWall.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        instantiatedPiece = Instantiate(VerticalWall, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "ICNE")
                    {
                        instantiatedPiece = Instantiate(InnerCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                        instantiatedPiece.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
                    }
                    else if (currentRoom.innerGrid[x][y] == "ICNW")
                    {
                        instantiatedPiece = Instantiate(InnerCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                        instantiatedPiece.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
                    }
                    else if (currentRoom.innerGrid[x][y] == "ICSE")
                    {
                        instantiatedPiece = Instantiate(InnerCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                        instantiatedPiece.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
                    }
                    else if (currentRoom.innerGrid[x][y] == "ICSW")
                    {
                        instantiatedPiece = Instantiate(InnerCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "OCNE")
                    {
                        // 111 
                        // 001
                        // 001
                        newXPos = (newXPos - unit / 2) + OuterCorner.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        newYPos = (newYPos - unit / 2) + OuterCorner.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                        instantiatedPiece = Instantiate(OuterCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "OCNW")
                    {
                        newXPos = (newXPos + unit / 2) - OuterCorner.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        newYPos = (newYPos - unit / 2) + OuterCorner.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                        instantiatedPiece = Instantiate(OuterCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "OCSE")
                    {
                        newXPos = (newXPos - unit / 2) + OuterCorner.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        newYPos = (newYPos + unit / 2) - OuterCorner.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                        instantiatedPiece = Instantiate(OuterCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }
                    else if (currentRoom.innerGrid[x][y] == "OCSW")
                    {
                        newXPos = (newXPos + unit / 2) - OuterCorner.GetComponent<SpriteRenderer>().bounds.size.x / 2;
                        newYPos = (newYPos + unit / 2) - OuterCorner.GetComponent<SpriteRenderer>().bounds.size.y / 2;
                        instantiatedPiece = Instantiate(OuterCorner, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                    }

                    if (instantiatedPiece != null)
                    {
                        instantiatedPiece.transform.parent = loadedMapObject.transform;
                        renderedPieces.Add(instantiatedPiece);
                    }
                }
            }

            // If this is a zone's first room instantiate the list
            if (!renderedZoneList.ContainsKey(zone))
            {
                renderedRoomList.Add(roomId, renderedPieces);
                renderedZoneList.Add(zone, renderedRoomList);
            }
            else
            {
                renderedZoneList[zone].Add(roomId, renderedPieces);
            }
        }

        return renderedZoneList;
    }

}
