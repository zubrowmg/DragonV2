using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiDotGraphClasses
{
    public class DiDotGraph<T>
    {
        // This class is probably going to be the main way to create a digraph
        //      Just a connection of nodes, instead of edges and nodes since it's too much effort to add edges during the digraph creation
        //      Once done you can export your DiDotGraph into a normal DiGraph with nodes and edges

        Dictionary<T, DiDotNode<T>> objToNode = new Dictionary<T, DiDotNode<T>>();
        T currentObject = default(T);

        public DiDotGraph()
        {
        }

        public void addStartingNode(ref T startingObject)
        {
            currentObject = startingObject;
            DiDotNode<T> newNode = new DiDotNode<T>(ref startingObject);
            objToNode.Add(startingObject, newNode);
        }

        public void addObject(ref T nextObject)
        {
            DiDotNode<T> existingNode;
            bool nextObjExist = objToNode.TryGetValue(nextObject, out existingNode);

            DiDotNode<T> currentNode;
            objToNode.TryGetValue(this.currentObject, out currentNode);

            if (nextObjExist == false)
            {
                // Create a new node and link to current node
                DiDotNode<T> newNode = new DiDotNode<T>(ref nextObject);
                objToNode.Add(nextObject, newNode);
                linkNodes(ref newNode, ref currentNode);
            }
            else
            {
                // Link current node to the existing node
                linkNodes(ref existingNode, ref currentNode);
            }
        }

        public void switchContext(ref T currentObj)
        {
            this.currentObject = currentObj;
        }

        void linkNodes(ref DiDotNode<T> newNode, ref DiDotNode<T> existingNode)
        {
            newNode.addNode(ref existingNode);
            existingNode.addNode(ref newNode);
        }

    }

    
}