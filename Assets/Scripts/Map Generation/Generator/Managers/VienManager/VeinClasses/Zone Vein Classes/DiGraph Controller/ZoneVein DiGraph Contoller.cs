using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;
using CommonlyUsedFunctions;
using VeinEnums;
using TileManagerClasses;

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

            CoordsInt destinationMapConnCoord = zoneVeinGenContainer.zoneVeinNavigationController.findEmptySpaceCoord(out bool foundEmptySpace, out TwoDList<Tile> tileMapConnections_JustTile);

            
            if (foundEmptySpace == false)
                Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find free space, space is probably too packed with veins already");
            else
            {
                bool adhearToMinEdgeLength = true;
                CoordsInt startNodeCoord = findClosestNodeInEdge(destinationMapConnCoord, adhearToMinEdgeLength, ref tileMapConnections_JustTile, out DirectionBias newDirBias); // <========== NOT DONE !!!!!!!!!!!!!!!!!!!!
                CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(startNodeCoord);

                startNodeCoord.print("CLOSEST COORD TO FREE SPACE: ");
                worldSpaceCoords.print("CLOSEST WORLD COORD TO FREE SPACE: ");


                // Then you need to check to make sure that you can move a tile in either direction from the startNodeCoord
                //      There are cases where you can't, search adjacent coords until you get a coord that can
                //  Take the
                
            }
            
        }


        // Finds the closest node coord (TileMapConnection coord) based on the inputed destination world coord
        //      Needs to adhear to controller rules
        //          1. Edges can't be shorter than newlyAddedMinEdgeLength (currently 3)
        //          2. Node can't have more than 4 connections
        //      Needs to also make sure that the closest node cords can be traveled to
        CoordsInt findClosestNodeInEdge(CoordsInt destinationTileMapConnCoord, bool adhearToMinEdgeLength, ref TwoDList<Tile> tileMapConnections_JustTile, out DirectionBias newDirBias)
        {
            CoordsInt startCoords = new CoordsInt(-1, -1);
            List<DiDotEdge<CoordsInt>> allDiGraphEdges = this.diGraph.getListOfEdges();
            bool startCoordIsValid = false;

            bool exhuastedAllNodesInEdge = false;
            List<DiDotEdge<CoordsInt>> rejectedEdges = new List<DiDotEdge<CoordsInt>>();
            //while (exhuastedAllNodesInEdge == false)
            //{
            //  exhuastedAllNodesInEdge = false;
            //

                // Finds closest node to the destination coords, doesn't care if it's valid or not
                findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> startNode, out DiDotNode<CoordsInt> originalStartNode, out DiDotEdge<CoordsInt> startEdge, allDiGraphEdges, rejectedEdges, destinationTileMapConnCoord, adhearToMinEdgeLength);

                if (allDiGraphEdges.Count == rejectedEdges.Count)
                {
                    Debug.Log("EXHAUSTED ALL EDGES IN ZONE");
                    //break;
                }


                // Calculate the direction
                newDirBias = CommonFunctions.calculatePrimaryDirection(startNode.getObject(), destinationTileMapConnCoord);
                int maxFloodedArea = 100;

                // Test if the start coords can actually travel in the new direction bias
                //      1. Take the two d tile list and flood the dim list based on the destination coord
                //      2. Check if the start coord touches the flooded dim list, by going 1 unit in the new dir bias
                DimensionList floodedDimList = this.zoneVeinGenContainer.dimVeinZoneCreator.getFloodedDimensionsUsingAlternateTileMap(destinationTileMapConnCoord, this.zoneVeinGenContainer.debugMode, maxFloodedArea, ref tileMapConnections_JustTile);

            
                Debug.Log("FLOODED DIMS: ");
                floodedDimList.printGrid(false);
                newDirBias.print();

                // Get the list of nodes that is acceptable
                List<DiDotNode<CoordsInt>> listOfNodes = null;
                if (adhearToMinEdgeLength == true)
                    listOfNodes = startEdge.getNodeListExcludingEdgeNodes(this.newlyAddedMinEdgeLength);
                else
                    listOfNodes = startEdge.getNodeList();

                // If the start coord has no adjecent flood coords, then try other edge coords
                List<DiDotNode<CoordsInt>> rejectedStartNodes = new List<DiDotNode<CoordsInt>>();
                while (startCoordIsValid == false)
                {
                    startCoords = startNode.getObject();
                    startCoords.print("NEW EDGE START: ");

                    // First check adjacent coords to see if they are floods
                    for (int i = 0; i < 2; i++)
                    {
                        CoordsInt checkFloodedIsNextToStart = startCoords.deepCopyInt();
                        Direction dir = Direction.None;

                        if (i == 0)
                            dir = newDirBias.getHorizontalDir();
                        else
                            dir = newDirBias.getVerticalDir();

                        switch (dir)
                        {
                            case Direction.None:
                                continue;

                            case Direction.North:
                                checkFloodedIsNextToStart.incY();
                                break;

                            case Direction.East:
                                checkFloodedIsNextToStart.incX();
                                break;

                            case Direction.South:
                                checkFloodedIsNextToStart.decY();
                                break;

                            case Direction.West:
                                checkFloodedIsNextToStart.decX();
                                break;
                        }

                        if (floodedDimList.coordIsMarked(checkFloodedIsNextToStart) == true)
                        {
                            startCoordIsValid = true;
                            break;
                        }
                    }

                    // If the adjacent coords are not a part of the flood coords, then seach along the edge for a new node/start coord
                    if (startCoordIsValid == false)
                    {
                        Debug.Log("REJECTED NEW EDGE START");
                        rejectedStartNodes.Add(startNode);

                        bool foundNewStartNode = false;
                        while (foundNewStartNode == false)
                        {
                            List<DiDotNode<CoordsInt>> startNodeConnections = startNode.getRawListOfConnections();

                            // Check the start nodes adjacent connected nodes
                            foreach (var nodeConn in startNodeConnections)
                            {
                                if (listOfNodes.Contains(nodeConn) == true && rejectedStartNodes.Contains(nodeConn) == false)
                                {
                                    startNode = nodeConn;
                                    foundNewStartNode = true;
                                    break;
                                }
                            }

                            // It's possible that the orignal start node was in the middle of the edge
                            //      This while loop can head up/down the edge looking for nodes, but it will check one direction first
                            //      Once we traveled all the way in one direction, assign the start node to the original start node so that we can search in the other directions
                            if (foundNewStartNode == false)
                            {
                                startNode = originalStartNode;

                                // Exhuasted all edge nodes once list of nodes count and rejected start nodes count is the same it means all nodes have been checked
                                if (listOfNodes.Count == rejectedStartNodes.Count)
                                    exhuastedAllNodesInEdge = true;
                            }
                        }
                    }
                }

                if (exhuastedAllNodesInEdge == true)
                {
                    rejectedEdges.Add(startEdge);
                    Debug.Log("EXHAUSTED ALL NODES IN THE EDGE");
                }

            //}

            return startCoords;
        }

        // Finds closest node to the destination coords, may or may not be what is needed (hence why it's Raw)
        void findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> startNode, out DiDotNode<CoordsInt> originalStartNode, out DiDotEdge<CoordsInt> startEdge,
                                      List<DiDotEdge<CoordsInt>> allDiGraphEdges, List<DiDotEdge<CoordsInt>> rejectedEdges, CoordsInt destinationTileMapConnCoord, bool adhearToMinEdgeLength)
        {
            //    Distance,                        Node, Edge
            MinValue<float, Double<DiDotNode<CoordsInt>, DiDotEdge<CoordsInt>>> minDistanceList = new MinValue<float, Double<DiDotNode<CoordsInt>, DiDotEdge<CoordsInt>>>(1);
            Double<DiDotNode<CoordsInt>, DiDotEdge<CoordsInt>> doubleEntry = new Double<DiDotNode<CoordsInt>, DiDotEdge<CoordsInt>>(new DiDotNode<CoordsInt>(), new DiDotEdge<CoordsInt>());
            minDistanceList.addValueToQueue((float)System.UInt16.MaxValue, doubleEntry);

            List<DiDotNode<CoordsInt>> listOfNodes = null;
            foreach (var edge in allDiGraphEdges)
            {
                if (rejectedEdges.Contains(edge) == true)
                    continue;

                if (adhearToMinEdgeLength == true)
                    listOfNodes = edge.getNodeListExcludingEdgeNodes(this.newlyAddedMinEdgeLength);
                else
                    listOfNodes = edge.getNodeList();

                foreach (var node in listOfNodes)
                {
                    CoordsInt allocatedTileMapCoords = node.getObject();
                    //CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(allocatedTileMapCoords);

                    //allocatedTileMapCoords.print("\t\t\tCOORD: ");
                    //worldSpaceCoords.print("\t\t\tWORLD COORD: ");

                    if (adhearsToNodeMaxCondition(node) == true)
                    {
                        float distance = CommonFunctions.calculateCoordsDistance(allocatedTileMapCoords, destinationTileMapConnCoord);
                        doubleEntry = new Double<DiDotNode<CoordsInt>, DiDotEdge<CoordsInt>>(node, edge);
                        minDistanceList.addValueToQueue(distance, doubleEntry);
                    }
                }
            }

            startNode = minDistanceList.getMinVal().Value.getOne();
            originalStartNode = minDistanceList.getMinVal().Value.getOne();
            startEdge = minDistanceList.getMinVal().Value.getTwo();
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
