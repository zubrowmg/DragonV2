using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using TileManagerClasses;
using CommonlyUsedFunctions;
using CommonlyUsedDefinesAndEnums;
using VeinManagerClasses;

namespace VeinManagerClasses
{
    public class ZoneVeinNavigationController : ContainerAccessor
    {
        ZoneVeinGeneratorContainer zoneVeinGenContainer;

        // State and state tracker
        ZoneVeinState currentState = new ZoneVeinState();
        ZoneVeinStateHistory stateHistory = new ZoneVeinStateHistory();

        // Rollback
        bool currentlyInRollBack = false;
        bool rolledBackTooFar = false;

        // Direction and momentum
        //      Momentum starts at 0 in any direction, has a percentage chance of changing direction after each increment. Once it hits the max then force direction change
        int maxMomentum = 3;
        float primaryDirectionPercentage = .65f;
        List<float> momentumPercentTable = new List<float> { .85f, .60f, .25f, .05f };

        DirectionBias currentDirectionBias;
        List<Direction> primaryDir;
        List<Direction> secondaryDir;

        // Edge Lengths
        int maxTrunkLength = 18;
        int maxEdgeLenth = 8;

        // Vein Controls
        SetCoordsVein newVein;
        int veinWidth = 5;

        public ZoneVeinNavigationController(ref ZoneVeinGeneratorContainer zoneVeinGenContainerInst, ref GeneratorContainer contInst) : base(ref contInst)
        {
            this.zoneVeinGenContainer = zoneVeinGenContainerInst;
        }

        // =====================================================================================
        //                                    Init Functions
        // =====================================================================================

        public void initTrunk()
        {
            // Typically the first edge created is the trunk, so just use the current zones dir bias as the defualt
            init(zoneVeinGenContainer.currentZone.getDirBias());
        }

        // Must be ran whenever a new edge is going to be ran
        void init(DirectionBias dirBias)
        {
            this.stateHistory.init();
            this.currentState.setCurrentCoords(new CoordsInt(0, 0));
            this.currentState.setPrevCoords(new CoordsInt(0, 0));
            this.currentState.setPrevDir(Direction.None);
            this.currentState.setCurrentDir(Direction.None);

            this.currentlyInRollBack = false;
            this.rolledBackTooFar = false;

            this.newVein = new SetCoordsVein(ref getContainerInst(), 0, new CoordsInt(0, 0), new CoordsInt(0, 0), false, false, false, this.veinWidth);

            this.currentDirectionBias = dirBias;
            setPrimaryAndSecondaryDir();
        }


        void determineTrunkStartDirection()
        {
            // Randomly choose vertical or horizontal direction to start
            DirectionBias zoneDirBias = this.currentDirectionBias;
            int rand = Random.Range(0, 2);

            if (rand == 0)
            {
                this.currentState.setCurrentDir(zoneDirBias.getHorizontalDir());
                if (this.currentState.getCurrentDir() == Direction.None)
                    this.currentState.setCurrentDir(zoneDirBias.getVerticalDir());

            }
            else
            {
                this.currentState.setCurrentDir(zoneDirBias.getVerticalDir());
                if (this.currentState.getCurrentDir() == Direction.None)
                    this.currentState.setCurrentDir(zoneDirBias.getHorizontalDir());
            }

            if (this.currentState.getCurrentDir() == Direction.None)
                Debug.LogError("ZoneVeinGenerator - determineTrunkStartDirection(): Start Direction has no direction to start in");
        }

        void determineBranchStartDirection()
        {
            // Needs to choose a start direction based on zone direction bias
            //      And if there is open space in that direction

            // foreach dir
            // {
            //this.zoneVeinGenContainer.tileMapConnCoordIsLocked__ForCurrentPass();
            // }


            if (this.currentState.getCurrentDir() == Direction.None)
                Debug.LogError("ZoneVeinGenerator - determineBranchStartDirection(): Start Direction has no direction to start in");
        }

        // Based on the current zones primary and secondary directions
        void setPrimaryAndSecondaryDir()
        {
            primaryDir = new List<Direction>();
            secondaryDir = new List<Direction>();

            if (this.currentDirectionBias.getHorizontalDir() != Direction.None)
                primaryDir.Add(this.currentDirectionBias.getHorizontalDir());
            if (this.currentDirectionBias.getVerticalDir() != Direction.None)
                primaryDir.Add(this.currentDirectionBias.getVerticalDir());


            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None)
                    continue;

                if (primaryDir.Contains(dir) == false)
                    secondaryDir.Add(dir);
            }

        }

        // =====================================================================================
        //                              Create Zone Vein Functions (Edge only)
        // =====================================================================================

        public List<CoordsInt> randomlyGenerateZoneVeinTrunk(CoordsInt startCoords, out bool edgeCreationFailed)
        {
            initTrunk();
            determineTrunkStartDirection();
            return createEdge(startCoords, maxTrunkLength, out edgeCreationFailed);
        }

        // This function is used if you have determined the start coord and second coord
        //      Typically the start coord is already on a tile that is a vein (locked tile)
        //          And the second coord is on a unlocked tile
        public List<CoordsInt> randomlyGenerateZoneVeinBranch(CoordsInt startCoords, CoordsInt nextCoords, DirectionBias dirBias, out bool edgeCreationFailed)
        {
            init(dirBias);

            // Need to determine a start direction
            // determineBranchStartDirection();

            // Going to add the start coord to the new vein, aka won't be able to roll back this coord
            // newVein.addSetCoord(startCoords);  // NEEDS TO BE THE WORLD COORD
            
            // Then use nextCoords as the actual start coords
            return createEdge(nextCoords, maxEdgeLenth, out edgeCreationFailed);
        }

        // Creates the trunk of the zone vein and returns the zone vein coords of the trunk
        private List<CoordsInt> createEdge(CoordsInt startCoords, int maxLength, out bool edgeCreationFailed)
        {
            this.currentState.setCurrentCoords(startCoords.deepCopyInt());

            bool edgeFinished = false;
            edgeCreationFailed = false;

            //this.currentCoords.print("Start Coords: ");
            this.currentState.setCurrentWorldCoords(zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));
            this.stateHistory.addState(this.currentState.deepCopy());

            while (edgeFinished == false)
            {
                // Determine if the edge is too long
                if (this.stateHistory.getLength() > maxLength)
                {
                    edgeFinished = true;
                    break;
                }

                // Travel one unit in the current direction
                this.currentState.setPrevCoords(this.currentState.getCurrentCoords());
                travelOneUnit(this.currentState.getCurrentDir());

                // Record the point
                this.currentState.setCurrentWorldCoords(zoneVeinGenContainer.getWorldMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));

                // Handle momentum
                if (this.currentState.getCurrentDir() == this.currentState.getPrevDir() && this.currentState.getCurrentMomentum() < this.maxMomentum)
                    this.currentState.incCurrentMomentum();
                else if (this.currentState.getCurrentDir() != this.currentState.getPrevDir())
                    this.currentState.resetMomentum();

                // Determine the next/current direction
                this.currentState.setPrevDir(this.currentState.getCurrentDir());
                determineNewDirection();
                this.currentlyInRollBack = false;

                if (this.rolledBackTooFar == true)
                {
                    edgeCreationFailed = true;
                    break;
                }
                // Track the current state
                this.stateHistory.addState(this.currentState.deepCopy());

                // Clear any variables in current state that aren't easily overwritten
                //      Mainly lists
                this.currentState.clearRejectedDir();
            }

            if (edgeCreationFailed == false)
            {
                // Add all recorded world coords from the state history and trigger vein generation
                newVein.addSetCoord(this.stateHistory.getListOfWorldCoords());
                createVein();
            }
            

            List<CoordsInt> listOfZoneVeinCoords = this.stateHistory.getListOfZoneVeinCoords();

            return listOfZoneVeinCoords;
        }

        void createVein()
        {
            newVein.triggerSetCoordsVeinGeneration();

            // Copy tiles from the new vein to the zone vein class
            List<Tile> newVeinTiles = newVein.getAssociatedTiles();
            zoneVeinGenContainer.currentVeinZone.addAssociatedTiles(ref newVeinTiles);
        }

        void travelOneUnit(Direction dir)
        {
            CoordsInt currentCoordsCopy = this.currentState.getCurrentCoords();
            CoordsInt attemptedTileMapCoords = currentCoordsCopy.deepCopyInt();

            switch (dir)
            {
                case Direction.North:
                    attemptedTileMapCoords.incY();
                    break;
                case Direction.East:
                    attemptedTileMapCoords.incX();
                    break;
                case Direction.South:
                    attemptedTileMapCoords.decY();
                    break;
                case Direction.West:
                    attemptedTileMapCoords.decX();
                    break;
                case Direction.None:
                    Debug.LogError("ZoneVeinGenerator - travelOneUnit(): Direction.None passed in");
                    break;
            }

            this.currentState.setCurrentCoords(attemptedTileMapCoords.deepCopyInt());

            // Don't allow travel to a location we have already traveled to
            //      Also don't want snaking for now, so don't allow travel to adjacent point FOR THE PREVIOUS POINT
            //          If you limit travel for the current point then you are immediatly cutting off travel for yourself
            bool lockTileMapConn = true;
            markTileMapPointsAroundCoord(this.currentState.getPrevCoords(), lockTileMapConn);
        }

        void markTileMapPointsAroundCoord(CoordsInt coords, bool lockTileMapConn)
        {
            CoordsInt attemptedCoord = coords.deepCopyInt();

            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                switch (dir)
                {
                    case Direction.North:
                        attemptedCoord.incY();
                        break;
                    case Direction.East:
                        attemptedCoord.incX();
                        break;
                    case Direction.South:
                        attemptedCoord.decY();
                        break;
                    case Direction.West:
                        attemptedCoord.decX();
                        break;
                    case Direction.None:
                        break; // Mark the current tile as well
                }

                // If point is inside the bounds then mark it as not travelable
                bool pointIsInsideBounds = zoneVeinGenContainer.coordsAreInsideTileMapBoundries(attemptedCoord);
                if (pointIsInsideBounds == true)
                {
                    if (lockTileMapConn == true)
                        zoneVeinGenContainer.incCurrentPassLock(attemptedCoord);
                    else
                        zoneVeinGenContainer.decCurrentPassLock(attemptedCoord);
                }

                attemptedCoord = coords.deepCopyInt();
            }
        }


        // Determine direction with no pre-determined rejected directions
        void determineNewDirection()
        {
            List<Direction> rejectedDirections = new List<Direction>();
            bool dirForRollback = false;
            determineNewDirection(rejectedDirections, dirForRollback);
        }

        void determineNewDirection(List<Direction> rejectedDirections, bool dirForRollback)
        {
            // Permanently rejected dirs are directions that the current state absolutly cannot go to
            //      Ex Out of bounds, tile is locked
            List<Direction> permanentlyRejectedDir = new List<Direction>(rejectedDirections);

            // Reject directions that are locked
            bool locked = true;
            List<Direction> lockedDir = checkDirectionLockStatus(this.currentState.getCurrentCoords(), locked);
            CommonFunctions.addIfItemDoesntExist(ref permanentlyRejectedDir, lockedDir);
            CommonFunctions.addIfItemDoesntExist(ref rejectedDirections, lockedDir);

            // Determine if we need to change directions
            //      Don't permanently reject this dir yet
            Direction attempedDir = this.currentState.getCurrentDir();
            bool changeDirection = determineIfDirNeedsToBeChanged(ref rejectedDirections, dirForRollback, attempedDir);

            // Select if the next direction should be based on the primary or secondary directions
            bool primaryDirSelected = detemineIfPrimaryDirShouldBeSelected();

            // No U turns
            Direction oppositeDir = CommonFunctions.getOppositeDir(this.currentState.getPrevDir());
            CommonFunctions.addIfItemDoesntExist(ref rejectedDirections, oppositeDir);
            CommonFunctions.addIfItemDoesntExist(ref permanentlyRejectedDir, oppositeDir);

            // Determine which direction should be changed to
            //      Make sure the trunk doesn't run over itself or run out of bounds
            bool moveAccepted = false;
            while (moveAccepted == false)
            {
                // If all directions are rejected, attempt to find any direction that works
                if (rejectedDirections.Count == 4)
                    rejectedDirections = permanentlyRejectedDir;

                if (changeDirection == true && rejectedDirections.Count != 4)
                    attempedDir = selectDirBasedOnRejectedDir(rejectedDirections, ref primaryDirSelected);

                moveAccepted = isNextMoveValid(attempedDir);

                if (moveAccepted == true)
                {
                    this.currentState.setCurrentDir(attempedDir);
                    this.currentState.setRejectedDir(permanentlyRejectedDir);
                }
                else if (moveAccepted == false)
                {
                    // If the current direction will lead into a wall, then change the direction
                    changeDirection = true;

                    CommonFunctions.addIfItemDoesntExist(ref rejectedDirections, attempedDir);
                    CommonFunctions.addIfItemDoesntExist(ref permanentlyRejectedDir, attempedDir);

                    // If all directions are rejected, then we need to rollback
                    if (rejectedDirections.Count == 4)
                    {
                        // Not actual errors, leaving for future debuging purposes
                        /*if (dirForRollback == true)
                            Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from Rollback");
                        else
                            Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from normal operation (not Rollback)");
                        */

                        // If this is the first iteration of a roll back, then the current state isn't recorded yet
                        //      We want to rollback from the current state, not the previous state which is recorded
                        if (this.currentlyInRollBack == false)
                            this.stateHistory.addState(this.currentState.deepCopy());
                        this.currentlyInRollBack = true;

                        rollBackState();
                        break;
                    }

                }
            }
        }

        bool determineIfDirNeedsToBeChanged(ref List<Direction> rejectedDirections, bool dirForRollback, Direction attempedDir)
        {
            float randFloat = Random.Range(0f, 1f);
            bool changeDirection = false;

            // Momentum Check
            if (randFloat >= momentumPercentTable[this.currentState.getCurrentMomentum()]/* && dirForRollback == false*/)
            {
                changeDirection = true;
                CommonFunctions.addIfItemDoesntExist(ref rejectedDirections, this.currentState.getCurrentDir());
            }
            // If the current dir is rejected, then we have to change directions
            else if (rejectedDirections.Contains(attempedDir) == true)
                changeDirection = true;

            // When rolling back you want change direction to be set, so that the algorithm searches all possible directions
            if (dirForRollback == true)
                changeDirection = true;

            return changeDirection;
        }

        bool detemineIfPrimaryDirShouldBeSelected()
        {
            bool primaryDirSelected = false;
            float randFloat = Random.Range(0f, 1f);
            if (randFloat >= primaryDirectionPercentage)
                primaryDirSelected = true;

            return primaryDirSelected;
        }

        Direction selectDirBasedOnRejectedDir(List<Direction> rejectedDirections, ref bool primaryDirSelected)
        {
            List<Direction> possibleDirections = new List<Direction>();

            while (possibleDirections.Count == 0)
            {
                if (primaryDirSelected)
                {
                    foreach (var dir in primaryDir)
                    {
                        if (rejectedDirections.Contains(dir) == false)
                            possibleDirections.Add(dir);
                    }

                    if (possibleDirections.Count == 0)
                        primaryDirSelected = false;
                }
                else
                {
                    foreach (var dir in secondaryDir)
                    {
                        if (rejectedDirections.Contains(dir) == false)
                            possibleDirections.Add(dir);
                    }

                    if (possibleDirections.Count == 0)
                        primaryDirSelected = true;
                }
            }

            return CommonFunctions.randomlySelectFromList(possibleDirections);
        }

        // Check which directions are locked/not locked
        List<Direction> checkDirectionLockStatus(CoordsInt coords, bool locked)
        {
            List<Direction> dirCheck = new List<Direction>();
            CoordsInt attemptedCoord = coords.deepCopyInt();

            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None)
                    continue;

                switch (dir)
                {
                    case Direction.North:
                        attemptedCoord.incY();
                        break;
                    case Direction.East:
                        attemptedCoord.incX();
                        break;
                    case Direction.South:
                        attemptedCoord.decY();
                        break;
                    case Direction.West:
                        attemptedCoord.decX();
                        break;
                }

                // If point is inside the bounds then check if it can be traveled to
                bool pointIsInsideBounds = zoneVeinGenContainer.coordsAreInsideTileMapBoundries(attemptedCoord);
                if (pointIsInsideBounds == true)
                {
                    if (locked == true)
                    {
                        if (zoneVeinGenContainer.tileMapConnCoordIsLocked__ForCurrentPass(attemptedCoord) == true)
                            dirCheck.Add(dir);
                    }
                    else
                    {
                        if (zoneVeinGenContainer.tileMapConnCoordIsLocked__ForCurrentPass(attemptedCoord) == false)
                            dirCheck.Add(dir);
                    }
                }
                else if (locked == true)
                    dirCheck.Add(dir);

                attemptedCoord = coords.deepCopyInt();
            }

            return dirCheck;
        }


        // Check if the next direction will hit the edge, also check if a change of directions will lead into a dead end pocket
        bool isNextMoveValid(Direction attempedDir)
        {
            CoordsInt attemptedTileMapCoords = this.currentState.getCurrentCoords().deepCopyInt();

            switch (attempedDir)
            {
                case Direction.North:
                    attemptedTileMapCoords.incY();
                    break;
                case Direction.East:
                    attemptedTileMapCoords.incX();
                    break;
                case Direction.South:
                    attemptedTileMapCoords.decY();
                    break;
                case Direction.West:
                    attemptedTileMapCoords.decX();
                    break;
                case Direction.None:
                    Debug.LogError("ZoneVeinGenerator - goDir(): Direction.None passed in");
                    break;
            }

            // Check if the bounds are correct, also check if it can be traveled to
            bool accepted = !zoneVeinGenContainer.checkTileMapConnPoint(attemptedTileMapCoords);

            return accepted;
        }

        void rollBackState()
        {
            // Go to a previous state
            this.currentState = stateHistory.rollBackState(out bool rollBackedTooFar);
            //Debug.Log("\t\tROLLBACK 1");

            if (rollBackedTooFar == true)
            {
                this.rolledBackTooFar = true;
                Debug.LogError("ZoneVeinGenerator - rollBackState(): ROLLBACKED TOO FAR 1");
                return;
            }

            bool lockTileMapConn = false;
            markTileMapPointsAroundCoord(this.currentState.getCurrentCoords(), lockTileMapConn);

            // Get the rejected direction list from the recorded state
            //      Reject the direction of the next state, since that is where the path got stuck
            List<Direction> rejectedDirList = this.currentState.getRejectedDirList();
            if (rejectedDirList.Contains(this.currentState.getCurrentDir()) == false)
                rejectedDirList.Add(this.currentState.getCurrentDir());

            // If all 4 directions are rejected
            if (rejectedDirList.Count == 4)
            {
                rollBackState();
            }
            // If they are not then attempt to find a new direction
            //      This else statement assumes that all directions have already been assessed. Aka it know if they are locked/unlocked
            //      checkDirectionLockStatus() at the top of determineNewDirection() handles this for us
            else
            {
                // Determine a new direction that can be attempted
                bool dirForRollback = true;
                determineNewDirection(rejectedDirList, dirForRollback);

                // Once determined, the current state in the state history is outdated. Needs to be popped off
                //      Main while loop will save the updated state
                stateHistory.rollBackState(out rollBackedTooFar);

                if (rollBackedTooFar == true)
                {
                    this.rolledBackTooFar = true;
                    Debug.LogError("ZoneVeinGenerator - rollBackState(): ROLLBACKED TOO FAR 2");
                    return;
                }
            }
        }

        // =====================================================================================
        //                           DiGraph Controller Help Functions
        // =====================================================================================

        

        public CoordsInt findEmptySpaceCoord(out bool foundFreeSpace, out TwoDList<Tile> tileMapConnections_JustTile)
        {
            // Uses dimVeinZoneCreator to find empty space in a restricted tile map dimension (The one that the zone is restricted to)
            //      Does not check all locations in the tile map, does a "lazy job". Coords checked are based on the size of allocated tile map for the zone
            float overlapThreshold = .6f;

            // Get the "lazy" coords
            List<CoordsInt> reducedTileMapConnCoordsList = zoneVeinGenContainer.getTileMapConnReducedCoordsList();
            tileMapConnections_JustTile = new TwoDList<Tile>();
            for (int x = 0; x < zoneVeinGenContainer.getTileMapConnX(); x++)
            {
                for (int y = 0; y < zoneVeinGenContainer.getTileMapConnY(); y++)
                {
                    CoordsInt accessCoord = new CoordsInt(x, y);
                    tileMapConnections_JustTile.addRefElement(accessCoord, ref zoneVeinGenContainer.getTileFromTileMapConn(accessCoord));
                }
            }

            // Restrict the search area conditions, these shouldn't be touched all willy nilly
            int maxTotalSearchArea = 9;
            int minTotalSearchArea = 4;

            // Once coords to check are calculated we search for free space
            //      Valid zone free areas need to be above the minimum search area, aka don't think that a "small" vein free pocket should have a new edge inside it
            //      Also don't want areas that overlap more than 60% to both be valid, as it will skew the random chances. Instead of 1:1:1 ratio chance it becomes 2:1 if 2 areas are overlapping
            List<DimensionList> freeAreas = new List<DimensionList>();
            foreach (var reducedStartCoords in reducedTileMapConnCoordsList)
            {
                DimensionList newEmptySpace = 
                    this.zoneVeinGenContainer.dimVeinZoneCreator.getDimensionsUsingAlternateTileMap(reducedStartCoords, this.zoneVeinGenContainer.debugMode, maxTotalSearchArea, ref tileMapConnections_JustTile);

                // If the area is acceptable
                if (newEmptySpace.getArea() >= minTotalSearchArea)
                {
                    if (freeAreas.Count == 0)
                        freeAreas.Add(newEmptySpace);
                    else
                    {
                        // Then make sure that it doesn't have a big overlap
                        //      It's important that we are checking the new empty spaces overlap percentage
                        //      AKA want newEmptySpace.checkOverlapPercent(area) and not area.checkOverlapPercent(newEmptySpace)
                        //      These two are not the same
                        bool newEmptySpaceOverlaps = false;
                        foreach (var area in freeAreas)
                        {
                            float percentOverlap = newEmptySpace.checkOverlapPercent(area);
                            if (percentOverlap > overlapThreshold)
                            {
                                newEmptySpaceOverlaps = true;
                                break;
                            }
                        }
                        if (newEmptySpaceOverlaps == false)
                            freeAreas.Add(newEmptySpace);
                    }
                }
            }

            //Debug.Log("NEW SPACE COUNT: " + freeAreas.Count);

            // Randomly select one of the free areas
            DimensionList choosenFreeArea = null;
            foundFreeSpace = true;
            if (freeAreas.Count == 0)
                foundFreeSpace = false;
            else
                choosenFreeArea = CommonFunctions.randomlySelectInList(ref freeAreas);

            // !!!!!!!!!!!!
            //choosenFreeArea.getCenterCoord().print("\tCENTER COORD: ");
            //zoneVeinGenContainer.currentZone.freeSpaces.Add(choosenFreeArea); // Debuging only, remove when done
            // !!!!!!!!!!!!
            
            return choosenFreeArea.getCenterCoord();
        }

    }
}
