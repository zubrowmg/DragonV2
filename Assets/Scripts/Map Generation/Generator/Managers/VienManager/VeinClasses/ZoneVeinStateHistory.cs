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
        QueueWrapper<List<ZoneVeinState>> historyQueue = new QueueWrapper<List<ZoneVeinState>>(15);
        List<CoordsInt> discardedWorldCoords = new List<CoordsInt>();


        // Roll back conditions
        int rollbackAmount = 1; 

        public ZoneVeinStateHistory()
        { }

        

        public void addState(ZoneVeinState newState)
        {
            bool firstAdd = false;

            if (currentStraightLine.Count == 0)
            {
                currentStraightLine.Add(newState);
                firstAdd = true;
            }

            ZoneVeinState currentState = getCurrentState();

            this.currentStraightLine[this.currentStraightLine.Count - 1].setNextDirection(newState.getCurrentDir());


            // If the direction has changed then add the current straight line list to the history queue and create a new staight line list
            if (currentState.getCurrentDir() != newState.getCurrentDir())
            {
                List<ZoneVeinState> discardedStates = historyQueue.enqueueGetRemovedItem(this.currentStraightLine, out bool itemOverflow);

                Debug.Log("TURN SAVED");


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

            if (firstAdd == false)
            {
                Debug.Log("STRAIGHT LINE SAVED");
                this.currentStraightLine.Add(newState);

            }

            newState.getCurrentCoords().print("STATE SAVED: ");
            newState.printRejectedDir("STATE SAVED REJECTED DIR: ");
        }

        public ZoneVeinState rollBackState(out bool rollBackedTooFar)
        {
            // It's easier to manipulate the raw history queue
            //List<List<ZoneVeinState>> rawHistoryQueue = historyQueue.getQueueList();
            rollBackedTooFar = false;
            
            // Roll back the roll back amount as long as it doesn't rollback the last trun
            if (currentStraightLine.Count > rollbackAmount)
            {
                for (int i = 0; i < rollbackAmount; i++)
                {
                    Debug.Log("REMOVE 1");
                    Debug.Log("COUNT: " + currentStraightLine.Count);
                    foreach (var state in currentStraightLine)
                    {
                        state.getCurrentCoords().print("STRAIGHT LINE COORDS: ");
                    }
                    currentStraightLine.RemoveAt(currentStraightLine.Count - 1);
                }
            }
            // This means that the current state is the turn, need to get the previous straight line list
            else if (currentStraightLine.Count == 1)
            {
                Debug.Log("REMOVE 2");

                List<ZoneVeinState> potentialRollBackList = historyQueue.dequeLastAdded(out bool queueEmpty);

                if (queueEmpty == true)
                    rollBackedTooFar = true;
                else
                    currentStraightLine = potentialRollBackList;

            }
            else
                Debug.LogError("ZoneVeinStateHistory - rollBackState(): IDK how you got here");

            return getCurrentState();
        }



        // =================================================================================
        //                                   Setters/Getters
        // =================================================================================
        ZoneVeinState getCurrentState()
        {
            return this.currentStraightLine[this.currentStraightLine.Count - 1];
        }

        public ZoneVeinState getStateBeforeCurrentState()
        {
            int currentLineCount = this.currentStraightLine.Count;
            ZoneVeinState prevState = new ZoneVeinState();

            if (currentLineCount >= 2)
                prevState = this.currentStraightLine[this.currentStraightLine.Count - 2];
            else if (currentLineCount == 1)
            {
                if (this.historyQueue.getCount() == 0)
                    Debug.LogError("ZoneVeinStateHistory Class - getStateBeforeCurrentState()___1: Trying to get the previous state, but there is none. " +
                    "\n\tEither we rolled back too far, rolling back at the start of generation (AKA there is no history), or history class is not properly loading currentStraightLine");

                List<ZoneVeinState> lastAddedHistoryQueueList = this.historyQueue.getElement(this.historyQueue.getCount() - 1);
                prevState = lastAddedHistoryQueueList[lastAddedHistoryQueueList.Count - 1];
            }
            else if (currentLineCount == 0)
                Debug.LogError("ZoneVeinStateHistory Class - getStateBeforeCurrentState()___2: Trying to get the previous state, but there is none. " +
                    "\n\tEither we rolled back too far, rolling back at the start of generation (AKA there is no history), or history class is not properly loading currentStraightLine");

            return prevState;
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

        public int getLength()
        {
            int discardedCount = this.discardedWorldCoords.Count;
            int historyQueueCount = this.historyQueue.getCount();
            int currentLineCount = this.currentStraightLine.Count;

            return discardedCount + historyQueueCount + currentLineCount;
        }
    }
}
