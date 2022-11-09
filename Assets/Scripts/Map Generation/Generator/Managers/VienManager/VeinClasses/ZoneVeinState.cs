using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace VeinManagerClasses
{
    public class ZoneVeinState
    {
        CoordsInt currentWorldCoords = new CoordsInt(0, 0);

        CoordsInt currentCoords = new CoordsInt(0, 0);
        CoordsInt prevCoords = new CoordsInt(0, 0);

        Direction currentDirection = Direction.None;
        Direction prevDirection = Direction.None;

        Direction nextDirection = Direction.None;

        int currentMomentum = 0;

        List<Direction> rejectedDirList = new List<Direction>();

        public ZoneVeinState()
        { }

        public ZoneVeinState(CoordsInt currentWorldCoords, CoordsInt currentCoords, CoordsInt prevCoords, Direction currentDirection, Direction prevDirection, Direction nextDirection, int currentMomentum, List<Direction> rejectedDirList)
        {
            this.currentWorldCoords = currentWorldCoords;
            this.currentCoords = currentCoords;
            this.prevCoords = prevCoords;
            this.currentDirection = currentDirection;
            this.prevDirection = prevDirection;
            this.nextDirection = nextDirection;
            this.currentMomentum = currentMomentum;
            this.rejectedDirList = rejectedDirList;
        }

        public ZoneVeinState deepCopy()
        {
            List<Direction> rejectedDirListCopy = new List<Direction>();
            rejectedDirList.AddRange(this.rejectedDirList);
            return new ZoneVeinState(this.currentWorldCoords, this.currentCoords, this.prevCoords, this.currentDirection, this.prevDirection, this.nextDirection, this.currentMomentum, rejectedDirListCopy);
        }





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

        public CoordsInt getCurrentWorldCoords()
        {
            return this.currentWorldCoords;
        }
        public void setCurrentWorldCoords(CoordsInt coords)
        {
            this.currentWorldCoords = coords;
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

