using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;

public class gridUnitScript : MonoBehaviour
{
    public int x;
    public int y;

    // Properties
    public bool isUsed;
    public bool isOccupied;
    public bool isDoor;
    public GeneralVeinDirection veinDirection;

    // Veins
    public enum GeneralVeinDirection { None, Left, Right, Up, Down };
    public bool isVein;
    public bool isVeinMain;
    public bool isBedRock;

    // POIs
    public bool isPOI;
    public bool isPOICore;
    public bool isPOICoreSmall;
    public bool isPOICoreMedium;
    public bool isPOICoreLarge;

    // GameObject
    public GameObject door;
    public GameObject room;

    // Zones
    //    It is expected that vein preset zones will overwrite the vein tunnels
    public List<int> zoneArea = new List<int>();  // Meant to be used to evaluate zone overlapping and adding a hybird zone
    public List<ZoneUnitProperties> zoneProperties = new List<ZoneUnitProperties>();  

    public intendedUnitType intendedType = intendedUnitType.None;  // Meant to be used to used to show what the grid unit was placed down to be
    public int intendedZoneId = GlobalDefines.defaultId; // Only used when intendedType is a zone
}
