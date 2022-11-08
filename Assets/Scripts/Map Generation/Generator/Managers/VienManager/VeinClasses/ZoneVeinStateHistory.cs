using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace VeinManagerClasses
{
    public class ZoneVeinStateHistory
    {
        // First state is the first added state after a turn
        List<ZoneVeinState> currentStraightLine = new List<ZoneVeinState>();
        QueueWrapper<List<ZoneVeinState>> historyQueue = new QueueWrapper<List<ZoneVeinState>>(3);

        // Roll back conditions
        //      We either rollback 2 states or if the last turn if it's 1 state ago
        int rollbackAmount = 2; // Go back 2 states

        public ZoneVeinStateHistory()
        { }

        public void addState(ZoneVeinState newState)
        {
            if (currentStraightLine.Count == 0)
                currentStraightLine.Add(newState);

            ZoneVeinState currentState = getCurrentState();

            this.currentStraightLine[this.currentStraightLine.Count - 1].setNextDirection(newState.getCurrentDir());


            // If the direction has changed then add the current straight line list to the history queue and create a new staight line list
            if (currentState.getCurrentDir() != newState.getCurrentDir())
            {
                historyQueue.enqueue(this.currentStraightLine);
                this.currentStraightLine = new List<ZoneVeinState>();
            }

            this.currentStraightLine.Add(newState);
        }

        public ZoneVeinState rollBackState()
        {
            // It's easier to manipulate the raw history queue
            //List<List<ZoneVeinState>> rawHistoryQueue = historyQueue.getQueueList();

            
            // Roll back the roll back amount as long as it doesn't rollback the last trun
            if (currentStraightLine.Count > rollbackAmount)
            {
                for (int i = 0; i < rollbackAmount; i++)
                {
                    currentStraightLine.RemoveAt(currentStraightLine.Count - 1);
                }
            }
            else
            {
                currentStraightLine = historyQueue.dequeLastAdded();
            }

            return getCurrentState();
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
