using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotGraph<T>
    {
        // This class is probably going to be the main way to create a digraph
        //      Just a connection of nodes, instead of edges and nodes since it's too much effort to add edges during the digraph creation
        //      Once done you can export your DiDotGraph into a normal DiGraph with nodes and edges

        Dictionary<T, DiDotNode<T>> objToNode = new Dictionary<T, DiDotNode<T>>();
        bool diGraphChanged = false;
        //T currentObject = default(T);

        // Current Graph Characteristics
        List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();
        List<DiDotCircularEdge<T>> listOfCircularEdges = new List<DiDotCircularEdge<T>>();

        int currentEdgeId = 0;
        int currentCircularEdgeId = 0;

        public DiDotGraph()
        {
        }

        // =====================================================================================
        //                              Di Dot Graph Functions
        // =====================================================================================

        DiDotNode<T> addNode(ref T startingObject)
        {
            DiDotNode<T> newNode = new DiDotNode<T>(ref startingObject);
            objToNode.Add(startingObject, newNode);
            return newNode;
        }

        // Adding new object
        //      Previous object should be an object that already has been added
        //      It should also be the object that connects to the new object
        public void addObject(ref T prevObject, ref T newObject)
        {
            this.diGraphChanged = true;

            DiDotNode<T> existingNode;
            bool newObjectAlreadyExists = objToNode.TryGetValue(newObject, out existingNode);

            DiDotNode<T> prevNode;
            bool prevObjectExists = objToNode.TryGetValue(prevObject, out prevNode);

            // First Nodes
            if (prevObjectExists == false && objToNode.Count == 0)
            {
                DiDotNode <T> startNode = addNode(ref prevObject);
                DiDotNode <T> nextNode = addNode(ref newObject);
                linkNodes(ref startNode, ref nextNode);
            }
            // Previous node should exist
            else if (prevObjectExists == false)
                Debug.LogError("DiDotGraph Class - addObject(): Previous object does not exist");
            // If the new object does not exist, then creat a new node and add it
            else if (newObjectAlreadyExists == false)
            {
                DiDotNode<T> newNode = addNode(ref newObject);
                linkNodes(ref newNode, ref prevNode);
            }
            // If the new object already has a node, then link it to the previous node
            else
                linkNodes(ref existingNode, ref prevNode);

        }

        void linkNodes(ref DiDotNode<T> newNode, ref DiDotNode<T> existingNode)
        {
            newNode.addNode(ref existingNode);
            existingNode.addNode(ref newNode);
        }

        // =====================================================================================
        //                              Di Dot Analysis Functions
        // =====================================================================================

        // Will analyze the graph to get a list of edges and list of circular edges
        //    
        public void analyzeGraph()
        {
            // Only analyze if there's been an edit to the graph
            if (this.diGraphChanged == true)
            {
                this.currentEdgeId = 0;
                this.currentCircularEdgeId = 0;

                identifyGraphEdges();
                analyzeGraphStats();
            }

            this.diGraphChanged = false;
        }

        public void analyzeGraphStats()
        {
            // For now this function doesn't do anything, for basic enablement I only care about number of edges and circular edge
            //      In the future I want stats such as:
            //          - How central the circular edges are
            //          - How big the circular edges are
            //          - Amount of dead ends
            //          - Length of dead ends
            //          - Any stats will help future controllers to decide their next move
        }

        // Will traverse the graph to get a list of edges and list of circular edges
        //      Currently only supports graphs that are connected, aka will not work for a graph split in two
        public void identifyGraphEdges()
        {
            this.listOfEdges = new List<DiDotEdge<T>>();
            this.listOfCircularEdges = new List<DiDotCircularEdge<T>>();

            // Get a node to start at
            DiDotNode<T> startNode = findNodeStartForAnalysis();
            List<DiDotNode<T>> doNotTravelList = new List<DiDotNode<T>>();

            // Will get a list of edges from a node, the node supplied SHOULD be an endnode or intersecting node
            //      Traverses entire graph
            //      Should also connect edges to one another
            List<DiDotEdge<T>> currentListOfEdges = getEdgesStartingFromNodeStart(startNode, ref doNotTravelList);
            CommonFunctions.addIfItemDoesntExist(ref this.listOfEdges, currentListOfEdges);

            // Check if any edges are circular
            // Go through each edge in the list and find all possible routes back to the same edge (AKA a circular edge)
            List<DiDotEdge<T>> alreadyCheckedEdges = new List<DiDotEdge<T>>();
            foreach (var edge in this.listOfEdges)
            {
                List<List<DiDotEdge<T>>> listOfCircularEdges = new List<List<DiDotEdge<T>>>();

                // If the edge is already a part of a circular edge, then don't search for it
                if (alreadyCheckedEdges.Contains(edge) == false)
                    listOfCircularEdges = getCircularEdgesStart(edge);

                if (listOfCircularEdges.Count == 1)
                {
                    createAndAddCircularEdge(listOfCircularEdges[0]);
                    alreadyCheckedEdges.AddRange(listOfCircularEdges[0]);
                }
                // If list of circular edges is greater than 1, means we have multiple circular paths
                //      Idea is that we only need to create the smallest circular edge in the list
                //      If there are multiple circular edges with the same edge count (that are min edge count), then create all of them
                else if (listOfCircularEdges.Count > 1)
                {
                    List<DiDotEdge<T>> smallestCircularEdge = CommonFunctions.getSmallestListCount(listOfCircularEdges);
                    List<List<DiDotEdge<T>>> listOfMinCircularEdges = new List<List<DiDotEdge<T>>>();

                    foreach (var cirEdge in listOfCircularEdges)
                    {
                        if (cirEdge.Count == smallestCircularEdge.Count)
                            listOfMinCircularEdges.Add(cirEdge);
                    }

                    foreach (var cirEdge in listOfMinCircularEdges)
                    {
                        createAndAddCircularEdge(cirEdge);
                        alreadyCheckedEdges.AddRange(cirEdge);
                    }
                }
            }
        }

        void createAndAddCircularEdge(List<DiDotEdge<T>> edge)
        {
            DiDotCircularEdge<T> newCircularEdge = new DiDotCircularEdge<T>(edge, currentCircularEdgeId);
            this.listOfCircularEdges.Add(newCircularEdge);
            currentCircularEdgeId++;
        }

        DiDotNode<T> findNodeStartForAnalysis()
        {
            // First search for a any dead end node, should be more probable to find a dead end than a node intersection
            DiDotNode<T> startNode = findAnyDeadEndNode(out bool foundDeadEnd);

            // If a dead end node couldn't be found, then it might mean that the graph is currently a loop (One circular edge)
            if (foundDeadEnd == false)
            {
                startNode = findAnyNodeIntersection(out bool foundIntersection);
                if (foundIntersection == false)
                    Debug.LogError("DiDotGraph Class - findNodeStartForAnalysis: Could not find a start node. Is there even a graph?");
            }

            return startNode;
        }

        // Will search through objToNode dictionary for any node that is a dead end
        DiDotNode<T> findAnyDeadEndNode(out bool foundDeadEnd)
        {
            DiDotNode<T> deadEndNode = null;
            foundDeadEnd = false;

            foreach (var pair in objToNode)
            {
                // Search for first dead end
                if (pair.Value.isDeadEnd() == true)
                {
                    foundDeadEnd = true;
                    deadEndNode = pair.Value;
                    break;
                }
                    
            }

            return deadEndNode;
        }
        
        // Will search through objToNode dictionary for any node that is an intersection
        DiDotNode<T> findAnyNodeIntersection(out bool foundIntersection)
        {
            DiDotNode<T> intersection = null;
            foundIntersection = false;

            foreach (var pair in objToNode)
            {
                // Search for first intersection
                if (pair.Value.isIntersection() == true)
                {
                    foundIntersection = true;
                    intersection = pair.Value;
                    break;
                }
            }

            return intersection;
        }

        // =====================================================================================
        //                            Recursive Search Functions
        // =====================================================================================

        // Will get a list of nodes aka an edge, starting from a specified node
        List<DiDotEdge<T>> getEdgesStartingFromNodeStart(DiDotNode<T> startNode, ref List<DiDotNode<T>> doNotTravelList)
        {
            List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();
            List<DiDotNode<T>> currentPath = new List<DiDotNode<T>>();
            DiDotEdge<T> prevEdge = null;

            getEdgeStartingFromNode(startNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref prevEdge);
            return listOfEdges;
        }


        void getEdgeStartingFromNode(DiDotNode<T> currentNode, ref List<DiDotNode<T>> currentPath, ref List<DiDotNode<T>> doNotTravelList, 
                                    ref List<DiDotEdge<T>> listOfEdges, ref DiDotEdge<T> prevEdge)
        {
            currentPath.Add(currentNode);
            CommonFunctions.addIfItemDoesntExist(ref doNotTravelList, currentNode);

            // Go through each connection in the current node
            foreach (var nextNode in currentNode.getRawListOfConnections())
            {
                bool nodeCanBeTraveledTo = !doNotTravelList.Contains(nextNode);

                // If this node hasn't been traveled to or is not on the doNotTravelList then proceed
                if (nodeCanBeTraveledTo == true)
                {
                    // If we hit a dead end or an intersection, then create and edge and link to previous edge
                    if (nextNode.isDeadEnd() == true || nextNode.isIntersection() == true)
                    {
                        CommonFunctions.addIfItemDoesntExist(ref doNotTravelList, nextNode);

                        // Need to create a new node list for memory reasons
                        List<DiDotNode<T>> tempPath = new List<DiDotNode<T>>(currentPath); 
                        tempPath.Add(nextNode);

                        // Create a new edge
                        bool firstNodeIsNodeOne = true;
                        DiDotEdge<T> newEdge = new DiDotEdge<T>(tempPath, firstNodeIsNodeOne, this.currentEdgeId);
                        currentEdgeId++;
                        listOfEdges.Add(newEdge);

                        // Connect to previous edge, if prev edge is null then that means this is the first edge found
                        DiDotEdge<T> prevEdgeCopy = null;
                        if (prevEdge != null)
                        {
                            prevEdge.addEdgeConnections(ref newEdge);
                            newEdge.addEdgeConnections(ref prevEdge);
                            prevEdgeCopy = prevEdge.deepCopy();
                        }

                        // Continue traversing the graph
                        if (nextNode.isDeadEnd() == false)
                            getEdgeStartingFromNode(nextNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref newEdge);

                        // Find the prev edge in the list of edges, to make sure we keep editing the same edge
                        //      MIGHT NEED THIS FOR MEMORY SHANANAGINS!!!!!!!!!!!
                        //int prevIndex = listOfEdges.IndexOf(prevEdgeCopy);
                        //if (prevIndex != -1)
                        //    prevEdge = listOfEdges[prevIndex];
                    }
                    else
                    {
                        // Continue traversing the graph
                        getEdgeStartingFromNode(nextNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref prevEdge);
                    }
                    
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            currentPath.Remove(currentNode);
        }

        // Will get a list of nodes aka an edge, starting from a specified node
        List<List<DiDotEdge<T>>> getCircularEdgesStart(DiDotEdge<T> startEdge)
        {
            List<List<DiDotEdge<T>>> listOfCircularEdges = new List<List<DiDotEdge<T>>>();
            List<DiDotEdge<T>> doNotTravelList = new List<DiDotEdge<T>>();
            List<DiDotEdge<T>> currentPath = new List<DiDotEdge<T>>();
            DiDotNode<T> startEdgeNode = startEdge.getNodeOne();

            // If any node is a dead end, then it's impossible for it to be a circular edge
            if (startEdge.nodeOneIsDeadEnd() == true || startEdge.nodeTwoIsDeadEnd() == true)
                return listOfCircularEdges;

            getCircularEdges(startEdge, startEdgeNode, startEdge, startEdgeNode, ref currentPath, ref doNotTravelList, ref listOfCircularEdges);
            return listOfCircularEdges;
        }


        void getCircularEdges(DiDotEdge<T> startEdge, DiDotNode<T> startEdgeNode, DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode,
                            ref List<DiDotEdge<T>> currentPath, ref List<DiDotEdge<T>> doNotTravelList, ref List<List<DiDotEdge<T>>> listOfCircularEdges)
        {
            currentPath.Add(currentEdge);
            CommonFunctions.addIfItemDoesntExist(ref doNotTravelList, currentEdge);

            // Determine which edge connections to iterate through
            List<DiDotEdge<T>> edgeConnections;
            DiDotNode<T> nextEdgeBaseNode;
            if (currentEdgeNode.Equals(currentEdge.getNodeOne()) == true)
            {
                edgeConnections = currentEdge.getNodeOneEdgeConnections();
                nextEdgeBaseNode = currentEdge.getNodeOne();
            }
            else
            {
                edgeConnections = currentEdge.getNodeTwoEdgeConnections();
                nextEdgeBaseNode = currentEdge.getNodeTwo();
            }

            // Go through each connection in the current edge
            foreach (var nextEdge in edgeConnections)
            {
                bool edgeCanBeTraveledTo = !doNotTravelList.Contains(nextEdge);

                // If this edge hasn't been traveled to or is not on the doNotTravelList then proceed
                if (edgeCanBeTraveledTo == true)
                {
                    // Figure out which node in the current edge connects to the next edge
                    bool nodeOneConnects = currentEdge.nodeOneConnectsToGivenEdge(nextEdge);
                    bool deadEnd = false;
                    if (nodeOneConnects == true)
                        deadEnd = nextEdge.nodeTwoIsDeadEnd();
                    else
                        deadEnd = nextEdge.nodeOneIsDeadEnd();

                    // If we make it back to the starting edge it can mean two things
                    if (nextEdge.Equals(startEdge) == true)
                    {
                        // If we hit the starting node of the starting edge, then we have a clean circular edge
                        //      Else we hit the other end of the starting edge, meaning that the graph probably has another circular edge
                        //      Ex. x is the starting node
                        //            x---o---o
                        //                |   |
                        //                o---o
                        DiDotNode<T> connectingNode = currentEdge.getNodeThatConnectsToGivenEdge(nextEdge);

                        // Record the clean edge
                        if (connectingNode.Equals(startEdgeNode) == true)
                        {
                            // Need to create a new edge list for memory reasons
                            List<DiDotEdge<T>> tempPath = new List<DiDotEdge<T>>(currentPath);
                            tempPath.Add(nextEdge);

                            // Connect to previous edge, if prev edge is null then that means this is the first edge found
                            listOfCircularEdges.Add(tempPath);
                        }
                    }
                    // If we hit a dead end stop here
                    else if (deadEnd == true)
                    {

                    }
                    // Else continue searching
                    else
                    {
                        // Need to get the next edge node, should be opposite of the base node
                        DiDotNode<T> nextEdgeNode;
                        if (nextEdge.getNodeOne().Equals(nextEdgeBaseNode) == true)
                            nextEdgeNode = nextEdge.getNodeTwo();
                        else
                            nextEdgeNode = nextEdge.getNodeOne();

                        getCircularEdges(startEdge, startEdgeNode, nextEdge, nextEdgeNode, ref currentPath, ref doNotTravelList, ref listOfCircularEdges);
                    }
                        

                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            currentPath.Remove(currentEdge);
            doNotTravelList.Remove(currentEdge);
        }

        // =====================================================================================
        //                              Getters and Setters
        // =====================================================================================

        public List<DiDotEdge<T>> getListOfEdges()
        {
            return this.listOfEdges;
        }

        public List<DiDotCircularEdge<T>> getListOfCircularEdges()
        {
            return this.listOfCircularEdges;
        }
    }
}