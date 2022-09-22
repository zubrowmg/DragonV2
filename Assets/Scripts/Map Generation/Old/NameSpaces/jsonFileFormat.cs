using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


namespace jsonFileFormatter
{
    [System.Serializable]
    public class jsonReadyRoom
    {
        [JsonProperty("roomId")]
        public int roomId { get; set; }

        [JsonProperty("gridXPos")]
        public int gridXPos { get; set; }
        [JsonProperty("gridYPos")]
        public int gridYPos { get; set; }

        //public List<string> adjacentRoom;
        [JsonProperty("zone")]
        public int zone;

        [JsonProperty("innerGrid")]
        public List<List<string>> innerGrid { get; set; }

        public jsonReadyRoom(int roomId, int zone, int gridXPos, int gridYPos, List<List<string>> innerGrid)
        {
            this.roomId = roomId;
            this.zone = zone;
            this.gridXPos = gridXPos;
            this.gridYPos = gridYPos;
            this.innerGrid = innerGrid;
        }
    }

    [System.Serializable]
    public class jsonRoomList
    {
        public List<jsonReadyRoom> list = new List<jsonReadyRoom>();
    }
}

