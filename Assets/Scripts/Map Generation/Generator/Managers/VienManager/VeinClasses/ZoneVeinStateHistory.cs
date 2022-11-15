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
        QueueWrapper<ZoneVeinState> historyQueue = new QueueWrapper<ZoneVeinState>(50);
        List<CoordsInt> discardedWorldCoords = new List<CoordsInt>();


        // Roll back conditions
        int rollbackAmount = 1; 

        public ZoneVeinStateHistory()
        { }

        

        public void addState(ZoneVeinState newState)
        {
            //if (historyQueue.getCount() == 0)
            //{
                this.historyQueue.enqueue(newState);
            //}
            //else 
            //{
            //    List<ZoneVeinState> rawList = this.historyQueue.getRawQueueList();
                //rawList[rawList.Count - 1].setNextDirection(newState.getCurrentDir());

                //Debug.Log("STRAIGHT LINE SAVED");
                //this.historyQueue.enqueue(newState);
            //}

            newState.getCurrentCoords().print("STATE SAVED: ");
            newState.printRejectedDir("STATE SAVED REJECTED DIR: ");
        }

        public ZoneVeinState rollBackState(out bool rollBackedTooFar)
        {
            // It's easier to manipulate the raw history queue
            //List<List<ZoneVeinState>> rawHistoryQueue = historyQueue.getQueueList();
            rollBackedTooFar = false;
            
            // Roll back the roll back amount as long as it doesn't rollback the last trun
            if (historyQueue.getCount() > rollbackAmount)
            {
                for (int i = 0; i < rollbackAmount; i++)
                {
                    Debug.Log("REMOVE 1");
                    ZoneVeinState test = historyQueue.dequeLastAdded(out bool queueEmpty);
                    test.getCurrentCoords().print("\t\t\tREMOVING COORDS: ");

                    if (queueEmpty == true)
                    {
                        rollBackedTooFar = true;
                        break;
                    }
                }
            }
            else
                rollBackedTooFar = true;

            return getCurrentState();
        }



        // =================================================================================
        //                                   Setters/Getters
        // =================================================================================
        ZoneVeinState getCurrentState()
        {
            return this.historyQueue.getElement(this.historyQueue.getCount() - 1);
            //return this.currentStraightLine[this.currentStraightLine.Count - 1];
        }

        public ZoneVeinState getStateBeforeCurrentState()
        {
            return this.historyQueue.getElement(this.historyQueue.getCount() - 2);
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
                ZoneVeinState state = this.historyQueue.getElement(i);
                worldCoords.Add(state.getCurrentWorldCoords());
            }


            return worldCoords;
        }

        public int getLength()
        {
            int discardedCount = this.discardedWorldCoords.Count;
            int historyQueueCount = this.historyQueue.getCount();

            return discardedCount + historyQueueCount;
        }
    }
}
