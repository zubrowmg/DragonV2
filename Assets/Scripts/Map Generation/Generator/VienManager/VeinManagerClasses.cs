using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public class VeinContainer
    {
        Dictionary<Tile, Vein> tileToVeinLookUp;
    }

    public class Vein : TileAccessor
    {
        // Test Vars
        public string name = "TEST____0";

        // Type
        VeinType veinType = VeinType.None_Set;

        // Direction
        Direction generalVeinDirection; // Not really used in calculations, only to help for debug
        VeinDirection intendedVeinDirection = VeinDirection.None_Set;
        Slope veinSlope;
        Coords<int> startCoords;
        Coords<int> endCoords;

        // Width and Distance
        int approxWidth = 6;
        int approxDistance = 6;

        // Vein varying properties
        bool varyVeinWidth = false;
        bool varyVeinLength = false;
        bool varyVeinSlope = false;

        // Vein properties used during vein creation time
        int currentWidth;
        int currentDistance = 0;
        VeinDirection currentVeinDirection = VeinDirection.None_Set;


        // List of Tiles
        List<Tile> associatedTiles = new List<Tile>();
        // List of Tile Bookmarks, Bookmarks are meant for future vein expansion


        // triggerVeinGeneration Variables
        float keepWidthPercent = .50f;
        float increaseWidthPercent = .25f;
        float decreaseWidthPercent = .25f;

        VeinDistanceTraveled veinDistanceState = VeinDistanceTraveled.None;
        bool justChangedStates = false;


        

        bool exitLoop = false;

        public Vein(ref GeneratorContainer contInst, VeinType type, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords, 
                        Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope) : base(ref contInst)
        {
            this.veinType = type;

            this.generalVeinDirection = generalDirection;
            this.startCoords = startCoords;
            this.endCoords = endCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection();
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxDistance = initVaryLength(varyLength, approxDistance);
        }

        public Vein(ref GeneratorContainer contInst, VeinType type, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords, 
                        Slope veinSlope, bool varyWidth, bool varyLength, bool varySlope, int width, int distance) : base(ref contInst)
        {
            this.veinType = type;

            this.generalVeinDirection = generalDirection;
            this.startCoords = startCoords;
            this.endCoords = endCoords;
            this.veinSlope = veinSlope;

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection();
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxWidth = width;
            this.approxDistance = initVaryLength(varyLength, distance);
        }

        int initVaryLength(bool varyLength, int distance)
        {
            if (varyLength == false)
            {
                return distance;
            }
            else
            {
                return distance + RandomProbability.getIntBasedOnPercentage(
                                    new RandomProbability.RandomSelection(0, 25, .25f),
                                    new RandomProbability.RandomSelection(26, 45, .75f),
                                    new RandomProbability.RandomSelection(46, 65, .0f));
            }
        }

        // ===================================================================================================
        //                               Vein Creation Functions
        // ===================================================================================================
        public void triggerVeinGeneration()
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

            Coords<int> prevCoords = new Coords<int>(getStartCoords().getX(), getStartCoords().getY());
            Coords<int> currentCoords = new Coords<int>(getStartCoords().getX(), getStartCoords().getY());

            //int width = approxWidth;
            int prevWidth = getCurrentWidth();
            int xBreakOut = getStartCoords().getX(); // Will probably depricate breakout
            int yBreakOut = getStartCoords().getY();

            for (int i = 0; i < getDistanceGoal(); i++)
            {

                handleDistanceState(currentCoords, prevCoords, ref veinDistanceState, ref justChangedStates);
                prevCoords.setX(currentCoords.getX());

                // If we reach the distance goal, then stop here
                if (getCurrentDistance() > getDistanceGoal())
                {
                    break;
                }

                if (getVeinType() == VeinType.Simple)
                {
                    simpleVein(ref i, ref currentCoords, ref prevCoords, startCoords, ref exitLoop, ref xBreakOut, ref yBreakOut);
                    if (exitLoop) break;
                }
                //else if (vein == veinType.U)
                //{
                //    handleUVeinSlope(ref distance, ref newDistance, ref slope, ref isUpDown, ref intendedSlope, ref dir, intendedDir,
                //                        shiftStart, ref partWidths, totalUParts, totalSections, lowerBoundries, upperBoundries);
                //    simpleVein(ref i, ref x, ref y, ref yPrev, ref yStart, ref newDistance, ref width, ref slope, ref exitLoop, ref dir, ref xBreakOut, ref yBreakOut, intendedDir, generalVeinDir);
                //}

                // Varies the width every cycle
                if (varyVeinWidth)
                {
                    //handleWidthChanges(ref width, ref prevWidth, ref approxWidth, ref x, ref y, ref dir, ref decreaseWidthPercent, ref keepWidthPercent, ref increaseWidthPercent);
                }

                // Varies the slope when varySlope every sixth of the distance
                // Only for simple veins
                if (varyVeinSlope && getVeinType() != VeinType.U)
                {
                    //handleSlopeChanges(ref state, ref justChangedStates, ref slope, ref intendedSlope, ref dir);
                }

                // Every 25 units of distance, we put down a split vein, then a vein template at the end and mark it as POI
                //if (splitVeins)
                //{
                    //handleSplitVein(ref state, ref justChangedStates, x, yPrev, xStart, yStart, slope, generalVeinDir);
               // }


                // Every 1/6 of the total distance, there is some variance introduced to the bias percents

            }
        }

        void simpleVein(ref int i, ref Coords<int> currentCoords, ref Coords<int> prevCoords, Coords<int> startCoords, ref bool exitLoop, ref int xBreakOut, ref int yBreakOut)
        {
            int yTempi;
            int yTempiMinus;
            int x = currentCoords.getX();
            int y = currentCoords.getY();

            if (i > getDistanceGoal())
            {
                //Debug.Log("OUT: ");
                exitLoop = true;
                return;
            }

            Coords<int> index = new Coords<int>(x, y);
            createStrip(currentCoords);

            //Debug.Log("CURRENT: " + currentCoords.getX() + "," + currentCoords.getY());
            //Debug.Log("PREV   : " + prevCoords.getX() + "," + prevCoords.getY());


            // Fill in the gaps of extreme slopes
            for (int change = 0; change < (Mathf.Abs(y - prevCoords.getY())); change++)
            {
                //Debug.Log("GAP FILLER SLOPE: " + getVeinSlope());

                if (getVeinSlope() > 0f)
                {

                    if (getCurrentVeinDirection() == VeinDirection.Right)
                    {
                        index = new Coords<int>(x, y - change - 1);
                        //Debug.Log("GAP FILLER 0: " + index.getX() + "," + index.getY());
                        createStrip(index);
                    }
                    else
                    {
                        index = new Coords<int>(x, y + change + 1);
                        //Debug.Log("GAP FILLER 1: " + index.getX() + "," + index.getY());
                        createStrip(index);
                    }
                }
                else
                {
                    if (getCurrentVeinDirection() == VeinDirection.Right)
                    {
                        index = new Coords<int>(x, y + change + 1);
                        createStrip(index);
                    }
                    else
                    {
                        index = new Coords<int>(x, y - change - 1);
                        createStrip(index);
                    }
                }
                //newDistance--;
            }
            xBreakOut = currentCoords.getX();
            yBreakOut = currentCoords.getY();
            prevCoords.setY(currentCoords.getY());

            if (getCurrentVeinDirection() == VeinDirection.Right)
            {
                if (-veinSlope.getMaxNonVerticalSlope() < veinSlope.getSlope() && veinSlope.getSlope() < veinSlope.getMaxNonVerticalSlope())
                {
                    currentCoords.setX(currentCoords.getX() + 1);
                }

                // In case the slope changes we have to chang y based on the difference of the previous y
                yTempiMinus = (int)Mathf.Floor((float)(i - 1) * veinSlope.getSlope()) + startCoords.getY();
                yTempi = (int)Mathf.Floor((float)i * veinSlope.getSlope()) + startCoords.getY();

                currentCoords.setY(currentCoords.getY() + (yTempi - yTempiMinus));
            }
            else
            {
                if (-veinSlope.getMaxNonVerticalSlope() < veinSlope.getSlope() && veinSlope.getSlope() < veinSlope.getMaxNonVerticalSlope())
                {
                    currentCoords.setX(currentCoords.getX() - 1);
                }

                // In case the slope changes we have to chang y based on the difference of the previous y
                yTempiMinus = (int)Mathf.Floor((float)(i - 1) * veinSlope.getSlope() * -1) + startCoords.getY();
                yTempi = (int)Mathf.Floor((float)i * veinSlope.getSlope() * -1) + startCoords.getY();

                currentCoords.setY(currentCoords.getY() + (yTempi - yTempiMinus));
            }



            // If the slope is extreme we will have a really tall vien, need to trim that here
            // Ex: slope = .01, widthSlope = -100
            if (Mathf.Abs(currentCoords.getY() - prevCoords.getY()) > (getDistanceGoal() - i))
            {
                if (getVeinSlope() < 0f)
                {
                    if (getCurrentVeinDirection() == VeinDirection.Right)
                        currentCoords.setY(prevCoords.getY() - (getDistanceGoal() - i));
                    else
                        currentCoords.setY(prevCoords.getY() + (getDistanceGoal() - i));

                }
                else
                {
                    if (getCurrentVeinDirection() == VeinDirection.Right)
                        currentCoords.setY(prevCoords.getY() + (getDistanceGoal() - i));
                    else
                        currentCoords.setY(prevCoords.getY() - (getDistanceGoal() - i));
                }
            }
        }


        public void triggerVeinGeneration2()
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

            Coords<int> prevCoords = new Coords<int>(getStartCoords().getX(), getStartCoords().getY());
            Coords<int> currentCoords = new Coords<int>(getStartCoords().getX(), getStartCoords().getY());

            //int width = approxWidth;
            int prevWidth = getCurrentWidth();
            int xBreakOut = getStartCoords().getX(); // Will probably depricate breakout
            int yBreakOut = getStartCoords().getY();

            bool hitDistanceGoal = false;
            int currentSlopeIndex = 0; // Keeps count of how many (Vein Main) points were ploted with the current slope. Resets to 0 when slope changes

            while (hitDistanceGoal == false)
            {
                createStrip(currentCoords);
                currentCoords = calculateIndexToCoords(currentCoords, currentSlopeIndex);

                // Calculate distance that next coord would put the vein at
                //      If it goes over the distance goal end while loop here
                currentSlopeIndex++;
                if (currentSlopeIndex > 110)
                    break;
            }
        }

        Coords<int> calculateIndexToCoords(Coords<int> currentCoords, int currentSlopeIndex)
        {
            float currentSlope = veinSlope.getSlope();
            int nextX = 0;
            int nextY = 0;

            if (currentSlope <= 1f)
            {
                // If the slope is less than or equal to one, we can treat slope index as X
                nextX++;
                nextY = Mathf.FloorToInt(currentSlopeIndex * currentSlope);
            }
            else
            {
                // If the slope is greater than one, need to increment Y and calculate next X
                float choppedUpSlope = (float)(1f / currentSlope);
                nextX = Mathf.FloorToInt(currentSlopeIndex * choppedUpSlope);
                nextY++;
            }

            nextX += currentCoords.getX();
            nextY += currentCoords.getY();
            Coords<int> nextCoords = new Coords<int>(nextX, nextY);

            return nextCoords;
        }

        void markCurrentVeinPosition()
        {

        }

        void createStrip(Coords<int> currentCoords)
        {

            int halfWidth = currentWidth / 2;
            halfWidth++;

            int notFlat = System.Convert.ToInt32(true);
            float widthSlope = ((float)-1 / veinSlope.getSlope());

            if (-.15f < veinSlope.getSlope() && veinSlope.getSlope() < .15f)
            {
                notFlat = System.Convert.ToInt32(false);
            }

            int x = currentCoords.getX();
            int y = currentCoords.getY();
            int yPrev = y;
            int yStart = y;
            int yUpper = y;
            int yUpperPrev = y;

            markTileAsVein(new Coords<int>(x, y), DebugVeinTileType.VeinMain);

            for (int i = 0; i < halfWidth; i++)
            {
                markTileAsVein(new Coords<int>(x + (i * notFlat), y), DebugVeinTileType.Vein);
                markTileAsVein(new Coords<int>(x - (i * notFlat), yUpper), DebugVeinTileType.Vein);

                // Fill in the gaps of extreme slopes
                if (Mathf.Abs(y - yPrev) > 1)
                {
                    for (int change = 0; change < (Mathf.Abs(y - yPrev)); change++)
                    {

                        if (widthSlope < 0f)
                        {
                            markTileAsVein(new Coords<int>(x + (i * notFlat), y + change + 1), DebugVeinTileType.Vein);
                            markTileAsVein(new Coords<int>(x - (i * notFlat), yUpper - change - 1), DebugVeinTileType.Vein);
                        }
                        else
                        {
                            markTileAsVein(new Coords<int>(x + (i * notFlat), y - change - 1), DebugVeinTileType.Vein);
                            markTileAsVein(new Coords<int>(x - (i * notFlat), yUpper + change + 1), DebugVeinTileType.Vein);
                        }
                        halfWidth--;
                    }
                }


                if (i > halfWidth) break;

                yPrev = y;
                yUpperPrev = yUpper;
                y = (int)Mathf.Floor((float)i * widthSlope) + yStart;
                yUpper = ((int)Mathf.Floor((float)i * widthSlope) * -1) + yStart;

                // If the widthSlope is extreme we will have a really wide vien, need to trim that here
                // Ex: slope = .01, widthSlope = -100
                if (Mathf.Abs(y - yPrev) > (halfWidth - i))
                {
                    if (widthSlope < 0f)
                    {
                        y = yPrev - (halfWidth - i);
                        yUpper = yUpperPrev + (halfWidth - i);
                    }
                    else
                    {
                        y = yPrev + (halfWidth - i);
                        yUpper = yUpperPrev - (halfWidth - i);
                    }
                }
            }
        }

        void markTileAsVein(Coords<int> index, DebugVeinTileType type)
        {
            //ZoneUnitProperties newZoneAndAbilities = null;
            bool accessSuccessful = false;
            Tile selectedTile = getTile(index, ref accessSuccessful);

            if (accessSuccessful == true)
            {
                associatedTiles.Add(selectedTile);

                switch (type)
                {
                    case DebugVeinTileType.Vein:
                        selectedTile.setTileAsVein(this);
                        break;

                    case DebugVeinTileType.VeinMain:
                        selectedTile.setTileAsVeinMain(this);

                        break;

                        //case gridType.POI:
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                        //    xPOI.Add(x);
                        //    yPOI.Add(y);
                        //    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;

                        //    break;

                        //case gridType.POICoreSmall:
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICoreSmall = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                        //    xPOICore.Add(x);
                        //    yPOICore.Add(y);

                        //    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                        //    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities);

                        //    break;

                        //case gridType.POICoreMedium:
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICoreMedium = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                        //    xPOICore.Add(x);
                        //    yPOICore.Add(y);

                        //    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                        //    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities);

                        //    break;

                        //case gridType.POICoreLarge:
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOI = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICore = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isPOICoreLarge = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().isUsed = true;
                        //    grid[x, y].GetComponent<gridUnitScript>().veinDirection = dir;
                        //    xPOICore.Add(x);
                        //    yPOICore.Add(y);

                        //    newZoneAndAbilities = GlobalDefines.themeAndAbilityManager.getNewZone(timing);
                        //    grid[x, y].GetComponent<gridUnitScript>().zoneProperties.Add(newZoneAndAbilities);

                        //    break;
                }
            }
            
        }

        void handleDistanceState(Coords<int> currentCoords, Coords<int> prevCoords, ref VeinDistanceTraveled state, ref bool justChangedStates)
        {
            // Calculate how far this vein has traveled
            int xChange = Mathf.Abs(currentCoords.getX() - prevCoords.getX());
            int yChange = Mathf.Abs(currentCoords.getY() - prevCoords.getY());
            int newTraveledDistance = getCurrentDistance() + (int)Mathf.Floor(Mathf.Sqrt((xChange * xChange) + (yChange * yChange)));
            setCurrentDistance(newTraveledDistance);

            //Debug.Log(xChange + "," + yChange);
            //Debug.Log("Distance: " + totalDistanceTraveled + "   totalDistance: " + totalDistance);
            if (state == VeinDistanceTraveled.None)
            {
                if (getCurrentDistance() >= (getDistanceGoal() / 6))
                {
                    state = VeinDistanceTraveled.One_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.One_Sixths)
            {
                if (getCurrentDistance() >= (2 * (getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Two_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Two_Sixths)
            {
                if (getCurrentDistance() >= (3 * (getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Three_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Three_Sixths)
            {
                if (getCurrentDistance() >= (4 * (getDistanceGoal() / 6)))
                {
                    state = VeinDistanceTraveled.Four_Sixths;
                    justChangedStates = true;
                }
                else
                    justChangedStates = false;
            }
            else if (state == VeinDistanceTraveled.Four_Sixths)
            {
                if (getCurrentDistance() >= (5 * (getDistanceGoal() / 6)))
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

        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================
        public VeinDirection getIntendedVeinDirection()
        {
            return this.intendedVeinDirection;
        }

        public VeinDirection getCurrentVeinDirection()
        {
            return this.currentVeinDirection;
        }

        public VeinType getVeinType()
        {
            return this.veinType;
        }

        public VeinDirection calculateCurrentVeinDirection()
        {
            VeinDirection veinDirection = VeinDirection.None_Set;

            if (startCoords.getX() < endCoords.getX() || startCoords.getX() == endCoords.getX())
            {
                veinDirection = VeinDirection.Right;
            }
            else
            {
                veinDirection = VeinDirection.Left;
            }

            return veinDirection;
        }

        public Coords<int> getStartCoords()
        {
            return startCoords;
        }

        public Coords<int> getEndCoords()
        {
            return endCoords;
        }

        public int getCurrentWidth()
        {
            return currentWidth;
        }

        public int getCurrentDistance()
        {
            return this.currentDistance;
        }

        public void setCurrentDistance(int newDistance)
        {
            this.currentDistance = newDistance;
        }

        public int getDistanceGoal()
        {
            return approxDistance;
        }

        public float getVeinSlope()
        {
            return veinSlope.getSlope();
        }

        public ref List<Tile> getAssociatedTiles()
        {
            return ref associatedTiles;
        }
    }

    public class Slope
    {
        // Hard coded limits
        float maxSlope = 5f;
        float maxNonVerticalSlope = 4.9f;

        int xChange;
        int yChange;

        float slopeFloat;

        public Slope()
        {
            this.xChange = 1;
            this.yChange = 1;
            this.slopeFloat = 1f;
        }

        public Slope(Coords<int> startDestination, Coords<int> endDestination)
        {
            this.xChange = endDestination.getX() - startDestination.getX();
            this.yChange = endDestination.getY() - startDestination.getY();
            this.slopeFloat = calculateSlope(xChange, yChange, startDestination, endDestination);
        }

        // Calculates slope, also if slope is beyond limits this will fix that
        public float calculateSlope(int xChange, int yChange, Coords<int> startDestination, Coords<int> endDestination)
        {
            float slope = (float)this.yChange / (float)this.xChange;

            if (slope > maxSlope)
            {
                if (Mathf.Abs(startDestination.getX() - endDestination.getX()) > 30)
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
                if (Mathf.Abs(startDestination.getX() - endDestination.getX()) > 30)
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

            return slope;
        }


        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================
        public int getXChange()
        {
            return xChange;
        }

        public int getYChange()
        {
            return yChange;
        }

        public float getSlope()
        {
            return slopeFloat;
        }

        public float getMaxSlope()
        {
            return maxSlope;
        }

        public float getMaxNonVerticalSlope()
        {
            return maxNonVerticalSlope;
        }
    }
}
