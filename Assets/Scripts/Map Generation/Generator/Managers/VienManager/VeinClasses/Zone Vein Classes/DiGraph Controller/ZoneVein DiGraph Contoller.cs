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

        int newlyAddedMinEdgeLength = 3; // 3 nodes long
        int maxNodeConnections = 4;


        // Rejected DiDot Node and DiDot Edge
        Dictionary<DiDotEdge<CoordsInt>, List<DiDotNode<CoordsInt>>> edgeToRejectedStartNodesDict;
        DiDotNode<CoordsInt> currentStartNode;
        DiDotEdge<CoordsInt> currentStartEdge;

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
                    configureNewBranchEdge(out branchStartCoords, out branchSecondCoords, out dirBias, out edgeConfigFailed);
                    break;

                case GraphEdgeType.CircularEdge:
                    configureNewBranchEdge(out branchStartCoords, out branchSecondCoords, out dirBias, out edgeConfigFailed);
                    //configureCicularNewEdge();
                    break;
            }

            //Debug.Log("NEXT EDGE TYPE: " + nextEdgeType);

        }


        List<CoordsInt> configureNewBranchEdge(out CoordsInt startCoords, out CoordsInt secondCoord, out DirectionBias dirBias, out bool edgeConfigFailed)
        {
            // Basic configuration for now, expecting something more deliberate in the future
            // 1. Scan the allocated tile map connection for an empty space
            // 2. Determine the closest point in the di graph controller to that empty space
            // 3. Get the direction from the point to the empty space

            configureNewBranchInit();

            bool adhearToMinEdgeLength = true;
            List<CoordsInt> listOfZoneVeinCoords = new List<CoordsInt>();
            CoordsInt destinationMapConnCoord = zoneVeinGenContainer.zoneVeinNavigationController.findEmptySpaceCoord(out bool foundEmptySpace, out TwoDList<Tile> tileMapConnections_JustTile);
            edgeConfigFailed = false;

            startCoords = new CoordsInt(-1, -1);
            secondCoord = new CoordsInt(-1, -1);
            dirBias = new DirectionBias(Direction.None, Direction.None);
            
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

                //Debug.Log("===============================================================================" + "\n===============================================================================");
                bool edgeGenerationWasSuccessful = false;
                while (edgeGenerationWasSuccessful == false)
                {
                    //Debug.Log("FINDING CLOSEST NODE IN DI GRAPH");

                    // Gets the closest start node and set this.currentStartNode
                    findClosestNodeInDiGraph(allDiGraphEdges, destinationMapConnCoord, adhearToMinEdgeLength, ref tileMapConnections_JustTile, out bool nodeSearchFailed, out DimensionList floodedDimList);
                    startCoords = this.currentStartNode.getObject();
                    
                    //CoordsInt worldSpaceCoords = zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(startCoords);
                    //startCoords.print("\tINITIAL START COORDS: ");
                    //worldSpaceCoords.print("CLOSEST WORLD COORD TO FREE SPACE: ");

                    if (nodeSearchFailed == false)
                    {
                        //Debug.Log("NODE SEARCH WAS SUCCESSFUL");
                        //destinationMapConnCoord.print("FREE SPACE COORD: ");

                        dirBias = configurePrimaryDirection(ref startCoords, destinationMapConnCoord, ref floodedDimList, out secondCoord);

                        // Attempt to generate a new vein
                        //bool edgeGenerationFailed = false;
                        listOfZoneVeinCoords = this.zoneVeinGenContainer.zoneVeinNavigationController.randomlyGenerateZoneVeinBranch(startCoords, secondCoord, dirBias, out bool edgeGenerationFailed);
                        edgeGenerationWasSuccessful = !edgeGenerationFailed;

                        // If the Navigation controller failed to generate a new edge, reject the current start node
                        if (edgeGenerationFailed == true)
                        {
                            Debug.LogError("ZoneVein DiGraph Controller Class - configureNewEdge(): Edge Generation failed. Moving onto a different start node");

                            //this.currentStartNode.getObject().print("---- REJECTING COORDS FOR GENERATION: ");
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
                    secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotLockedForAllPasses(ref startCoords, ref floodedDimList, dir, out secondCoord);

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
                        secondaryCoordIsValid = adjacentDirectionIsFloodedAndNotLockedForAllPasses(ref startCoords, ref floodedDimList, oppDir, out secondCoord);

                        if (secondaryCoordIsValid)
                            break;
                    }
                }
            }

            if (secondaryCoordIsValid == false)
                Debug.LogError("ZoneVein DiGraph Controller Class - configurePrimaryDirection(): Could not find a secondary coord??? Should have been validated in findClosestNodeInDiGraph()");

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

            // Using the destination coord, get the max possible dim list
            int maxFloodedArea = 100;
            floodedDimList = this.zoneVeinGenContainer.dimVeinZoneCreator.getFloodedDimensionsUsingAlternateTileMap(destinationTileMapConnCoord, this.zoneVeinGenContainer.debugMode, maxFloodedArea, ref tileMapConnections_JustTile);

            bool exhuastedAllNodesInEdge = false;
            List<DiDotEdge<CoordsInt>> rejectedEdges = new List<DiDotEdge<CoordsInt>>();
            while (startCoordIsValid == false)
            {
                exhuastedAllNodesInEdge = false;

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
                        rejectedEdges = new List<DiDotEdge<CoordsInt>>();
                        currentMinEdgeLength--;
                        continue;
                    }
                }

                //Debug.Log("\tFIND NODE RAW");

                // Finds closest node to the destination coords, doesn't care if it's valid or not
                findClosestNodeInEdgeRaw(out DiDotNode<CoordsInt> originalStartNode, allDiGraphEdges, rejectedEdges, destinationTileMapConnCoord, adhearToMinEdgeLength, currentMinEdgeLength);

                //this.currentStartNode.getObject().print("RAW NODE OUTPUT: ");

                

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
                    Debug.LogError("EXHAUSTED ALL NODES IN THE EDGE");
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
            //startCoords.print("Dir: " + dir +  "  ALL IN: " );

            CoordsInt checkFloodedIsNextToStart = startCoords.deepCopyInt();
            nextCoord = startCoords.deepCopyInt();

            if (dir == Direction.None)
                return startCoordIsValid;
            else
                checkFloodedIsNextToStart = CommonFunctions.changeCoordsBasedOnDir(checkFloodedIsNextToStart, dir, 1);

            //checkFloodedIsNextToStart.print("ALL OUT: ");

            if (floodedDimList.coordIsMarked(checkFloodedIsNextToStart) == true && zoneVeinGenContainer.tileMapConnCoordIsLocked__ForAllPasses(checkFloodedIsNextToStart) == false)
                startCoordIsValid = true;

            nextCoord = checkFloodedIsNextToStart;

            //startCoords.print("START: ");
            //nextCoord.print("NEXT: ");


            return startCoordIsValid;
        }
    }
}
