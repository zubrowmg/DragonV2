using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VeinManagerClasses
{
    public class TileTraveledToMarker
    {
        // Meant for Zone Vein Generation
        //      In Zone Vein Generation there are several "passes" that end up locking tiles from becoming zones
        //      Ex. First pass is the zone vein trunk generation, don't want the trunk to travel back on top of itself or to snake 
        //          Snaking: ---------|          Not Snaking  ---------| 
        //                      ------|                                |
        //                                                       ------|
        //      But when we start adding branches (new passes), we don't want to restrict travel to locked off tiles
        //      That's what this class aims to allieve

        // Perma locked is for connection points that are not travelable under any circumstance
        //      Pass locked is for locking off points during a trunk/branch being generated
        List<bool> passLocked;
        bool permaLocked; 

        public TileTraveledToMarker()
        {
            this.passLocked = new List<bool>();
            this.permaLocked = false;

            lockTrunkPass();
        }

        public void permaLock()
        {
            this.permaLocked = true;
        }

        void lockTrunkPass()
        {
            this.passLocked.Add(false);
        }
        
        public bool isPassLocked(int pass)
        {
            bool locked = false;

            if (this.permaLocked == true)
                locked = true;

            if (locked == false)
            {
                passExistCheck(pass);
                locked = passLocked[pass];
            }
            return locked;
        }

        public void lockPass(int pass)
        {
            passExistCheck(pass);
            passLocked[pass] = true;
        }

        public void unlockPass(int pass)
        {
            passExistCheck(pass);
            passLocked[pass] = false;
        }

        public void passExistCheck(int pass)
        {
            //Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);
            if (pass >= passLocked.Count)
            {
                for (int i = 0; i < pass; i++)
                {
                    if (i < passLocked.Count)
                        continue;
                    else
                        passLocked.Add(false);
                }
            }
            //Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);
        }
    }
}