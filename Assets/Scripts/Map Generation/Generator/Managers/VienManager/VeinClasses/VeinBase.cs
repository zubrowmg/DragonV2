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
        protected CoordsInt startCoords;
        protected CoordsInt prevCoords; // If not used move back into VeinClass
        protected CoordsInt currentCoords; // If not used move back into VeinClass

        // List of Tiles
        protected List<Tile> associatedTiles = new List<Tile>();

        // List of Vein Connections,
        protected List<VeinConnection> listOfVeinConnections = new List<VeinConnection>();


        public VeinBase(ref GeneratorContainer contInst, int id, CoordsInt startCoords) : base(ref contInst)
        {
            this.veinId = id;
            this.startCoords = startCoords;
            this.prevCoords = startCoords.deepCopyInt();
            this.currentCoords = startCoords.deepCopyInt();
        }


        public abstract VeinConnection getFurthestVeinConnectorFromStart();


        protected void addNewVeinConnection(ref Tile associatedTile)
        {
            VeinConnection newConnector = new VeinConnection(ref associatedTile);
            newConnector.addVeinLink(this);
            listOfVeinConnections.Add(newConnector);
        }

        public void addAssociatedTiles(ref List<Tile> tiles)
        {
            this.associatedTiles.AddRange(tiles);
        }

        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================

        public int getId()
        {
            return this.veinId;
        }

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
