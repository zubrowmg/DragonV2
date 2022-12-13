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

        public GetCircularEdgeVariables<T> getCircularEdgeVars;
        public TestEdgeVariables<T> testEdgeVars;

        public SpecifcEdgeVariables(ref GetCircularEdgeVariables<T> getCircularEdgeVars)
        {
            this.getCircularEdgeVars = getCircularEdgeVars;
        }

        public SpecifcEdgeVariables(ref TestEdgeVariables<T> testEdgeVars)
        {
            this.testEdgeVars = testEdgeVars;
        }
    }

    public class TestEdgeVariables<T>
    {
        public TestEdgeVariables(T blah)
        {
        }
    }

    public class GetCircularEdgeVariables<T>
    {
        public DiDotEdge<T> startEdge;
        public DiDotNode<T> startEdgeNode;
        public List<List<DiDotEdge<T>>> listOfCircularEdges;

        public GetCircularEdgeVariables(DiDotEdge<T> startEdge, DiDotNode<T> startEdgeNode)
        {
            this.startEdge = startEdge;
            this.startEdgeNode = startEdgeNode;
            this.listOfCircularEdges = new List<List<DiDotEdge<T>>>();
        }
    }


}
