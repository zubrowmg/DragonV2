using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotGraphNavigation<T>
    {
        // This class is only meant to hold recursive funtions that DiDotGraph can use to traverse

        public DiDotGraphNavigation()
        {
        }


        // ======================================================================
        //                  Recursive DiDot Edge Template Functions
        // ======================================================================

        // Will get a list of nodes aka an edge, starting from a specified node
        protected void getCurrentEndNodeConnections(DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode, out List<DiDotEdge<T>> edgeConnections, out DiDotNode<T> nextEdgeBaseNode)
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

        protected void getOppositeEndNode(DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode, out DiDotNode<T> oppositeEdgeBaseNode)
        {
            if (currentEdge.getNodeOne().Equals(currentEdgeNode) == true)
                oppositeEdgeBaseNode = currentEdge.getNodeTwo();
            else
                oppositeEdgeBaseNode = currentEdge.getNodeOne();
        }


        void edgeRecursionAgain(ref CommonEdgeVariables<T> commonEdgeVars, ref SpecifcEdgeVariables<T> specificEdgeVars, EdgeRecursionTypes recursionType,
                                ref DiDotEdge<T> nextEdge, ref DiDotNode<T> nextEdgeBaseNode)
        {
            // Need to get the next edge node, should be opposite of the base node
            getOppositeEndNode(nextEdge, nextEdgeBaseNode, out DiDotNode<T> nextEdgeNode);
            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
        }

        void edgeRecursionBase(ref CommonEdgeVariables<T> commonEdgeVars, ref SpecifcEdgeVariables<T> specificEdgeVars, EdgeRecursionTypes recursionType)
        {
            commonEdgeVars.currentPath.Add(commonEdgeVars.currentEdge);
            CommonFunctions.addIfItemDoesntExist(ref commonEdgeVars.doNotTravelList, commonEdgeVars.currentEdge);

            // Determine which edge connections to iterate through, depends on which edge node is sent into the function
            getCurrentEndNodeConnections(commonEdgeVars.currentEdge, commonEdgeVars.currentEdgeNode, 
                                         out List<DiDotEdge<T>> edgeConnections, out DiDotNode<T> nextEdgeBaseNode);

            // Go through each connection in the current edge
            for (int i = 0; i < edgeConnections.Count; i++)
            {
                DiDotEdge<T> nextEdge = edgeConnections[i];
                bool edgeCanBeTraveledTo = !commonEdgeVars.doNotTravelList.Contains(nextEdge);

                // If this edge hasn't been traveled to or is not on the doNotTravelList then proceed
                if (edgeCanBeTraveledTo == true)
                {
                    switch (recursionType)
                    {
                        case (EdgeRecursionTypes.GetCircularEdges):
                        {
                            getCircularEdgesConditions(ref nextEdge, ref nextEdgeBaseNode, ref commonEdgeVars, ref specificEdgeVars, recursionType);
                            break;
                        }
                    }
                }
            }

            // I think C# is retaining currentPath values across the recursion
            //     I have no idea WHY and nothing is useful online 
            //     So once you exit this function delete the current node
            commonEdgeVars.currentPath.Remove(commonEdgeVars.currentEdge);
            commonEdgeVars.doNotTravelList.Remove(commonEdgeVars.currentEdge);
        }

        //==============================
        //  Specific Edge Implementaion
        //==============================
        // name_Start() is meant to be called
        // name_Conditions will be used by edgeRecursionBase()

        protected List<List<DiDotEdge<T>>> getCircularEdgesStart(DiDotEdge<T> startEdge)
        {
            DiDotNode<T> startEdgeNode = startEdge.getNodeOne();
            List<DiDotEdge<T>> doNotTravelList = new List<DiDotEdge<T>>();

            CommonEdgeVariables<T> commonEdgeVars = new CommonEdgeVariables<T>(startEdge, startEdgeNode, ref doNotTravelList);
            GetCircularEdgeVariables<T> getCircularEdgeVars = new GetCircularEdgeVariables<T>(startEdge, startEdgeNode);
            SpecifcEdgeVariables<T> specificEdgeVars = new SpecifcEdgeVariables<T>(ref getCircularEdgeVars);
            EdgeRecursionTypes recursionType = EdgeRecursionTypes.GetCircularEdges;

            // If any node is a dead end, then it's impossible for it to be a circular edge
            if (startEdge.nodeOneIsDeadEnd() == true || startEdge.nodeTwoIsDeadEnd() == true)
                return specificEdgeVars.getCircularEdgeVars.listOfCircularEdges;


            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
            return specificEdgeVars.getCircularEdgeVars.listOfCircularEdges;
        }

        void getCircularEdgesConditions(ref DiDotEdge<T> nextEdge, ref DiDotNode<T> nextEdgeBaseNode, ref CommonEdgeVariables<T> commonEdgeVars, 
                                        ref SpecifcEdgeVariables<T> specificEdgeVars, EdgeRecursionTypes recursionType)
        {
            // Figure out which node in the current edge connects to the next edge
            bool nodeOneConnects = commonEdgeVars.currentEdge.nodeOneConnectsToGivenEdge(nextEdge);
            bool deadEnd = false;
            if (nodeOneConnects == true)
                deadEnd = nextEdge.nodeTwoIsDeadEnd();
            else
                deadEnd = nextEdge.nodeOneIsDeadEnd();

            // If we make it back to the starting edge it can mean two things
            if (nextEdge.Equals(specificEdgeVars.getCircularEdgeVars.startEdge) == true)
            {
                // If we hit the starting node of the starting edge, then we have a clean circular edge
                //      Else we hit the other end of the starting edge, meaning that the graph probably has another circular edge
                //      Ex. x is the starting node
                //            x---o---o
                //                |   |
                //                o---o
                DiDotNode<T> connectingNode = commonEdgeVars.currentEdge.getNodeThatConnectsToGivenEdge(nextEdge);

                // Record the clean edge
                if (connectingNode.Equals(specificEdgeVars.getCircularEdgeVars.startEdgeNode) == true)
                {
                    // Need to create a new edge list for memory reasons
                    List<DiDotEdge<T>> tempPath = new List<DiDotEdge<T>>(commonEdgeVars.currentPath);
                    tempPath.Add(nextEdge);

                    // Connect to previous edge, if prev edge is null then that means this is the first edge found
                    specificEdgeVars.getCircularEdgeVars.listOfCircularEdges.Add(tempPath);
                }
            }
            // If we hit a dead end stop here
            else if (deadEnd == true)
            {

            }
            // Else continue searching
            else
                edgeRecursionAgain(ref commonEdgeVars, ref specificEdgeVars, recursionType, ref nextEdge, ref nextEdgeBaseNode);
        }

        void testEdgesStart(DiDotEdge<T> startEdge)
        {
            DiDotNode<T> startEdgeNode = startEdge.getNodeOne();
            List<DiDotEdge<T>> doNotTravelList = new List<DiDotEdge<T>>();

            CommonEdgeVariables<T> commonEdgeVars = new CommonEdgeVariables<T>(startEdge, startEdgeNode, ref doNotTravelList);
            T blah = default(T);
            TestEdgeVariables<T> testEdgeVars = new TestEdgeVariables<T>(blah);
            SpecifcEdgeVariables<T> specificEdgeVars = new SpecifcEdgeVariables<T>(ref testEdgeVars);

            EdgeRecursionTypes recursionType = EdgeRecursionTypes.GetCircularEdges;

            
            edgeRecursionBase(ref commonEdgeVars, ref specificEdgeVars, recursionType);
        }
    }
}
