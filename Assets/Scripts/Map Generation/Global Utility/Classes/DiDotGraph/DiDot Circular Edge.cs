using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiDotGraphClasses
{
    public class DiDotCircularEdge<T>
    {
        List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();

        int id = -1;

        public DiDotCircularEdge(List<DiDotEdge<T>> listOfEdges, int id)
        {
            this.listOfEdges = listOfEdges;
            this.id = id;
        }

        public int getId()
        {
            return this.id;
        }
        public List<DiDotEdge<T>> getEdgeList()
        {
            return this.listOfEdges;
        }

        public bool circularEdgeContains(DiDotEdge<T> edge)
        {
            return listOfEdges.Contains(edge);
        }
    }
}
