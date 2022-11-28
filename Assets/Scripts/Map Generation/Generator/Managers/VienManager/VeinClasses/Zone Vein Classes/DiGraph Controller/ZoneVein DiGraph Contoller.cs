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

            Debug.Log(message + 
                      "\nEDGE COUNT: " + listOfEdges.Count + 
                      "\nEDGE LIST:");

            string strOutput = "";
            foreach (DiDotEdge<CoordsInt> edge in listOfEdges)
            {
                strOutput = "\tEDGE_" + edge.getId();
                foreach (var coord in edge.getNodeList())
                {
                    strOutput = strOutput + " " + coord.getObject().getPrintString();
                }

                List<DiDotEdge<CoordsInt>> nodeOneEdges = edge.getNodeOneEdgeConnections();
                List<DiDotEdge<CoordsInt>> nodeTwoEdges = edge.getNodeTwoEdgeConnections();

                foreach (var edgeConnection in nodeOneEdges)
                {
                    strOutput = strOutput + "\t" + "E_" + edge.getId() + " <-> " + "E_" + edgeConnection.getId() + ": " + edge.getNodeOne().getObject().getPrintString();
                }

                foreach (var edgeConnection in nodeTwoEdges)
                {
                    strOutput = strOutput + "\t" + "E_" + edge.getId() + " <-> " + "E_" + edgeConnection.getId() + ": " + edge.getNodeTwo().getObject().getPrintString();
                }


                Debug.Log(strOutput);
            }
        }
    }
}
