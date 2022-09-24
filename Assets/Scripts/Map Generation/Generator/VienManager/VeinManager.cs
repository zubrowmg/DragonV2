using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VeinManagerClasses;
using VeinEnums;
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
    // Start vein coords
    Coords<int> leftVeinStart;
    Coords<int> rightVeinStart;
    Coords<int> middleVeinStart;

    // Vary Width Percentages
    float keepWidthPercent = .50f;
    float increaseWidthPercent = .25f;
    float decreaseWidthPercent = .25f;

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
        veinQueue.Enqueue(configSendOffVeinProps(Direction.West)); // Left vein
        veinQueue.Enqueue(configSendOffVeinProps(Direction.South)); // Middle vein
        veinQueue.Enqueue(configSendOffVeinProps(Direction.East)); // Right vein

        // Function that will call createVein(vein)
        foreach (var vein in veinQueue)
        {
            createVein(vein);
        }
    }

    void createVein(Vein newVein)
    {
        // U VEINS ARE NOT SUPPORTED
        //bool isUpDown = Random.Range(0, 1 + 1) == 1 ? true : false;
        //int shiftStart = 2;
        //const int totalUParts = 4;
        //const int totalSections = 8;
        //int[] partWidths = new int[totalUParts];
        //int[] lowerBoundries = new int[totalUParts];
        //int[] upperBoundries = new int[totalUParts];

        //if (vein == veinType.U)
        //{
        //    configureUVein(ref shiftStart, ref partWidths, totalUParts, totalSections, ref newDistance, ref lowerBoundries, ref upperBoundries);
        //}

        float keepWidthPercent = .50f;
        float increaseWidthPercent = .25f;
        float decreaseWidthPercent = .25f;

        VeinDistanceTraveled veinDistanceState = VeinDistanceTraveled.None;
        bool justChangedStates = false;

        //int yPrev = y;
        //int xPrev = x;
        Coords<int> prevCoords = new Coords<int>(newVein.getStartCoords().getX(), newVein.getStartCoords().getY());
        Coords<int> currentCoords = new Coords<int>(newVein.getStartCoords().getX(), newVein.getStartCoords().getY());

        //int width = approxWidth;
        int prevWidth = newVein.getCurrentWidth();
        int xBreakOut = newVein.getStartCoords().getX(); // Will probably depricate breakout
        int yBreakOut = newVein.getStartCoords().getY();

        bool exitLoop = false;

        for (int i = 0; i < newVein.getDistanceGoal(); i++)
        {

            handleDistanceState(currentCoords, prevCoords, ref newVein, ref veinDistanceState, ref justChangedStates);
            prevCoords = currentCoords.deepCopy();

            // If we reach the distance goal, then stop here
            if (newVein.getCurrentDistance() > newVein.getDistanceGoal())
            {
                break;
            }

            if (newVein.getVeinType() == VeinType.Simple)
            {
                simpleVein(ref i, ref x, ref y, ref yPrev, ref yStart, ref newDistance, ref width, 
                                        ref slope, ref exitLoop, ref dir, ref xBreakOut, ref yBreakOut, intendedDir, generalVeinDir);
                if (exitLoop) break;
            }
            //else if (vein == veinType.U)
            //{
            //    handleUVeinSlope(ref distance, ref newDistance, ref slope, ref isUpDown, ref intendedSlope, ref dir, intendedDir,
            //                        shiftStart, ref partWidths, totalUParts, totalSections, lowerBoundries, upperBoundries);
            //    simpleVein(ref i, ref x, ref y, ref yPrev, ref yStart, ref newDistance, ref width, ref slope, ref exitLoop, ref dir, ref xBreakOut, ref yBreakOut, intendedDir, generalVeinDir);
            //}

            // Varies the width every cycle
            if (varyWidth)
            {
                handleWidthChanges(ref width, ref prevWidth, ref approxWidth, ref x, ref y, ref dir, ref decreaseWidthPercent, ref keepWidthPercent, ref increaseWidthPercent);
            }

            // Varies the slope when varySlope every sixth of the distance
            // Only for simple veins
            if (varySlope && vein != veinType.U)
            {
                handleSlopeChanges(ref state, ref justChangedStates, ref slope, ref intendedSlope, ref dir);
            }

            // Every 25 units of distance, we put down a split vein, then a vein template at the end and mark it as POI
            if (splitVeins)
            {
                handleSplitVein(ref state, ref justChangedStates, x, yPrev, xStart, yStart, slope, generalVeinDir);
            }


            // Every 1/6 of the total distance, there is some variance introduced to the bias percents

        }


        void simpleVein(ref int i, ref int x, ref int y, ref int yPrev, ref int yStart, ref int newDistance,
                                   ref int width, ref float slope, ref bool exitLoop, ref veinDirection dir,
                                   ref int xBreakOut, ref int yBreakOut, veinDirection intendedDir, GeneralVeinDirection generalVeinDir)
        {
            int yTempi;
            int yTempiMinus;

            if (i > newDistance)
            {
                //Debug.Log("OUT: ");
                exitLoop = true;
                return;
            }

            //Debug.Log("========== Top: " + x + "," + y + "   ==========");
            createStrip(x, y, width, ref slope, ref dir, generalVeinDir);

            // Fill in the gaps of extreme slopes
            for (int change = 0; change < (Mathf.Abs(y - yPrev)); change++)
            {
                //Debug.Log(y + "," + yPrev);
                if (slope > 0f)
                {

                    if (dir == veinDirection.Right)
                    {
                        //Debug.Log("IN0: " + x + "," + (y - change - 1));
                        createStrip(x, y - change - 1, width, ref slope, ref dir, generalVeinDir);
                    }
                    else
                    {
                        //Debug.Log("IN1: " + x + "," + (y + change + 1));
                        createStrip(x, y + change + 1, width, ref slope, ref dir, generalVeinDir);
                    }
                }
                else
                {
                    if (dir == veinDirection.Right)
                    {
                        //Debug.Log("IN2: " + x + "," + (y + change + 1));
                        createStrip(x, y + change + 1, width, ref slope, ref dir, generalVeinDir);
                    }
                    else
                    {
                        //Debug.Log("IN3: " + x + "," + (y - change - 1));
                        createStrip(x, y - change - 1, width, ref slope, ref dir, generalVeinDir);
                    }
                }
                //newDistance--;
            }
            xBreakOut = x;
            yBreakOut = y;

            if (dir == veinDirection.Right)
            {
                if (-maxNonVerticalSlope < slope && slope < maxNonVerticalSlope)
                {
                    x++;
                }

                // In case the slope changes we have to chang y based on the difference of the previous y
                yTempiMinus = (int)Mathf.Floor((float)(i - 1) * slope) + yStart;
                yTempi = (int)Mathf.Floor((float)i * slope) + yStart;

                y = y + (yTempi - yTempiMinus);
            }
            else
            {
                if (-maxNonVerticalSlope < slope && slope < maxNonVerticalSlope)
                {
                    x--;
                }

                // In case the slope changes we have to chang y based on the difference of the previous y
                yTempiMinus = (int)Mathf.Floor((float)(i - 1) * slope * -1) + yStart;
                yTempi = (int)Mathf.Floor((float)i * slope * -1) + yStart;

                y = y + (yTempi - yTempiMinus);
            }



            // If the slope is extreme we will have a really tall vien, need to trim that here
            // Ex: slope = .01, widthSlope = -100
            if (Mathf.Abs(y - yPrev) > (newDistance - i))
            {
                //Debug.Log("IN1   y: " + y + "    yPrev: " + yPrev + "   Change:" + (newDistance - i));
                if (slope < 0f)
                {
                    if (dir == veinDirection.Right)
                    {
                        y = yPrev - (newDistance - i);
                    }
                    else
                    {
                        y = yPrev + (newDistance - i);
                    }

                }
                else
                {
                    if (dir == veinDirection.Right)
                    {
                        y = yPrev + (newDistance - i);
                    }
                    else
                    {
                        y = yPrev - (newDistance - i);
                    }
                    //y = yPrev + (newDistance - i);
                }
            }
        }


        void handleDistanceState(Coords<int> currentCoords, Coords<int> prevCoords, ref Vein vein, ref VeinDistanceTraveled state, ref bool justChangedStates)
        {
            // Calculate how far this vein has traveled
            int xChange = Mathf.Abs(currentCoords.getX() - prevCoords.getX());
            int yChange = Mathf.Abs(currentCoords.getY() - prevCoords.getY());
            int newTraveledDistance = vein.getCurrentDistance() + (int)Mathf.Floor(Mathf.Sqrt((xChange * xChange) + (yChange * yChange)));
            vein.setCurrentDistance(newTraveledDistance);
            
            //Debug.Log(xChange + "," + yChange);
            //Debug.Log("Distance: " + totalDistanceTraveled + "   totalDistance: " + totalDistance);
            if (state == VeinDistanceTraveled.None)
            {
                if (vein.getCurrentDistance() >= (vein.getDistanceGoal() / 6))
                {
                    state = VeinDistanceTraveled.One_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.One_Sixths)
            {
                if (vein.getCurrentDistance() >= (2 * (vein.getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Two_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Two_Sixths)
            {
                if (vein.getCurrentDistance() >= (3 * (vein.getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Three_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Three_Sixths)
            {
                if (vein.getCurrentDistance() >= (4 * (vein.getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Four_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Four_Sixths)
            {
                if (vein.getCurrentDistance() >= (5 * (vein.getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Five_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Five_Sixths)
            {

                justChangedStates = false;

            }

        }
    }

    Vein configSendOffVeinProps(Direction dir)
    {
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

                initVein = new Vein(type, Direction.West, leftVeinStart, endDesination, new Slope(leftVeinStart, endDesination), varyVeinWidth, varyVeinLength, varyVeinSlope);
                break;
            case Direction.South:
                xChange = RandomProbability.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(xAxisOneFourth, xAxisOneThird, 0f),
                            new RandomProbability.RandomSelection(xAxisOneThird + 1, xAxisTwoThird, 1f),
                            new RandomProbability.RandomSelection(xAxisTwoThird + 1, xAxisThreeFourth, .0f));
                yChange = 0;
                endDesination = new Coords<int>(xChange, yChange);

                initVein = new Vein(type, Direction.South, middleVeinStart, endDesination, new Slope(middleVeinStart, endDesination), varyVeinWidth, varyVeinLength, varyVeinSlope);
                break;
            case Direction.East:
                xChange = getTileMapDims().getMaxX() - 1;
                yChange = RandomProbability.getIntBasedOnPercentage(
                            new RandomProbability.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new RandomProbability.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new RandomProbability.RandomSelection(yAxisTwoThird + 1, yAxis - 1, .0f));
                endDesination = new Coords<int>(xChange, yChange);

                initVein = new Vein(type, Direction.East, rightVeinStart, endDesination, new Slope(rightVeinStart, endDesination), varyVeinWidth, varyVeinLength, varyVeinSlope);
                break;
        }

        return initVein;
    }
}

