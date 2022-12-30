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

    public partial class ZoneVeinDiGraphContoller : ContainerAccessor
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

        public void createNextBranch(out bool graphIsDone, out bool edgeConfigFailed, out CoordsInt branchStartCoords, out CoordsInt branchSecondCoords, out DirectionBias dirBias)
        {
            graphIsDone = false;
            // Analyze the graph
            this.diGraph.analyzeGraph();

            this.print("================= " + zoneVeinGenContainer.currentVeinZone.getId().ToString() + " =================");

            // !!!!!!!!!!!!!!!!!!!!
            //      Will probably have to create a class to hold edge specific configs and circular edge specific configs
            //      Need to output configs back up one in createZoneVein()
            branchStartCoords = new CoordsInt(-1, -1);
            branchSecondCoords = new CoordsInt(-1, -1);
            dirBias = new DirectionBias(Direction.None, Direction.None);
            edgeConfigFailed = false;

            // Detemine what needs to be done to the graph based on the non hit min conditions
            GraphEdgeType nextEdgeType = decideNextEdgeType();
            switch(nextEdgeType)
            {
                case GraphEdgeType.None:
                    graphIsDone = true;
                    break;

                case GraphEdgeType.Edge:
                    configureNewEdge(out branchStartCoords, out branchSecondCoords, out dirBias, out edgeConfigFailed);
                    break;

                case GraphEdgeType.CircularEdge:
                    configureNewEdge(out branchStartCoords, out branchSecondCoords, out dirBias, out edgeConfigFailed);
                    //configureCicularNewEdge();
                    break;
            }

            Debug.Log("NEXT EDGE TYPE: " + nextEdgeType);

        }


        List<CoordsInt> configureNewEdge(out CoordsInt startCoords, out CoordsInt secondCoord, out DirectionBias dirBias, out bool edgeConfigFailed)
        {
            // Basic configuration for now, expecting something more deliberate in the future
            // 1. Scan the allocated tile map connection for an empty space
            // 2. Determine the closest point in the di graph controller to that empty space
            // 3. Get the direction from the point to the empty space

            List<CoordsInt> listOfZoneVeinCoords = new List<CoordsInt>();
            CoordsInt destinationMapConnCoord = zoneVeinGenContainer.zoneVeinNavigationController.findEmptySpaceCoord(out bool foundEmptySpace, out TwoDList<Tile> tileMapConnections_JustTile);
            edgeConfigFailed = false;

            startCoords = new CoordsInt(-1, -1);
            secondCoord = new CoordsInt(-1, -1);
            dirBias = new DirectionBias(Direction.None, Direction.None);
            
            // while edgeConfigFailed and 
                if (foundEmptySpace == false)
                {
                    Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find free space, space is probably too packed with veins already");
                    edgeConfigFailed = true;
                }
                else
                {
                    bool adhearToMinEdgeLength = true;
                    startCoords = findClosestNodeInDiGraph(destinationMapConnCoord, adhearToMinEdgeLength, ref tileMapConnections_JustTile, out bool nodeSearchFailed, out DimensionList floodedDimList);
                    //CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(startCoords);

                    //startCoords.print("CLOSEST COORD TO FREE SPACE: ");
                    //worldSpaceCoords.print("CLOSEST WORLD COORD TO FREE SPACE: ");

                    if (nodeSearchFailed == false)
                    {
                        //destinationMapConnCoord.print("FREE SPACE COORD: ");

                        dirBias = configurePrimaryDirection(ref startCoords, destinationMapConnCoord, ref floodedDimList, out secondCoord);

                        // ATTEMPT TO GENERATE THE BRANCH NOW, IF IT FAILS THEN TRY TO FIND ANOTHER NODE
                        //listOfZoneVeinCoords = this.zoneVeinGenContainer.zoneVeinNavigationController.randomlyGenerateZoneVeinBranch(startCoords, secondCoord, dirBias);

                        // BOOL FOR IF ABOVE FAILED
                    }
                    else
                        edgeConfigFailed = true;
                }

            return listOfZoneVeinCoords;
        }




        DirectionBias configurePrimaryDirection(ref CoordsInt startCoords, CoordsInt destinationMapConnCoord, ref DimensionList floodedDimList, out CoordsInt secondCoord)
        {
            secondCoord = new CoordsInt(-1, -1);

            // Calculate the direction
            int displacementNeeded = 1;
            DirectionBias dirBias = CommonFunctions.calculatePrimaryDirection(startCoords, destinationMapConnCoord, displacementNeeded);

            // Find the second coord based on the new dir bias
            bool secondaryCoordIsValid = false;
            List<Direction> primaryDir = dirBias.getPrimaryDirections();

            // Check primary directions first
            foreach (var dir in primaryDir)
            {
                if (dir.Equals(Direction.None) == false)
                {
                    secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, dir, out secondCoord);

                    if (secondaryCoordIsValid)
                        break;
                }
            }

            // If primary directions failed, then look in non primary directions
            if (secondaryCoordIsValid == false)
            {
                foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                {
                    if (primaryDir.Contains(dir) == false && dir.Equals(Direction.None) == false)
                    {
                        Direction oppDir = CommonFunctions.getOppositeDir(dir);
                        secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, oppDir, out secondCoord);

                        if (secondaryCoordIsValid)
                            break;
                    }
                }
            }

            if (secondaryCoordIsValid == false)
                Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find a secondary coord??? Should have been validated in findClosestNodeInDiGraph()");

            return dirBias;
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


    // ===========================================
    // Find closest node in edge functions
    // ===========================================
    public partial class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        // Finds the closest node coord (TileMapConnection coord) based on the inputed destination world coord
        //      Needs to adhear to controller rules
        //          1. Edges can't be shorter than newlyAddedMinEdgeLength (currently 3)
        //          2. Node can't have more than 4 connections
        //      Needs to also make sure that the closest node cords can be traveled to
        CoordsInt findClosestNodeInDiGraph(CoordsInt destinationTileMapConnCoord, bool adhearToMinEdgeLength, ref TwoDList<Tile> tileMapConnections_JustTile, out bool nodeSearchFailed, out DimensionList floodedDimList)
        {
            int currentMinEdgeLength = 0;
            if (adhearToMinEdgeLength == true)
                currentMinEdgeLength = this.newlyAddedMinEdgeLength;

            CoordsInt startCoords = new CoordsInt(-1, -1);
            List<DiDotEdge<CoordsInt>> allDiGraphEdges = this.diGraph.getListOfEdges();
            bool startCoordIsValid = false;
            nodeSearchFailed = false;

            List<DiDotNode<CoordsInt>> rejectedStartNodes = new List<DiDotNode<CoordsInt>>();
            Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>> edgeToRejectedStartNodesDict = new Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>>();

            int maxFloodedArea = 100;
            floodedDimList = this.zoneVeinGenContainer.dimVeinZoneCreator.getFloodedDimensionsUsingAlternateTileMap(destinationTileMapConnCoord, this.zoneVeinGenContainer.debugMode, maxFloodedArea, ref tileMapConnections_JustTile);

            bool exhuastedAllNodesInEdge = false;
            List<DiDotEdge<CoordsInt>> rejectedEdges = new List<DiDotEdge<CoordsInt>>();
            while (startCoordIsValid == false)
            {
                exhuastedAllNodesInEdge = false;

                // Finds closest node to the destination coords, doesn't care if it's valid or not
                findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> startNode, out DiDotNode<CoordsInt> originalStartNode, out DiDotEdge<CoordsInt> startEdge, 
                                            allDiGraphEdges, rejectedEdges, destinationTileMapConnCoord, ref edgeToRejectedStartNodesDict, adhearToMinEdgeLength, currentMinEdgeLength);

                if (allDiGraphEdges.Count == rejectedEdges.Count)
                {
                    //
                    if (currentMinEdgeLength == 0)
                    {
                        nodeSearchFailed = true;
                        Debug.LogError("EXHAUSTED ALL EDGES IN ZONE DIGRAPH");
                        break;
                    }
                    else
                    {
                        Debug.LogError("EXHAUSTED ALL EDGES IN ZONE DIGRAPH, REDUCING MIN EDGE LENGTH REQUIRMENT");
                        currentMinEdgeLength--;
                        continue;
                    }
                }

                // Get the list of nodes that is acceptable
                List<DiDotNode<CoordsInt>> listOfEdgeNodes = null;
                if (adhearToMinEdgeLength == true)
                    listOfEdgeNodes = startEdge.getNodeListExcludingEdgeNodes(currentMinEdgeLength);
                else
                    listOfEdgeNodes = startEdge.getNodeList();

                // Check if the start coord is valid
                rejectedStartNodes = edgeToRejectedStartNodesDict[startEdge];
                startCoordIsValid = isStartCoordValid(ref rejectedStartNodes, ref startCoords, ref startNode, ref floodedDimList, ref listOfEdgeNodes, ref originalStartNode, ref exhuastedAllNodesInEdge);

                if (exhuastedAllNodesInEdge == true)
                {
                    rejectedEdges.Add(startEdge);
                    Debug.LogError("EXHAUSTED ALL NODES IN THE EDGE");
                }
            }

            return startCoords;
        }

        bool isStartCoordValid(ref List<DiDotNode<CoordsInt>> rejectedStartNodes, ref CoordsInt startCoords, ref DiDotNode<CoordsInt> startNode, ref DimensionList floodedDimList, ref List<DiDotNode<CoordsInt>> listOfEdgeNodes, ref DiDotNode<CoordsInt> originalStartNode, ref bool exhuastedAllNodesInEdge)
        {
            bool startCoordIsValid = false;

            // Test if the start coords can actually travel in the new direction bias
            //      1. Take the two d tile list and flood the dim list based on the destination coord
            //      2. Check if the start coord touches the flooded dim list, by going 1 unit in the new dir bias
            //List<DiDotNode<CoordsInt>> rejectedStartNodes = new List<DiDotNode<CoordsInt>>();
            while (startCoordIsValid == false)
            {
                startCoords = startNode.getObject();
                //startCoords.print("NEW EDGE START: ");

                // First check all adjacent coords to see if they are a part of the flooded dim list
                startCoordIsValid = checkIfAnyAdjacentDirectionAreFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList);

                // If the adjacent coords are not a part of the flood coords, then seach along the edge for a new node/start coord
                if (startCoordIsValid == false)
                {
                    //Debug.Log("REJECTED NEW EDGE START");
                    findNextClosestNodeInEdge(ref rejectedStartNodes, ref listOfEdgeNodes, ref startNode, ref originalStartNode, ref exhuastedAllNodesInEdge);
                }
            }

            return startCoordIsValid;
        }

        // Finds closest node to the destination coords, may or may not be what is needed (hence why it's Raw)
        void findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> startNode, out DiDotNode<CoordsInt> originalStartNode, out DiDotEdge<CoordsInt> startEdge,
                                      List<DiDotEdge<CoordsInt>> allDiGraphEdges, List<DiDotEdge<CoordsInt>> rejectedEdges, CoordsInt destinationTileMapConnCoord, 
                                      ref Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>> edgeToRejectedStartNodesDict, bool adhearToMinEdgeLength, int currentMinEdgeLength)
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
                    listOfNodes = edge.getNodeListExcludingEdgeNodes(currentMinEdgeLength);
                else
                    listOfNodes = edge.getNodeList();

                // Add a new rejected start node list if it doesn't exist yet
                if (edgeToRejectedStartNodesDict.ContainsKey(edge) == false)
                    edgeToRejectedStartNodesDict.Add(edge, new List<DiDotNode<CoordsInt>>());

                List<DiDotNode<CoordsInt>> rejectedStartNodes = edgeToRejectedStartNodesDict[edge];

                foreach (var node in listOfNodes)
                {
                    CoordsInt allocatedTileMapCoords = node.getObject();
                    //CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(allocatedTileMapCoords);

                    //allocatedTileMapCoords.print("\t\t\tCOORD: ");
                    //worldSpaceCoords.print("\t\t\tWORLD COORD: ");

                    // If the node has less than 4 adjacent nodes AND hasn't already been rejected yet
                    if (adhearsToNodeMaxCondition(node) == true && rejectedStartNodes.Contains(node) == false)
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

        // Search along an edge by checking the passed in startNodes connected nodes
        void findNextClosestNodeInEdge(ref List<DiDotNode<CoordsInt>> rejectedStartNodes, ref List<DiDotNode<CoordsInt>> listOfEdgeNodes, ref DiDotNode<CoordsInt> startNode, ref DiDotNode<CoordsInt> originalStartNode, ref bool exhuastedAllNodesInEdge)
        {
            rejectedStartNodes.Add(startNode);

            bool foundNewStartNode = false;
            while (foundNewStartNode == false)
            {
                List<DiDotNode<CoordsInt>> startNodeConnections = startNode.getRawListOfConnections();

                // Check the start nodes adjacent connected nodes
                foreach (var nodeConn in startNodeConnections)
                {
                    if (listOfEdgeNodes.Contains(nodeConn) == true && rejectedStartNodes.Contains(nodeConn) == false)
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
                    if (listOfEdgeNodes.Count == rejectedStartNodes.Count)
                        exhuastedAllNodesInEdge = true;
                }
            }
        }

        bool checkIfAnyAdjacentDirectionAreFloodedAndNotPermaLocked(ref CoordsInt startCoords, ref DimensionList floodedDimList)
        {
            bool startCoordIsValid = false;
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                startCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, dir, out CoordsInt nextCoord); // nextCoord is not used here

                if (startCoordIsValid == true)
                    break;
            }
            return startCoordIsValid;
        }

        bool adjacentDirectionIsFloodedAndNotPermaLocked(ref CoordsInt startCoords, ref DimensionList floodedDimList, Direction dir, out CoordsInt nextCoord)
        {
            bool startCoordIsValid = false;

            
            CoordsInt checkFloodedIsNextToStart = startCoords.deepCopyInt();
            nextCoord = startCoords.deepCopyInt();

            switch (dir)
            {
                case Direction.None:
                    return startCoordIsValid;

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

            if (floodedDimList.coordIsMarked(checkFloodedIsNextToStart) == true && zoneVeinGenContainer.tileMapConnCoordIsPermaLocked(checkFloodedIsNextToStart) == false)
                startCoordIsValid = true;

            nextCoord = checkFloodedIsNextToStart;

            return startCoordIsValid;
        }
    }
}
