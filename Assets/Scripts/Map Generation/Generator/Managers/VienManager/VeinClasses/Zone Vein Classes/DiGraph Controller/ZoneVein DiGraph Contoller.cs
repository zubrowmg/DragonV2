using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;
using CommonlyUsedFunctions;
using VeinEnums;

namespace VeinManagerClasses
{

    public class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //  Future enhancement idea, for zones that need to have more of an identy refer to enhacements section in binder
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        ZoneVeinGeneratorContainer zoneVeinGenContainer;
        DiDotGraph<CoordsInt> diGraph;

        // Vein Zone Order of adding edges/circular edges
        //Queue<GraphEdgeType> edgeAddOrder = new Queue<GraphEdgeType>();

        // Vein Zone Properties
        int minCircularEdgeCount = 1;
        int maxCircularEdgeCount = 1;
        int targetCircularEdgeCount = 0;

        int minEdgeCount = 3;
        int maxEdgeCount = 4;
        int targetEdgeCount = 0;

        int newlyAddedMinEdgeLength = 3; // 3 nodes

        int maxNodeConnections = 4;

       

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
            this.targetEdgeCount = Random.Range(minEdgeCount, maxEdgeCount + 1);
            this.targetCircularEdgeCount = Random.Range(minCircularEdgeCount, maxCircularEdgeCount + 1);
        }

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


        // =====================================================================================
        //                           DiGraph Conditional Control Functions
        // =====================================================================================

        bool edgeConditionHit()
        {
            bool conditionMet = true;
            if (this.diGraph.getNumOfNonCircularEdges() < this.targetEdgeCount)
                conditionMet = false;
            return conditionMet;
        }

        bool circularEdgeConditionHit()
        {
            bool conditionMet = true;
            if (this.diGraph.getNumOfCircularEdges() < this.targetCircularEdgeCount)
                conditionMet = false;
            return conditionMet;
        }

        bool adhearsToNodeMaxCondition(DiDotNode<CoordsInt> node)
        {
            bool adhearsToNodeMax = true;
            if (node.numOfConnections() >= maxNodeConnections)
                adhearsToNodeMax = false;
            return adhearsToNodeMax;
        }

        // =====================================================================================
        //                              DiGraph Control Functions
        // =====================================================================================

        GraphEdgeType decideNextEdgeType()
        {
            bool edgeConditionMet = edgeConditionHit();
            bool circularEdgeConditionMet = circularEdgeConditionHit();
            GraphEdgeType nextEdgeType = GraphEdgeType.None;

            if (edgeConditionMet == false && circularEdgeConditionMet == false)
                nextEdgeType = Random.Range(0, 2) == 0 ? GraphEdgeType.Edge : GraphEdgeType.CircularEdge;
            else if (edgeConditionMet == false)
                nextEdgeType = GraphEdgeType.Edge;
            else if (circularEdgeConditionMet == false)
                nextEdgeType = GraphEdgeType.CircularEdge;

            return nextEdgeType;
        }

        public bool decideEndPoints()
        {
            bool graphIsDone = false;
            // Analyze the graph
            this.diGraph.analyzeGraph();

            this.print("================= " + zoneVeinGenContainer.currentVeinZone.getId().ToString() + " =================");

            // Detemine what needs to be done to the graph based on the non hit min conditions
            GraphEdgeType nextEdgeType = decideNextEdgeType();
            switch(nextEdgeType)
            {
                case GraphEdgeType.None:
                    graphIsDone = true;
                    break;

                case GraphEdgeType.Edge:
                    configureNewEdge();
                    break;

                case GraphEdgeType.CircularEdge:
                    configureNewEdge();
                    //configureCicularNewEdge();
                    break;
            }

            Debug.Log("NEXT EDGE TYPE: " + nextEdgeType);

            return graphIsDone;
        }

        void configureNewEdge()
        {
            // Basic configuration for now, expecting something more deliberate in the future
            // 1. Scan the allocated tile map connection for an empty space
            // 2. Determine the closest point in the di graph controller to that empty space
            // 3. Get the direction from the point to the empty space

            CoordsInt destinationWorldCoord = zoneVeinGenContainer.zoneVeinNavigationController.findEmptySpaceCoord(out bool foundEmptySpace);

            /*
            if (foundEmptySpace == false)
                Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find free space, space is probably too packed with veins already");
            else
            {
                bool adhearToMinEdgeLength = true;
                CoordsInt startNodeCoord = findClosestNode(destinationWorldCoord, adhearToMinEdgeLength); // <========== NOT DONE !!!!!!!!!!!!!!!!!!!!
                CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(startNodeCoord);

                //startNodeCoord.print("CLOSEST COORD TO FREE SPACE: ");
                //worldSpaceCoords.print("CLOSEST WORLD COORD TO FREE SPACE: ");

                // Calculate the direction

                // Then you need to check to make sure that you can move a tile in either direction from the startNodeCoord
                //      There are cases where you can't, search adjacent coords until you get a coord that can
                
            }
            */
        }


        // Finds the closest node coord (TileMapConnection coord) based on the inputed destination world coord
        //      Needs to adhear to controller rules
        //      1. Edges can't be shorter than newlyAddedMinEdgeLength (currently 3)
        //      2. Node can't have more than 4 connections
        CoordsInt findClosestNode(CoordsInt destinationWorldCoords, bool adhearToMinEdgeLength)
        {
            List<DiDotEdge<CoordsInt>> listOfEdges = this.diGraph.getListOfEdges();

            //    Distance, Not World Coord, World Coords
            MinValue<float, Double<CoordsInt, CoordsInt>> minDistanceList = new MinValue<float, Double<CoordsInt, CoordsInt>>(1);
            Double<CoordsInt, CoordsInt> doubleVal = new Double<CoordsInt, CoordsInt>(new CoordsInt(System.UInt16.MaxValue, System.UInt16.MaxValue), 
                                                                                      new CoordsInt(System.UInt16.MaxValue, System.UInt16.MaxValue));
            minDistanceList.addValueToQueue((float)System.UInt16.MaxValue, doubleVal);

            foreach (var edge in listOfEdges)
            {

                List<DiDotNode<CoordsInt>> listOfNodes = null; 
                if (adhearToMinEdgeLength == true)
                    listOfNodes = edge.getNodeListExcludingEdgeNodes(this.newlyAddedMinEdgeLength);
                else
                    listOfNodes = edge.getNodeList();

                foreach (var node in listOfNodes)
                {
                    CoordsInt allocatedTileMapCoords = node.getObject();
                    CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(allocatedTileMapCoords);

                    //allocatedTileMapCoords.print("\t\t\tCOORD: ");
                    //worldSpaceCoords.print("\t\t\tWORLD COORD: ");

                    if (adhearsToNodeMaxCondition(node) == true)
                    {
                        float distance = CommonFunctions.calculateCoordsDistance(worldSpaceCoords, destinationWorldCoords);
                        doubleVal.setOne(allocatedTileMapCoords);
                        doubleVal.setTwo(worldSpaceCoords);
                        minDistanceList.addValueToQueue(distance, doubleVal);
                    }
                }
            }

            CoordsInt startCoords = minDistanceList.getMinVal().Value.getOne();
            CoordsInt startWorldCoords = minDistanceList.getMinVal().Value.getTwo();
            DirectionBias newDirBias = CommonFunctions.calculatePrimaryDirectionWithWorldCoords(startWorldCoords, destinationWorldCoords);

            // Test if the start coords can actually travel in the new direction bias

            return startCoords;
        }



        void configureCicularNewEdge()
        {
            
        }

        void print(string message)
        {
            List<DiDotEdge<CoordsInt>> listOfEdges = this.diGraph.getListOfEdges();
            List<DiDotCircularEdge<CoordsInt>> listOfCircularEdges = this.diGraph.getListOfCircularEdges();

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

            strOutput = "";
            foreach (DiDotCircularEdge<CoordsInt> cirEdge in listOfCircularEdges)
            {
                strOutput = "\tCIR_EDGE_" + cirEdge.getId();
                foreach (var edge in cirEdge.getEdgeList())
                {
                    strOutput = strOutput + "  E_" + edge.getId();
                }
              
                Debug.Log(strOutput);
            }
        }
    }
}
