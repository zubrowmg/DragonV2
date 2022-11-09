using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;

namespace VeinManagerClasses
{
    public class ZoneVeinStateHistory
    {
        // Have discardedWorldCoords to save memory, historyQueue has a capped queue length for this reason as well
        //      When the historyQueue cap is hit, the first in ZoneVeinState list is discarded and added to the discardedWorldCoords list
        // State order is as follows:
        //      1. discardedWorldCoords[0] (if you want world coords, else disregard discardedWorldCoords)
        //      2. historyQueue[0][0]
        //      3. currentStraightLine[0]
        List<ZoneVeinState> currentStraightLine = new List<ZoneVeinState>();
        QueueWrapper<List<ZoneVeinState>> historyQueue = new QueueWrapper<List<ZoneVeinState>>(3);
        List<CoordsInt> discardedWorldCoords = new List<CoordsInt>();


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
                List<ZoneVeinState> discardedStates = historyQueue.enqueueGetRemovedItem(this.currentStraightLine, out bool itemOverflow);

                // Record the discarded world coords
                if (itemOverflow == true)
                {
                    foreach (var state in discardedStates)
                    {
                        this.discardedWorldCoords.Add(state.getCurrentWorldCoords());
                    }
                }

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

        public List<CoordsInt> getListOfWorldCoords()
        {
            List<CoordsInt> worldCoords = new List<CoordsInt>();

            // First add the discarded world coords
            foreach (var coords in discardedWorldCoords)
            {
                worldCoords.Add(coords);
            }

            // Second add the history queue states world coords
            for (int i = 0; i < this.historyQueue.getCount(); i++)
            {
                List<ZoneVeinState> currentStateList = this.historyQueue.getElement(i);

                foreach (var state in currentStateList)
                {
                    worldCoords.Add(state.getCurrentWorldCoords());
                    //state.getCurrentWorldCoords().print("   WORLD COORDS: ");
                }
            }

            // Third add the current straight line states world coords
            foreach (var state in this.currentStraightLine)
            {
                worldCoords.Add(state.getCurrentWorldCoords());
                //state.getCurrentWorldCoords().print("   WORLD COORDS: ");
            }

            return worldCoords;
        }
    }
}
