using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using VeinEnums;
using CommonlyUsedClasses;
using CommonlyUsedDefinesAndEnums;
using AbilityAndThemeEnums;

// ==========================================================
//              Vien Manager Accessors
// ==========================================================
public partial class VeinAccessor
{
}

// ==========================================================
//                         Class
// ==========================================================
// Vein Placement:
//      1. Veins should not be placed willy nilly. Only the first starting Veins from the sendoff room are allowed to
//      2. Veins when generated should place VeinNodes, which is where new Veins should be generated from
//          - Includes preset Veins
//  Vein Node Placement:
//      1. For regular Veins every X units should place a Vein Node. If a vein shorter than X, then you probably shouldn't place a Vein Node
//      2. Preset Veins should have Vein Nodes predetermined
//  Preset (Zone) Vein Placement:
//      1. For Zone Veins, there should be a library of Vein Pieces. With Vein Nodes predetermined on the pieces
//      2. !!!!! Determining how the pieces should connect should be done via topographical algorithm
//          - Not every path needs to connect every where (aka a circle), try to do a tree pattern where the "branches" circle
//          - If the algorithm feels that a connection needs to be made, feel free to use a normal vein
public class VeinManager : ContainerAccessor
{
    List<Vein> veinList = new List<Vein>();

    // Start vein coords
    Coords<int> leftVeinStart;
    Coords<int> rightVeinStart;
    Coords<int> middleVeinStart;

    // Defines
    int xAxis;
    int xAxisOneThird;
    int xAxisTwoThird;
    int xAxisOneFourth;
    int xAxisTwoFourth;
    int xAxisThreeFourth;

    int yAxis;
    int yAxisOneThird;
    int yAxisTwoThird;
    int yAxisOneFourth;
    int yAxisTwoFourth;
    int yAxisThreeFourth;

    // Zone Vein Generation
    ZoneVeinGenerator zoneVeinGenerator;

    public VeinManager(ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.leftVeinStart = new Coords<int>(tileAccessor.getTileMapCenter().getX() - 25 + 2, tileAccessor.getTileMapCenter().getY() - 13);
        this.middleVeinStart = new Coords<int>(tileAccessor.getTileMapCenter().getX(), tileAccessor.getTileMapCenter().getY() - 19);
        this.rightVeinStart = new Coords<int>(tileAccessor.getTileMapCenter().getX() + 24 - 2, tileAccessor.getTileMapCenter().getY() - 13);

        this.xAxis = tileAccessor.getTileMapDims().getMaxX();
        this.xAxisOneThird = tileAccessor.getTileMapDims().getMaxX() / 3;
        this.xAxisTwoThird = 2 * tileAccessor.getTileMapDims().getMaxX() / 3;
        this.xAxisOneFourth = tileAccessor.getTileMapDims().getMaxX() / 4;
        this.xAxisTwoFourth = 2 * tileAccessor.getTileMapDims().getMaxX() / 4;
        this.xAxisThreeFourth = 3 * tileAccessor.getTileMapDims().getMaxX() / 4;

        this.yAxis = tileAccessor.getTileMapDims().getMaxY();
        this.yAxisOneThird = tileAccessor.getTileMapDims().getMaxY() / 3;
        this.yAxisTwoThird = 2 * tileAccessor.getTileMapDims().getMaxY() / 3;
        this.yAxisOneFourth = tileAccessor.getTileMapDims().getMaxY() / 4;
        this.yAxisTwoFourth = 2 * tileAccessor.getTileMapDims().getMaxY() / 4;
        this.yAxisThreeFourth = 3 * tileAccessor.getTileMapDims().getMaxY() / 4;

        this.zoneVeinGenerator = new ZoneVeinGenerator(ref contInst);
    }

    public void startVeinGeneration()
    {
        createSendOffRoomVeins();

        //installPresetVeins(ref bossRoomLocations);
        // checkAndInstallHybirdZones(ref bossRoomLocations);

        createUniqueAreaVeins();

        //installBedRock();
    }

    void createUniqueAreaVeins()
    {
        // Percent chance to create niche areas:
        //      - Hole vein, has construction elements. Beams to hold up cave
        //      - ?Serpenting vein (has bandit enemies (snakey bandit), more loot)
        //      Meant to be a small mini area, with more focus on connecting the map and fleshing out the world
        //      Can add mini bosses
    }

    void getSimpleTestVeins(ref Queue<Vein> veinQueue, List<int> select)
    {
        // Dims 588, 288
        int approxWidthDistance, width = 6;
        int approxVeinDistance, distance = 120;

        // Up/ Down
        Coords<int> end = new Coords<int>(leftVeinStart.getX(), leftVeinStart.getY() + 50);
        if (select[0] == 1)
        { 
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(rightVeinStart.getX(), rightVeinStart.getY() - 50);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, rightVeinStart, end, true, true, true, width, distance));
        }
        // Left Down
        if (select[1] == 1)
        {
            end = new Coords<int>(180, 0);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(0, 0);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(0, 70);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
        }

        // Left Up
        if (select[2] == 1)
        {
            end = new Coords<int>(0, 220);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(0, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(180, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, true, width, distance));
        }

        // Right Down
        if (select[3] == 1)
        {
            end = new Coords<int>(480, 0);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(588, 0);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(588, 70);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
        }

        // Right Up
        if (select[4] == 1)
        {
            end = new Coords<int>(588, 220);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(588, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(420, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
        }
    }

    void getUTestVeins(ref Queue<Vein> veinQueue, List<int> select)
    {
        // Dims 588, 288
        int approxWidthDistance, width = 6;
        int approxVeinDistance, distance = 150;

        // Up/ Down
        Coords<int> end = new Coords<int>(leftVeinStart.getX(), leftVeinStart.getY() + 50);
        if (select[0] == 1)
        {
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
            end = new Coords<int>(rightVeinStart.getX(), rightVeinStart.getY() - 50);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, rightVeinStart, end, true, true, width, distance));
        }

        // Left Down
        if (select[1] == 1)
        {
            end = new Coords<int>(180, 0);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
            end = new Coords<int>(0, 0);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
            end = new Coords<int>(0, 70);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
        }

        // Left Up
        if (select[2] == 1)
        {
            end = new Coords<int>(0, 220);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
            end = new Coords<int>(0, 300);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
            end = new Coords<int>(180, 300);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.West, leftVeinStart, end, true, true, width, distance));
        }

        // Right Down
        if (select[3] == 1)
        {
            end = new Coords<int>(480, 0);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, width, distance));
            end = new Coords<int>(588, 0);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, width, distance));
            end = new Coords<int>(588, 70);
            veinQueue.Enqueue(new UVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, width, distance));
        }

        // Right Up
        if (select[4] == 1)
        {
            end = new Coords<int>(588, 220);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(588, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
            end = new Coords<int>(420, 300);
            veinQueue.Enqueue(new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, end, true, true, true, width, distance));
        }
    }

    void createSendOffRoomVeins()
    {
        // Create Vein with basic settings
        Queue<Vein> veinQueue = new Queue<Vein>();
        veinQueue.Enqueue(configSendOffVeinProps(Direction.West)); // Left vein
        veinQueue.Enqueue(configSendOffVeinProps(Direction.South)); // Middle vein
        veinQueue.Enqueue(configSendOffVeinProps(Direction.East)); // Right vein

        getSimpleTestVeins(ref veinQueue, new List<int> { 0, 0, 0, 0, 0});
        getUTestVeins(ref veinQueue, new List<int> { 0, 0, 0, 0, 0 });

        foreach (var vein in veinQueue)
        {
            vein.triggerVeinGeneration(/*out lastVeinConnectPoint */);


            // !!!!!
            // veinBookMarkQueue.add(lastVeinConnectPoint);
            veinList.Add(vein);
        }

        // ALLOCATE THESE VEINS WITH IDS

        // You will want to create a DiDotGraph so that you can connect zone veins in a controlled manner
        //      Maybe start connecting veins once all zone veins are created????


        /*
        // Go through each starting zone start connect point:
        //      1. Figure out the allocated dimensions, should be skewed down and away from the center
        //      2. Grab the zones theme and ability, since zone generation will rely on these parameters
        //          Zone_New newZone = createNewZone(GameTiming.Early);
        //      3. zoneVeinGenerator.generateZoneVein(newZone, startCoords, Dims);

        */
    }

    Vein configSendOffVeinProps(Direction dir)
    {
        int approxWidthDistance = 6;
        int approxVeinDistance = 30;
        Vein initVein = null;

        Coords<int> endDesination;
        int xChange = 0;
        int yChange = 0;

        bool varyVeinLength = true;
        bool varyVeinWidth = true;
        bool varyVeinSlope = true;

        VeinType type = VeinType.Simple;

        switch (dir)
        {
            case Direction.West:
                xChange = 0;
                yChange = RandomProbability.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new RandomProbability.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new RandomProbability.RandomSelection(yAxisTwoThird + 1, yAxis - 1, .0f));
                endDesination = new Coords<int>(xChange, yChange);

                initVein = new SimpleVein(ref getContainerInst(), Direction.West, leftVeinStart, endDesination, varyVeinWidth, varyVeinLength, 
                                                        varyVeinSlope, approxWidthDistance, approxVeinDistance);
                break;
            case Direction.South:
                xChange = RandomProbability.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(xAxisOneFourth, xAxisOneThird, 0f),
                            new RandomProbability.RandomSelection(xAxisOneThird + 1, xAxisTwoThird, 1f),
                            new RandomProbability.RandomSelection(xAxisTwoThird + 1, xAxisThreeFourth, .0f));
                yChange = 0;
                endDesination = new Coords<int>(xChange, yChange);

                initVein = new SimpleVein(ref getContainerInst(), Direction.South, middleVeinStart, endDesination, varyVeinWidth, varyVeinLength, 
                                                        varyVeinSlope, approxWidthDistance, approxVeinDistance);
                break;
            case Direction.East:
                xChange = tileAccessor.getTileMapDims().getMaxX() - 1;
                yChange = RandomProbability.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new RandomProbability.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new RandomProbability.RandomSelection(yAxisTwoThird + 1, yAxis - 1, .0f));
                endDesination = new Coords<int>(xChange, yChange);

                initVein = new SimpleVein(ref getContainerInst(), Direction.East, rightVeinStart, endDesination, varyVeinWidth, varyVeinLength, 
                                                        varyVeinSlope, approxWidthDistance, approxVeinDistance);
                break;
        }

        return initVein;
    }

    public ref List<Vein> getVeinList()
    {
        return ref veinList;
    }
}

