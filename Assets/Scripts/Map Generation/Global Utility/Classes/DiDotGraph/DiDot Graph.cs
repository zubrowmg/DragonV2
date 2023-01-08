using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotGraph<T> : DiDotGraphNavigation<T>
    {
        // This class is probably going to be the main way to create a digraph
        //      Just a connection of nodes, instead of edges and nodes since it's too much effort to add edges during the digraph creation
        //      Once done you can export your DiDotGraph into a normal DiGraph with nodes and edges

        Dictionary<T, DiDotNode<T>> objToNode = new Dictionary<T, DiDotNode<T>>();
        bool diGraphChanged = false;
        //T currentObject = default(T);

        // Current Graph Characteristics
        List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();
        List<DiDotEdge<T>> listOfNonCircularEdges = new List<DiDotEdge<T>>();
        List<DiDotCircularEdge<T>> listOfCircularEdges = new List<DiDotCircularEdge<T>>();

        // Id counters
        int currentEdgeId = 0;
        int currentCircularEdgeId = 0;

        

        public DiDotGraph() : base()
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

            // First Node added to the di graph
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
        void identifyGraphEdges()
        {
            this.listOfEdges = new List<DiDotEdge<T>>();
            this.listOfNonCircularEdges = new List<DiDotEdge<T>>();
            this.listOfCircularEdges = new List<DiDotCircularEdge<T>>();

            // Get a node to start at
            DiDotNode<T> startNode = findNodeStartForAnalysis();
            List<DiDotNode<T>> doNotTravelList = new List<DiDotNode<T>>();


            // Will get a list of edges from a node, the node supplied SHOULD be an endnode or intersecting node
            //      Traverses entire graph
            //      Should also connect edges to one another
            List<DiDotEdge<T>> currentListOfEdges = getAllEdgesStartingFromNodeStart(startNode, ref doNotTravelList);
            CommonFunctions.addIfItemDoesntExist(ref this.listOfEdges, currentListOfEdges);

            // Check if any edges are circular
            // Go through each edge in the list and find all possible routes back to the same edge (AKA a circular edge)
            List<DiDotEdge<T>> alreadyCheckedEdges = new List<DiDotEdge<T>>();
            foreach (var edge in this.listOfEdges)
            {
                List<List<DiDotEdge<T>>> listOfCircularEdges = new List<List<DiDotEdge<T>>>();

                // If the edge is already a part of a circular edge, then don't search for it
                if (alreadyCheckedEdges.Contains(edge) == false)
                    listOfCircularEdges = getCircularEdges__Start(edge);

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

            // Add to listOfNonCircularEdges, basically any edges that are not circular edges
            foreach (var edge in this.listOfEdges)
            {
                bool edgeIsNotCircular = true;
                foreach (var cirEdge in this.listOfCircularEdges)
                {
                    if (cirEdge.circularEdgeContains(edge) == true)
                    {
                        edgeIsNotCircular = false;
                        break;
                    }
                }
                if (edgeIsNotCircular == true)
                    this.listOfNonCircularEdges.Add(edge);
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
        // Recursive functions that search through each di graph node
        // ===================================


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // getEdgesStartingFromNodeStart() and getEdgeStartingFromNode() need to be templated into DiDotGraphNavigation class
        //      Just like edge recursion was done
        //      This function traverses through all nodes

        // Will get a list of nodes aka an edge, starting from a specified node
        List<DiDotEdge<T>> getAllEdgesStartingFromNodeStart(DiDotNode<T> startNode, ref List<DiDotNode<T>> doNotTravelList)
        {
            List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();
            List<DiDotNode<T>> currentPath = new List<DiDotNode<T>>();
            DiDotEdge<T> prevEdge = null;

            // Will get all edges in the digraph, but they won't be connected
            traverseEdgesStartingFromNode(startNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref prevEdge);

            // Connect all edges to one another
            Dictionary<DiDotNode<T>, List<DiDotEdge<T>>> nodeToEdge = new Dictionary<DiDotNode<T>, List<DiDotEdge<T>>>();
            List<DiDotNode<T>> endNodes = new List<DiDotNode<T>>();

            // First get all edges that share a common node
            foreach (var edge in listOfEdges)
            {
                // Each edge has 2 end nodes
                for (int i = 0; i < 2; i++)
                {
                    DiDotNode<T> node = i == 0 ? edge.getNodeOne() : edge.getNodeTwo();
                    bool nodeIsDeadEnd = node.isDeadEnd();

                    if (nodeIsDeadEnd == false)
                    {

                        if (nodeToEdge.ContainsKey(node) == false)
                            nodeToEdge.Add(node, new List<DiDotEdge<T>> { edge });
                        else
                            nodeToEdge[node].Add(edge);
                    }
                }
            }

            // Then connect all edges to each other
            foreach (var item in nodeToEdge)
            {
                DiDotNode<T> node = item.Key;
                List<DiDotEdge<T>> edgeList = item.Value;

                foreach (var edge in edgeList)
                {
                    foreach (var edge2 in edgeList)
                    {
                        if (edge.Equals(edge2) == false)
                            edge.addEdgeConnections(edge2);
                    }
                }
            }

            return listOfEdges;
        }

        void traverseEdgesStartingFromNode(DiDotNode<T> currentNode, ref List<DiDotNode<T>> currentPath, ref List<DiDotNode<T>> doNotTravelList, 
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
                        /*
                        DiDotEdge<T> prevEdgeCopy = null;
                        if (prevEdge != null)
                        {
                            prevEdge.addEdgeConnections(ref newEdge);
                            newEdge.addEdgeConnections(ref prevEdge);
                            prevEdgeCopy = prevEdge.deepCopy();
                        }
                        */

                        // Continue traversing the graph
                        if (nextNode.isDeadEnd() == false)
                        {
                            // Reset current path
                            currentPath = new List<DiDotNode<T>>();

                            traverseEdgesStartingFromNode(nextNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref newEdge);

                        }

                        // Find the prev edge in the list of edges, to make sure we keep editing the same edge
                        //      MIGHT NEED THIS FOR MEMORY SHANANAGINS!!!!!!!!!!!
                        //int prevIndex = listOfEdges.IndexOf(prevEdgeCopy);
                        //if (prevIndex != -1)
                        //    prevEdge = listOfEdges[prevIndex];
                    }
                    else
                    {
                        // Continue traversing the graph
                        traverseEdgesStartingFromNode(nextNode, ref currentPath, ref doNotTravelList, ref listOfEdges, ref prevEdge);
                    }
                    
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            currentPath.Remove(currentNode);
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        // =====================================================================================
        //                              Getters and Setters
        // =====================================================================================

        public List<DiDotEdge<T>> getListOfEdges()
        {
            return this.listOfEdges;
        }
        public int getNumOfEdges()
        {
            return this.listOfEdges.Count;
        }

        public List<DiDotCircularEdge<T>> getListOfCircularEdges()
        {
            return this.listOfCircularEdges;
        }
        public int getNumOfCircularEdges()
        {
            return this.listOfCircularEdges.Count;
        }


        public List<DiDotEdge<T>> getListOfNonCircularEdges()
        {
            return this.listOfNonCircularEdges;
        }
        public int getNumOfNonCircularEdges()
        {
            return this.listOfNonCircularEdges.Count;
        }

        public List<DiDotNode<T>> allNodesInGraph()
        {
            List<DiDotNode<T>> nodeList = new List<DiDotNode<T>>();
            foreach (var edge in listOfEdges)
            {
                nodeList.AddRange(edge.getNodeList());
            }
            return nodeList;
        }
    }
}