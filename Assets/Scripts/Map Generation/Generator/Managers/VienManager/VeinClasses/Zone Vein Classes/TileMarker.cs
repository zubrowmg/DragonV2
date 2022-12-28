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
        List<int> passLocked; // 0 is unlocked, any other int means that it needs that number of unlocks to unlock
        bool permaLocked;

        public TileTraveledToMarker()
        {
            this.passLocked = new List<int>();
            this.permaLocked = false;

            initLockTrunkPass();
        }

        public void permaLock()
        {
            this.permaLocked = true;
        }

        // Initilize passLocked[0] (trunk pass) as unlocked
        void initLockTrunkPass()
        {
            this.passLocked.Add(0);
        }
        
        public bool isPermaLocked()
        {
            return this.permaLocked;
        }

        public bool isPassLocked(int pass)
        {
            bool locked = isPermaLocked();

            if (locked == false)
                locked = passIsLockedCheck(pass, true);

            return locked;
        }

        public bool isAnyPassLocked()
        {
            bool locked = isPermaLocked();

            if (locked == false)
            {
                // Go through each available pass
                for (int pass = 0; pass < passLocked.Count; pass++)
                {
                    locked = passIsLockedCheck(pass, false);
                    if (locked == true)
                        break;
                }
            }

            return locked;
        }

        private bool passIsLockedCheck(int pass, bool checkIfPassExists)
        {
            bool locked = false;
            
            if (checkIfPassExists)
                passExistCheck(pass);

            if (passLocked[pass] > 0)
                locked = true;

            return locked;
        }

        public void incLockPass(int pass)
        {
            passExistCheck(pass);
            passLocked[pass]++;
        }

        public void decLockPass(int pass)
        {
            passExistCheck(pass);
            passLocked[pass]--;
        }

        private void passExistCheck(int pass)
        {
            //Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);
            if (pass >= passLocked.Count)
            {
                for (int i = 0; i < pass; i++)
                {
                    if (i < passLocked.Count)
                        continue;
                    else
                        passLocked.Add(0);
                }
            }
            //Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);
        }
    }
}