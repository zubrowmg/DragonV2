using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiDotGraphClasses
{
    public class CommonEdgeVariables<T>
    {
        // This class is meant to be used by DiDotGraphNavigation for ALL Edge recursive travel
        //      Holds common variables
        public DiDotEdge<T> currentEdge;
        public DiDotNode<T> currentEdgeNode;
        public List<DiDotEdge<T>> currentPath;
        public List<DiDotEdge<T>> doNotTravelList;

        public CommonEdgeVariables(DiDotEdge<T> currentEdge, DiDotNode<T> currentEdgeNode, ref List<DiDotEdge<T>> doNotTravelList)
        {
            this.currentEdge = currentEdge;
            this.currentEdgeNode = currentEdgeNode;
            this.currentPath = new List<DiDotEdge<T>>();
            this.doNotTravelList = doNotTravelList;
        }
    }

   
    public class SpecifcEdgeVariables<T>
    {
        // This class is meant to be used by DiDotGraphNavigation for SPECIFIC Edge recursive travel
        //      Holds variables specific to the search function
        //      Since there can be multiple types of recursive uses you need create a constructor for each specific type

        public GetCircularEdge__Variables<T> getCircularEdgeVars;
        public ShortestDistanceFromNodeToNode__Variables<T> shortestDistanceFromNodeToNodeVars;
        public TestEdge__Variables<T> testEdgeVars;

        public SpecifcEdgeVariables(ref ShortestDistanceFromNodeToNode__Variables<T> shortestDistanceFromNodeToNode)
        {
            this.shortestDistanceFromNodeToNodeVars = shortestDistanceFromNodeToNode;
        }

        public SpecifcEdgeVariables(ref GetCircularEdge__Variables<T> getCircularEdgeVars)
        {
            this.getCircularEdgeVars = getCircularEdgeVars;
        }

        public SpecifcEdgeVariables(ref TestEdge__Variables<T> testEdgeVars)
        {
            this.testEdgeVars = testEdgeVars;
        }


    }

    public class TestEdge__Variables<T>
    {
        public TestEdge__Variables(T blah)
        {
        }
    }

    public class ShortestDistanceFromNodeToNode__Variables<T>
    {
        public DiDotEdge<T> endEdge;
        public DiDotNode<T> stopEdgeNode;
        public int shortestDistance = -1; 

        public ShortestDistanceFromNodeToNode__Variables(DiDotEdge<T> endEdge, DiDotNode<T> stopEdgeNode)
        {
            this.endEdge = endEdge;
            this.stopEdgeNode = stopEdgeNode;
        }
    }

    public class GetCircularEdge__Variables<T>
    {
        public DiDotEdge<T> startEdge;
        public DiDotNode<T> startEdgeNode;
        public List<List<DiDotEdge<T>>> listOfCircularEdges;

        public GetCircularEdge__Variables(DiDotEdge<T> startEdge, DiDotNode<T> startEdgeNode)
        {
            this.startEdge = startEdge;
            this.startEdgeNode = startEdgeNode;
            this.listOfCircularEdges = new List<List<DiDotEdge<T>>>();
        }
    }


}
