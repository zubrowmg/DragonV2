using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;
using TileManagerClasses;

public class ZoneVeinGenerator : ContainerAccessor
{
    // This class will use vein presets and homing veins to create a zone vein
    //      To keep track of connections a DiDotGraph (DiGraph) will be used to analyze if a connection is ok to be placed
    //          Ex a node should only have 4 connections max. Circular flow and branching out paths exist
    //      This class should make sure that the vein placement is spacially placed correctly. No over crossing veins
    //      Should place vein Connect points at the start and end, and a few others on the ends
    //      At the end this class should export the end product as a vein class

    TwoDList<Tile> allocatedTileMap; // Entire allocated dimensions
    TwoDList<Tile> tileMapConnections; // Allocated dims, but only the tiles spaced out every x amount
    Zone_New currentZone;

    // Zone Connection Node Generation
    int gapBetweenNodes = 5;

    public ZoneVeinGenerator(ref GeneratorContainer contInst) : base(ref contInst)
    {
    }

 
    public void generateZoneVein(ref Zone_New zone)
    {
        this.currentZone = zone;
        this.allocatedTileMap = currentZone.getTileMapRef();

        //setupZoneConnectionNodes();
    }

    // Don't want the Zone to generate one Tile at a time, need to setup nodes that need to be the only destination points
    public void setupZoneConnectionNodes()
    {
        CoordsInt newCoords = new CoordsInt(0, 0);

        for (int x = 0; x < allocatedTileMap.getXCount(); x = x + gapBetweenNodes)
        {
            for (int y = 0; y < allocatedTileMap.getYCount(); y = y + gapBetweenNodes)
            {
                Tile tileRef = allocatedTileMap.getElement(new CoordsInt(x, y));

                tileMapConnections.addRefElement(newCoords, ref tileRef);
                newCoords.incY();
            }
            newCoords.incX();
        }
    }

    // =====================================================================================
    //                                     Setters/Getters
    // =====================================================================================
}
