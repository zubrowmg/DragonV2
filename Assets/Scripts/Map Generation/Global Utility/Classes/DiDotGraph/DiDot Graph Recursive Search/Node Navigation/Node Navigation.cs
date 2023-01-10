using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;


namespace DiDotGraphClasses
{
    public class NodeNavigation<T>
    {
        public NodeNavigation()
        {
        }

        // ======================================================================
        //                  Recursive DiDot Node Template Functions
        // ======================================================================

        private void nodeRecursionAgain(ref CommonNodeVariables<T> commonNodeVars, ref SpecifcNodeVariables<T> specificNodeVars, NodeRecursionTypes recursionType,
                        ref DiDotNode<T> nextNode)
        {
            commonNodeVars.currentNode = nextNode;

            // Need to get the next edge node, should be opposite of the base node
            nodeRecursionBase(ref commonNodeVars, ref specificNodeVars, recursionType);
        }

        // Function that ALL recursion types need to go through
        private void nodeRecursionBase(ref CommonNodeVariables<T> commonNodeVars, ref SpecifcNodeVariables<T> specificNodeVars, NodeRecursionTypes recursionType)
        {
            // Add the current node to the current path
            commonNodeVars.currentPath.Add(commonNodeVars.currentNode);
            CommonFunctions.addIfItemDoesntExist(ref commonNodeVars.doNotTravelList, commonNodeVars.currentNode);

            // Determine which node connections to iterate through, depends on which node node is sent into the function
            List<DiDotNode<T>> nodeConnections = commonNodeVars.currentNode.getRawListOfConnections();

            // Go through each connection in the current node
            for (int i = 0; i < nodeConnections.Count; i++)
            {
                DiDotNode<T> nextNode = nodeConnections[i];
                bool edgeCanBeTraveledTo = !commonNodeVars.doNotTravelList.Contains(nextNode);

                // If this node hasn't been traveled to or is not on the doNotTravelList then proceed
                if (edgeCanBeTraveledTo == true)
                {
                    switch (recursionType)
                    {
                        case (NodeRecursionTypes.ShortestLengthFromNodeToNode):
                            shortestLengthFromNodeToNode__Conditions(ref nextNode, ref commonNodeVars, ref specificNodeVars, recursionType);
                            break;
                    }
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            commonNodeVars.currentPath.Remove(commonNodeVars.currentNode);
            commonNodeVars.doNotTravelList.Remove(commonNodeVars.currentNode);
        }



        // ================================
        // shortestDistanceFromNodeToNode
        //      Will return -1 if not found or if the node is farther than maxNodeTravelDistance
        public int shortestLengthFromNodeToNode__Start(DiDotNode<T> startNode, DiDotNode<T> endNode, int maxNodeLength)
        {
            // Setup Shortest Nodes specific and common type vars
            ShortestLengthFromNodeToNode__Variables<T> shortestLengthVars = new ShortestLengthFromNodeToNode__Variables<T>(maxNodeLength);
            SpecifcNodeVariables<T> specificEdgeVars = new SpecifcNodeVariables<T>(ref shortestLengthVars);
            NodeRecursionTypes recursionType = NodeRecursionTypes.ShortestLengthFromNodeToNode;

            // Setup Common Node vars
            CommonNodeVariables<T> commonNodeVars = new CommonNodeVariables<T>(startNode, endNode);

            // Call node recursion base
            nodeRecursionBase(ref commonNodeVars, ref specificEdgeVars, recursionType);

            int shortestLength = specificEdgeVars.shortestLengthFromNodeToNodeVars.shortestLength;

            return shortestLength;
        }

        private void shortestLengthFromNodeToNode__Conditions(ref DiDotNode<T> nextNode, ref CommonNodeVariables<T> commonNodeVars, ref SpecifcNodeVariables<T> specificNodeVars,
                                                                NodeRecursionTypes recursionType)
        {
            // Setting this to clean up the code
            ShortestLengthFromNodeToNode__Variables<T> specificVars = specificNodeVars.shortestLengthFromNodeToNodeVars;

            // Update the distance traveled so far 
            specificVars.incTotalDistanceTravled(1);

            // If the next node is the destination stop node
            if (nextNode.Equals(commonNodeVars.endNode))
            {
                // Update the distance traveled into the end node
                specificVars.incTotalDistanceTravled(1);


                // If we are within the min travel bounds
                if (specificVars.distanceIsMoreThanMinTravelLength() == false)
                    specificVars.setShortestLength();

                // As we leave we need to decrement what we changed, since recursion is still happening
                specificVars.decTotalDistanceTravled(1);
            }
            // Stop here if the next node:              
            //      Have traveled too far
            else if (specificVars.distanceIsMoreThanMinTravelLength())
            {

            }
            // Else continue searching
            else
                nodeRecursionAgain(ref commonNodeVars, ref specificNodeVars, recursionType, ref nextNode);

            // As we leave we need to decrement what we changed, since recursion is still happening
            specificVars.decTotalDistanceTravled(1);
        }

    }


}
