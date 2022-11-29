using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotEdge<T>
    {
        // Order is as follows
        //      NodeOne -> orderedObjList[0] -> orderedObjList[count-1] -> NodeTwo
        DiDotNode<T> nodeOne = null;
        DiDotNode<T> nodeTwo = null;

        List<DiDotNode<T>> orderedNodeList;// = new List<T>();

        List<DiDotEdge<T>> nodeOneEdges = new List<DiDotEdge<T>>();
        List<DiDotEdge<T>> nodeTwoEdges = new List<DiDotEdge<T>>();

        int id = -1;
        bool firstIsNodeOne = false;

        public DiDotEdge(List<DiDotNode<T>> list, bool firstIsNodeOne, int id)
        {
            this.id = id;
            this.firstIsNodeOne = firstIsNodeOne;

            if (firstIsNodeOne == false)
                list.Reverse();

            addNodeAsStart(list[0]);
            addNodeAsEnd(list[list.Count - 1]);
            this.orderedNodeList = new List<DiDotNode<T>>(list);
        }

        // For deep copies
        public DiDotEdge(DiDotNode<T> nodeOne, DiDotNode<T> nodeTwo, List<DiDotNode<T>> orderedNodeList, 
            List<DiDotEdge<T>> nodeOneEdges, List<DiDotEdge<T>> nodeTwoEdges, bool firstIsNodeOne, int id)
        {
            this.id = id;
            this.firstIsNodeOne = firstIsNodeOne;

            this.nodeOne = nodeOne;
            this.nodeTwo = nodeTwo;
            this.nodeOneEdges = nodeOneEdges;
            this.nodeTwoEdges = nodeTwoEdges;

            addNodeAsStart(orderedNodeList[0]);
            addNodeAsEnd(orderedNodeList[orderedNodeList.Count - 1]);
            this.orderedNodeList = new List<DiDotNode<T>>(orderedNodeList);
        }

        public DiDotEdge<T> deepCopy()
        {
            return new DiDotEdge<T>(this.nodeOne, this.nodeTwo, this.orderedNodeList, this.nodeOneEdges, this.nodeTwoEdges, this.firstIsNodeOne, this.id);
        }

        public bool addEdgeConnections(ref DiDotEdge<T> connectingEdge)
        {
            DiDotNode<T> connectingNodeOne = connectingEdge.getNodeOne();
            DiDotNode<T> connectingNodeTwo = connectingEdge.getNodeTwo();
            bool connectionAdded = true;

            if (connectingNodeOne.Equals(this.getNodeOne()) == true)
                CommonFunctions.addIfItemDoesntExist(ref nodeOneEdges, connectingEdge);
            else if (connectingNodeOne.Equals(this.getNodeTwo()) == true)
                CommonFunctions.addIfItemDoesntExist(ref nodeTwoEdges, connectingEdge);
            else if (connectingNodeTwo.Equals(this.getNodeOne()) == true)
                CommonFunctions.addIfItemDoesntExist(ref nodeOneEdges, connectingEdge);
            else if (connectingNodeTwo.Equals(this.getNodeTwo()) == true)
                CommonFunctions.addIfItemDoesntExist(ref nodeTwoEdges, connectingEdge);
            else
                connectionAdded = false;

            return connectionAdded;
        }

        public void addNodeAsStart(DiDotNode<T> newNode)
        {
            if (nodeOne == null)
                nodeOne = newNode;
            else
                Debug.LogError("DiDotNode - addNode(): nodeOne exists");
        }

        public void addNodeAsEnd(DiDotNode<T> newNode)
        {
            if (nodeTwo == null)
                nodeTwo = newNode;
            else
                Debug.LogError("DiDotNode - addNode(): nodeTwo exists!");
        }

        // =====================================================================================
        //                              Getters and Setters
        // =====================================================================================

        public ref DiDotNode<T> getNodeOne()
        {
            return ref this.nodeOne;
        }
        public ref DiDotNode<T> getNodeTwo()
        {
            return ref this.nodeTwo;
        }

        public ref List<DiDotNode<T>> getNodeList()
        {
            return ref this.orderedNodeList;
        }

        public ref List<DiDotEdge<T>> getNodeOneEdgeConnections()
        {
            return ref this.nodeOneEdges;
        }

        public ref List<DiDotEdge<T>> getNodeTwoEdgeConnections()
        {
            return ref this.nodeTwoEdges;
        }

        public int getId()
        {
            return this.id;
        }

        public bool nodeOneConnectsToGivenEdge(DiDotEdge<T> edge)
        {
            bool nodeOneConnects = false;
            if (nodeOneEdges.Contains(edge))
                nodeOneConnects = true;
            return nodeOneConnects;
        }

        public bool nodeOneIsDeadEnd()
        {
            bool deadEnd = false;
            if (nodeOneEdges.Count == 0)
                deadEnd = true;
            return deadEnd;
        }

        public bool nodeTwoIsDeadEnd()
        {
            bool deadEnd = false;
            if (nodeTwoEdges.Count == 0)
                deadEnd = true;
            return deadEnd;
        }

        public DiDotNode<T> getNodeThatConnectsToGivenEdge(DiDotEdge<T> edge)
        {
            DiDotNode<T> nodeConnection = null;

            if (nodeOneEdges.Contains(edge) == true)
                nodeConnection = nodeOne;
            else if (nodeTwoEdges.Contains(edge) == true)
                nodeConnection = nodeTwo;
            else
                Debug.LogError("DiDotEdge Class - getNodeThatConnectsToGivenEdge(): Edge_" + edge.getId() + " does not connect to this Edge_" + getId());

            return nodeConnection;
        }
    }
}
