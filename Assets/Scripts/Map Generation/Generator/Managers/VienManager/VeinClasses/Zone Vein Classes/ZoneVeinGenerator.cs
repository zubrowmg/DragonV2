using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DiDotGraphClasses;
using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedFunctions;
using CommonlyUsedDefinesAndEnums;
using VeinManagerClasses;

public class ZoneVeinGenerator : ContainerAccessor
{
    // This class will use vein presets and homing veins to create a zone vein
    //      To keep track of connections a DiDotGraph (DiGraph) will be used to analyze if a connection is ok to be placed
    //          Ex a node should only have 4 connections max. Circular flow and branching out paths exist
    //      This class should make sure that the vein placement is spacially placed correctly. No over crossing veins
    //      Should place vein Connect points at the start and end, and a few others on the ends

    // This class also handles basic zone vein connection setup

    // !!!!!!!!!!!!!!!!!!!!!!!!
    //      At the end this class should export the end product as a vein class
    // !!!!!!!!!!!!!!!!!!!!!!!!

    ZoneVeinGeneratorContainer zoneVeinGenContainer = new ZoneVeinGeneratorContainer();
    ZoneVeinNavigationController zoneVeinNavigationController;
    ZoneVeinDiGraphContoller zoneVeinDiGraphController;

    // Zone Connection Node Generation
    int gapBetweenNodes = 9;

    public ZoneVeinGenerator(ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.zoneVeinNavigationController = new ZoneVeinNavigationController(ref this.zoneVeinGenContainer, ref contInst);
        this.zoneVeinDiGraphController = new ZoneVeinDiGraphContoller(ref this.zoneVeinGenContainer, ref contInst);
    }
    // =====================================================================================
    //                                     Main Function
    // =====================================================================================

    public VeinZone generateZoneVein(ref Zone_New zone, int veinId)
    {
        init(veinId, ref zone);

        // Creates a grid of vein connection nodes
        //      Also sets current coords to the start coords
        CoordsInt startCoords = setupZoneConnectionNodes();

        createZoneVein(startCoords);

        //exportVeinZoneValues();

        return zoneVeinGenContainer.currentVeinZone;
    }

    // =====================================================================================
    //                                   Setup Functions
    // =====================================================================================

    void init(int veinId, ref Zone_New zone)
    {
        zoneVeinGenContainer.currentVeinZone = new VeinZone(ref getContainerInst(), veinId, zone.getStartCoords());
        zoneVeinGenContainer.currentZone = zone;
        zoneVeinGenContainer.tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>();

        zoneVeinGenContainer.currentVeinPass = 0;

        zoneVeinNavigationController.init();
        zoneVeinDiGraphController.init();
    }

    // Don't want the Zone to generate one Tile at a time, need to setup Tile nodes that need to be the only destination points
    public CoordsInt setupZoneConnectionNodes()
    {
        TwoDList<Tile> allocatedTileMap = zoneVeinGenContainer.currentZone.getTileMapRef(); // Entire allocated dimensions
        DimensionList allocatedDimList = zoneVeinGenContainer.currentZone.getVeinZoneDimList(); // Entire allocated dimensions list (0s and 1s)

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
                else
                {
                    //bool travelTo = false;
                    TileTraveledToMarker travelToMarker = new TileTraveledToMarker();

                    // Mark non vein points as not allowed to be traveled to
                    if (allocatedDimList.getGridVal(currentCoords) == 0)
                        travelToMarker.permaLock();

                    Tile tileRef = allocatedTileMap.getElement(currentCoords);
                    Double<TileTraveledToMarker, Tile> newElement = new Double<TileTraveledToMarker, Tile>(travelToMarker, tileRef);
                    zoneVeinGenContainer.tileMapConnections.addRefElement(newCoords, ref newElement);

                    // Get the point that is the closest to the zone start coords
                    CoordsInt adjustedCoords = allocatedDimList.getMinCoords().deepCopyInt();
                    adjustedCoords.incX(x);
                    adjustedCoords.incY(y);

                    float distance = CommonFunctions.calculateCoordsDistance(zoneVeinGenContainer.currentZone.getStartCoords(), adjustedCoords);
                    minDistance.addValueToQueue(distance, newCoords.deepCopyInt());

                    newCoords.incY();
                }
            }
            newCoords.incX();
            newCoords.setY(0);
        }

        zoneVeinGenContainer.currentZone.setVeinZoneConnectionList(ref zoneVeinGenContainer.tileMapConnections);

        // Set the current coords so that we start generating at the proper start
        startCoords = minDistance.getMinVal().Value;
        //this.currentState.setCurrentCoords(startCoords.deepCopyInt());

        return startCoords.deepCopyInt();
    }



    // =====================================================================================
    //                              Create Zone Vein Functions
    // =====================================================================================

    public void createZoneVein(CoordsInt startCoords)
    {
        // Create the main "trunk" of the zone vein
        List<CoordsInt> listOfZoneVeinCoords = this.zoneVeinNavigationController.createZoneVeinTrunk(startCoords);
        this.zoneVeinDiGraphController.addNodes(listOfZoneVeinCoords);

        // Have the Di Graph Controller decide where the next connection point should be
        this.zoneVeinDiGraphController.decideEndPoints();

        // createBranches();
    }

    // =====================================================================================
    //                                     Setters/Getters
    // =====================================================================================
}
