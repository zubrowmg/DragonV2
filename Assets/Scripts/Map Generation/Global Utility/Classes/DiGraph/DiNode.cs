using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiGraphClasses
{
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
}
