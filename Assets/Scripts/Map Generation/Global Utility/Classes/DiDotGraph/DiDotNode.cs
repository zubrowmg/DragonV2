using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiDotGraphClasses
{
    public class DiDotNode<T>
    {
        List<DiDotNode<T>> listOfConnections = new List<DiDotNode<T>>();
        T nodeObject = default(T);

        public DiDotNode(ref T obj)
        {
            this.nodeObject = obj;
        }

        public void addNode(ref DiDotNode<T> node)
        {
            listOfConnections.Add(node);
        }
    }
}
