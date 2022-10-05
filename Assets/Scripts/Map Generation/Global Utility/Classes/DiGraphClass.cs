using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiGraphClasses
{
    public class DiGraph<T>
    {
        List<DiNode<T>> listOfNodes = new List<DiNode<T>>();

        Dictionary<DiNode<T>, List<DiEdge<T>>> nodeToEdgeDict = new Dictionary<DiNode<T>, List<DiEdge<T>>>();
        Dictionary<DiEdge<T>, List<DiNode<T>>> edgeToNodeDict = new Dictionary<DiEdge<T>, List<DiNode<T>>>();

        Dictionary<T, DiNode<T>> objToNode = new Dictionary<T, DiNode<T>>();
        Dictionary<T, DiEdge<T>> objToEdge = new Dictionary<T, DiEdge<T>>();

        // Variables for DiGraph creation
        T currentObject = default(T);
        bool currentObjectIsEdge = false;
        bool currentObjectIsNode = false;

        public DiGraph()
        {

        }

        public void addNodesAndEdges(ref DiNode<T> node, List<DiEdge<T>> edges)
        {
            // Function assumes that node and edges are linked
            //bool nodeLogged = nodeIsLogged(node);

            //// Add new node to edge dict entry if needed
            //if (nodeLogged == false)
            //{
            //    nodeToEdgeDict.Add(node, new List<DiEdge<T>>());
            //    listOfNodes.Add(node);
            //}

            //for (int i = 0; i < edges.Count; i++)
            //{
            //    DiEdge<T> currentEdge = edges[i];

            //    // Add the edge to the node to edge dict
            //    if (nodeToEdgeDict[node].Contains(currentEdge) == false)
            //        nodeToEdgeDict[node].Add(currentEdge);

            //    // Add new edge to node dict entry if needed
            //    bool edgeLogged = edgeIsLogged(currentEdge);
            //    if (edgeLogged == false)
            //        edgeToNodeDict.Add(currentEdge, new List<DiNode<T>>());


            //}
        }

        // Pretty much always will start a DiGraph with an edge
        public void addStartingEdge(ref T startingEdgeObject)
        {
            currentObjectIsEdge = true;
            currentObjectIsNode = false;

            currentObject = startingEdgeObject;
            DiEdge<T> newEdge = new DiEdge<T>(ref startingEdgeObject);
            objToEdge.Add(startingEdgeObject, newEdge);
        }

        public void addObject(ref T nextObject)
        {
            if (currentObjectIsEdge == true)
            {
                // If the object does not exist then extend the current edge
                    // Extend the current edge
                    objToEdge[this.currentObject].extendEdge(ref nextObject);
                // If the object does exist, then create a new node

            }
            else
            {
                // Switch context from node to edge and add the new edge to the node

            }
        }

        public void switchContext(ref T existingObject)
        {
            // Switch the current object to an ALREADY LOGGED object
        }

        //public void extendEdge(ref DiEdge<T> edge, cu)

        bool nodeIsLogged(DiNode<T> node)
        {
            bool nodeIsLogged = false;
            if (nodeToEdgeDict.ContainsKey(node) == true)
                nodeIsLogged = true;
            return nodeIsLogged;
        }

        bool edgeIsLogged(DiEdge<T> edge)
        {
            bool edgeIsLogged = false;
            if (edgeToNodeDict.ContainsKey(edge) == true)
                edgeIsLogged = true;
            return edgeIsLogged;
        }
    }

    public class DiNode<T>
    {
        List<DiEdge<T>> listOfEdges = new List<DiEdge<T>>();
        T nodeObject = default(T);

        public DiNode(ref T obj)
        {
            this.nodeObject = obj;
        }

        public void addEdge(ref DiEdge<T> edge)
        {
            listOfEdges.Add(edge);
        }
    }

    public class DiEdge<T>
    {
        List<T> orderedObjList = new List<T>(); 

        public DiEdge(ref T obj)
        {
            this.orderedObjList.Add(obj);
        }

        public void extendEdge(ref T obj)
        {
            this.orderedObjList.Add(obj);
        }
    }
}

