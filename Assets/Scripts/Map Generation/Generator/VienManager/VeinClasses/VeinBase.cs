using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public abstract class VeinBase : TileAccessor
    {
        // Type
        protected VeinType veinType = VeinType.None_Set;
        int veinId = CommonDefines.DefualtId;

        // Coords
        protected Coords<int> startCoords;
        protected Coords<int> prevCoords; // If not used move back into VeinClass
        protected Coords<int> currentCoords; // If not used move back into VeinClass

        // List of Tiles
        protected List<Tile> associatedTiles = new List<Tile>();

        // List of Vein Connections,
        protected List<VeinConnection> listOfVeinConnections = new List<VeinConnection>();


        public VeinBase(ref GeneratorContainer contInst, int id, Coords<int> startCoords) : base(ref contInst)
        {
            this.veinId = id;
            this.startCoords = startCoords;
            this.prevCoords = startCoords.deepCopy();
            this.currentCoords = startCoords.deepCopy();
        }


        public abstract VeinConnection getFurthestVeinConnectorFromStart();


        protected void addNewVeinConnection(ref Tile associatedTile)
        {
            VeinConnection newConnector = new VeinConnection(ref associatedTile);
            newConnector.addVeinLink(this);
            listOfVeinConnections.Add(newConnector);
        }

        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================


        public List<VeinConnection> getVeinConnections()
        {
            return this.listOfVeinConnections;
        }

        public ref List<Tile> getAssociatedTiles()
        {
            return ref associatedTiles;
        }

    }
}
