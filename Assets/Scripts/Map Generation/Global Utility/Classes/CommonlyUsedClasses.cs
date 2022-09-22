using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonlyUsedClasses
{
    public class Coords<T>
    {
        T x;
        T y;

        public Coords(T x, T y)
        {
            this.x = x;
            this.y = y;
        }

        public T getX()
        {
            return x;
        }

        public T getY()
        {
            return y;
        }
    }

    public class Dimensions
    {
        Coords<int> minCoords;
        Coords<int> maxCoords;

        public Dimensions(Coords<int> minCoords, Coords<int> maxCoords)
        {
            this.minCoords = minCoords;
            this.maxCoords = maxCoords;
        }

        public int getMinX()
        {
            return minCoords.getX();
        }

        public int getMinY()
        {
            return minCoords.getY();
        }

        public int getMaxX()
        {
            return maxCoords.getX();
        }

        public int getMaxY()
        {
            return maxCoords.getY();
        }
    }
}
