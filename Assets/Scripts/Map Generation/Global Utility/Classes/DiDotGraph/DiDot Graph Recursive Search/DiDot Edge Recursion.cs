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
        public ShortestLengthFromNodeToNode__Variables<T> shortestLengthFromNodeToNodeVars;
        public TestEdge__Variables<T> testEdgeVars;

        public SpecifcEdgeVariables(ref ShortestLengthFromNodeToNode__Variables<T> shortestLengthFromNodeToNodeVars)
        {
            this.shortestLengthFromNodeToNodeVars = shortestLengthFromNodeToNodeVars;
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

    public class ShortestLengthFromNodeToNode__Variables<T>
    {
        public DiDotEdge<T> endEdge;
        public DiDotNode<T> stopEdgeNode;
        public int shortestLength = -1;
        
        public int currentDistanceTraveled = 0;
        public int maxNodeLength;

        public ShortestLengthFromNodeToNode__Variables(DiDotEdge<T> endEdge, DiDotNode<T> stopEdgeNode, int maxNodeLength)
        {
            this.endEdge = endEdge;
            this.stopEdgeNode = stopEdgeNode;
            this.maxNodeLength = maxNodeLength;
        }

        public void incTotalDistanceTravled(int incVal)
        {
            currentDistanceTraveled = currentDistanceTraveled + incVal;
        }

        public void decTotalDistanceTravled(int decVal)
        {
            currentDistanceTraveled = currentDistanceTraveled - decVal;
        }

        // Checks if the current distance traveled is shorter than it was before
        public void setShortestLength()
        {
            bool updateShortestDistance = false;

            if (shortestLength == -1)
                updateShortestDistance = true;
            else if (currentDistanceTraveled + 1 < shortestLength)
                updateShortestDistance = true;

            if (updateShortestDistance)
                shortestLength = currentDistanceTraveled + 1; // Current distance is off by 1

        }

        public bool distanceIsMoreThanMinTravelLength()
        {
            return this.currentDistanceTraveled + 1 >= this.maxNodeLength;  // Current distance is off by 1
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
