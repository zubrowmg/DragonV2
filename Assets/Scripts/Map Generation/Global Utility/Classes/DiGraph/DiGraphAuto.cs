using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DiGraphClasses
{
    public class DiGraphAuto<T> : DiGraphBase<T>
    {


        public DiGraphAuto() : base()
        {

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
            DiEdge<T> existingEdge;
            bool nextObjExistsAsEdge = objToEdge.TryGetValue(nextObject, out existingEdge);

            DiNode<T> existingNode;
            bool nextObjExistsAsNode = objToNode.TryGetValue(nextObject, out existingNode);

            DiEdge<T> currentEdge;
            objToEdge.TryGetValue(this.currentObject, out currentEdge);

            DiNode<T> currentNode;
            objToNode.TryGetValue(this.currentObject, out currentNode);

            if (nextObjExistsAsEdge && nextObjExistsAsNode)
                Debug.LogError("DiGraph - addObject(): Next object exists as a Node and Edge. How?");

            if (currentObjectIsEdge == true)
            {
                // If we currently are in a edge and the next obj does not exist, then just extend the current edge
                if (nextObjExistsAsEdge == false && nextObjExistsAsNode == false)
                {
                    existingEdge.extendEdge(ref nextObject);
                }
                // If the object does exist as a node, then link the two
                else if (nextObjExistsAsEdge == false && nextObjExistsAsNode == true)
                {
                    linkExistingEdgeAndNode(ref currentEdge, ref existingNode);
                    switchContextToNode(ref nextObject);
                }
                // If the object does exist as an edge, then there are 2 options:
                //      1. Extend the edge
                //      2. Split the edge in half, making a new node
                else if (nextObjExistsAsEdge == true && nextObjExistsAsNode == false)
                {
                    //convertEdgeToNode();
                }
            }
            else
            {
                // If we currently are in a node and the next obj does not exist, then just create a new edge
                if (nextObjExistsAsEdge == false && nextObjExistsAsNode == false)
                {
                    DiEdge<T> newEdge = createNewEdge(ref nextObject);
                    linkNewEdgeAndNode(ref newEdge, ref currentNode);
                    switchContextToEdge(ref nextObject);
                }
                // If we currently are in a node and the next obj exists as an edge then:
                //      1. Need to split the edge into either
                //          - 1 Node, 1 edge
                //          - 1 node, 2 edges
                //      2. Merge the current node and the new node
                else if (nextObjExistsAsEdge == true && nextObjExistsAsNode == false)
                {

                }
                // If we currently are in a node and the next obj exists as an node then merge the nodes
                else if (nextObjExistsAsEdge == false && nextObjExistsAsNode == true)
                {
                    mergeNodes(ref currentNode, ref existingNode);
                }
                else
                    Debug.LogError("DiGraph - addObject: Should not be here as the node should exist");
            }
        }

        void mergeNodes(ref DiNode<T> currentNode, ref DiNode<T> existingNode)
        {

        }

        //void split

        DiEdge<T> createNewEdge(ref T obj)
        {
            DiEdge<T> newEdge = new DiEdge<T>(ref obj);
            objToEdge.Add(obj, newEdge);

            return newEdge;
        }

        public void switchContextToEdge(ref T existingObject)
        {
            // Switch the current object to an ALREADY LOGGED object. For edges only
            this.currentObject = existingObject;

            currentObjectIsEdge = true;
            currentObjectIsNode = false;
        }

        public void switchContextToNode(ref T existingObject)
        {
            // Switch the current object to an ALREADY LOGGED object. For edges only
            this.currentObject = existingObject;

            currentObjectIsEdge = false;
            currentObjectIsNode = true;
        }

        //public void extendEdge(ref DiEdge<T> edge, cu)

        void linkExistingEdgeAndNode(ref DiEdge<T> edge, ref DiNode<T> node)
        {
            // Link node to edge
            node.addEdge(ref edge);

            // Link edge to node
            edge.addNodeAsEnd(ref node);
        }


        void linkNewEdgeAndNode(ref DiEdge<T> edge, ref DiNode<T> node)
        {
            // Link node to edge
            node.addEdge(ref edge);

            // Link edge to node
            edge.addNodeAsStart(ref node);
        }

    }




}

