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

        Direction nextDirection = Direction.None;

        int currentMomentum = 0;

        List<Direction> rejectedDirList = new List<Direction>();

        public ZoneVeinState()
        { }






        // =================================================================================
        //                                   Setters/Getters
        // =================================================================================

        public Direction getNextDirection()
        {
            return this.nextDirection;
        }
        public void setNextDirection(Direction dir)
        {
            this.nextDirection = dir;
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

        public int getCurrentMomentum()
        {
            return this.currentMomentum;
        }
        public void setCurrentMomentum(int momentum)
        {
            this.currentMomentum = momentum;
        }
        public void resetMomentum()
        {
            this.currentMomentum = 0;
        }
        public void incCurrentMomentum()
        {
            this.currentMomentum++;
        }

        public void setRejectedDir(List<Direction> rejectedDir)
        {
            this.rejectedDirList.AddRange(rejectedDir);
        }
        public List<Direction> getRejectedDirList()
        {
            return this.rejectedDirList;
        }
    }
}

