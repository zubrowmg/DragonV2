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

    // !!!!!!!!!!!!!!!!!!!!!!!!
    //      At the end this class should export the end product as a vein class
    // !!!!!!!!!!!!!!!!!!!!!!!!

    Zone_New currentZone;

    // Tile Map Connections
    //       Double< CanTravelTo, Tile >
    TwoDList<Double<TileTraveledToMarker, Tile>> tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>(); // Allocated dims, but only the tiles spaced out every x amount

    // State and state tracker
    ZoneVeinState currentState = new ZoneVeinState();
    ZoneVeinStateHistory stateHistory = new ZoneVeinStateHistory();

    // Rollback
    bool currentlyInRollBack = false;

    // Zone Connection Node Generation
    int gapBetweenNodes = 9;

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
    VeinZone currentVeinZone;
    List<CoordsInt> setCoords;
    SetCoordsVein newVein;
    int veinWidth = 5;

    // Vein passes (Trunk = pass0, branches are pass1, 2, 3, etc)
    int currentVeinPass = 0;


    public ZoneVeinGenerator(ref GeneratorContainer contInst) : base(ref contInst)
    {
    }
    // =====================================================================================
    //                                     Main Function
    // =====================================================================================

    public VeinZone generateZoneVein(ref Zone_New zone, int veinId)
    {
        init(veinId, ref zone);


        // Creates a grid of vein connection nodes
        //      Also sets current coords to the start coords
        setupZoneConnectionNodes();

        

        createZoneVein();

        //exportVeinZoneValues();

        return currentVeinZone;
    }

    // =====================================================================================
    //                                   Setup Functions
    // =====================================================================================

    void init(int veinId, ref Zone_New zone)
    {
        this.currentVeinZone = new VeinZone(ref getContainerInst(), veinId, zone.getStartCoords());
        this.currentZone = zone;
        this.tileMapConnections = new TwoDList<Double<TileTraveledToMarker, Tile>>();
        this.setCoords = new List<CoordsInt>();

        this.stateHistory = new ZoneVeinStateHistory();
        this.currentState.setCurrentCoords(new CoordsInt(0, 0));
        this.currentState.setPrevCoords(new CoordsInt(0, 0));
        this.currentState.setPrevDir(Direction.None);
        this.currentState.setCurrentDir(Direction.None);

        this.currentlyInRollBack = false;


        this.newVein = new SetCoordsVein(ref getContainerInst(), 0, new CoordsInt(0, 0), new CoordsInt(0, 0), false, false, false, this.veinWidth);

        this.currentVeinPass = 0;

        setPrimaryAndSecondaryDir();
    }

    public void determineStartDirection()
    {
        // Randomly choose vertical or horizontal direction to start
        DirectionBias zoneDirBias = this.currentZone.getDirBias();
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

    // Don't want the Zone to generate one Tile at a time, need to setup Tile nodes that need to be the only destination points
    public void setupZoneConnectionNodes()
    {
        TwoDList<Tile> allocatedTileMap = this.currentZone.getTileMapRef(); // Entire allocated dimensions
        DimensionList allocatedDimList = this.currentZone.getVeinZoneDimList(); // Entire allocated dimensions list (0s and 1s)

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
                    tileMapConnections.addRefElement(newCoords, ref newElement);

                    // Get the point that is the closest to the zone start coords
                    CoordsInt adjustedCoords = allocatedDimList.getMinCoords().deepCopyInt();
                    adjustedCoords.incX(x);
                    adjustedCoords.incY(y);

                    float distance = CommonFunctions.calculateCoordsDistance(this.currentZone.getStartCoords(), adjustedCoords);
                    minDistance.addValueToQueue(distance, newCoords.deepCopyInt());

                    newCoords.incY();
                }
            }
            newCoords.incX();
            newCoords.setY(0);
        }

        this.currentZone.setVeinZoneConnectionList(ref this.tileMapConnections);

        // Set the current coords so that we start generating at the proper start
        startCoords = minDistance.getMinVal().Value;
        this.currentState.setCurrentCoords(startCoords.deepCopyInt());
    }

    public void setPrimaryAndSecondaryDir()
    {
        primaryDir = new List<Direction>();
        secondaryDir = new List<Direction>();

        if (currentZone.getDirBias().getHorizontalDir() != Direction.None)
            primaryDir.Add(currentZone.getDirBias().getHorizontalDir());
        if (currentZone.getDirBias().getVerticalDir() != Direction.None)
            primaryDir.Add(currentZone.getDirBias().getVerticalDir());


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

    public void createZoneVein()
    {
        createZoneVeinTrunk();

        // createBranches();
    }

    public void createZoneVeinTrunk()
    {
        determineStartDirection();

        bool trunkFinished = false;

        //this.currentCoords.print("Start Coords: ");
        this.currentState.setCurrentWorldCoords(getTileMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));
        this.stateHistory.addState(this.currentState.deepCopy());

        //newVein.addSetCoord(currentWorldCoords);

        Debug.Log("==============================");
        Debug.Log("        TRUNK START");
        Debug.Log("==============================");

        this.currentState.getCurrentCoords().print("START COORDS: ");

        while (trunkFinished == false)
        {
            Debug.Log("==========================================================\nTRUNK TOP\n==========================================================");

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
            this.currentState.setCurrentWorldCoords(getTileMapCoordsFromTileMapConns(this.currentState.getCurrentCoords()));
            //newVein.addSetCoord(currentWorldCoords);



            // Decide on a new direction
            if (this.currentState.getCurrentDir() == this.currentState.getPrevDir() && this.currentState.getCurrentMomentum() < this.maxMomentum)
                this.currentState.incCurrentMomentum();
            else if (this.currentState.getCurrentDir() != this.currentState.getPrevDir())
                this.currentState.resetMomentum();

            //Debug.Log("Prev Dir: " + prevDirection);
            this.currentState.setPrevDir(this.currentState.getCurrentDir());
            determineNewDirection();
            this.currentlyInRollBack = false;
            Debug.Log("SET ROLL BACK TO FALSE");
            Debug.Log("CURRENT DIRECTION: " + this.currentState.getCurrentDir());

            //Debug.Log("New Dir: " + currentDirection);



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
        this.currentVeinZone.addAssociatedTiles(ref newVeinTiles);
    }

    // =====================================================================================
    //                                     Navigation Functions
    // =====================================================================================

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

        // Check if the bounds are correct, also check if it can be traveled to
        //bool rejected = checkTileMapConnPoint(attemptedTileMapCoords);

        //if (rejected == false)
        //{
        Double<TileTraveledToMarker, Tile> attemptedTileMapConnElement = this.tileMapConnections.getElement(attemptedTileMapCoords);
        CoordsInt attemptedWorldTileMapCoords = attemptedTileMapConnElement.getTwo().getTileMapCoords();
        CoordsInt currentWorldTileMapCoords = this.tileMapConnections.getElement(currentCoordsCopy).getTwo().getTileMapCoords();

        // If the attempted to travel to coord is not exactly to the left in world coords, then reject
        //rejected = !checkGapDistance(attemptedWorldTileMapCoords, currentWorldTileMapCoords);
        checkGapDistance(attemptedWorldTileMapCoords, currentWorldTileMapCoords);
            //if (rejected == false)
                this.currentState.setCurrentCoords(attemptedTileMapCoords.deepCopyInt());
        //}

        // Don't allow travel to a location we have already traveled to
        //      Also don't want snaking for now, so don't allow travel to adjacent point FOR THE PREVIOUS POINT
        //          If you limit travel for the current point then you are immediatly cutting off travel for yourself
        bool lockTileMapConn = true;
        markTileMapPointsAroundCoord(this.currentState.getPrevCoords(), lockTileMapConn);
        //attemptedTileMapConnElement.setOne(false);
        

        //return rejected;
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
            bool pointIsInsideBounds = tileMapConnections.isInsideBounds(attemptedCoord);
            //attemptedCoord.print("\t\t\t LOCK: " + lockTileMapConn + "   COORDS: ");
            if (pointIsInsideBounds == true)
            {
                Double<TileTraveledToMarker, Tile> tileMapConnElement = this.tileMapConnections.getElement(attemptedCoord);
                if (lockTileMapConn == true)
                    tileMapConnElement.getOne().incLockPass(currentVeinPass);
                else
                    tileMapConnElement.getOne().decLockPass(currentVeinPass);
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

        Debug.Log("SELECT DIR");

        foreach (var dir in rejectedDirections)
        {
            Debug.Log("REJECTED DIR: " + dir);
        }

        foreach (var dir in primaryDir)
        {
            Debug.Log("PRIMARY DIR: " + dir);
        }

        foreach (var dir in secondaryDir)
        {
            Debug.Log("SECONDARY DIR: " + dir);
        }

        while (possibleDirections.Count == 0)
        {
            if (primaryDirSelected)
            {
                foreach (var dir in primaryDir)
                {
                    if (rejectedDirections.Contains(dir) == false)
                    {
                        Debug.Log("Adding Primary: " + dir);
                        possibleDirections.Add(dir);
                    }
                }

                if (possibleDirections.Count == 0)
                    primaryDirSelected = false;
            }
            else
            {
                foreach (var dir in secondaryDir)
                {
                    if (rejectedDirections.Contains(dir) == false)
                    {
                        Debug.Log("Adding Secondary: " + dir);
                        possibleDirections.Add(dir);
                    }
                }

                if (possibleDirections.Count == 0)
                    primaryDirSelected = true;
            }
        }

        return CommonFunctions.randomlySelectFromList(possibleDirections);
    }

    void determineNewDirection(List<Direction> rejectedDirections, bool dirForRollback)
    {
        // Permanently rejected dirs are directions that the current state absolutly cannot go to
        //      Ex Out of bounds, tile is locked
        List<Direction> permanentlyRejectedDir = new List<Direction>(rejectedDirections);

        // Reject directions that are locked
        bool locked = true;
        this.currentState.getCurrentCoords().print("\tCurrent Coords: ");
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

        Debug.Log("\tChange Direction Top ===================");
        this.currentState.getCurrentCoords().print("\tCurrent Coords: ");

        //foreach (var dir in rejectedDirections)
        //{
        //    Debug.Log("Rejected Start: " + dir);
        //}

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

            Debug.Log("\tATTEMPTED DIR: " + attempedDir);
            moveAccepted = isNextMoveValid(attempedDir);


            if (moveAccepted == true)
            {
                this.currentState.setCurrentDir(attempedDir);
                this.currentState.setRejectedDir(permanentlyRejectedDir);

                foreach (var dir in permanentlyRejectedDir)
                {
                    Debug.Log("\tFinal Rejected Dirs: " + dir);
                }
            }
            else if (moveAccepted == false)
            {
                Debug.Log("\tMOVE REJECTED: " + attempedDir);

                // If the current direction will lead into a wall, then change the direction
                changeDirection = true;

                CommonFunctions.addIfItemDoesntExist(ref rejectedDirections, attempedDir);
                CommonFunctions.addIfItemDoesntExist(ref permanentlyRejectedDir, attempedDir);

                
                // If all directions are rejected, then we need to rollback
                if (rejectedDirections.Count == 4)
                {
                    if (dirForRollback == true)
                        Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from Rollback");
                    else
                        Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from normal operation (not Rollback)");


                    //foreach (var dir in rejectedDirections)
                    //{
                    //    Debug.Log("Final Rejected Dirs: " + dir);
                    //}

                    // If this is the first iteration of a roll back, then the current state isn't recorded yet
                    //      We want to rollback from the current state, not the previous state which is recorded
                    if (this.currentlyInRollBack == false)
                    {
                        this.stateHistory.addState(this.currentState.deepCopy());
                    }
                    this.currentlyInRollBack = true;
                    Debug.Log("\tENTERING ROLLBACK FROM determineNewDir()");
                    //Debug.Log("\tSET ROLL BACK TO TRUE");
                    //Debug.Log("CURRENTLY IN ROLL BACK: " + this.currentlyInRollBack);

                    rollBackState();
                    break;
                }
                
            }
        }
    }


    // Check which directions are locked/not locked
    List<Direction> checkDirectionLockStatus(CoordsInt coords, bool locked)
    {
        List<Direction> dirCheck = new List<Direction>();
        CoordsInt attemptedCoord = coords.deepCopyInt();

        coords.print("\t\t\t\t\tCHECK DIR COORDS: ");

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

            attemptedCoord.print("\t\t\t\t\t DIR: " + dir + "CHECK DIR COORDS: ");


            // If point is inside the bounds then check if it can be traveled to
            bool pointIsInsideBounds = tileMapConnections.isInsideBounds(attemptedCoord);
            if (pointIsInsideBounds == true)
            {
                Double<TileTraveledToMarker, Tile> tileMapConnElement = this.tileMapConnections.getElement(attemptedCoord);
                if (locked == true)
                {
                    if (tileMapConnElement.getOne().isPassLocked(currentVeinPass) == true)
                    {
                        dirCheck.Add(dir);
                        attemptedCoord.print("DIR LOCKED: " + dir + " AT COORDS: ");
                    }

                }
                else
                {
                    if (tileMapConnElement.getOne().isPassLocked(currentVeinPass) == false)
                        dirCheck.Add(dir);
                }
            }
            else if(locked == true)
            {
                dirCheck.Add(dir);
                attemptedCoord.print("DIR OUT OF BOUNDS: " + dir + " AT COORDS: ");
            }

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
        bool accepted = !checkTileMapConnPoint(attemptedTileMapCoords);

        // If we are turning, check if the turn will lead into a dead end pocket
        //      Probably depricated, instead of checking we will rollback the state if we hit a dead end pocket
        //if (accepted == true && attempedDir != this.currentState.getPrevDir())
        //    accepted = !leadsToDeadEndPocket(attemptedTileMapCoords);

        return accepted;
    }

    // Checks for boundries and if the point cannot be traveled to
    bool checkTileMapConnPoint(CoordsInt coords)
    {
        // Check Boundry
        bool rejected = !tileMapConnections.isInsideBounds(coords);

        if (rejected == true)
            return rejected;

        Double<TileTraveledToMarker, Tile> attemptedTileMapConnElement = this.tileMapConnections.getElement(coords);

        // Tile has already been traveled to
        if (attemptedTileMapConnElement.getOne().isPassLocked(currentVeinPass) == true)
            rejected = true;

        return rejected;
    }

    // Checks if a turn will lead to a dead end pocket
    bool leadsToDeadEndPocket(CoordsInt coords)
    {
        bool leadsToDeadEnd = false;

        // Go in a straight line (in the attempted direction), till you hit a locked tile

        // Once the locked tile is hit, calculate the area of the enclosed space

        // If it's 25%-33% of the total area, then reject the direction

        // Doesn't work!!!!!!!!!!!!! Algoritm will never want to turn towards itself

        return leadsToDeadEnd;
    }

    public void rollBackState()
    {
        Debug.Log("\t\tROLLBACK______________TOP");
        this.currentState.getCurrentCoords().print("\tPRE ROLLBACK COORDS: ");

        // Go to a previous state
        this.currentState = stateHistory.rollBackState(out bool rollBackedTooFar);
        Debug.Log("\t\tROLLBACK 1");

        if (rollBackedTooFar == true)
        {
            Debug.LogError("ZoneVeinGenerator - rollBackState(): ROLLBACKED TOO FAR 1");
            return;
        }
        this.currentState.getCurrentCoords().print("\t\tPOST ROLLBACK COORDS: ");

        bool lockTileMapConn = false;
        markTileMapPointsAroundCoord(this.currentState.getCurrentCoords(), lockTileMapConn);

        // Get the rejected direction list from the recorded state
        //      Reject the direction of the next state, since that is where the path got stuck
        List<Direction> rejectedDirList = this.currentState.getRejectedDirList();
        Debug.Log("\t\tROLLBACK PREV DIRECTION: " + this.currentState.getPrevDir());
        Debug.Log("\tROLLBACK NEXT/CURRENT DIRECTION: " + this.currentState.getCurrentDir());
        if (rejectedDirList.Contains(this.currentState.getCurrentDir()) == false)
            rejectedDirList.Add(this.currentState.getCurrentDir());


        Debug.Log("\t\tREJECTED DIRS: ");
        foreach (var dir in rejectedDirList)
        {
            Debug.Log("\t\t" + dir);
        }

        // If all 4 directions are rejected
        if (rejectedDirList.Count == 4)
        {
            Debug.Log("\tENTERING ROLLBACK FROM rollBackState()");
            rollBackState();
        }
        // If they are not then attempt to find a new direction
        //      This else statement assumes that all directions have already been assessed. Aka it know if they are locked/unlocked
        //      checkDirectionLockStatus() at the top of determineNewDirection() handles this for us
        else
        {
            Debug.Log("\t\tPRE ROLLBACK 2");

            // Determine a new direction that can be attempted
            bool dirForRollback = true;
            determineNewDirection(rejectedDirList, dirForRollback);

            // Once determined, the current state in the state history is outdated. Needs to be popped off
            //      Main while loop will save the updated state
            stateHistory.rollBackState(out rollBackedTooFar);
            Debug.Log("\t\tROLLBACK 2");

            if (rollBackedTooFar == true)
            {
                Debug.LogError("ZoneVeinGenerator - rollBackState(): ROLLBACKED TOO FAR 2");
                return;
            }
        }
    }

    CoordsInt getTileMapCoordsFromTileMapConns(CoordsInt coords)
    {
        return tileMapConnections.getElement(coords).getTwo().getTileMapCoords();
    }

    bool checkGapDistance(CoordsInt coordsOne, CoordsInt coordsTwo)
    {
        bool gapDistanceIsGood = true;
        if (CommonFunctions.calculateCoordsDistance(coordsOne, coordsTwo) != (float)gapBetweenNodes)
            gapDistanceIsGood = false;

        return gapDistanceIsGood;
    }

    // =====================================================================================
    //                                     Setters/Getters
    // =====================================================================================
}
