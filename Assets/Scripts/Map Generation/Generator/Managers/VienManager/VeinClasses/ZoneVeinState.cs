using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace VeinManagerClasses
{
    public class ZoneVeinState
    {
        CoordsInt currentCoords = new CoordsInt(0, 0);
        CoordsInt prevCoords = new CoordsInt(0, 0);

        Direction currentDirection = Direction.None;
        Direction prevDirection = Direction.None;

        Direction nextStateChangeOfDirection = Direction.None;

        public ZoneVeinState()
        { }






        // =================================================================================
        //                                   Setters/Getters
        // =================================================================================

        public Direction getNextStateChangeOfDirection()
        {
            return this.nextStateChangeOfDirection;
        }

        public void setNextStateChangeOfDirection(Direction dir)
        {
            this.nextStateChangeOfDirection = dir;
        }

        public Direction getCurrentDir()
        {
            return this.currentDirection;
        }

        public void setCurrentDir(Direction dir)
        {
            this.currentDirection = dir;
        }

        public Direction getPrevDir()
        {
            return this.prevDirection;
        }

        public void setPrevDir(Direction dir)
        {
            this.prevDirection = dir;
        }

        public CoordsInt getCurrentCoords()
        {
            return this.currentCoords;
        }

        public void setCurrentCoords(CoordsInt coords)
        {
            this.currentCoords = coords;
        }

        public CoordsInt getPrevCoords()
        {
            return this.prevCoords;
        }

        public void setPrevCoords(CoordsInt coords)
        {
            this.prevCoords = coords;
        }
    }
}

