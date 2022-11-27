using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotIntersection<T> : DiDotNode<T>
    {
        public DiDotIntersection(ref T obj) : base(ref obj)
        {
        }

        public DiDotIntersection(DiDotNode<T> exsistingNode) : base(ref exsistingNode.getObject())
        {
        }
    }
}
