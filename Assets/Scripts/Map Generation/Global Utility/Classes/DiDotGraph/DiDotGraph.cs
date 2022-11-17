using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;

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
        List<List<DiDotNode<T>>> listOfEdges = new List<List<DiDotNode<T>>>();

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

        public void analyzeGraph()
        {
            listOfEdges = new List<List<DiDotNode<T>>>();

            // Only analyze if there's bee an edit to the graph
            if (this.diGraphChanged == true)
            {
                // Get a node to start at
                DiDotNode<T> startNode = findNodeStartForAnalysis();
                List<DiDotNode<T>> doNotTravelList = new List<DiDotNode<T>>();

                List<DiDotNode<T>> currentEdge = getEdgeStartingFromNodeStart(startNode, ref doNotTravelList);

                listOfEdges.Add(currentEdge);
            }

            this.diGraphChanged = false;
        }

        // Will get a list of nodes aka an edge, starting from a specified node
        List<DiDotNode<T>> getEdgeStartingFromNodeStart(DiDotNode<T> startNode, ref List<DiDotNode<T>> doNotTravelList)
        {
            List<DiDotNode<T>> currentPath = new List<DiDotNode<T>>();
            getEdgeStartingFromNode(startNode, ref currentPath, ref doNotTravelList);
            return currentPath;
        }



        void getEdgeStartingFromNode(DiDotNode<T> currentNode, ref List<DiDotNode<T>> currentPath, ref List<DiDotNode<T>> doNotTravelList)
        {
            currentPath.Add(currentNode);

            // Go through each connection in the current node
            foreach (var nextNode in currentNode.getRawListOfConnections())
            {
                bool nodeCanBeTraveledTo = !doNotTravelList.Contains(nextNode);

                // If this node hasn't been traveled to or is not on the doNotTravelList then proceed
                if (nodeCanBeTraveledTo == true)
                {
                    // If we hit a dead end or an intersection, then we end the search here
                    if (nextNode.isDeadEnd() == true || nextNode.isIntersection() == true)
                    {
                        doNotTravelList.Add(nextNode);
                        currentPath.Add(nextNode);
                    }
                    else
                    {
                        doNotTravelList.Add(nextNode);
                        getEdgeStartingFromNode(nextNode, ref currentPath, ref doNotTravelList);
                    }
                }
            }
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
        //                              Getters and Setters
        // =====================================================================================

        public List<List<DiDotNode<T>>> getListOfEdges()
        {
            return this.listOfEdges;
        }
    }
}