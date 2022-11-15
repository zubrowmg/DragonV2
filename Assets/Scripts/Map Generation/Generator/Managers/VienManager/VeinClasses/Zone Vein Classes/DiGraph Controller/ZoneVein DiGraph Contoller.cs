using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VeinManagerClasses
{
    public class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        ZoneVeinGeneratorContainer zoneVeinGenContainer;

        public ZoneVeinDiGraphContoller(ref ZoneVeinGeneratorContainer zoneVeinGenContainerInst, ref GeneratorContainer contInst) : base(ref contInst)
        {
            this.zoneVeinGenContainer = zoneVeinGenContainerInst;
        }

        // =====================================================================================
        //                                    Init Functions
        // =====================================================================================

        public void init()
        {

        }

        // =====================================================================================
        //                              DiGraph Control Functions
        // =====================================================================================

        public void addNode()
        {

        }
    }
}
