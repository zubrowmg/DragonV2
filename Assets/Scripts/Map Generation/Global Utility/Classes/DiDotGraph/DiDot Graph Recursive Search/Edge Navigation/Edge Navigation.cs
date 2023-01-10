using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class EdgeNavigation<T>
    {
        public EdgeNavigation()
        {
        }

        // This class is only meant to hold recursive funtions that DiDotGraph can use to traverse


        //=============================================
        //          Specific Edge Implementaion
        //=============================================
        // name__Start() is meant to be called in Di Dot Graph
        // name__Conditions() will be used by edgeRecursionBase()
        // Basic flow:
        //      Call name__Start() -> edgeRecursionBase() -> name__Conditions() -> edgeRecursionAgain() -> edgeRecursionBase()
        //          - edgeRecursionBase() will iterate over all possible edges
        //          - edgeRecursionBase() will then select a name__Conditions() function, based on the recursion type
        //          - name__Conditions() needs to include edgeRecursionAgain()
        //          - edgeRecursionAgain() handles the direction aspect of the edge, aka an edge has two end points. Handles which endpoint to go to next
        //
        //
        // name__Start() needs to include:
        //      - name__Variables<T>, is needed by SpecifcEdgeVariables<T>
        //      - SpecifcEdgeVariables<T>, handles all recursive variables for the specific type of recursion
        //      - CommonEdgeVariables<T>, handles all recursive variables any type of recursion needs

        // edgeRecursionBase() needs to have name__Conditions() specified in the case statement


        // ======================================================================
        //                  Recursive DiDot Edge Template Functions
        // ======================================================================

        // Will get a list of nodes aka an edge, starting from a specified node
        private void getCurrentEndNodeConnections(DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode, out List<DiDotEdge<T>> edgeConnections, out DiDotNode<T> nextEdgeBaseNode)
        {
            if (currentEdgeNode.Equals(currentEdge.getNodeOne()) == true)
            {
                edgeConnections = currentEdge.getNodeOneEdgeConnections();
                nextEdgeBaseNode = currentEdge.getNodeOne();
            }
            else
            {
                edgeConnections = currentEdge.getNodeTwoEdgeConnections();
                nextEdgeBaseNode = currentEdge.getNodeTwo();
            }
        }

        private void getOppositeEndNode(DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode, out DiDotNode<T> oppositeEdgeBaseNode)
        {
            if (currentEdge.getNodeOne().Equals(currentEdgeNode) == true)
                oppositeEdgeBaseNode = currentEdge.getNodeTwo();
            else
                oppositeEdgeBaseNode = currentEdge.getNodeOne();
        }


        private void edgeRecursionAgain(ref CommonEdgeVariables<T> commonEdgeVars, ref SpecifcEdgeVariables<T> specificEdgeVars, EdgeRecursionTypes recursionType,
                                ref DiDotEdge<T> nextEdge, ref DiDotNode<T> nextEdgeBaseNode)
        {
            // Need to get the next edge node, should be opposite of the base node
            getOppositeEndNode(nextEdge, nextEdgeBaseNode, out DiDotNode<T> nextEdgeNode);
            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
        }

        // Function that ALL recursion types need to go through
        private void edgeRecursionBase(ref CommonEdgeVariables<T> commonEdgeVars, ref SpecifcEdgeVariables<T> specificEdgeVars, EdgeRecursionTypes recursionType)
        {
            // Add the current edge to the current path
            commonEdgeVars.currentPath.Add(commonEdgeVars.currentEdge);
            CommonFunctions.addIfItemDoesntExist(ref commonEdgeVars.doNotTravelList, commonEdgeVars.currentEdge);

            // Determine which edge connections to iterate through, depends on which edge node is sent into the function
            getCurrentEndNodeConnections(commonEdgeVars.currentEdge, commonEdgeVars.currentEdgeEndNode,
                                         out List<DiDotEdge<T>> edgeConnections, out DiDotNode<T> nextEdgeBaseNode);

            // Go through each connection in the current edge
            for (int i = 0; i < edgeConnections.Count; i++)
            {
                //Debug.Log("DIFFERENT EDGE");
                DiDotEdge<T> nextEdge = edgeConnections[i];
                bool edgeCanBeTraveledTo = !commonEdgeVars.doNotTravelList.Contains(nextEdge);

                // If this edge hasn't been traveled to or is not on the doNotTravelList then proceed
                if (edgeCanBeTraveledTo == true)
                {
                    // Figure out which node in the current edge connects to the next edge
                    bool nodeOneConnects = commonEdgeVars.currentEdge.nodeOneConnectsToGivenEdge(nextEdge);

                    bool nextEdgeIsDeadEnd = false;
                    if (nodeOneConnects == true)
                        nextEdgeIsDeadEnd = nextEdge.nodeTwoIsDeadEnd();
                    else
                        nextEdgeIsDeadEnd = nextEdge.nodeOneIsDeadEnd();

                    switch (recursionType)
                    {
                        case (EdgeRecursionTypes.GetCircularEdges):
                            getCircularEdges__Conditions(ref nextEdge, nextEdgeIsDeadEnd, ref nextEdgeBaseNode, ref commonEdgeVars, ref specificEdgeVars, recursionType);
                            break;
                    }
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            commonEdgeVars.currentPath.Remove(commonEdgeVars.currentEdge);
            commonEdgeVars.doNotTravelList.Remove(commonEdgeVars.currentEdge);
        }


        // ================================
        // getCircularEdges
        public List<List<DiDotEdge<T>>> getCircularEdges__Start(DiDotEdge<T> startEdge)
        {
            DiDotNode<T> startEdgeNode = startEdge.getNodeOne();
            DiDotNode<T> startingEndEdgeNode = startEdge.getNodeOne();
            List<DiDotEdge<T>> doNotTravelList = new List<DiDotEdge<T>>();

            // Setup CircularEdges specific and common type vars
            GetCircularEdge__Variables<T> getCircularEdgeVars = new GetCircularEdge__Variables<T>(startEdge, startEdgeNode);
            SpecifcEdgeVariables<T> specificEdgeVars = new SpecifcEdgeVariables<T>(ref getCircularEdgeVars);
            CommonEdgeVariables<T> commonEdgeVars = new CommonEdgeVariables<T>(startEdge, startEdgeNode, startEdge, startingEndEdgeNode, ref doNotTravelList);
            EdgeRecursionTypes recursionType = EdgeRecursionTypes.GetCircularEdges;

            // If any node is a dead end, then it's impossible for it to be a circular edge
            if (startEdge.nodeOneIsDeadEnd() == true || startEdge.nodeTwoIsDeadEnd() == true)
                return specificEdgeVars.getCircularEdgeVars.listOfCircularEdges;

            // Call edge recursion base
            //      We only need to call this once, since we are looking for circular edges
            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
            List<List<DiDotEdge<T>>> circularEdgesList = specificEdgeVars.getCircularEdgeVars.listOfCircularEdges;

            return circularEdgesList;
        }

        private void getCircularEdges__Conditions(ref DiDotEdge<T> nextEdge, bool nextEdgeIsDeadEnd, ref DiDotNode<T> nextEdgeBaseNode,
                                                  ref CommonEdgeVariables<T> commonEdgeVars, ref SpecifcEdgeVariables<T> specificEdgeVars,
                                                  EdgeRecursionTypes recursionType)
        {
            // If we make it back to the starting edge it can mean two things
            if (nextEdge.Equals(specificEdgeVars.getCircularEdgeVars.startEdge) == true)
            {
                // If we hit the starting node of the starting edge, then we have a clean circular edge
                //      Else we hit the other end of the starting edge, meaning that the graph probably has another circular edge
                //      Ex. x is the starting node
                //            x---o---o
                //                |   |
                //                o---o
                DiDotNode<T> nodeThatConnectsCurrentToNextEdge = commonEdgeVars.currentEdge.getNodeThatConnectsToGivenEdge(nextEdge);

                // Record the clean edge
                if (nodeThatConnectsCurrentToNextEdge.Equals(specificEdgeVars.getCircularEdgeVars.startEdgeNode) == true)
                {
                    // Need to create a new edge list for memory reasons
                    List<DiDotEdge<T>> tempPath = new List<DiDotEdge<T>>(commonEdgeVars.currentPath);
                    tempPath.Add(nextEdge);

                    // Connect to previous edge, if prev edge is null then that means this is the first edge found
                    specificEdgeVars.getCircularEdgeVars.listOfCircularEdges.Add(tempPath);
                }
            }
            // If we hit a dead end stop here
            else if (nextEdgeIsDeadEnd == true)
            {

            }
            // Else continue searching
            else
                edgeRecursionAgain(ref commonEdgeVars, ref specificEdgeVars, recursionType, ref nextEdge, ref nextEdgeBaseNode);
        }

        // ================================
        // testEdgesStart - just a dummy function to help setup recursion

        void testEdgesStart(DiDotEdge<T> startEdge)
        {
            DiDotNode<T> startEdgeNode = startEdge.getNodeOne();
            List<DiDotEdge<T>> doNotTravelList = new List<DiDotEdge<T>>();

            // Setup TEST specific and common type vars
            T blah = default(T);
            TestEdge__Variables<T> testEdgeVars = new TestEdge__Variables<T>(blah);
            SpecifcEdgeVariables<T> specificEdgeVars = new SpecifcEdgeVariables<T>(ref testEdgeVars);
            CommonEdgeVariables<T> commonEdgeVars = new CommonEdgeVariables<T>(null, null, startEdge, startEdgeNode, ref doNotTravelList);

            EdgeRecursionTypes recursionType = EdgeRecursionTypes.GetCircularEdges;


            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
        }
    }
}
