using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiDotGraphClasses
{
    public class DiDotCircularEdge<T>
    {
        List<DiDotEdge<T>> listOfEdges = new List<DiDotEdge<T>>();

        public DiDotCircularEdge(List<DiDotEdge<T>> listOfEdges)
        {
            this.listOfEdges = listOfEdges;
        }


    }
}
