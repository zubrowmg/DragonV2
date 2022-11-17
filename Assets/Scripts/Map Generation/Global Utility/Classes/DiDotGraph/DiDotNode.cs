using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

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

        public T getObject()
        {
            return this.nodeObject;
        }

        public void addNode(ref DiDotNode<T> node)
        {
            CommonFunctions.addIfItemDoesntExist(ref listOfConnections, node);
        }

        public List<DiDotNode<T>> getRawListOfConnections()
        {
            return this.listOfConnections;
        }

        public int numOfConnections()
        {
            return this.listOfConnections.Count;
        }

        public bool isDeadEnd()
        {
            bool deadEnd = false;
            if (numOfConnections() == 1)
                deadEnd = true;
            return deadEnd;
        }

        public bool isIntersection()
        {
            bool deadEnd = false;
            if (numOfConnections() > 2)
                deadEnd = true;
            return deadEnd;
        }
    }
}
