using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiGraphClasses
{
    public class DiGraphBase<T>
    {
        // This class is meant to be used for navigation purposes, use DiGraphSimple to create a digraph
        //      DiGraphSimple is used for manually controlling node and edge connections DURING creation aka for zone vein creation
        //          The T object order DOES NOT matter inside of the edge
        //      DiGraphAuto is used for mapping an ALREADY existing map aka mapping the already connected rooms
        //          The T object order DOES matter inside of the edge

        protected Dictionary<T, DiNode<T>> objToNode = new Dictionary<T, DiNode<T>>();
        protected Dictionary<T, DiEdge<T>> objToEdge = new Dictionary<T, DiEdge<T>>();

        // Variables for DiGraph creation
        protected T currentObject = default(T);
        protected bool currentObjectIsEdge = false;
        protected bool currentObjectIsNode = false;

        public DiGraphBase()
        {
        }

    }
}
