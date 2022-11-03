using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using CommonlyUsedDefinesAndEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class VeinZone : VeinBase
    {
        // !!!!!!!!!!!!!!!!!!!!!!!!
        //      ZoneVeinGenerator should spit out a this VeinZone Class
        // !!!!!!!!!!!!!!!!!!!!!!!!


        public VeinZone(ref GeneratorContainer contInst, int id, CoordsInt startCoords) : base(ref contInst, id, startCoords)
        {
            this.veinType = VeinType.Zone;
        }


        public override VeinConnection getFurthestVeinConnectorFromStart()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // NOT CORRECT
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return listOfVeinConnections[listOfVeinConnections.Count - 1];
        }
}
}
