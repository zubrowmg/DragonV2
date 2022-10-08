using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedDefinesAndEnums;
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
        int veinId = CommonDefines.DefualtId;

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

        // List of Vein Connections, Connections are meant for future vein expansion
        float veinConnectionCounter = 0;
        float defualtVeinConnectionDistance = 30;
        float actualVeinConnectionDistance = 30;
        List<VeinConnection> listOfVeinConnections = new List<VeinConnection>();

        // Distance state variables
        VeinDistanceTraveled veinDistanceState = VeinDistanceTraveled.None;
        bool justChangedStates = false;

        // =========================================================================
        //                            End Variables
        // =========================================================================
        

        void initGeneralProperties(int id, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                                bool varyWidth, bool varyLength, bool varySlope)
        {
            this.veinId = id;
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

        public Vein(ref GeneratorContainer contInst, int id, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                        bool varyWidth, bool varyLength, bool varySlope) : base(ref contInst)
        {
            initGeneralProperties(id, generalDirection, startCoords, endCoords, varyWidth, varyLength, varySlope);

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection(startCoords);
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxDistance = initVaryLength(varyLength, approxDistance);

            configVeinConnectionDistance();

        }

        // If you want to set width/distance
        public Vein(ref GeneratorContainer contInst, int id, Direction generalDirection, Coords<int> startCoords, Coords<int> endCoords,
                        bool varyWidth, bool varyLength, bool varySlope, int width, int distance) : base(ref contInst)
        {
            initGeneralProperties(id, generalDirection, startCoords, endCoords, varyWidth, varyLength, varySlope);

            this.currentWidth = approxWidth;
            this.currentVeinDirection = calculateCurrentVeinDirection(startCoords);
            this.intendedVeinDirection = this.currentVeinDirection;

            this.approxWidth = width;
            this.approxDistance = initVaryLength(varyLength, distance);

            configVeinConnectionDistance();

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

        void configVeinConnectionDistance()
        {
            
            // If the vein is really small then there is no reason to put a connection in
            if (getDistanceGoal() < defualtVeinConnectionDistance)
                actualVeinConnectionDistance = int.MaxValue;
            // If the vein is slightly bigger than small then put only one connection in the middle
            else if (getDistanceGoal() < defualtVeinConnectionDistance * 2f)
                actualVeinConnectionDistance = getDistanceGoal() / 2f;
            // else leave the actualVeinConnectionDistance as the default amount

            //Debug.Log("GOAL: " + getDistanceGoal() + "\n" + "DEFUALT: " + defualtVeinConnectionDistance + "\n" + "ACTUAL: " + actualVeinConnectionDistance);
        }

        // ===================================================================================================
        //                               Vein Creation Functions
        // ===================================================================================================

        protected void addNewVeinConnection(ref Tile associatedTile)
        {
            VeinConnection newConnector = new VeinConnection(ref associatedTile);
            newConnector.addVeinLink(this);
            listOfVeinConnections.Add(newConnector);
        }

        protected void handleMiddleVeinConnections(float currentDistance, float nextDistance)
        {
            veinConnectionCounter = veinConnectionCounter + calculateDifference(currentDistance, nextDistance);
            float approxDistanceLeft = calculateDifference(getDistanceGoal(), getCurrentDistance());
           
            // Don't place connections leading up to the end of the vein
            if (approxDistanceLeft < actualVeinConnectionDistance)
            {
                // nothing
            }
            // Place vein connection every x distance (x = actualVeinConnectionDistance)
            else if (veinConnectionCounter > actualVeinConnectionDistance)
            {
                placeVeinConnection(this.currentCoords);
                veinConnectionCounter = 0f;
            }
        }

        protected void placeVeinConnection(Coords<int> coords)
        {
            bool accessSuccessful = false;
            Tile selectedTile = getTile(coords, ref accessSuccessful);

            if (accessSuccessful)
                addNewVeinConnection(ref selectedTile);
        }

        public abstract void triggerVeinGeneration();

        protected void changeSlopeEveryXDistance(ref int currentSlopeIndex, ref Coords<int> currentSlopeStartCoords, Coords<int> currentCoords, VeinDistanceTraveled distanceState)
        {
            bool slopeChanged = true;
           
            // First randomly decide if it needs to be changed
            slopeChanged = decideSlopeChanges();

            // If slope changes then reset these slope dependant variables
            if (slopeChanged)
            {
                currentSlopeIndex = 0;
                currentSlopeStartCoords = currentCoords.deepCopy();
            }
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

        protected float calculateNewPosition(Coords<int> nextCoords, ref Coords<int> currentCoords, 
                                                    ref Coords<int> prevCoords, float currentDistance)
        {
            prevCoords = currentCoords.deepCopy();
            currentCoords = nextCoords.deepCopy();

            float xChange = calculateDifference(currentCoords.getX(), prevCoords.getX()); //(float)Mathf.Abs(currentCoords.getX() - prevCoords.getX());
            float yChange = calculateDifference(currentCoords.getY(), prevCoords.getY()); //(float)Mathf.Abs(currentCoords.getY() - prevCoords.getY()); //
            float distanceChange = Mathf.Sqrt((xChange * xChange) + (yChange * yChange));
            currentDistance = currentDistance + distanceChange;

            return currentDistance;
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
                    currentDistanceLeft = calculateNewPosition(nextCoordsLeft, ref currentCoordsLeft, ref prevCoordsLeft, currentDistanceLeft);
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
                    currentDistanceRight = calculateNewPosition(nextCoordsRight, ref currentCoordsRight, ref prevCoordsRight, currentDistanceRight);
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

        float calculateDifference(float numOne, float numTwo)
        {
            return (float)Mathf.Abs(numOne - numTwo);
        }

        // ===================================================================================================
        //                               Setters/Getters
        // ===================================================================================================

        public List<VeinConnection> getVeinConnections()
        {
            return this.listOfVeinConnections;
        }

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

        public void setCurrentDistance(float newDistance)
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
