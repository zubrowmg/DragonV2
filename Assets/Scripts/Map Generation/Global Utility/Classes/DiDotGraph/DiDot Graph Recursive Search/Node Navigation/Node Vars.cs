using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiDotGraphClasses
{
    public class CommonNodeVariables<T>
    {
        // This class is meant to be used by DiDotGraphNavigation for ALL Node recursive travel
        //      Holds common variables
        public DiDotNode<T> currentNode;
        public DiDotNode<T> startNode;
        public DiDotNode<T> endNode;

        public List<DiDotNode<T>> doNotTravelList;
        public List<DiDotNode<T>> currentPath;

        public CommonNodeVariables(DiDotNode<T> startNode, DiDotNode<T> endNode, ref List<DiDotNode<T>> doNotTravelList)
        {
            init(startNode, endNode);
            this.doNotTravelList = doNotTravelList;
        }

        public CommonNodeVariables(DiDotNode<T> startNode, DiDotNode<T> endNode)
        {
            init(startNode, endNode);
            this.doNotTravelList = new List<DiDotNode<T>>();
        }

        private void init(DiDotNode<T> startNode, DiDotNode<T> endNode)
        {
            this.startNode = startNode;
            this.currentNode = startNode;
            this.endNode = endNode;
            currentPath = new List<DiDotNode<T>>();
        }
    }


    public class SpecifcNodeVariables<T>
    {
        // This class is meant to be used by DiDotGraphNavigation for SPECIFIC Node recursive travel
        //      Holds variables specific to the search function
        //      Since there can be multiple types of recursive uses you need create a constructor for each specific type

        public GetAllEdges__Variables<T> getAllEdgeVars;
        public ShortestLengthFromNodeToNode__Variables<T> shortestLengthFromNodeToNodeVars;
        public TestNode__Variables<T> testNodeVars;

        public SpecifcNodeVariables(ref GetAllEdges__Variables<T> getAllEdgeVars)
        {
            this.getAllEdgeVars = getAllEdgeVars;
        }

        public SpecifcNodeVariables(ref ShortestLengthFromNodeToNode__Variables<T> shortestLengthFromNodeToNodeVars)
        {
            this.shortestLengthFromNodeToNodeVars = shortestLengthFromNodeToNodeVars;
        }


        public SpecifcNodeVariables(ref TestNode__Variables<T> testNodeVars)
        {
            this.testNodeVars = testNodeVars;
        }

    }

    public class GetAllEdges__Variables<T>
    {

    }

    public class ShortestLengthFromNodeToNode__Variables<T>
    {
        public int shortestLength = -1;

        public int currentDistanceTraveled = 0;
        public int maxNodeLength;

        public ShortestLengthFromNodeToNode__Variables(int maxNodeLength)
        {
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

    public class TestNode__Variables<T>
    {
        public TestNode__Variables(T blah)
        {
        }
    }

}
