using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneContainer
{
    List<Zone_New> zoneList = new List<Zone_New>();
    public ZoneContainer()
    {

    }

    public void addZone(ref Zone_New newZone)
    {
        zoneList.Add(newZone);
    }
}
