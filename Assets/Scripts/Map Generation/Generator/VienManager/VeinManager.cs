using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using CommonlyUsedClasses;
using CommonlyUsedEnums;

// ==========================================================
//              Vien Manager Accessors
// ==========================================================
public partial class ContainerAccessor
{

}

// ==========================================================
//                         Class
// ==========================================================
public class VeinManager : ContainerAccessor
{
    RandomProbability randProb = new RandomProbability();

    Coords<int> leftVeinStart;
    Coords<int> rightVeinStart;
    Coords<int> middleVeinStart;

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

    public VeinManager(ref GeneratorContainer contInst) : base(ref contInst)
    {
        this.leftVeinStart = new Coords<int>(getTileMapCenter().getX() - 25 + 2, getTileMapCenter().getY() - 13);
        this.middleVeinStart = new Coords<int>(getTileMapCenter().getX(), getTileMapCenter().getY() - 19);
        this.rightVeinStart = new Coords<int>(getTileMapCenter().getX() + 24 - 2, getTileMapCenter().getY() - 13);

        this.xAxis = getTileMapDims().getMaxX();
        this.xAxisOneThird = getTileMapDims().getMaxX() / 3;
        this.xAxisTwoThird = 2 * getTileMapDims().getMaxX() / 3;
        this.xAxisOneFourth = getTileMapDims().getMaxX() / 4;
        this.xAxisTwoFourth = 2 * getTileMapDims().getMaxX() / 4;
        this.xAxisThreeFourth = 3 * getTileMapDims().getMaxX() / 4;

        this.yAxis = getTileMapDims().getMaxY();
        this.yAxisOneThird = getTileMapDims().getMaxY() / 3;
        this.yAxisTwoThird = 2 * getTileMapDims().getMaxY() / 3;
        this.yAxisOneFourth = getTileMapDims().getMaxY() / 4;
        this.yAxisTwoFourth = 2 * getTileMapDims().getMaxY() / 4;
        this.yAxisThreeFourth = 3 * getTileMapDims().getMaxY() / 4;
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

    void createSendOffRoomVeins()
    {
        // Create Vein with basic settings
        Queue<Vein> veinQueue = new Queue<Vein>();
        veinQueue.Enqueue(getSendOffVeinInitProps(Direction.West)); // Left vein
        veinQueue.Enqueue(getSendOffVeinInitProps(Direction.South)); // Middle vein
        veinQueue.Enqueue(getSendOffVeinInitProps(Direction.East)); // Right vein

        // Function that will call createVein(vein)
    }

    Vein getSendOffVeinInitProps(Direction dir)
    {
        Vein initVein = null;
        int xChange = 0;
        int yChange = 0;

        switch (dir)
        {

            case Direction.West:
                xChange = 0;
                yChange = randProb.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new RandomProbability.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new RandomProbability.RandomSelection(yAxisTwoThird + 1, yAxis - 1, .0f));

                initVein = new Vein(Direction.West, leftVeinStart, new Slope(xChange, yChange));
                break;
            case Direction.South:
                xChange = randProb.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(xAxisOneFourth, xAxisOneThird, 0f),
                            new RandomProbability.RandomSelection(xAxisOneThird + 1, xAxisTwoThird, 1f),
                            new RandomProbability.RandomSelection(xAxisTwoThird + 1, xAxisThreeFourth, .0f));
                yChange = 0;

                initVein = new Vein(Direction.South, middleVeinStart, new Slope(xChange, yChange));
                break;
            case Direction.East:
                xChange = getTileMapDims().getMaxX() - 1;
                yChange = randProb.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new RandomProbability.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new RandomProbability.RandomSelection(yAxisTwoThird + 1, yAxis - 1, .0f));

                initVein = new Vein(Direction.East, rightVeinStart, new Slope(xChange, yChange));
                break;
        }

        return initVein;
    }
}

