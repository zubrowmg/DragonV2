using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LockingClasses;
using GameObjectCollection;

//namespace GridControllerClasses
//{
    public class gridControllerClasses : MonoBehaviour
    {
        Vector3 positionIncrement = new Vector3(0f, -30f, 0f);
        GameObject button;
        RoomLock roomLock;
        bool toggled;
        string buttonName;

        public gridControllerClasses(RoomLock roomLock, ref GameObject parent, GameObject room, Vector3 prevPosition)
        {
            // Button Properties
            this.buttonName = room.name + "_" + roomLock.getDoorId().ToString();
            this.button = Singleton.getNewGridControllerProgressionButton();
            this.button.name = buttonName;
            this.button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = buttonName;
            this.button.transform.position = prevPosition + positionIncrement;
            this.button.transform.SetParent(parent.transform);



            this.roomLock = roomLock;
            this.toggled = false;
        }

        public RoomLock getRoomLock()
        {
            return roomLock;
        }

        public bool getIsToggled()
        {
            return toggled;
        }

        public Vector3 getButtonPosition()
        {
            return button.transform.position;
        }

        public void toggleButton()
        {
            Debug.Log(this.buttonName);
            toggled = !toggled;
        }
    }
//}
