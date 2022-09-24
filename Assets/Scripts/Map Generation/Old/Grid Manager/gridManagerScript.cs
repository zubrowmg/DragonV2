using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;
using Classes;

public class gridManagerScript : MonoBehaviour
{
    

    public enum vertical  { Yes, No }
    
    public enum distanceTraveled { none, one_sixths, two_sixths, three_sixths, four_sixths, five_sixths };
    public enum slopeChange { Increase, IncreasePlus, IncreaseMinus, Decrease, DecreasePlus, DecreaseMinus };
    public enum gridType { Vein, VeinMain, Used, POI, POICore, POICoreSmall, POICoreMedium, POICoreLarge };

    // veinDirection is used for graphing vein purposes, only need left and right
    // GeneralVeinDirection is used for vein preset orientation
    public enum veinDirection { Left, Right };
    public enum GeneralVeinDirection { Left, Right, Up, Down };
    public enum veinType { Simple, U, Wave, ReallyStrong };
    public enum uVeinStage { Part1, Part2, Part3, Part4 };
    public enum uVeinType { Shift, Widen, Shift_Widen }; 
    public enum veinStart { Left, Middle, Right };

    
    public bool varyVeinWidth = true;
    public bool dontVaryWeinWidth = false;
    public bool varyVeinLength = true;
    public bool dontVaryVeinLength = false;
    public bool varyVeinSlope = true;
    public bool dontVaryVeinSlope = false;
    public bool splitVein = true;
    public bool dontSplitVein = false;

    public bool markEndAsPOICore = true;
    public bool dontMarkEndAsPOICore = false;

    // Vein properties
    float maxSlope = 5f;
    float maxNonVerticalSlope = 4.9f;
    int minWidth;
    public static int leftVeinStartX;
    public static int leftVeinStartY;
    public static int middleVeinStartX;
    public static int middleVeinStartY;
    public static int rightVeinStartX;
    public static int rightVeinStartY;



    // Bounds
    public const int xAxis = 147*2*2; // *2 is for the half scale of the grid unit. Second *2 is for increasing the grid size
    public const int yAxis = 72*2*2;

    public const int xAxisOneThird = xAxis / 3;
    public const int xAxisTwoThird = 2 * xAxis / 3;
    public const int xAxisOneFourth = xAxis / 4;
    public const int xAxisTwoFourth = 2 * xAxis / 4;
    public const int xAxisThreeFourth = 3 * xAxis / 4;
    

    public const int yAxisOneThird = yAxis / 3;
    public const int yAxisTwoThird = 2 * yAxis / 3;
    public const int yAxisOneFourth = yAxis / 4;
    public const int yAxisTwoFourth = 2 * yAxis / 4;
    public const int yAxisThreeFourth = 3 * yAxis / 4;

    public float gridHeight;
    public float gridWidth;
    public const int gridCenterX = xAxis / 2;
    public const int gridCenterY = yAxis / 2;
   
    public const int gridStartX = xAxis / 2;
    public const int gridStartY = (yAxis / 2) + 50;

    // GameObjects
    public GameObject[,] grid = new GameObject[xAxis, yAxis];
    public GameObject gridUnitTemplate; // x/y =
    public GameObject randProbabiliryManager;
    public GameObject veinPresetManager;

    // Vein Helpers
    public List<int> xPOI;
    public List<int> yPOI;
    public List<int> xPOICore;
    public List<int> yPOICore;

    // Start is called from the MapGenerator script, which is the "main" script
    public void createGrid(ref List<RoomPreset> bossRoomLocations)
    {

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // For memory reasons you will have to replace grid units with a non GameObject class
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        float xStart = -.3f;
        int yStart = 0;

        gridHeight = gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.y;
        gridWidth = gridUnitTemplate.GetComponent<SpriteRenderer>().bounds.size.x;

        

        for (int x = 0; x < xAxis; x++)
        {
            for (int y = 0; y < yAxis; y++)
            {
                // Get new position
                float newXPos = ((xStart + x) * gridWidth) + (gridWidth / 2);
                float newYPos = ((yStart + y) * gridHeight) + (gridHeight / 2);

                // Create new grid unit, set parent to GridManager
                GameObject newGrid = Instantiate(gridUnitTemplate, new Vector3(newXPos, newYPos, 0), Quaternion.identity);
                newGrid.transform.SetParent(this.transform);
                newGrid.GetComponent<gridUnitScript>().x = x;
                newGrid.GetComponent<gridUnitScript>().y = y;
                newGrid.name = "GridUnit";

                // Add new gridUnit to array
                grid[x, y] = newGrid;
            }
        }

        createRoomVeins(ref bossRoomLocations);
    }

    void createRoomVeins(ref List<RoomPreset> bossRoomLocations)
    {
        createStartAreaVeins();

        installPresetVeins(ref bossRoomLocations);
        // checkAndInstallHybirdZones(ref bossRoomLocations);

        createUniqueAreaVeins();


        installBedRock();
    }

    void installBedRock()
    {

    }

    void createUniqueAreaVeins()
    {
        // Percent chance to create niche areas:
        //      - Hole vein, has construction elements. Beams to hold up cave
        //      - ?Serpenting vein (has bandit enemies (snakey bandit), more loot)
        //      Meant to be a small mini area, with more focus on connecting the map and fleshing out the world
        //      Can add mini bosses
    }

    void createStartAreaVeins()
    {
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Start area veins might need to avoid POI, so they don't cross
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        GeneralVeinDirection generalVeinDir = GeneralVeinDirection.Left;

        int x = gridCenterX;
        int y = gridCenterY;
        int xBreakOut = x;
        int yBreakOut = y;

        leftVeinStartX = gridStartX - 25;
        leftVeinStartY = gridStartY - 13;
        middleVeinStartX = gridStartX;
        middleVeinStartY = gridStartY - 19; 
        rightVeinStartX = gridStartX + 24;
        rightVeinStartY = gridStartY - 13;

        int xDir = 0;
        int yDir = 0;

        minWidth = 4;
        int approxWidth = 6;
        int approxDistance = 120;

        bool varyWidth = varyVeinWidth;
        bool varyLength = varyVeinLength;
        bool varySlope = varyVeinSlope;
        bool splitVeins = splitVein;

        // (588, 288)

        Debug.Log("Remember that you can make vein presets that create big room pockets with a snaking path.");
        Debug.Log("Look for big double exclamation marks in the binder");

        Debug.Log(" NEW VEIN =======================================================================");
        veinType type = (veinType)Random.Range(0, 2);
        configureStartVein(veinStart.Left, ref x, ref y, ref xDir, ref yDir, ref generalVeinDir);
        createVein(x, y, xDir, yDir, ref xBreakOut, ref yBreakOut, approxWidth, approxDistance, type, varyWidth, varyLength, varySlope, splitVeins, generalVeinDir);

        Debug.Log(" NEW VEIN =======================================================================");
        type = (veinType)Random.Range(0, 2);
        configureStartVein(veinStart.Middle, ref x, ref y, ref xDir, ref yDir, ref generalVeinDir);
        createVein(x, y, xDir, yDir, ref xBreakOut, ref yBreakOut, approxWidth, approxDistance, type, varyWidth, varyLength, varySlope, splitVeins, generalVeinDir);

        Debug.Log(" NEW VEIN =======================================================================");
        type = (veinType)Random.Range(0, 2);
        configureStartVein(veinStart.Right, ref x, ref y, ref xDir, ref yDir, ref generalVeinDir);
        createVein(x, y, xDir, yDir, ref xBreakOut, ref yBreakOut, approxWidth, approxDistance, type, varyWidth, varyLength, varySlope, splitVeins, generalVeinDir);
    }

    void configureStartVein(veinStart startLocation, ref int x, ref int y, ref int xDir, ref int yDir, ref GeneralVeinDirection generalVeinDir)
    {
        switch (startLocation)
        {

            case veinStart.Left:
                generalVeinDir = GeneralVeinDirection.Left;
                x = leftVeinStartX + 2;
                y = leftVeinStartY;

                xDir = 0;
                yDir =  randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                            new randomProbabilityManagerScript.RandomSelection(yAxisOneFourth,  yAxisOneThird, .5f),
                            new randomProbabilityManagerScript.RandomSelection(yAxisOneThird+1, yAxisTwoThird, .5f),
                            new randomProbabilityManagerScript.RandomSelection(yAxisTwoThird+1,       yAxis-1, .0f));
                //Debug.Log(xDir + ", " + yDir);
                break;
            case veinStart.Middle:
                generalVeinDir = GeneralVeinDirection.Down;
                x = middleVeinStartX;
                y = middleVeinStartY;


                xDir = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                            new randomProbabilityManagerScript.RandomSelection(   xAxisOneFourth, xAxisOneThird, 0f),
                            new randomProbabilityManagerScript.RandomSelection(xAxisOneThird + 1, xAxisTwoThird, 1f),
                            new randomProbabilityManagerScript.RandomSelection(xAxisTwoThird + 1, xAxisThreeFourth, .0f));
                yDir = 0;
                //Debug.Log(xDir + ", " + yDir);

                break;
            case veinStart.Right:
                generalVeinDir = GeneralVeinDirection.Right;
                x = rightVeinStartX - 2;
                y = rightVeinStartY;

                xDir = xAxis-1;
                yDir = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                            new randomProbabilityManagerScript.RandomSelection(yAxisOneFourth, yAxisOneThird, .5f),
                            new randomProbabilityManagerScript.RandomSelection(yAxisOneThird + 1, yAxisTwoThird, .5f),
                            new randomProbabilityManagerScript.RandomSelection(yAxisTwoThird + 1,     yAxis - 1, .0f));
                //Debug.Log(xDir + ", " + yDir);

                break;
        }      
    }

    // Use bias instead of start stop
    void createVein(int xStart, int yStart, int xDir, int yDir, ref int xBreakOut, ref int yBreakOut, int approxWidth, int approxDistance, veinType vein,
                        bool varyWidth, bool varyLength, bool varySlope, bool splitVeins, GeneralVeinDirection generalVeinDir)
    {
        //Debug.Log(" NEW VEIN =======================================================================");
        //Debug.Log(" NEW VEIN =======================================================================");
        //Debug.Log(" NEW VEIN =======================================================================");

        veinDirection dir = veinDirection.Right;
        float slope = 0f;

        calculateSlopeAndDirection(ref xStart, ref yStart, ref xDir, ref yDir, ref slope, ref dir);

        float intendedSlope = slope;
        int x = xStart; 
        int y = yStart;

        int randomDistanceChange = 0;
        if (varyLength)
        {
            randomDistanceChange = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                                        new randomProbabilityManagerScript.RandomSelection(0, 25, .25f),
                                        new randomProbabilityManagerScript.RandomSelection(26, 45, .75f),
                                        new randomProbabilityManagerScript.RandomSelection(46, 65, .0f));
        }
        int newDistance = approxDistance + randomDistanceChange;

        bool isUpDown = Random.Range(0, 1+1) == 1 ? true : false;
        //Debug.Log(isUpDown);
        int shiftStart = 2;
        const int totalUParts = 4;
        const int totalSections = 8;
        int[] partWidths = new int[totalUParts];
        int[] lowerBoundries = new int[totalUParts];
        int[] upperBoundries = new int[totalUParts];

        if (vein == veinType.U)
        {
            configureUVein(ref shiftStart, ref partWidths, totalUParts, totalSections, ref newDistance, ref lowerBoundries, ref upperBoundries);
        }      

        float keepWidthPercent = .50f;
        float increaseWidthPercent = .25f;
        float decreaseWidthPercent = .25f;

        distanceTraveled state = distanceTraveled.none;
        bool justChangedStates = false;

        int yPrev = y;
        int xPrev = x;
        int width = approxWidth;
        int prevWidth = width;
        xBreakOut = x;
        yBreakOut = y;
        
        int distance = 0;
        bool exitLoop = false;
        veinDirection intendedDir = dir;

        //Debug.Log("Intended Slope: " + intendedSlope);

        for (int i = 0; i < newDistance; i++)
        {
            
            handleDistanceState(Mathf.Abs(x - xPrev), Mathf.Abs(y - yPrev), ref distance, ref newDistance, ref state, ref justChangedStates);
            xPrev = x;

            if (distance > newDistance)
            {
                break;
            }

            if (vein == veinType.Simple)
            {
                simpleVein(ref i, ref x, ref y, ref yPrev, ref yStart, ref newDistance, ref width, ref slope, ref exitLoop, ref dir, ref xBreakOut, ref yBreakOut, intendedDir, generalVeinDir);
                if (exitLoop) break;
            }
            else if (vein == veinType.U)
            {
                handleUVeinSlope(ref distance, ref newDistance, ref slope, ref isUpDown, ref intendedSlope, ref dir, intendedDir, 
                                    shiftStart, ref partWidths, totalUParts, totalSections, lowerBoundries, upperBoundries);
                simpleVein(ref i, ref x, ref y, ref yPrev, ref yStart, ref newDistance, ref width, ref slope, ref exitLoop, ref dir, ref xBreakOut, ref yBreakOut, intendedDir, generalVeinDir);
            }

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

       

    }

    void handleSplitVein(ref distanceTraveled state, ref bool justChangedStates, int xStart, int yStart, int xMainVeinStart, int yMainVeinStart, float currentSlope, GeneralVeinDirection generalVeinDir)
    {
        if ((state == distanceTraveled.one_sixths /*|| state == distanceTraveled.three_sixths*/) && justChangedStates)
        {
            

            int distance = 20;
            int width = 6;
            float splitSlope = ((float)-1 / currentSlope);
            int xBreakOut = xStart;
            int yBreakOut = yStart;

            int xDir = 10;
            int randomDirection = 0;

            switch (state)
            {
                case distanceTraveled.one_sixths:
                    // Then make sure the split vein goes away from the start
                    float yRight = Mathf.RoundToInt(splitSlope * xDir);
                    float yLeft  = Mathf.RoundToInt(splitSlope * xDir * -1);

                    float xNewRight = xStart + xDir;
                    float yNewRight = yStart + yRight;
                    float xNewLeft  = xStart + (xDir * -1);
                    float yNewLeft  = yStart + yLeft;

                    float distanceRight = Mathf.Sqrt(Mathf.Pow(xNewRight - gridStartX, 2f) + Mathf.Pow(yNewRight - gridStartY, 2f));
                    float distanceLeft  = Mathf.Sqrt(Mathf.Pow(xNewLeft - gridStartX, 2f) + Mathf.Pow(yNewLeft - gridStartY, 2f));

                    if (distanceLeft >= distanceRight)
                    {
                        randomDirection = 1;
                    }
                    //randomDirection = Random.Range(0, 2);
                    break;
                case distanceTraveled.three_sixths:
                    randomDirection = Random.Range(0, 2);
                    break;
            }

            //Debug.Log(randomDirection);

            // Randomly choose a perpendicular direction
            if (randomDirection == 0)
            {
                // Go right
                // Nothing
            }
            else
            {
                // Go left
                xDir = xDir * -1;
            }
            int yDir = (int)Mathf.RoundToInt(splitSlope * xDir);

            xDir += xStart;
            yDir += yStart;

            //Debug.Log("Split:" + splitSlope + "     current: " + currentSlope);
            //Debug.Log("xDir:" + xDir + "     yDir: " + yDir);

            // Change start of split to POI
            changeGridUnitProperties(xStart, yStart, gridType.POI, generalVeinDir, gameTiming.Early);

            createVein(xStart, yStart, xDir, yDir, ref xBreakOut, ref yBreakOut, width, distance, veinType.Simple,
                        dontVaryWeinWidth, dontVaryVeinLength, dontVaryVeinSlope, dontSplitVein, generalVeinDir);


            // Change start of split to POI Core
            changeGridUnitProperties(xBreakOut, yBreakOut, gridType.POICoreSmall, generalVeinDir, gameTiming.Early);
        }
    }

    void installPresetVeins(ref List<RoomPreset> bossRoomLocations)
    {
        veinPresetManagerScript.POICoreType POIType = veinPresetManagerScript.POICoreType.Small;
        int zone = 0;

        // Iterate through all POI Cores
        for (int i = 0; i < xPOICore.Count; i++)
        {

            //print("NEW CORE");

             

            // ref int[,] preset, int xPresetStart, int yPresetStart
            int xPresetStart = xPOICore[i];
            int yPresetStart = yPOICore[i];
            List<List<int>> preset = new List<List<int>>();

            // Get the POICore type
            gridUnitScript gridUnitProperties = grid[xPresetStart, yPresetStart].GetComponent<gridUnitScript>();
            gridUnitScript.GeneralVeinDirection dir = gridUnitProperties.veinDirection;

            ZoneUnitProperties choosenZoneAndAbilities = gridUnitProperties.zoneProperties[0];
            zone = choosenZoneAndAbilities.zoneAreaId;

            if (gridUnitProperties.isPOICoreSmall) {
                POIType = veinPresetManagerScript.POICoreType.Small;
            } else if (gridUnitProperties.isPOICoreMedium) {
                POIType = veinPresetManagerScript.POICoreType.Medium;
            } else if (gridUnitProperties.isPOICoreLarge) {
                POIType = veinPresetManagerScript.POICoreType.Large;
            }

            veinPresetManager.GetComponent<veinPresetManagerScript>().getCorePOI(ref preset, ref xPresetStart, ref yPresetStart, 
                                                                                        POIType, dir, ref bossRoomLocations, choosenZoneAndAbilities);
            


            // Install selected preset into the grid
            int xPlacement = xPresetStart;
            int yPlacement = yPresetStart;

            for (int x = 0; x < preset.Count; x++)
            {
                
                yPlacement = yPresetStart;
                for (int y = 0; y < preset[0].Count; y++)
                {
                    changeGridUnitZone(xPlacement, yPlacement, zone);
                    if (preset[x][y] == 1)
                    {
                        changeGridUnitProperties(xPlacement, yPlacement, gridType.Vein, dir, gameTiming.Null);
                        changeGridUnitIntendedType(xPlacement, yPlacement, zone, intendedUnitType.Zone);
                        changeGridUnitZoneAndAbilities(xPlacement, yPlacement, choosenZoneAndAbilities);
                    }
                    yPlacement++;
                }
                xPlacement++;
            }
        }
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

        //Debug.Log("y: " + y + "    yPrev: " + yPrev);
        yPrev = y;
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

     
    void configureUVein(ref int shiftStart, ref int[] partWidths, int totalUParts, int totalSections, ref int totalDistance, ref int[] lowerBoundries, ref int[] upperBoundries)
    {
        
        int credits = totalSections - 5;
        int partWidth = 0;
        int part = 0;

        uVeinType uType = (uVeinType)Random.Range(0, 2+1);
        if (uType == uVeinType.Shift)
        {
            // Can also shift 0 times aka None
            // Can only shift up to 3
            shiftStart += Random.Range(0, 3+1);
        }
        else if (uType == uVeinType.Widen)
        {
            while (credits > 0)
            {
                // Choose a random width
                partWidth = Random.Range(1, credits+1);
                credits -= partWidth;

                // Give it to a random part
                part = Random.Range(0, partWidths.Length);
                for (int i = 0; i < partWidths.Length; i++)
                {
                    if (part == i)
                    {
                        partWidths[part] += partWidth;
                    }
                }
            }
        }
        else if (uType == uVeinType.Shift_Widen)
        {
            // Can only shift up to 2, need credits for widening
            int shiftStartChange = 1;// Random.Range(1, 2+1);
            shiftStart += shiftStartChange;
            credits -= shiftStartChange;

            while (credits > 0)
            {
                // Choose a random width
                partWidth = Random.Range(1, credits+1);
                credits -= partWidth;

                // Give it to a random part
                part = Random.Range(0, partWidths.Length);
                for (int i = 0; i < partWidths.Length; i++)
                {
                    if (part == i)
                    {
                        partWidths[part] += partWidth;
                    }
                }
            }
        }
        //Debug.Log("Start: " + shiftStart);
        //Debug.Log("Part Widths: " + partWidths[0] + " | " + partWidths[1] + " | " + partWidths[2] + " | " + partWidths[3]);
        //Debug.Log(uType);

        configurePartBoundries(ref partWidths, ref lowerBoundries, ref upperBoundries, totalSections, ref shiftStart, ref totalDistance);
    }

    void configurePartBoundries(ref int[] partWidths, ref int[] lowerBoundries, ref int[] upperBoundries, int totalSections, ref int shiftStart, ref int totalDistance)
    {
        int combinedWidths = 0; 
        for (int i = 0; i < partWidths.Length; i++)
        {
            lowerBoundries[i] = (shiftStart + combinedWidths + i-2) * (totalDistance / totalSections);

            combinedWidths += partWidths[i];

            upperBoundries[i] = (shiftStart + combinedWidths + i) * (totalDistance / totalSections);
            //Debug.Log("Low: " + lowerBoundries[i] + "    High: " + upperBoundries[i]);
        }
    }

    void handleUVeinSlope(ref int distance, ref int totalDistance, ref float slope, ref bool isUpDown,
                             ref float intendedSlope, ref veinDirection dir, veinDirection intendedDir,
                             int shiftStart, ref int[] partWidths, int totalUParts, int totalSections, int[] lowerBoundries, int[] upperBoundries)
    {
        float perpendicularSlope = ((float)1 / intendedSlope);
        float medianSlope = (intendedSlope + perpendicularSlope) / 2;
        veinDirection newDir = dir;
 
        //Debug.Log("Distance: " + distance);

        float newSlope = 0f;
        if (distance < 1 * (totalDistance / totalSections))
        {
            // DON'T EVER START TURNING IN HERE
        }
        else if (lowerBoundries[0] <= distance && distance < upperBoundries[0])
        {
            handleUtype(ref isUpDown, uVeinStage.Part1, intendedSlope,  ref newDir, ref dir, ref slope, newSlope, intendedDir);
        }
        else if (lowerBoundries[1] <= distance && distance < upperBoundries[1])
        {
            handleUtype(ref isUpDown, uVeinStage.Part2, intendedSlope, ref newDir, ref dir, ref slope, newSlope, intendedDir);
        }
        else if (lowerBoundries[2] <= distance && distance < upperBoundries[2])
        {
            handleUtype(ref isUpDown, uVeinStage.Part3, intendedSlope, ref newDir, ref dir, ref slope, newSlope, intendedDir);
        }
        else if (lowerBoundries[3] <= distance && distance < upperBoundries[3])
        {
            handleUtype(ref isUpDown, uVeinStage.Part4, intendedSlope, ref newDir, ref dir, ref slope, newSlope, intendedDir);
        }
    }

    void handleUtype(ref bool isUpDown, uVeinStage stage, float intendedSlope, ref veinDirection newDir, ref veinDirection dir,
                            ref float slope, float newSlope, veinDirection intendedDir)
    {

        //Debug.Log("                " + stage);
        if (stage == uVeinStage.Part1)
        {
            if (isUpDown)
            {
                newSlope = calculateAngleChange(intendedDir, intendedSlope, dir, slope, 45f, ref newDir);
                if (isMovingAngleLessThanTargetAngle(slope, dir, newSlope, newDir))
                {
                    changeSlope(slopeChange.IncreaseMinus, ref slope, ref dir);
                }
            }
            else
            {
                newSlope = calculateAngleChange(intendedDir, intendedSlope, dir, slope, -45f, ref newDir);
                if (isMovingAngleGreaterThanTargetAngle(slope, dir, newSlope, newDir))
                {
                    changeSlope(slopeChange.DecreaseMinus, ref slope, ref dir);
                }
            }
        }
        else if (stage == uVeinStage.Part2)
        {
            if (isUpDown)
            {
                if (isMovingAngleGreaterThanTargetAngle(slope, dir, intendedSlope, intendedDir))
                {
                    changeSlope(slopeChange.DecreaseMinus, ref slope, ref dir);
                }
            }
            else
            {
                if (isMovingAngleLessThanTargetAngle(slope, dir, intendedSlope, intendedDir))
                {
                    changeSlope(slopeChange.IncreaseMinus, ref slope, ref dir);
                }
            }
        }
        else if (stage == uVeinStage.Part3)
        {
            if (isUpDown)
            {
                newSlope = calculateAngleChange(intendedDir, intendedSlope, dir, slope, -45f, ref newDir);
                if (isMovingAngleGreaterThanTargetAngle(slope, dir, newSlope, newDir))
                {
                    changeSlope(slopeChange.DecreaseMinus, ref slope, ref dir);
                }
            }
            else
            {
                newSlope = calculateAngleChange(intendedDir, intendedSlope, dir, slope, 45f, ref newDir);
                if (isMovingAngleLessThanTargetAngle(slope, dir, newSlope, newDir))
                {
                    changeSlope(slopeChange.IncreaseMinus, ref slope, ref dir);
                }
            }
        }
        else if (stage == uVeinStage.Part4)
        {
            if (isUpDown)
            {
                if (isMovingAngleLessThanTargetAngle(slope, dir, intendedSlope, intendedDir))
                {
                    changeSlope(slopeChange.IncreaseMinus, ref slope, ref dir);
                }
            }
            else
            {
                if (isMovingAngleGreaterThanTargetAngle(slope, dir, intendedSlope, intendedDir))
                {
                    changeSlope(slopeChange.DecreaseMinus, ref slope, ref dir);
                }
            }
        }
    }

    bool isMovingAngleGreaterThanTargetAngle(float slope, veinDirection dir, float newSlope, veinDirection newDir)
    {
        float movingAngle = getAngleFromSlope(slope, dir);
        float targetAngle = getAngleFromSlope(newSlope, newDir);

        bool isGreater = false;
        //Debug.Log("Moving Dir: " + dir + "    New Dir: " + newDir);
        //Debug.Log("Moving Ang: " + movingAngle + "    Target Ang: " + targetAngle);

        if (dir == veinDirection.Right && newDir == veinDirection.Right)
        {
            if (0f < movingAngle && movingAngle < 90f && 270f < targetAngle && targetAngle < 360f)
            {
                isGreater = true;
            }
            else if (270f < movingAngle && movingAngle < 360f && 0f < targetAngle && targetAngle < 90f)
            {
                isGreater = false;
            }
            else if (targetAngle < movingAngle)
            {
                isGreater = true;
            }
        }
        else if (targetAngle < movingAngle)
        {
            isGreater = true;
        }

        return isGreater;
    }


    bool isMovingAngleLessThanTargetAngle(float slope, veinDirection dir, float newSlope, veinDirection newDir)
    {
        float movingAngle = getAngleFromSlope(slope, dir);
        float targetAngle = getAngleFromSlope(newSlope, newDir);

        bool isLess = false;
        //Debug.Log("Moving Dir: " + dir + "    Target Dir: " + newDir);
        //Debug.Log("Moving Ang: " + movingAngle + "    Target Ang: " + targetAngle);

        if (dir == veinDirection.Right && newDir == veinDirection.Right)
        {
            // This is to handle the 360 degrees to 0 degrees transition
            if (0f < targetAngle && targetAngle < 90f && 270f < movingAngle && movingAngle < 360f)
            {
                isLess = true;
            }
            else if (270f < targetAngle && targetAngle < 360f && 0f < movingAngle && movingAngle < 90f)
            {
                isLess = false;
            }
            else if (movingAngle < targetAngle)
            {
                isLess = true;
            }
        }
        else if(movingAngle < targetAngle)
        {
            isLess = true;
        }

        return isLess;
    }

    float getAngleFromSlope(float slope, veinDirection dir)
    {
        float newAngle = 0f; 

        if (dir == veinDirection.Right)
        {
            newAngle = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Abs(slope), 1);
            if (slope < 0f)
            {
                newAngle = 360f - newAngle;
            }
        }
        else
        {
            newAngle = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Abs(slope), 1);
            if (slope < 0f)
            {
                newAngle = 180f - newAngle;
            }
            else
            {
                newAngle += 180f;
            }
        }

        return newAngle;
    }

    float calculateAngleChange(veinDirection intendedDir, float intendedSlope, veinDirection dir, float slope, float angleDiff, ref veinDirection newDir)
    {
        float oldAngle = Mathf.Rad2Deg * Mathf.Atan2(intendedSlope, 1);
        float currentAngle = Mathf.Rad2Deg * Mathf.Atan2(slope, 1);
        float newAngle = (angleDiff + (Mathf.Rad2Deg * Mathf.Atan2(intendedSlope, 1)));

        float tempCurrentAngle = currentAngle;
        float tempNewAngle = newAngle;

        /*Debug.Log("Slope: " + slope);
        Debug.Log("Int Slope: " + intendedSlope);
        Debug.Log("Pre Curr Ang: " + currentAngle);
        Debug.Log("Pre New Ang: " + newAngle);
        Debug.Log("Int Ang: " + oldAngle);
        Debug.Log("Ang Diff: " + angleDiff);
        */

        if (dir == veinDirection.Left)
        {
            if (tempCurrentAngle < 0f)
            {
                tempCurrentAngle = 180f - Mathf.Abs(currentAngle);
                currentAngle = 180f - Mathf.Abs(currentAngle);
                if (0f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 90f)
                {
                    dir = veinDirection.Right;
                    //Debug.Log("      Right");
                }
            }
            else
            {
                tempCurrentAngle = Mathf.Abs(currentAngle) + 180f;
                currentAngle = 180f + Mathf.Abs(currentAngle);
                if (270f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 360f)
                {
                    dir = veinDirection.Right;
                    //Debug.Log("      Right");
                }
            }
        }
        else if  (dir == veinDirection.Right)
        {
            if (currentAngle < 0f)
            {
                tempCurrentAngle = 360 - Mathf.Abs(currentAngle);
                currentAngle = 360 - Mathf.Abs(currentAngle);           
                if (180f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 270f)
                {
                    dir = veinDirection.Left;
                    //Debug.Log("      LEFT");
                }
            }
            else
            {
                tempCurrentAngle = currentAngle;
                //currentAngle = currentAngle;
                if (90f < tempCurrentAngle + angleDiff && tempCurrentAngle + angleDiff < 180f)
                {
                    dir = veinDirection.Left;
                    //Debug.Log("      Left");
                }
            }
        }

        if (intendedDir == veinDirection.Left)
        {
            if (oldAngle < 0f)
            {
                oldAngle = 180f - Mathf.Abs(oldAngle);
                newAngle = oldAngle + angleDiff;
                if (0f < newAngle && newAngle < 90f)
                {
                    newDir = veinDirection.Right;
                    //Debug.Log("      Right");
                }
                else
                {
                    newDir = veinDirection.Left;
                }
            }
            else
            {
                oldAngle = Mathf.Abs(oldAngle) + 180f;
                newAngle = oldAngle + angleDiff;
                if (270f < newAngle && newAngle < 360f)
                {
                    newDir = veinDirection.Right;
                    //Debug.Log("      Right");
                }
                else
                {
                    newDir = veinDirection.Left;
                }
            }
        }
        else if (intendedDir == veinDirection.Right)
        {
            if (oldAngle < 0f)
            {
                oldAngle = 360f - Mathf.Abs(oldAngle);
                newAngle = oldAngle + angleDiff;
                if (newAngle > 360f)
                {
                    newAngle = newAngle - 360f;
                }
                if (180f < newAngle && newAngle < 270f)
                {
                    newDir = veinDirection.Left;
                    //Debug.Log("      LEFT");
                }
                else
                {
                    newDir = veinDirection.Right;
                }
            }
            else
            {
                //oldAngle = oldAngle;
                newAngle = oldAngle + angleDiff;
                if (newAngle < 0f)
                {
                    newAngle = 360f + newAngle;
                }
                if (90f < newAngle && newAngle < 180f)
                {
                    newDir = veinDirection.Left;
                    //Debug.Log("      Left");
                }
                else
                {
                    newDir = veinDirection.Right;
                }
            }
        }

        /*
        Debug.Log("Int Ang: " + oldAngle);
        Debug.Log("Curr Ang: " + currentAngle);
        Debug.Log("New Ang: " + newAngle);

        Debug.Log("Temp Curr Ang: " + tempCurrentAngle);
        Debug.Log("Temp New Ang: " + tempNewAngle);
        
        Debug.Log("New Slope1: " + Mathf.Tan(Mathf.Deg2Rad * newAngle));
        Debug.Log("New Ang1: " + newAngle);
        */

        float newSlope = Mathf.Tan(Mathf.Deg2Rad * newAngle);

        /*if (intendedSlope > -maxNonVerticalSlope && newSlope < -maxNonVerticalSlope)
        {
            newSlope = -maxNonVerticalSlope;
        } 
        else */
        if (intendedSlope > -maxSlope && newSlope < -maxSlope)
        {
            newSlope = -maxSlope;
        }
        /*else if (intendedSlope == -maxSlope && newSlope > 0)
        {
            newSlope = maxNonVerticalSlope;
        }

        if (intendedSlope < maxNonVerticalSlope && newSlope > maxNonVerticalSlope)
        {
            newSlope = maxNonVerticalSlope;
        }
        else*/
        if (intendedSlope < maxSlope && newSlope > maxSlope)
        {
            newSlope = maxSlope;
        }
        /*else if(intendedSlope == maxSlope && newSlope < 0)
        {
            newSlope = -maxNonVerticalSlope;
        }*/


        //Debug.Log("Target Slope: " + newSlope);

        return newSlope;
    }

    void createStrip(int x, int y, int width, ref float slope, ref veinDirection dir, GeneralVeinDirection generalVeinDir)
    {

        width = width / 2;
        width++;

        int notFlat = System.Convert.ToInt32(true);
        float widthSlope = ((float)-1 / slope);

        if (-.15f < slope && slope < .15f)
        {
            notFlat = System.Convert.ToInt32(false);
        }

        int yPrev = y;
        int yStart = y;
        int yUpper = y;
        int yUpperPrev = y;

        changeGridUnitProperties(x,  y, gridType.VeinMain, generalVeinDir, gameTiming.Null);      
        
        for (int i = 0; i < width; i++)
        {

            changeGridUnitProperties(x + (i * notFlat), y,      gridType.Vein, generalVeinDir, gameTiming.Null);
            changeGridUnitProperties(x - (i * notFlat), yUpper, gridType.Vein, generalVeinDir, gameTiming.Null);       

            // Fill in the gaps of extreme slopes
            if (Mathf.Abs(y - yPrev) > 1)
            {
                for (int change = 0; change < (Mathf.Abs(y - yPrev)); change++)
                {

                    if (widthSlope < 0f)
                    {
                        changeGridUnitProperties(x + (i * notFlat),      y + change + 1, gridType.Vein, generalVeinDir, gameTiming.Null);
                        changeGridUnitProperties(x - (i * notFlat), yUpper - change - 1, gridType.Vein, generalVeinDir, gameTiming.Null);
                    }
                    else
                    {
                        changeGridUnitProperties(x + (i * notFlat),      y - change - 1, gridType.Vein, generalVeinDir, gameTiming.Null);
                        changeGridUnitProperties(x - (i * notFlat), yUpper + change + 1, gridType.Vein, generalVeinDir, gameTiming.Null);
                    }
                    width--;
                }
            }
            

            if (i > width) break;

            yPrev = y;
            yUpperPrev = yUpper;
            y = (int)Mathf.Floor((float)i * widthSlope) + yStart;
            yUpper = ((int)Mathf.Floor((float)i * widthSlope) * -1) + yStart;

            // If the widthSlope is extreme we will have a really wide vien, need to trim that here
            // Ex: slope = .01, widthSlope = -100
            if (Mathf.Abs(y - yPrev) > (width - i)){
                if (widthSlope < 0f)
                {
                    y = yPrev - (width - i);
                    yUpper = yUpperPrev + (width - i);
                }
                else
                {
                    y = yPrev + (width - i);
                    yUpper = yUpperPrev - (width - i);
                }
            }
        }
    }

    void calculateSlopeAndDirection(ref int xStart, ref int yStart, ref int xDir, ref int yDir, ref float slope, ref veinDirection dir)
    {
        //Find the direction
        if (xStart < xDir || xStart == xDir)
        {
            dir = veinDirection.Right;
        }
        else
        {
            dir = veinDirection.Left;
        }
        
        // Find the slope
        int xChange = xDir - xStart;
        int yChange = yDir - yStart;

        slope = (float)yChange / (float)xChange;
        
        if (slope > maxSlope)
        {
            if (Mathf.Abs(xStart - xDir) > 30)
            {
                slope = maxNonVerticalSlope - .05f;
            }
            else
            {
                slope = maxSlope;
            }
        }
        else if (slope < -maxSlope)
        {
            if (Mathf.Abs(xStart - xDir) > 30)
            {
                slope = -maxNonVerticalSlope + .05f;
            }
            else
            {
                slope = -maxSlope;
            }
        }
        else if (-.05f < slope && slope < .05f)
        {
            slope = .01f;
        }
        //Debug.Log("Slope: " + slope);
    }

    void handleDistanceState(int xChange, int yChange, ref int totalDistanceTraveled, ref int totalDistance, ref distanceTraveled state, ref bool justChangedStates)
    {
        totalDistanceTraveled = totalDistanceTraveled + (int)Mathf.Floor(Mathf.Sqrt((xChange * xChange) + (yChange * yChange)));
        //Debug.Log(xChange + "," + yChange);
        //Debug.Log("Distance: " + totalDistanceTraveled + "   totalDistance: " + totalDistance);
        if (state == distanceTraveled.none)
        {
            if (totalDistanceTraveled >= (totalDistance / 6))
            {
                state = distanceTraveled.one_sixths;
                justChangedStates = true;
            }
            else
            {
                justChangedStates = false;
            }
        }
        else if (state == distanceTraveled.one_sixths)
        {
            if (totalDistanceTraveled >= (2 * (totalDistance / 6)))
            {
                state = distanceTraveled.two_sixths;
                justChangedStates = true;
            }
            else
            {
                justChangedStates = false;
            }
        }
        else if (state == distanceTraveled.two_sixths)
        {
            if (totalDistanceTraveled >= (3 * (totalDistance / 6)))
            {
                state = distanceTraveled.three_sixths;
                justChangedStates = true;
            }
            else
            {
                justChangedStates = false;
            }
        }
        else if (state == distanceTraveled.three_sixths)
        {
            if (totalDistanceTraveled >= (4 * (totalDistance / 6)))
            {
                state = distanceTraveled.four_sixths;
                justChangedStates = true;
            }
            else
            {
                justChangedStates = false;
            }
        }
        else if (state == distanceTraveled.four_sixths)
        {
            if (totalDistanceTraveled >= (5 * (totalDistance / 6))) 
            {
                state = distanceTraveled.five_sixths;
                justChangedStates = true;
            }
            else
            {
                justChangedStates = false;
            }
        }
        else if (state == distanceTraveled.five_sixths)
        {
            
            justChangedStates = false;
            
        }
        
    }

    void changeSlope(slopeChange slopeChangeType, ref float slope, ref veinDirection dir)
    {
        float[] fivePointSevenDegrees = new float[] { .1f, .17f, .2f, .49f, .78f, 1.17f, 1.71f, 2.4f };

        float halfChange = slopeChangeType == slopeChange.IncreaseMinus ? 1f : 1f;

        float incOrDec = 0f;
        if (slopeChangeType == slopeChange.IncreasePlus || slopeChangeType == slopeChange.Increase || slopeChangeType == slopeChange.IncreaseMinus)
        {
            incOrDec = 1f;
        }
        else
        {
            incOrDec = -1f;
        }

        //Debug.Log("Pre Dir: " + dir);
        //Debug.Log("Pre Slope: " + slope);
        if (Mathf.Abs(slope) < .5f)
        {
            slope = slope + (fivePointSevenDegrees[0] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 1f)
        {
            slope = slope + (fivePointSevenDegrees[1] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 1.5f)
        {
            slope = slope + (fivePointSevenDegrees[2] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 2f)
        {
            slope = slope + (fivePointSevenDegrees[3] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 2.5f)
        {
            slope = slope + (fivePointSevenDegrees[4] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 3f)
        {
            slope = slope + (fivePointSevenDegrees[5] * (incOrDec ));
        }
        else if (Mathf.Abs(slope) < 3.5f)
        {
            slope = slope + (fivePointSevenDegrees[6] * (incOrDec ));

            /*if (Mathf.Abs(slope) < maxSlope)
            {
                slope = maxSlope * incOrDec;
            }*/
        }
        else if (Mathf.Abs(slope) < 4f)
        {
            slope = slope + (fivePointSevenDegrees[7] * (incOrDec ));
            /*if (Mathf.Abs(slope) < maxSlope)
            {
                slope = maxSlope * incOrDec;
            }*/
        }
        /*else if (Mathf.Abs(slope) < maxNonVerticalSlope)
        {
            slope = maxNonVerticalSlope * incOrDec;
            //slope = slope + (fivePointSevenDegrees[7] * incOrDec);
        }*/
        else if (Mathf.Abs(slope) < maxSlope)
        {
            if (slope > 0)
            {
                if (incOrDec > 0)
                {
                    slope = maxSlope;
                }
                else
                {
                    slope = slope + (fivePointSevenDegrees[7] * incOrDec);
                }
                
            }
            else
            {
                if (incOrDec < 0)
                {
                    slope = -maxSlope;
                }
                else
                {
                    slope = slope + (fivePointSevenDegrees[7] * incOrDec);
                }
                //slope = maxSlope * incOrDec;
            }
            //slope = slope + (fivePointSevenDegrees[7] * incOrDec);
        }
        else
        {          
            if (slope > 0)
            {
                // If our slope is above 4.9
                slope = slope + (fivePointSevenDegrees[6] * incOrDec); //+ .1f;
            }
            else if (slope < 0)
            {
                // If our slope is below -4.9
                slope = slope + (fivePointSevenDegrees[6] * incOrDec);// - .1f;
            }
            else
            {
                slope = slope + (fivePointSevenDegrees[6] * incOrDec);
            }

        }

        //Debug.Log("Mid Dir: " + dir);
        //Debug.Log("Mid Slope: " + slope);

        if (slope > 0)
        {
            if (slope > maxSlope)
            {
                slope = -maxSlope + Mathf.Abs(slope - maxSlope);
                if (dir == veinDirection.Right)
                {
                    dir = veinDirection.Left;
                }
                else
                {
                    dir = veinDirection.Right;
                }
            }
        }
        else if (slope < 0)
        {
            if (slope < -maxSlope)
            {
                slope = maxSlope - Mathf.Abs(slope + maxSlope);
                if (dir == veinDirection.Right)
                {
                    dir = veinDirection.Left;
                }
                else
                {
                    dir = veinDirection.Right;
                }
            }
        }

        //Debug.Log("Dir Change: " + dir);
        //Debug.Log("Slope Change: " + slope);
    }

    void handleSlopeChanges(ref distanceTraveled state, ref bool justChangedStates, ref float slope, ref float intendedSlope, ref veinDirection dir)
    {
        // Change the slope randomly every 1/6 distance traveled
        // The last state happens way more quickly than intended(we decrease newDistance), so ignore that state
        if (justChangedStates && state != distanceTraveled.five_sixths)
        {
            float equal = .4f;
            float increase = equal;
            float decrease = equal;
            float keep = 1f - (2f * equal);

            if (slope == intendedSlope)
            {
                // No change
            }
            else if (slope > intendedSlope)
            {
                //decrease = decrease + (float)((slope - intendedSlope) / intendedSlope);
                //if (decrease > (2 * equal)) {
                //    decrease = 2f * equal;
                //}
                //increase = (2f * equal) - decrease;
                if (intendedSlope < 0 && slope > 0)
                {
                    decrease = equal - .05f;
                    increase = equal + .05f;
                }
                else
                {
                    decrease = equal + .05f;
                    increase = equal - .05f;
                }
                
            }
            else if (slope < intendedSlope)
            {
                /*increase = increase + (float)(slope / intendedSlope);
                if (increase > (2 * equal))
                {
                    increase = 2f * equal;
                }
                decrease = (2f * equal) - increase;
                */
                if (intendedSlope > 0 && slope < 0)
                {
                    decrease = equal + .05f;
                    increase = equal - .05f;
                }
                else
                {
                    decrease = equal - .05f;
                    increase = equal + .05f;
                }               
            }
            //Debug.Log("Dec: " + decrease + "   Keep: " + keep + "    Inc: " + increase);

            int random = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                        new randomProbabilityManagerScript.RandomSelection(-1, -1, decrease),
                        new randomProbabilityManagerScript.RandomSelection( 0,  0, keep),
                        new randomProbabilityManagerScript.RandomSelection( 1,  1, increase));

            //Debug.Log("======= Before Slope: " + slope);
            if (random == -1)
            {
                changeSlope(slopeChange.Decrease, ref slope, ref dir);
            }
            else if (random == 1)
            {
                changeSlope(slopeChange.Increase, ref slope, ref dir);
            }
            //Debug.Log("======= Slope: " + slope);
        }

    }
   

    void handleWidthChanges(ref int width, ref int prevWidth, ref int approxWidth, ref int x, ref int y, ref veinDirection dir, 
                                           ref float decreaseWidthPercent, ref float keepWidthPercent, ref float increaseWidthPercent)
    {
        // Changes the width of the vein, recenters the vein based on the new width, and changes the percentages based on new width

        int widthChange = Mathf.Abs(width - approxWidth);
        //Debug.Log("width: " + width + "widthchange: " + widthChange);
        if (width <= minWidth)
        {
            //Debug.Log("width: " + width);
            //Debug.Log("width IN");
            keepWidthPercent = .65f;
            increaseWidthPercent = .35f;
            decreaseWidthPercent = .00f;
        }
        else if (width == approxWidth)
        {
            // Keep width percentages
            keepWidthPercent = .50f;
            increaseWidthPercent = .25f;
            decreaseWidthPercent = .25f;
        }
        else if (width < approxWidth)
        {
            // Want increaseWidthPercent to increase
            if (widthChange >= 2)
            {
                keepWidthPercent = .60f;
                increaseWidthPercent = .30f;
                decreaseWidthPercent = .10f;
            }
            else if (widthChange >= 4)
            {
                keepWidthPercent = .35f;
                increaseWidthPercent = .60f;
                decreaseWidthPercent = .05f;
            }
        }
        else if (width > approxWidth)
        {
            // Want decreaseWidthPercent to increase
            
            
            
            /*if (widthChange >= 8)
            {
                keepWidthPercent = .40f;
                increaseWidthPercent = .025f;
                decreaseWidthPercent = .60f;
            }
            else*/ if (widthChange >= 8)
            {
                keepWidthPercent = .50f;
                increaseWidthPercent = .15f;
                decreaseWidthPercent = .35f;
            }
            else if (widthChange >= 4)
            {
                keepWidthPercent = .50f;
                increaseWidthPercent = .20f;
                decreaseWidthPercent = .30f;
            }
            else if (widthChange >= 2)
            {
                keepWidthPercent = .50f;
                increaseWidthPercent = .25f;
                decreaseWidthPercent = .25f;
            }
            

        }
        //Debug.Log("Keep: " + keepWidthPercent + "  inc: " + increaseWidthPercent + "  dec: " + decreaseWidthPercent);
        int random = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                    new randomProbabilityManagerScript.RandomSelection(-2, -2, decreaseWidthPercent),
                    new randomProbabilityManagerScript.RandomSelection(0, 0, keepWidthPercent),
                    new randomProbabilityManagerScript.RandomSelection(2, 2, increaseWidthPercent));

        prevWidth = width;
        width = width + random;
    }

    void changeGridUnitZone(int x, int y, int zone)
    {
        if (0 <= x && x < xAxis && 0 <= y && y < yAxis)
        {
            grid[x, y].GetComponent<gridUnitScript>().zoneArea.Add(zone);
        }
    }

    void changeGridUnitZoneAndAbilities(int x, int y, ZoneUnitProperties zoneProperties)
    {
        if (0 <= x && x < xAxis && 0 <= y && y < yAxis)
        {
            grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(zoneProperties);
        }
    }

    void changeGridUnitIntendedType(int x, int y, int zoneId, intendedUnitType intendedZone)
    {
        if (0 <= x && x < xAxis && 0 <= y && y < yAxis)
        {
            switch (intendedZone)
            {
                case intendedUnitType.Zone:
                    grid[x, y].GetComponent<gridUnitScript>().intendedType = intendedZone;
                    grid[x, y].GetComponent<gridUnitScript>().intendedZoneId = zoneId;

                    break;

                case intendedUnitType.Vein:
                    grid[x, y].GetComponent<gridUnitScript>().intendedType = intendedZone;

                    break;
            }
            
        }
    }

    void changeGridUnitProperties(int x, int y, gridType type, GeneralVeinDirection generalVeinDir, gameTiming timing)
    {
        gridUnitScript.GeneralVeinDirection dir = gridUnitScript.GeneralVeinDirection.Left;

        switch (generalVeinDir)
        {
            case GeneralVeinDirection.Left:
                dir = gridUnitScript.GeneralVeinDirection.Left;
                break;
            case GeneralVeinDirection.Right:
                dir = gridUnitScript.GeneralVeinDirection.Right;
                break;
            case GeneralVeinDirection.Up:
                dir = gridUnitScript.GeneralVeinDirection.Up;
                break;
            case GeneralVeinDirection.Down:
                dir = gridUnitScript.GeneralVeinDirection.Down;
                break;
        }

        changeGridUnitProperties(x, y, type, dir, timing);
    }

    void changeGridUnitProperties(int x, int y, gridType type, gridUnitScript.GeneralVeinDirection dir, gameTiming timing)
    {
        if (0 <= x && x < xAxis && 0 <= y && y < yAxis)
        {
            ZoneUnitProperties newZoneAndAbilities = null;

            switch (type)
            {
                case gridType.Vein:
                    grid[x, y].GetComponent<gridUnitScript>().isVein = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;

                    changeGridUnitIntendedType(x, y, -1, intendedUnitType.Vein);
                    break;

                case gridType.VeinMain:
                    grid[x, y].GetComponent<gridUnitScript>().isVein = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    grid[x, y].GetComponent<gridUnitScript>().isVeinMain = true;
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;

                    changeGridUnitIntendedType(x, y, -1, intendedUnitType.Vein);
                    break;

                case gridType.POI:
                    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    xPOI.Add(x);
                    yPOI.Add(y);
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;

                    break;

                case gridType.POICoreSmall:
                    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICoreSmall = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                    xPOICore.Add(x);
                    yPOICore.Add(y);

                    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities); 

                    break;

                case gridType.POICoreMedium:
                    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICoreMedium = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                    xPOICore.Add(x);
                    yPOICore.Add(y);

                    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities);

                    break;

                case gridType.POICoreLarge:
                    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                    grid[x, y].GetComponent<gridUnitScript>().isPOICoreLarge = true;
                    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                    xPOICore.Add(x);
                    yPOICore.Add(y);

                    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities);

                    break;
            }


        }
    }

    public void getGridPosition(ref int x, ref int y, ref float xPos, ref float yPos)
    {
        // You subtract half the height to get the coordinates of the bottom left corner
        xPos = grid[x,y].transform.position.x - (gridWidth / 2);
        yPos = grid[x,y].transform.position.y - (gridHeight / 2);
    }


    public void getGridStartRoom(ref int x, ref int y)
    {
        x = gridStartX;
        y = gridStartY;
    }

    public void deleteUnusedGrids()
    {
        for (int x = 0; x < xAxis; x++)
        {
            for (int y = 0; y < yAxis; y++)
            {
                if (grid[x, y].GetComponent<gridUnitScript>().isUsed == false)
                {
                    Destroy(grid[x, y]); 
                }
            }
        }
    }
}
