using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedFunctions;

public class ZoneVeinGenerator : ContainerAccessor
{
    // This class will use vein presets and homing veins to create a zone vein
    //      To keep track of connections a DiDotGraph (DiGraph) will be used to analyze if a connection is ok to be placed
    //          Ex a node should only have 4 connections max. Circular flow and branching out paths exist
    //      This class should make sure that the vein placement is spacially placed correctly. No over crossing veins
    //      Should place vein Connect points at the start and end, and a few others on the ends

    // !!!!!!!!!!!!!!!!!!!!!!!!
    //      At the end this class should export the end product as a vein class
    // !!!!!!!!!!!!!!!!!!!!!!!!

    Zone_New currentZone;

    // Tile Map Connections
    TwoDList<Tile> tileMapConnections = new TwoDList<Tile>(); // Allocated dims, but only the tiles spaced out every x amount
    CoordsInt currentCoords = new CoordsInt(0, 0);

    // Zone Connection Node Generation
    int gapBetweenNodes = 5;

    public ZoneVeinGenerator(ref GeneratorContainer contInst) : base(ref contInst)
    {
    }

 
    public void generateZoneVein(ref Zone_New zone)
    {

        this.currentZone = zone;
        this.currentCoords = new CoordsInt(0, 0);



        // Creates a grid of vein connection nodes
        setupZoneConnectionNodes();
        

        createZoneVein();
    }

    // Don't want the Zone to generate one Tile at a time, need to setup nodes that need to be the only destination points
    public void setupZoneConnectionNodes()
    {
        TwoDList<Tile> allocatedTileMap = this.currentZone.getTileMapRef(); // Entire allocated dimensions
        DimensionList allocatedDimList = this.currentZone.getVeinZoneDimList(); // Entire allocated dimensions list (0s and 1s)

        CoordsInt newCoords = new CoordsInt(0, 0);
        CoordsInt startCoords = new CoordsInt(0, 0);

        MinValue<float, CoordsInt> minDistance = new MinValue<float, CoordsInt>(1);

        for (int x = gapBetweenNodes - 1; x < allocatedTileMap.getXCount(); x = x + gapBetweenNodes)
        {
            for (int y = gapBetweenNodes - 1; y < allocatedTileMap.getYCount(x); y = y + gapBetweenNodes)
            {
                CoordsInt currentCoords = new CoordsInt(x, y);
                if (x > allocatedTileMap.getXCount() - gapBetweenNodes + 1 || y > allocatedTileMap.getYCount(x) - gapBetweenNodes + 1)
                {
                    // Do nothing, don't want to mark the edges as travel points
                }
                else if (allocatedDimList.getGridVal(currentCoords) == 0)
                {
                    // Do nothing, don't want to mark the non vein points as travel points
                }
                else
                {
                    Tile tileRef = allocatedTileMap.getElement(currentCoords);

                    tileMapConnections.addRefElement(newCoords, ref tileRef);

                    // Get the point that is the closest to the zone start coords
                    CoordsInt adjustedCoords = allocatedDimList.getMinCoords().deepCopyInt();
                    adjustedCoords.incX(x);
                    adjustedCoords.incY(y);

                    float distance = CommonFunctions.calculateCoordsDistance(allocatedDimList.getStartCoords(), adjustedCoords);
                    minDistance.addValueToQueue(distance, newCoords.deepCopyInt());

                    newCoords.incY();
                }
            }
            newCoords.incX();
            newCoords.setY(0);
        }

        this.currentZone.setVeinZoneConnectionList(ref this.tileMapConnections);

        // Set the current coords so that we start generating at the proper start
        startCoords = minDistance.getMinVal().Value;
        this.currentCoords = startCoords.deepCopyInt();
    }


    public void createZoneVein()
    {
        bool done = false;

        while (done == false)
        {


            done = true;
        }
    }


    public void goLeft(out bool rejected)
    {
        rejected = false;

        // Left most border check
        if (this.currentCoords.getX() - 1 < 0)
            rejected = true;
        // Different y height check
        //  o    <- o can't go left       
        //  x
        // xx
        // xx
        else if (this.currentCoords.getY() >= this.tileMapConnections.getYCount(this.currentCoords.getX() - 1))
            rejected = true;
        else
        {
            CoordsInt tempCoords = this.currentCoords.deepCopyInt();
            tempCoords.decX();

            CoordsInt attemptedTileMapCoords = this.tileMapConnections.getElement(tempCoords).getTileMapCoords();
            CoordsInt currentTileMapCoords = this.tileMapConnections.getElement(currentCoords).getTileMapCoords();

            // If the attempted to travel to coord is exactly to the left in world coords, then reject
            if (CommonFunctions.calculateCoordsDistance(attemptedTileMapCoords, currentTileMapCoords) != (float)gapBetweenNodes)
            {
                rejected = true;
            }
            else
            {
                this.currentCoords.decX();
            }
        }
    }

    public void goRight(out bool rejected)
    {
        rejected = false;

        // Right most border check
        if (this.currentCoords.getX() + 1 >= this.tileMapConnections.getXCount())
            rejected = true;
        // Different y height check
        // o    <- o can't go right       
        // x
        // xx
        // xx
        else if (this.currentCoords.getY() >= this.tileMapConnections.getYCount(this.currentCoords.getX() + 1))
            rejected = true;
        else
        {
            CoordsInt tempCoords = this.currentCoords.deepCopyInt();
            tempCoords.incX();

            CoordsInt attemptedTileMapCoords = this.tileMapConnections.getElement(tempCoords).getTileMapCoords();
            CoordsInt currentTileMapCoords = this.tileMapConnections.getElement(currentCoords).getTileMapCoords();

            // If the attempted to travel to coord is exactly to the left in world coords, then reject
            if (CommonFunctions.calculateCoordsDistance(attemptedTileMapCoords, currentTileMapCoords) != (float)gapBetweenNodes)
            {
                rejected = true;
            }
            else
            {
                this.currentCoords.incX();
            }
        }
    }


    // =====================================================================================
    //                                     Setters/Getters
    // =====================================================================================
}
