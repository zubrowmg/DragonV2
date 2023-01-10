using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedFunctions;

namespace DiDotGraphClasses
{
    public class DiDotGraphNavigationBase<T>
    {
        protected EdgeNavigation<T> edgeNav = new EdgeNavigation<T>();
        protected NodeNavigation<T> nodeNav = new NodeNavigation<T>();

        public DiDotGraphNavigationBase()
        {
        }

    }
}
