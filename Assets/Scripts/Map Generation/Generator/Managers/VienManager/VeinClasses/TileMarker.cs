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


        List<bool> passLocked;

        public TileTraveledToMarker()
        {
            this.passLocked = new List<bool>();
            lockTrunkPass();
        }

        void lockTrunkPass()
        {
            this.passLocked.Add(false);
        }
        
        public bool isPassLocked(int pass)
        {
            passExistCheck(pass);
            return passLocked[pass];
        }

        public void lockPass(int pass)
        {
            passExistCheck(pass);
            passLocked[pass] = true;
        }

        public void passExistCheck(int pass)
        {
            Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);
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
            Debug.Log("PASS: " + pass + "  \nCOUNT: " + passLocked.Count);

        }
    }
}