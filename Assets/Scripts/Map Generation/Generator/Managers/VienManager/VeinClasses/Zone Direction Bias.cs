using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;


namespace VeinManagerClasses
{
    public class ZoneDirectionBias
    {
        Direction horizontalDir;
        Direction verticalDir;

        public ZoneDirectionBias(Direction horizontalDir, Direction verticalDir)
        {
            this.horizontalDir = horizontalDir;
            this.verticalDir = verticalDir;
        }

        public Direction getHorizontalDir()
        {
            return this.horizontalDir;
        }

        public Direction getVerticalDir()
        {
            return this.verticalDir;
        }
    }
}

