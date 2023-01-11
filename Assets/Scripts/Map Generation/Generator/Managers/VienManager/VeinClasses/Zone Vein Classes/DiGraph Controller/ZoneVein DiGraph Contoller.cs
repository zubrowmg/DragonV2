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

        // Edge Controls
        int newlyAddedMinEdgeLength = 3; // Attempts to generate edges that are X nodes long
        int maxNodeConnections = 4;

        // Circular Edge Controls
        int minCircularEdgeSearchLength = 9; // Won't attempt to connect nodes that are X nodes appart 
        int circularEdgeSearchLength = 3; // If node 0 is selected, then node X + 0, 2X + 0 etc. will be selected next

        // Rejected DiDot Node and DiDot Edge
        Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>> edgeToRejectedStartNodesDict;
        DiDotNode<CoordsInt> currentStartNode;
        DiDotEdge<CoordsInt> currentStartEdge;

        // Rejected DiDot Node and DiDot Edge for circular edges
        List<Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>> rejectedStartEndPairs;

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

        public void configureNewBranchInit()
        {
            this.edgeToRejectedStartNodesDict = new Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>>();
            this.currentStartEdge = new DiDotEdge<CoordsInt>();
            this.currentStartNode = new DiDotNode<CoordsInt>();
        }

        public void configureNewCircularBranchInit()
        {
            this.rejectedStartEndPairs = new List<Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>>();
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
            GraphEdgeType nextEdgeType = GraphEdgeType.None;

            // If you have only one edge, then make another edge. No point in making a circular edge with only 1 edge
            //      Maybe it isn't pointless, it could lead to more interesting generation
            if (this.diGraph.getNumOfNonCircularEdges() <= 1)
                nextEdgeType = GraphEdgeType.Edge;
            else
            {
                // Decide between new edge and circular edge
                bool edgeConditionMet = edgeConditionHit();
                bool circularEdgeConditionMet = circularEdgeConditionHit();

                if (edgeConditionMet == false && circularEdgeConditionMet == false)
                    nextEdgeType = Random.Range(0, 2) == 0 ? GraphEdgeType.Edge : GraphEdgeType.CircularEdge;
                else if (edgeConditionMet == false)
                    nextEdgeType = GraphEdgeType.Edge;
                else if (circularEdgeConditionMet == false)
                    nextEdgeType = GraphEdgeType.CircularEdge;
            }
            

            return nextEdgeType;
        }

        public List<CoordsInt> createNextBranch(out bool graphIsDone, out bool edgeConfigFailed)
        {
            List<CoordsInt> newBranchCoords = new List<CoordsInt>();
            graphIsDone = false;
            // Analyze the graph
            this.diGraph.analyzeGraph();


            // !!!!!!!!!!!!!!!!!!!!
            //      Will probably have to create a class to hold edge specific configs and circular edge specific configs
            //      Need to output configs back up one in createZoneVein()
            edgeConfigFailed = false;

            // Detemine what needs to be done to the graph based on the non hit min conditions
            GraphEdgeType nextEdgeType = decideNextEdgeType();
            switch(nextEdgeType)
            {
                case GraphEdgeType.None:
                    graphIsDone = true;
                    break;

                case GraphEdgeType.Edge:
                    newBranchCoords = configureNewBranchEdge(out edgeConfigFailed);
                    break;

                case GraphEdgeType.CircularEdge:
                    newBranchCoords = configureNewBranchEdge(out edgeConfigFailed);
                    //newBranchCoords = configureNewCicularEdge();
                    break;
            }

            //Debug.Log("NEXT EDGE TYPE: " + nextEdgeType);
            return newBranchCoords;
        }


        List<CoordsInt> configureNewBranchEdge(out bool edgeConfigFailed)
        {
            // Basic configuration for now, expecting something more deliberate in the future
            // 1. Scan the allocated tile map connection for an empty space
            // 2. Determine the closest point in the di graph controller to that empty space
            // 3. Get the direction from the point to the empty space
            // 4. Randomly generate the edge based on the direction from step 3

            configureNewBranchInit();

            bool adhearToMinEdgeLength = true;
            List<CoordsInt> listOfZoneVeinCoords = new List<CoordsInt>();
            CoordsInt destinationMapConnCoord = zoneVeinGenContainer.zoneVeinNavigationController.findEmptySpaceCoord(out bool foundEmptySpace, out TwoDList<Tile> tileMapConnections_JustTile);
            edgeConfigFailed = false;

            CoordsInt startCoords = new CoordsInt(-1, -1);
            DirectionBias dirBias = new DirectionBias(Direction.None, Direction.None);
            
            if (foundEmptySpace == false)
            {
                Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find free space, space is probably too packed with veins already");
                edgeConfigFailed = true;
            }
            else
            {
                // Add each edge to the dictionary
                List<DiDotEdge<CoordsInt>> allDiGraphEdges = this.diGraph.getListOfEdges();
                foreach (var edge in allDiGraphEdges)
                {
                    this.edgeToRejectedStartNodesDict.Add(edge, new List<DiDotNode<CoordsInt>>());
                }

                bool edgeGenerationWasSuccessful = false;
                while (edgeGenerationWasSuccessful == false)
                {
                    // Gets the closest start node and set this.currentStartNode
                    findClosestNodeInDiGraph(allDiGraphEdges, destinationMapConnCoord, adhearToMinEdgeLength, ref tileMapConnections_JustTile, out bool nodeSearchFailed, out DimensionList floodedDimList);
                    startCoords = this.currentStartNode.getObject();


                    if (nodeSearchFailed == false)
                    {
                        // Get the primary direction and determines a potential second point
                        //      Second point needs to be next to an existing vein, so the tile will be locked on a pass. It just can't be perma locked
                        dirBias = configurePrimaryDirection(ref startCoords, destinationMapConnCoord, ref floodedDimList, out List<CoordsInt> potentialSecondCoords, out bool secondaryCoordIsValid);

                        // Attempt to generate a new vein
                        bool edgeGenerationFailed = false;
                        if (secondaryCoordIsValid == true)
                            listOfZoneVeinCoords = this.zoneVeinGenContainer.zoneVeinNavigationController.randomlyGenerateZoneVeinBranch(startCoords, potentialSecondCoords, dirBias, out edgeGenerationFailed);
                        else
                            edgeGenerationFailed = true;

                        edgeGenerationWasSuccessful = !edgeGenerationFailed;
                        

                        // If the Navigation controller failed to generate a new edge, reject the current start node
                        if (edgeGenerationFailed == true)
                        {
                            if (zoneVeinGenContainer.debugMode == true)
                            {
                                zoneVeinGenContainer.currentZone.debugInfo.addLine("ZoneVein DiGraph Controller", "configureNewBranchEdge()",
                                                        "Edge Generation failed. Moving onto a different start node\n" + "Rejected Start Node: " + startCoords.getPrintString());
                            }

                            this.edgeToRejectedStartNodesDict[this.currentStartEdge].Add(this.currentStartNode);
                        }
                        else if (zoneVeinGenContainer.debugMode == true)
                        {
                            // Convert the flooded dim list points to world space coords
                            List<CoordsInt> floodedFreeSpaceWorldCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(floodedDimList.getAllSelectedGridCoords());
                            zoneVeinGenContainer.currentZone.floodedFreeSpaces.Add(floodedFreeSpaceWorldCoords);
                        }
                    }
                    else
                    {
                        edgeConfigFailed = true;
                        Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Could not find any suitable node in the DiGraph for new edge generation");
                        break;
                    }
                }
            }

            return listOfZoneVeinCoords;
        }

        public List<CoordsInt> configureNewCicularEdge(out bool edgeConfigFailed)
        {
            // Basic configuration for now, expecting something more deliberate in the future
            // 1. Find a start node and end node, find a the closest possible pair
            // 2. Check that the pair are touching in a flooded dim list
            // 3. Generate the line, not randomly. But on a straight path from start to end
            
            // REMOVE THIS ANALYZE!!!! DONE IN ONE FUNCTION ABOVE
            this.diGraph.analyzeGraph();
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            configureNewCircularBranchInit();

            List<CoordsInt> listOfZoneVeinCoords = new List<CoordsInt>();
            edgeConfigFailed = false;
            bool adhearToMinEdgeLength = true;
            int currentMinEdgeLength = this.newlyAddedMinEdgeLength;

            List<DiDotEdge<CoordsInt>> allDiGraphEdges = this.diGraph.getListOfEdges();

            // Double< StartNode, EndNode >
            Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>> startEndPair = findStartEndPair(out bool foundStartEndPair, adhearToMinEdgeLength, currentMinEdgeLength, allDiGraphEdges);

            if (foundStartEndPair == false)
            {
                Debug.LogError("ZoneVein DiGraph Controller Class - configureNewCicularEdge(): Could not find a start and end pair, space is probably too packed with veins already");
                edgeConfigFailed = true;
            }
            else
            {

            }

            return listOfZoneVeinCoords;
        }



        DirectionBias configurePrimaryDirection(ref CoordsInt startCoords, CoordsInt destinationMapConnCoord, ref DimensionList floodedDimList, out List<CoordsInt> potentialSecondCoords, out bool secondaryCoordIsValid)
        {
            CoordsInt secondCoord = new CoordsInt(-1, -1);
            potentialSecondCoords = new List<CoordsInt>();

            // Calculate the direction
            int displacementNeeded = 1;
            DirectionBias dirBias = CommonFunctions.calculatePrimaryDirection(startCoords, destinationMapConnCoord, displacementNeeded);

            // Find the second coord based on the new dir bias
            secondaryCoordIsValid = false;
            List<Direction> primaryDir = dirBias.getPrimaryDirections();
            List<Direction> secondaryDir = dirBias.getSecondaryDirections();

            // Check primary directions first
            foreach (var dir in primaryDir)
            {
                if (dir.Equals(Direction.None) == false)
                {
                    secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, dir, out secondCoord);

                    if (secondaryCoordIsValid)
                        potentialSecondCoords.Add(secondCoord);
                }
            }

            // If primary directions failed, then look in non primary directions
            if (secondaryCoordIsValid == false)
            {
                foreach (Direction dir in secondaryDir)
                {
                    if (dir.Equals(Direction.None) == false)
                    {
                        Direction oppDir = CommonFunctions.getOppositeDir(dir);
                        secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, oppDir, out secondCoord);

                        if (secondaryCoordIsValid)
                            potentialSecondCoords.Add(secondCoord);
                    }
                }
            }

            if (secondaryCoordIsValid == false)
            {
                if (zoneVeinGenContainer.debugMode == true)
                {
                    zoneVeinGenContainer.currentZone.debugInfo.addLine("ZoneVein DiGraph Controller", "configurePrimaryDirection()",
                                            "Could not find a secondary coord for the branch generation");
                }
            }

            return dirBias;
        }

        public void print(int zoneId)
        {
            this.diGraph.analyzeGraph();

            List<DiDotEdge<CoordsInt>> listOfEdges = this.diGraph.getListOfEdges();
            List<DiDotCircularEdge<CoordsInt>> listOfCircularEdges = this.diGraph.getListOfCircularEdges();

            Debug.Log("================= " + zoneId.ToString() + " =================" + 
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

                strOutput = strOutput + "\n";

                foreach (var edgeConnection in nodeOneEdges)
                {
                    strOutput = strOutput + "\t\t" + "E_" + edge.getId() + " <-> " + "E_" + edgeConnection.getId() + ": " + edge.getNodeOne().getObject().getPrintString();
                }

                foreach (var edgeConnection in nodeTwoEdges)
                {
                    strOutput = strOutput + "\t\t" + "E_" + edge.getId() + " <-> " + "E_" + edgeConnection.getId() + ": " + edge.getNodeTwo().getObject().getPrintString();
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
    // Circular Edge functions
    // ===========================================
    public partial class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        // Finds the closest of two node. And validates that they can travel to each other
        //      This does not mean that the generation will succeed. Since a flooded dim list can have a width of 2 or 1 in certain areas
        Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>> findStartEndPair(out bool foundStartEndPair, bool adhearToMinEdgeLength, int currentMinEdgeLength, List<DiDotEdge<CoordsInt>> allDiGraphEdges)
        {
            DiDotNode<CoordsInt> startNode = new DiDotNode<CoordsInt>();
            DiDotNode<CoordsInt> endNode = new DiDotNode<CoordsInt>();
            foundStartEndPair = false;

            // First get all possible combinations of start end pairs
            List<Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>> allStartEndPairs = getAllStartEndPairsRaw(adhearToMinEdgeLength, currentMinEdgeLength, allDiGraphEdges);

            return new Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>(startNode, endNode);
        }
        
        // Will return all possible combinations of start end pairs, will not check if they are valid
        List<Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>> getAllStartEndPairsRaw(bool adhearToMinEdgeLength, int currentMinEdgeLength, List<DiDotEdge<CoordsInt>> allDiGraphEdges)
        {
            DoubleListNoDuplicates<DiDotNode<CoordsInt>> allStartEndPairs = new DoubleListNoDuplicates<DiDotNode<CoordsInt>>();
            Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>> possibleStartEndNodes = new Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>>();

            // First grab all start/end nodes from the current di graph, we don't grab all of the nodes to gain performance
            //      First create possible start end pairs in each edge. Does NOT create start end pairs from one edge to another edge
            //      This function will filter results if they don't abide by this.circularEdgeSearchLength
            foreach (var edge in allDiGraphEdges)
            {
                List<DiDotNode<CoordsInt>> currentEdgeStartStopNodes = new List<DiDotNode<CoordsInt>>();
                List<DiDotNode<CoordsInt>> listOfEdgeNodes = null;
                if (adhearToMinEdgeLength == true)
                    listOfEdgeNodes = edge.getNodeListExcludingEdgeNodes(currentMinEdgeLength);
                else
                    listOfEdgeNodes = edge.getNodeList();

                List<DiDotNode<CoordsInt>> edgeNodes = edge.getNodeList();

                // Always add the end nodes 
                currentEdgeStartStopNodes.Add(edgeNodes[0]);
                currentEdgeStartStopNodes.Add(edgeNodes[edgeNodes.Count - 1]);

                // Add the rest if they are circularEdgeSearchLength apart
                for (int i = this.circularEdgeSearchLength; i < edgeNodes.Count - 1; i += this.circularEdgeSearchLength)
                    currentEdgeStartStopNodes.Add(edgeNodes[i]);

                possibleStartEndNodes.Add(edge, currentEdgeStartStopNodes);

                // Now check if the current edge nodes can create start end pairs
                if (edge.getEdgeLength() > this.minCircularEdgeSearchLength)
                {
                    //Debug.Log("NEW EDGE:");
                    foreach (var node1 in currentEdgeStartStopNodes)
                    {
                        foreach (var node2 in currentEdgeStartStopNodes)
                        {
                            if (node1.Equals(node2) == false)
                            {

                                if (edge.getTotalDistanceFromNodeToNode(node1, node2) > this.minCircularEdgeSearchLength)
                                {
                                    //Debug.Log("\tNODE:" + node1.getObject().getPrintString() + "  " + node2.getObject().getPrintString());
                                    allStartEndPairs.addDoubleVal(node1, node2);
                                }
                            }
                        }
                    }
                }
            }


            // Now find all possible start end pairs from edge to edge, does not care if nodes abide by this.circularEdgeSearchLength or not
            //      edgeOne.Key == DiDotEdge
            //      edgeOne.Value == List of possible start/end Nodes
            foreach (var edgeOne in possibleStartEndNodes)
            {
                foreach (var edgeTwo in possibleStartEndNodes)
                {
                    // Make sure that the edges aren't the same edge
                    if (edgeOne.Key.Equals(edgeTwo.Key) == false)
                    {
                        foreach (var edgeOneNode in edgeOne.Value)
                        {
                            foreach (var edgeTwoNode in edgeTwo.Value)
                            {
                                // No need to have a pair with the same node
                                if (edgeOneNode.Equals(edgeTwoNode) == false)
                                {
                                    // Finds the shortest length, will return -1 if none is found or is longer than this.circularEdgeSearchLength


                                    int shortestStartEndPairLength = diGraph.shortestLengthFromNodeToNode(edgeOneNode, edgeTwoNode, this.circularEdgeSearchLength);

                                    Debug.Log("LENGTH: " + shortestStartEndPairLength + 
                                        "\n\t< " + edgeOneNode.getObject().getPrintString() + "  " + edgeTwoNode.getObject().getPrintString() + " >");

                                    if (shortestStartEndPairLength != -1)
                                        allStartEndPairs.addDoubleVal(edgeOneNode, edgeTwoNode);
                                }
                            }
                        }
                    }
                }
            }

            return allStartEndPairs.getRawList();
        }

        void organizeAllStartEndPairs(List<Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>> allStartEndPairs)
        {
            //      To save performance, we first find each possible start end pair, regardless of this.minCircularEdgeSearchLength
            //      Then find the shortest distance (displacement, not node length) pair from start to end
            //      Then check if it abides by this.minCircularEdgeSearchLength

            MinValue<float, Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>> startStopPairMinDisplacement = new MinValue<float, Double<DiDotNode<CoordsInt>, DiDotNode<CoordsInt>>>(allStartEndPairs.Count);

            // Calculate the diplacement between start and end nodes
            foreach (var pairs in allStartEndPairs)
            {


            }
        }
    }

    

    // ===========================================
    // Find closest node in edge functions
    // ===========================================
    public partial class ZoneVeinDiGraphContoller : ContainerAccessor
    {
        // Finds the closest node coord (TileMapConnection coord) based on the inputed destination world coord
        // Sets the global currentStartCoord
        //      Needs to adhear to controller rules
        //          1. Edges can't be shorter than newlyAddedMinEdgeLength (currently 3)
        //          2. Node can't have more than 4 connections
        //      Needs to also make sure that the closest node cords can be traveled to
        void findClosestNodeInDiGraph(List<DiDotEdge<CoordsInt>> allDiGraphEdges, CoordsInt destinationTileMapConnCoord, bool adhearToMinEdgeLength, ref TwoDList<Tile> tileMapConnections_JustTile, out bool nodeSearchFailed, out DimensionList floodedDimList)
        {
            int currentMinEdgeLength = 0;
            if (adhearToMinEdgeLength == true)
                currentMinEdgeLength = this.newlyAddedMinEdgeLength;

            bool startCoordIsValid = false;
            nodeSearchFailed = false;

            List<DiDotNode<CoordsInt>> rejectedStartNodes = new List<DiDotNode<CoordsInt>>();

            // Using the destination coord, get the flooded (as big as possible) dim list
            int maxFloodedArea = 100;
            floodedDimList = this.zoneVeinGenContainer.dimVeinZoneCreator.getFloodedDimensionsUsingAlternateTileMap(destinationTileMapConnCoord, this.zoneVeinGenContainer.debugMode, maxFloodedArea, ref tileMapConnections_JustTile);

            bool exhuastedAllNodesInEdge = false;
            List<DiDotEdge<CoordsInt>> rejectedEdges = new List<DiDotEdge<CoordsInt>>();
            while (startCoordIsValid == false)
            {
                exhuastedAllNodesInEdge = false;

                if (allDiGraphEdges.Count == rejectedEdges.Count)
                {
                    if (currentMinEdgeLength == 0)
                    {
                        nodeSearchFailed = true;
                        Debug.LogError("EXHAUSTED ALL EDGES IN ZONE DIGRAPH");
                        break;
                    }
                    else
                    {
                        if (zoneVeinGenContainer.debugMode == true)
                        {
                            zoneVeinGenContainer.currentZone.debugInfo.addLine("ZoneVein DiGraph Controller", "findClosestNodeInDiGraph()",
                                            "Exhausted all edges in zone digraph, reducing min edge length requirement");
                        }

                        rejectedEdges = new List<DiDotEdge<CoordsInt>>();
                        currentMinEdgeLength--;
                        continue;
                    }
                }

                //Debug.Log("\tFIND NODE RAW");

                // Finds closest node to the destination coords, doesn't care if it's valid or not
                findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> originalStartNode, allDiGraphEdges, rejectedEdges, destinationTileMapConnCoord, adhearToMinEdgeLength, currentMinEdgeLength);

                // Get the list of nodes that is acceptable
                List<DiDotNode<CoordsInt>> listOfEdgeNodes = null;
                if (adhearToMinEdgeLength == true)
                    listOfEdgeNodes = this.currentStartEdge.getNodeListExcludingEdgeNodes(currentMinEdgeLength);
                else
                    listOfEdgeNodes = this.currentStartEdge.getNodeList();

                // Check if the start coord is valid
                rejectedStartNodes = this.edgeToRejectedStartNodesDict[this.currentStartEdge];
                startCoordIsValid = isStartCoordValid(ref rejectedStartNodes, ref floodedDimList, ref listOfEdgeNodes, ref originalStartNode, ref exhuastedAllNodesInEdge);

                if (exhuastedAllNodesInEdge == true)
                {
                    rejectedEdges.Add(this.currentStartEdge);
                    if (zoneVeinGenContainer.debugMode == true)
                    {
                        zoneVeinGenContainer.currentZone.debugInfo.addLine("ZoneVein DiGraph Controller", "findClosestNodeInDiGraph()",
                                            "Exhausted all nodes in current edge");
                    }
                }
            }

        }

        bool isStartCoordValid(ref List<DiDotNode<CoordsInt>> rejectedStartNodes, ref DimensionList floodedDimList, ref List<DiDotNode<CoordsInt>> listOfEdgeNodes, 
                               ref DiDotNode<CoordsInt> originalStartNode, ref bool exhuastedAllNodesInEdge)
        {
            bool startCoordIsValid = false;
            //Debug.Log("\tSTART COORD IS VALID -- IN");

            // Test if the start coords can actually travel in the new direction bias
            //      1. Take the two d tile list and flood the dim list based on the destination coord
            //      2. Check if the start coord touches the flooded dim list, by going 1 unit in the new dir bias
            //List<DiDotNode<CoordsInt>> rejectedStartNodes = new List<DiDotNode<CoordsInt>>();
            while (startCoordIsValid == false)
            {
                CoordsInt startCoords = this.currentStartNode.getObject();
                //startCoords.print("NEW EDGE START: ");

                // First check all adjacent coords to see if they are a part of the flooded dim list
                startCoordIsValid = checkIfAnyAdjacentDirectionAreFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList);

                // If the adjacent coords are not a part of the flood coords, then seach along the edge for a new node/start coord
                if (startCoordIsValid == false)
                {
                    //this.currentStartNode.getObject().print("----- REJECTING COORDS: ");
                    rejectedStartNodes.Add(this.currentStartNode);

                    //Debug.Log("REJECTED NEW EDGE START");
                    findNextClosestNodeInEdge(ref rejectedStartNodes, ref listOfEdgeNodes, ref originalStartNode, ref exhuastedAllNodesInEdge);
                    //this.currentStartNode.getObject().print("NEXT NODE FOUND: ");

                    if (exhuastedAllNodesInEdge == true)
                        break;
                }
            }
            //Debug.Log("\tSTART COORD IS VALID -- OUT");

            return startCoordIsValid;
        }

        // Finds closest node to the destination coords, may or may not be what is needed (hence why it's Raw)
        void findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> originalStartNode, List<DiDotEdge<CoordsInt>> allDiGraphEdges, List<DiDotEdge<CoordsInt>> rejectedEdges, CoordsInt destinationTileMapConnCoord,
                                      bool adhearToMinEdgeLength, int currentMinEdgeLength)
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


                List<DiDotNode<CoordsInt>> rejectedStartNodes = this.edgeToRejectedStartNodesDict[edge];

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

            this.currentStartNode = minDistanceList.getMinVal().Value.getOne();
            originalStartNode = minDistanceList.getMinVal().Value.getOne();
            this.currentStartEdge = minDistanceList.getMinVal().Value.getTwo();
        }

        // Search along an edge by checking the passed in startNodes connected nodes
        //      1. This chooses the adjacent node to the current start node if possible
        //      2. If it can't it will revert back to the original start node and attempt adjacent nodes to that
        //      3. If the original start node has no open connections, then randomly select a start node
        void findNextClosestNodeInEdge(ref List<DiDotNode<CoordsInt>> rejectedStartNodes, ref List<DiDotNode<CoordsInt>> listOfEdgeNodes, ref DiDotNode<CoordsInt> originalStartNode, ref bool exhuastedAllNodesInEdge)
        {
            //rejectedStartNodes.Add(this.currentStartNode);

            bool foundNewStartNode = false;
            while (foundNewStartNode == false)
            {
                List<DiDotNode<CoordsInt>> startNodeConnections = this.currentStartNode.getRawListOfConnections();

                // Check the start nodes adjacent connected nodes
                foreach (var nodeConn in startNodeConnections)
                {
                    // Attempt to grab the next closest node to the current start node
                    if (listOfEdgeNodes.Contains(nodeConn) == true && rejectedStartNodes.Contains(nodeConn) == false)
                    {
                        this.currentStartNode = nodeConn;
                        foundNewStartNode = true;
                        break;
                    }
                }

                // If all orignal start node connections are rejected, find any unblocked node
                if (foundNewStartNode == false && this.currentStartNode.Equals(originalStartNode))
                {
                    //Debug.Log("ORINGAL REJECTED");
                    //Debug.Log("\tTOTAL COUNT: " + listOfEdgeNodes.Count + "\n\tREJECTED COUNT: " + rejectedStartNodes.Count);
                    foreach (var node in listOfEdgeNodes)
                    {
                        if (rejectedStartNodes.Contains(node) == false)
                        {
                            //node.getObject().print("ASSIGNED NEW NODE: ");

                            this.currentStartNode = node;
                            foundNewStartNode = true;
                            break;
                        }
                    }

                    // Exhuasted all edge nodes once list of nodes count and rejected start nodes count is the same it means all nodes have been checked
                    if (listOfEdgeNodes.Count == rejectedStartNodes.Count)
                    {
                        exhuastedAllNodesInEdge = true;
                        break;
                    }
                }
                // It's possible that the orignal start node was in the middle of the edge
                //      This while loop can head up/down the edge looking for nodes, but it will check one direction first
                //      Once we traveled all the way in one direction, assign the start node to the original start node so that we can search in the other directions
                else if (foundNewStartNode == false)
                {
                    //Debug.Log("NODE REJECTED");
                    this.currentStartNode = originalStartNode;

                    // Exhuasted all edge nodes once list of nodes count and rejected start nodes count is the same it means all nodes have been checked
                    if (listOfEdgeNodes.Count == rejectedStartNodes.Count)
                    {
                        exhuastedAllNodesInEdge = true;
                        break;
                    }
                }
            }
        }

        bool checkIfAnyAdjacentDirectionAreFloodedAndNotPermaLocked(ref CoordsInt startCoords, ref DimensionList floodedDimList)
        {
            bool startCoordIsValid = false;
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (dir != Direction.None)
                {
                    startCoordIsValid = adjacentDirectionIsFloodedAndNotPermaLocked(ref startCoords, ref floodedDimList, dir, out CoordsInt nextCoord); // nextCoord is not used here

                    if (startCoordIsValid == true)
                        break;
                }
            }
            return startCoordIsValid;
        }

        bool adjacentDirectionIsFloodedAndNotPermaLocked(ref CoordsInt startCoords, ref DimensionList floodedDimList, Direction dir, out CoordsInt nextCoord)
        {
            bool startCoordIsValid = false;

            //startCoords.print("PERMA IN: " + "   Dir: " + dir);

            CoordsInt checkFloodedIsNextToStart = startCoords.deepCopyInt();
            nextCoord = startCoords.deepCopyInt();

            if (dir == Direction.None)
                return startCoordIsValid;
            else
                checkFloodedIsNextToStart = CommonFunctions.changeCoordsBasedOnDir(checkFloodedIsNextToStart, dir, 1);


            //checkFloodedIsNextToStart.print("PERMA OUT: ");

            if (floodedDimList.coordIsMarked(checkFloodedIsNextToStart) == true && zoneVeinGenContainer.tileMapConnCoordIsPermaLocked(checkFloodedIsNextToStart) == false)
                startCoordIsValid = true;

            nextCoord = checkFloodedIsNextToStart;

            //startCoords.print("START: ");
            //nextCoord.print("NEXT: ");


            return startCoordIsValid;
        }

        bool adjacentDirectionIsFloodedAndNotLockedForAllPasses(ref CoordsInt startCoords, ref DimensionList floodedDimList, Direction dir, out CoordsInt nextCoord)
        {
            bool startCoordIsValid = false;


            CoordsInt checkFloodedIsNextToStart = startCoords.deepCopyInt();
            nextCoord = startCoords.deepCopyInt();

            if (dir == Direction.None)
                return startCoordIsValid;
            else
                checkFloodedIsNextToStart = CommonFunctions.changeCoordsBasedOnDir(checkFloodedIsNextToStart, dir, 1);

            //checkFloodedIsNextToStart.print("---------- Dir: " + dir +  "  OUT COORDS: " );

            if (floodedDimList.coordIsMarked(checkFloodedIsNextToStart) == true && zoneVeinGenContainer.tileMapConnCoordIsLocked__ForAllPasses(checkFloodedIsNextToStart) == false)
                startCoordIsValid = true;

            nextCoord = checkFloodedIsNextToStart;

            //startCoords.print("START: ");
            //nextCoord.print("NEXT: ");


            return startCoordIsValid;
        }
    }
}
