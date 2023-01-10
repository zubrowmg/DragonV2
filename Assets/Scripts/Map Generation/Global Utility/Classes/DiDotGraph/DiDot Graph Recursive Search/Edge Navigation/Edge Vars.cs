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
        public DiDotNode<T> currentEdgeEndNode;
        public List<DiDotEdge<T>> currentPath;
        public List<DiDotEdge<T>> doNotTravelList;

        public DiDotEdge<T> startEdge;
        public DiDotNode<T> startEdgeNode;

        public CommonEdgeVariables(DiDotEdge<T> startEdge, DiDotNode<T> startEdgeNode, DiDotEdge<T> currentEdge, DiDotNode<T> startingEdgeEndNode, ref List<DiDotEdge<T>> doNotTravelList)
        {
            this.currentEdge = currentEdge;
            this.currentEdgeEndNode = startingEdgeEndNode;
            this.currentPath = new List<DiDotEdge<T>>();
            this.doNotTravelList = doNotTravelList;

            this.startEdge = startEdge;
            this.startEdgeNode = startEdgeNode;
        }
    }

   
    public class SpecifcEdgeVariables<T>
    {
        // This class is meant to be used by DiDotGraphNavigation for SPECIFIC Edge recursive travel
        //      Holds variables specific to the search function
        //      Since there can be multiple types of recursive uses you need create a constructor for each specific type

        public GetCircularEdge__Variables<T> getCircularEdgeVars;
        public TestEdge__Variables<T> testEdgeVars;

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
