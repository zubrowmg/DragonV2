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

        // Direction and momentum
        //      Momentum starts at 0 in any direction, has a percentage chance of changing direction after each increment. Once it hits the max then force direction change
        int maxMomentum = 3;
        float primaryDirectionPercentage = .65f;
        List<float> momentumPercentTable = new List<float> { .85f, .60f, .25f, .05f };
        List<Direction> primaryDir;
        List<Direction> secondaryDir;

        // Initial zone length
        int maxTrunkLength = 18;

        // Vein
        
        SetCoordsVein newVein;
        int veinWidth = 5;

        public ZoneVeinNavigationController(ref ZoneVeinGeneratorContainer zoneVeinGenContainerInst, ref GeneratorContainer contInst) : base(ref contInst)
        {
            this.zoneVeinGenContainer = zoneVeinGenContainerInst;
        }

        // =====================================================================================
        //                                    Init Functions
        // =====================================================================================

        public void init()
        {
            this.stateHistory = new ZoneVeinStateHistory();
            this.currentState.setCurrentCoords(new CoordsInt(0, 0));
            this.currentState.setPrevCoords(new CoordsInt(0, 0));
            this.currentState.setPrevDir(Direction.None);
            this.currentState.setCurrentDir(Direction.None);

            this.currentlyInRollBack = false;

            this.newVein = new SetCoordsVein(ref getContainerInst(), 0, new CoordsInt(0, 0), new CoordsInt(0, 0), false, false, false, this.veinWidth);

            setPrimaryAndSecondaryDir();
        }

        public void determineStartDirection()
        {
            // Randomly choose vertical or horizontal direction to start
            DirectionBias zoneDirBias = zoneVeinGenContainer.currentZone.getDirBias();
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
                Debug.LogError("ZoneVeinGenerator - determinStartDirection(): Start Direction has no direction to start in");
        }

        public void setPrimaryAndSecondaryDir()
        {
            primaryDir = new List<Direction>();
            secondaryDir = new List<Direction>();

            if (zoneVeinGenContainer.currentZone.getDirBias().getHorizontalDir() != Direction.None)
                primaryDir.Add(zoneVeinGenContainer.currentZone.getDirBias().getHorizontalDir());
            if (zoneVeinGenContainer.currentZone.getDirBias().getVerticalDir() != Direction.None)
                primaryDir.Add(zoneVeinGenContainer.currentZone.getDirBias().getVerticalDir());


            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None)
                    continue;

                if (primaryDir.Contains(dir) == false)
                    secondaryDir.Add(dir);
            }

        }

        // =====================================================================================
        //                              Create Zone Vein Functions
        // =====================================================================================

        public void createZoneVeinTrunk(CoordsInt startCoords)
        {
            this.currentState.setCurrentCoords(startCoords.deepCopyInt());

            determineStartDirection();

            bool trunkFinished = false;

            //this.currentCoords.print("Start Coords: ");
            this.currentState.setCurrentWorldCoords(zoneVeinGenContainer.getTileMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));
            this.stateHistory.addState(this.currentState.deepCopy());

            while (trunkFinished == false)
            {
                // Determine if the trunk is too long
                if (this.stateHistory.getLength() > maxTrunkLength)
                {
                    trunkFinished = true;
                    break;
                }

                // Travel one unit in the current direction
                this.currentState.setPrevCoords(this.currentState.getCurrentCoords());
                travelOneUnit(this.currentState.getCurrentDir());

                // Record the point
                this.currentState.setCurrentWorldCoords(zoneVeinGenContainer.getTileMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));

                // Decide on a new direction
                if (this.currentState.getCurrentDir() == this.currentState.getPrevDir() && this.currentState.getCurrentMomentum() < this.maxMomentum)
                    this.currentState.incCurrentMomentum();
                else if (this.currentState.getCurrentDir() != this.currentState.getPrevDir())
                    this.currentState.resetMomentum();

                // Determine the next/current direction
                this.currentState.setPrevDir(this.currentState.getCurrentDir());
                determineNewDirection();
                this.currentlyInRollBack = false;

                // Track the current state
                this.stateHistory.addState(this.currentState.deepCopy());

                // Clear any variables in current state that aren't easily overwritten
                //      Mainly lists
                this.currentState.clearRejectedDir();
            }

            // Add all recorded world coords from the state history
            newVein.addSetCoord(this.stateHistory.getListOfWorldCoords());

            // Create the vein once all points are choosen
            createVein();
        }

        void createVein()
        {
            newVein.triggerSetCoordsVeinGeneration();

            // Copy tiles from the new vein to the zone vein class
            List<Tile> newVeinTiles = newVein.getAssociatedTiles();
            zoneVeinGenContainer.currentVeinZone.addAssociatedTiles(ref newVeinTiles);
        }

        public void travelOneUnit(Direction dir)
        {
            //bool directionRejected = 
            goDir(dir);

            //if (directionRejected == true)
            //    Debug.LogError("ZoneVeinGenerator - travelOneUnit(): Direction rejected, what to do?");
        }

        public void goDir(Direction dir)
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
                    Debug.LogError("ZoneVeinGenerator - goDir(): Direction.None passed in");
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
                bool pointIsInsideBounds = zoneVeinGenContainer.tileMapConnections.isInsideBounds(attemptedCoord);
                if (pointIsInsideBounds == true)
                {
                    Double<TileTraveledToMarker, Tile> tileMapConnElement = zoneVeinGenContainer.tileMapConnections.getElement(attemptedCoord);
                    if (lockTileMapConn == true)
                        tileMapConnElement.getOne().incLockPass(zoneVeinGenContainer.currentVeinPass);
                    else
                        tileMapConnElement.getOne().decLockPass(zoneVeinGenContainer.currentVeinPass);
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
                bool pointIsInsideBounds = zoneVeinGenContainer.tileMapConnections.isInsideBounds(attemptedCoord);
                if (pointIsInsideBounds == true)
                {
                    Double<TileTraveledToMarker, Tile> tileMapConnElement = zoneVeinGenContainer.tileMapConnections.getElement(attemptedCoord);
                    if (locked == true)
                    {
                        if (tileMapConnElement.getOne().isPassLocked(zoneVeinGenContainer.currentVeinPass) == true)
                            dirCheck.Add(dir);
                    }
                    else
                    {
                        if (tileMapConnElement.getOne().isPassLocked(zoneVeinGenContainer.currentVeinPass) == false)
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

        public void rollBackState()
        {
            // Go to a previous state
            this.currentState = stateHistory.rollBackState(out bool rollBackedTooFar);
            //Debug.Log("\t\tROLLBACK 1");

            if (rollBackedTooFar == true)
            {
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
                    Debug.LogError("ZoneVeinGenerator - rollBackState(): ROLLBACKED TOO FAR 2");
                    return;
                }
            }
        }
    }
}
