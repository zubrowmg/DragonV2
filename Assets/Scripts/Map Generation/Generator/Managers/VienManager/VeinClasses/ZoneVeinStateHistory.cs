using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace VeinManagerClasses
{
    public class ZoneVeinStateHistory
    {
        QueueWrapper<List<ZoneVeinState>> historyQueue = new QueueWrapper<List<ZoneVeinState>>(3);

        // First state is the first added state after a turn
        List<ZoneVeinState> currentStraightLine = new List<ZoneVeinState>(); 

        public ZoneVeinStateHistory()
        { }

        public void addState(ZoneVeinState newState)
        {
            if (currentStraightLine.Count == 0)
                currentStraightLine.Add(newState);

            ZoneVeinState currentState = getCurrentState();

            // If the direction has changed then add the current straight line list to the history queue and create a new staight line list
            if (currentState.getCurrentDir() != newState.getCurrentDir())
            {
                this.currentStraightLine[this.currentStraightLine.Count - 1].setNextStateChangeOfDirection(newState.getCurrentDir());
                historyQueue.enqueue(this.currentStraightLine);
                this.currentStraightLine = new List<ZoneVeinState>();
            }

            this.currentStraightLine.Add(newState);
        }



        // =================================================================================
        //                                   Setters/Getters
        // =================================================================================
        ZoneVeinState getCurrentState()
        {
            return this.currentStraightLine[this.currentStraightLine.Count - 1];
        }

    }
}
