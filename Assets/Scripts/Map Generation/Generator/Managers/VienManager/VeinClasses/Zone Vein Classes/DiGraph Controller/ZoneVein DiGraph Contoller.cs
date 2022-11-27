using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;

namespace VeinManagerClasses
{
    public class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        ZoneVeinGeneratorContainer zoneVeinGenContainer;
        DiDotGraph<CoordsInt> diGraph;

        public ZoneVeinDiGraphContoller(ref ZoneVeinGeneratorContainer zoneVeinGenContainerInst, ref GeneratorContainer contInst) : base(ref contInst)
        {
            this.zoneVeinGenContainer = zoneVeinGenContainerInst;
        }

        // =====================================================================================
        //                                    Init Functions
        // =====================================================================================

        public void init()
        {
            this.diGraph = new DiDotGraph<CoordsInt>();
        }

        // =====================================================================================
        //                              DiGraph Control Functions
        // =====================================================================================

        // Add a line of nodes to the graph
        public void addNodes(List<CoordsInt> coords)
        {
            // Adding two points at a time
            for (int i = 0; i < coords.Count - 1; i++)
            {
                CoordsInt prevCoord = coords[i];
                CoordsInt currentCoord = coords[i + 1];

                //prevCoord.print("FIRST COORDS: ");
                //currentCoord.print("SECOND COORDS: ");

                this.diGraph.addObject(ref prevCoord, ref currentCoord);
            }
        }

        public void decideEndPoints()
        {
            // Analyze the graph
            this.diGraph.analyzeGraph();

            this.print("TEMP");

            // Detemine what needs to be done to the graph

        }

        void print(string message)
        {
            List<DiDotEdge<CoordsInt>> listOfEdges = this.diGraph.getListOfEdges();

            Debug.Log(message);
            Debug.Log("EDGE COUNT: " + listOfEdges.Count);
            Debug.Log("EDGE LIST:");

            string edgeList = "";
            int i = 0;
            foreach (DiDotEdge<CoordsInt> edge in listOfEdges)
            {
                edgeList = "\tEDGE_" + i;
                foreach (var coord in edge.getNodeList())
                {
                    edgeList = edgeList + " " + coord.getObject().getPrintString();
                }
                i++;
                Debug.Log(edgeList);
            }
        }
    }
}
