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

        public DiDotEdge(List<DiDotNode<T>> list, bool firstIsNodeOne)
        {

            if (firstIsNodeOne == false)
                list.Reverse();

            addNodeAsStart(list[0]);
            addNodeAsEnd(list[list.Count - 1]);
            this.orderedNodeList = new List<DiDotNode<T>>(list);
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
    }
}
