using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedEnums;
using CommonlyUsedClasses;
using TileManagerClasses;
using VeinEnums;

namespace VeinManagerClasses
{
    public abstract class Vein : TileAccessor
    {
        // Test Vars
        public string name = "TEST____0";

        // Type
        protected VeinType veinType = VeinType.None_Set;

        // Direction
        protected Direction generalVeinDirection; // Not really used in calculations, only to help for debug
        protected VeinDirection intendedVeinDirection = VeinDirection.None_Set;
        protected Coords<int> startCoords;
        protected Coords<int> endCoords;

        // Width and Distance
        int minWidth = 4;
        int currentWidth;
        int approxWidth = 6;
        int approxDistance = 6;

        // Slope properties and percentages
        protected Slope veinSlope;
        protected Slope intendedVeinSlope;
        TargetProbabilityManager slopeTargetManager;


        // Vein varying properties
        protected bool varyVeinWidth = false;
        protected bool varyVeinLength = false;
        protected bool varyVeinSlope = false;

        // Vein properties used during vein creation time
        protected float currentDistance = 0f;
        protected Coords<int> prevCoords;
        protected Coords<int> currentCoords;

        protected VeinDirection currentVeinDirection = VeinDirection.None_Set;


        // List of Tiles
        List<Tile> associatedTiles = new List<Tile>();
        // List of Tile Bookmarks, Bookmarks are meant for future vein expansion



        VeinDistanceTraveled veinDistanceState = VeinDistanceTraveled.None;
        bool justChangedStates = false;



        void initGeneralProperties(Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                                bool varyWidth, bool varyLength, bool varySlope)
        {
            this.generalVeinDirection = generalDirection;
            this.startCoords = startCoords.deepCopy();
            this.endCoords = endCoords;
            this.veinSlope = new Slope(startCoords, endCoords);
            this.intendedVeinSlope = new Slope(startCoords, endCoords);

            this.varyVeinWidth = varyWidth;
            this.varyVeinLength = varyLength;
            this.varyVeinSlope = varySlope;

            this.prevCoords = startCoords.deepCopy();
            this.currentCoords = startCoords.deepCopy();

            this.slopeTargetManager = new TargetProbabilityManager(.1f, 
                                                                   veinSlope.getSlope(), 
                                                                   veinSlope.getSlope(), 
                                                                   new List<float> { .3f, .4f, .3f }, 
                                                                   new List<int> { -1, 0, 1 }, 
                                                                   1, 
                                                                   .5f);
        }

        public Vein(ref GeneratorContainer contInst, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                        bool varyWidth, bool varyLength, bool varySlope) : base(ref contInst)
        {
            initGeneralProperties(generalDirection, startCoords, endCoords, varyWidth, varyLength, varySlope);

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection(startCoords);
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxDistance = initVaryLength(varyLength, approxDistance);
        }

        // If you want to set width/distance
        public Vein(ref GeneratorContainer contInst,Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                        bool varyWidth, bool varyLength, bool varySlope, int width, int distance) : base(ref contInst)
        {
            initGeneralProperties(generalDirection, startCoords, endCoords, varyWidth, varyLength, varySlope);

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection(startCoords);
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

                //handleDistanceState(currentCoords, prevCoords, ref veinDistanceState, ref justChangedStates);
                prevCoords.setX(currentCoords.getX());

                // If we reach the distance goal, then stop here
                if (getCurrentDistance() > getDistanceGoal())
                {
                    break;
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

        public abstract void triggerVeinGeneration2();
        

        protected void changeSlopeEveryXDistance(ref int currentSlopeIndex, ref Coords<int> currentSlopeStartCoords, Coords<int> currentCoords, VeinDistanceTraveled distanceState)
        {
            bool slopeChanged = true;
            // Check if slope needs to be changed
            //if (distanceState == VeinDistanceTraveled.Three_Sixths)
            //{
                // First randomly decide if it needs to be changed
                slopeChanged = decideSlopeChanges();

                // If slope changes then reset these slope dependant variables
                if (slopeChanged)
                {
                    currentSlopeIndex = 0;
                    currentSlopeStartCoords = currentCoords.deepCopy();
                }
            //}
        }

        protected void handleWidthChanges()
        {
            // Changes the width of the vein, recenters the vein based on the new width, and changes the percentages based on new width
            float keepWidthPercent = .4f;
            float incWidthPercent = .2f;
            float decWidthPercent = .2f;

            int widthChange = Mathf.Abs(currentWidth - approxWidth);
            //Debug.Log("width: " + width + "widthchange: " + widthChange);
            if (currentWidth <= minWidth)
            {
                keepWidthPercent = .65f;
                incWidthPercent = .35f;
                decWidthPercent = .00f;
            }
            else if (currentWidth == approxWidth)
            {
                // Keep width percentages
                keepWidthPercent = .50f;
                incWidthPercent = .25f;
                decWidthPercent = .25f;
            }
            else if (currentWidth < approxWidth)
            {
                // Want increaseWidthPercent to increase
                if (widthChange >= 2)
                {
                    keepWidthPercent = .60f;
                    incWidthPercent = .30f;
                    decWidthPercent = .10f;
                }
                else if (widthChange >= 4)
                {
                    keepWidthPercent = .35f;
                    incWidthPercent = .60f;
                    decWidthPercent = .05f;
                }
            }
            else if (currentWidth > approxWidth)
            {
                // Want decreaseWidthPercent to increase
                if (widthChange >= 8)
                {
                    keepWidthPercent = .50f;
                    incWidthPercent = .15f;
                    decWidthPercent = .35f;
                }
                else if (widthChange >= 4)
                {
                    keepWidthPercent = .50f;
                    incWidthPercent = .20f;
                    decWidthPercent = .30f;
                }
                else if (widthChange >= 2)
                {
                    keepWidthPercent = .50f;
                    incWidthPercent = .25f;
                    decWidthPercent = .25f;
                }


            }
            int random = RandomProbability.getIntBasedOnPercentage(
                        new RandomProbability.RandomSelection(-2, -2, decWidthPercent),
                        new RandomProbability.RandomSelection(0, 0, keepWidthPercent),
                        new RandomProbability.RandomSelection(2, 2, incWidthPercent));

            currentWidth = currentWidth + random;
        }

        // The initial slope is the main target line, should randomly decide if we go back towards the target or not
        bool decideSlopeChanges()
        {
            bool slopeChanged = true;
            int slopeChange = slopeTargetManager.getControledRandomizedValue();

            if (slopeChange == -1)
                this.veinSlope.changeSlope(SlopeChange.Dec, ref currentVeinDirection);
            else if (slopeChange == 1)
                this.veinSlope.changeSlope(SlopeChange.Inc, ref currentVeinDirection);
            else
                slopeChanged = false;

            return slopeChanged;
        }

        protected Coords<int> calculateIndexToCoords(Coords<int> currentSlopeStartCoords, int currentSlopeIndex, float currentSlope, VeinDirection currentVeinDir)
        {
            //float currentSlope = veinSlope.getSlope();
            //VeinDirection currentVeinDir = getCurrentVeinDirection();
            int nextX = 0;
            int nextY = 0;

            //  If the direction is to the left then set the dirModifier to the left
            float dirModifier = 1f;
            if (currentVeinDir == VeinDirection.Left)
                dirModifier = -1f;

            // If the slope is less than or equal to one, we can treat slope index as X
            if (Mathf.Abs(currentSlope) <= 1f)
            {
                // If the direction is to the left then we need to adjust the y direction (up/down)
                float slopeModifier = 1f;
                if (currentSlope < 0f && currentVeinDir == VeinDirection.Left)
                    slopeModifier = -1f;
                else if (currentSlope > 0f && currentVeinDir == VeinDirection.Left)
                    slopeModifier = -1f;

                nextX = currentSlopeIndex * (int)dirModifier;
                nextY = Mathf.FloorToInt(currentSlopeIndex * currentSlope * slopeModifier);
            }
            // If the slope is greater than one, need to increment Y and calculate next X
            else
            {
                // Because currentSlope is not used in nextY (unlike above), manually set the slopeModifier
                float slopeModifier = 1f;
                if ((currentSlope < 0f && currentVeinDir == VeinDirection.Right) || (currentSlope > 0f && currentVeinDir == VeinDirection.Left))
                    slopeModifier = -1f;

                float choppedUpSlope = (float)(1f / currentSlope);
                nextX = Mathf.FloorToInt(currentSlopeIndex * Mathf.Abs(choppedUpSlope) * dirModifier);
                nextY = currentSlopeIndex * (int)slopeModifier;
            }

            nextX += currentSlopeStartCoords.getX();
            nextY += currentSlopeStartCoords.getY();
            Coords<int> nextCoords = new Coords<int>(nextX, nextY);

            return nextCoords;
        }

        protected void updatePosition(Coords<int> nextCoords, ref Coords<int> currentCoords, ref Coords<int> prevCoords, ref float currentDistance)
        {
            prevCoords = currentCoords.deepCopy();
            currentCoords = nextCoords.deepCopy();

            float xChange = (float)Mathf.Abs(currentCoords.getX() - prevCoords.getX());
            float yChange = (float)Mathf.Abs(currentCoords.getY() - prevCoords.getY());
            float distanceChange = Mathf.Sqrt((xChange * xChange) + (yChange * yChange));
            currentDistance = currentDistance + distanceChange;

        }


        // Uses same while loop logic used in triggerVeinGeneration()
        protected void createVeinStrip(Coords<int> currentCoords)
        {
            // Mark the middle "main" vein
            markTileAsVein(currentCoords, DebugVeinTileType.VeinMain);

            // Width properties
            float halfWidth = (float)currentWidth / 2f;
            halfWidth++;
            float widthSlope = ((float)-1 / veinSlope.getSlope());

            Coords<int> startCoords = currentCoords.deepCopy();

            float currentDistanceLeft = 0f;
            float currentDistanceRight = 0f;

            Coords<int> nextCoordsLeft = currentCoords.deepCopy();
            Coords<int> nextCoordsRight = currentCoords.deepCopy();

            Coords<int> currentCoordsLeft = currentCoords.deepCopy();
            Coords<int> currentCoordsRight = currentCoords.deepCopy();

            Coords<int> prevCoordsLeft = currentCoords.deepCopy();
            Coords<int> prevCoordsRight = currentCoords.deepCopy();

            int widthIndex = 0;

            bool hitDistanceGoalLeft = false;
            bool hitDistanceGoalRight = false;
            while (hitDistanceGoalLeft == false || hitDistanceGoalRight == false)
            {
                if (hitDistanceGoalLeft && hitDistanceGoalRight)
                    break;

                if (hitDistanceGoalLeft == false)
                {
                    updatePosition(nextCoordsLeft, ref currentCoordsLeft, ref prevCoordsLeft, ref currentDistanceLeft);
                    if (currentDistanceLeft > halfWidth)
                    {
                        hitDistanceGoalLeft = true;
                        goto MidLoop;
                    }

                    // Mark a tiles as a vein
                    markTilesAsVeinAroundPoint(currentCoordsLeft, DebugVeinTileType.Vein);

                    // Calculate next coords
                    nextCoordsLeft = calculateIndexToCoords(startCoords, widthIndex, widthSlope, VeinDirection.Left);
                }
            MidLoop:

                if (hitDistanceGoalRight == false)
                {
                    updatePosition(nextCoordsRight, ref currentCoordsRight, ref prevCoordsRight, ref currentDistanceRight);
                    if (currentDistanceRight > halfWidth)
                    {
                        hitDistanceGoalRight = true;
                        goto EndLoop;
                    }

                    // Mark a tiles as a vein
                    markTilesAsVeinAroundPoint(currentCoordsRight, DebugVeinTileType.Vein);

                    // Calculate next coords
                    nextCoordsRight = calculateIndexToCoords(startCoords, widthIndex, widthSlope, VeinDirection.Right);
                }
            EndLoop:


                widthIndex++;
            }
        }

        void markTilesAsVeinAroundPoint(Coords<int> coords, DebugVeinTileType veinType)
        {
            markTileAsVein(coords, veinType);
            markTileAsVein(new Coords<int>(coords.getX() - 1, coords.getY()), veinType);
            markTileAsVein(new Coords<int>(coords.getX() + 1, coords.getY()), veinType);
            markTileAsVein(new Coords<int>(coords.getX(), coords.getY() - 1), veinType);
            markTileAsVein(new Coords<int>(coords.getX(), coords.getY() + 1), veinType);
        }

        void createStripOld(Coords<int> currentCoords)
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
                if (associatedTiles.Contains(selectedTile) == false)
                {
                    associatedTiles.Add(selectedTile);
                }

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

        public VeinDirection calculateCurrentVeinDirection(Coords<int> currentCoords)
        {
            VeinDirection veinDirection = VeinDirection.None_Set;

            if (currentCoords.getX() < endCoords.getX() || currentCoords.getX() == endCoords.getX())
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

        protected float getCurrentDistance()
        {
            return this.currentDistance;
        }

        public void setCurrentDistance(int newDistance)
        {
            this.currentDistance = newDistance;
        }

        protected int getDistanceGoal()
        {
            return this.approxDistance;
        }

        public float getVeinSlope()
        {
            return veinSlope.getSlope();
        }

        public ref List<Tile> getAssociatedTiles()
        {
            return ref associatedTiles;
        }

        public void setVeinType(VeinType type)
        {
            this.veinType = type;
        }
    }
}
