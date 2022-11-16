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
        //T currentObject = default(T);

        public DiDotGraph()
        {
        }

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
                DiDotNode<T> newNode = addNode(ref prevObject);
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

        public void analyzeGraph()
        {

        }
    }

    
}