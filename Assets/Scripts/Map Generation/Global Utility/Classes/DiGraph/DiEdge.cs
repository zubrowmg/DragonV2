using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiGraphClasses
{
    public class DiEdge<T>
    {
        // Order is as follows
        //      NodeOne -> orderedObjList[0] -> orderedObjList[count-1] -> NodeTwo
        DiNode<T> nodeOne = null;
        DiNode<T> nodeTwo = null;
        List<T> orderedObjList = new List<T>();


        public DiEdge(ref T obj)
        {
            this.orderedObjList.Add(obj);
        }

        public void extendEdge(ref T obj)
        {
            this.orderedObjList.Add(obj);
        }

        // Meant to be used for brand new edges, where an edge is created starting from a node
        public void addNodeAsStart(ref DiNode<T> newNode)
        {
            if (nodeOne == null)
                nodeOne = newNode;
            else
                Debug.LogError("DiEdge - addNode(): nodeOne exists");
        }

        // Meant to be used for existing edges that run into a node
        public void addNodeAsEnd(ref DiNode<T> newNode)
        {
            if (nodeTwo == null)
                nodeTwo = newNode;
            else
                Debug.LogError("DiEdge - addNode(): nodeTwo exists!");
        }
    }
}
