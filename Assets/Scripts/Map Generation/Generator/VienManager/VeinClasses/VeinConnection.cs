using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TileManagerClasses;

namespace VeinManagerClasses
{
    public class VeinConnection
    {
        // Class is meant to serve as a vein connection or a "node"

        List<Vein> linkedVeins = new List<Vein>();
        Tile associatedTile;

        public VeinConnection(ref Tile associatedTile)
        {
            this.associatedTile = associatedTile;
        }

        public void addVeinLink(Vein vein)
        {
            this.linkedVeins.Add(vein);
        }

        public ref Tile getAssociatedTile()
        {
            return ref associatedTile;
        }
    }
}