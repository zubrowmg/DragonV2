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
    //CoordsInt currentCoords = new CoordsInt(0, 0);
    //CoordsInt prevCoords = new CoordsInt(0, 0);

    // State and state tracker
    ZoneVeinState currentState = new ZoneVeinState();
    ZoneVeinStateHistory stateHistory = new ZoneVeinStateHistory();

    // Zone Connection Node Generation
    int gapBetweenNodes = 9;

    // Direction and momentum
    //      Momentum starts at 0 in any direction, has a percentage chance of changing direction after each increment. Once it hits the max then force direction change
    //Direction currentDirection;
    //Direction prevDirection;
    int maxMomentum = 3;
    //int currentMomentum = 0;
    float primaryDirectionPercentage = .65f;
    List<float> momentumPercentTable = new List<float> { .85f, .60f, .25f, .05f };
    List<Direction> primaryDir;
    List<Direction> secondaryDir;

    // Initial zone length
    int maxTrunkLength = 12;

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

        int currentLength = 0;
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
            //this.currentCoords.print("Current Coords: ");

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
            //Debug.Log("New Dir: " + currentDirection);

            // Determine if the trunk is too long
            currentLength++;
            if (currentLength >= maxTrunkLength)
                trunkFinished = true;

            // Track the current state
            this.stateHistory.addState(this.currentState.deepCopy());
            //Debug.Log("====================================================");

            // break;
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
        markTileMapPointsAroundCoord(this.currentState.getPrevCoords());
        //attemptedTileMapConnElement.setOne(false);
        

        //return rejected;
    }

    void markTileMapPointsAroundCoord(CoordsInt coords)
    {
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

            // If point is inside the bounds then mark it as not travelable
            bool pointIsInsideBounds = tileMapConnections.isInsideBounds(attemptedCoord);
            if (pointIsInsideBounds == true)
            {
                Double<TileTraveledToMarker, Tile> tileMapConnElement = this.tileMapConnections.getElement(attemptedCoord);
                tileMapConnElement.getOne().lockPass(currentVeinPass);
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
        bool changeDirection = false;
        float randFloat = Random.Range(0f, 1f);
        Direction attempedDir = this.currentState.getCurrentDir();

        if (randFloat >= momentumPercentTable[this.currentState.getCurrentMomentum()])
        {
            changeDirection = true;
            if (rejectedDirections.Contains(this.currentState.getCurrentDir()) == false)
                rejectedDirections.Add(this.currentState.getCurrentDir());
        }

        //Debug.Log("Change Dir: " + changeDirection + "\nMomentrum %: " + momentumPercentTable[currentMomentum] + "\n% Choosen: " + randFloat);

        bool moveAccepted = false;
        bool primaryDirSelected = false;
        randFloat = Random.Range(0f, 1f);
        if (randFloat >= primaryDirectionPercentage)
            primaryDirSelected = true;


        // No U turns
        Direction oppositeDir = CommonFunctions.getOppositeDir(this.currentState.getPrevDir());
        if (rejectedDirections.Contains(oppositeDir) == false)
            rejectedDirections.Add(oppositeDir);

        Debug.Log("Change Direction Top \n =======================================================================");
        this.currentState.getCurrentCoords().print("Current Coords: ");

        foreach (var dir in rejectedDirections)
        {
            Debug.Log("Rejected Start: " + dir);
        }

        // Determine which direction should be changed to
        //      Make sure the trunk doesn't run over itself or run out of bounds
        while (moveAccepted == false)
        {
            if (changeDirection == true)
            {
                List<Direction> possibleDirections = new List<Direction>();

                while (possibleDirections.Count == 0)
                {
                    if (primaryDirSelected)
                    {
                        foreach (var dir in primaryDir)
                        {
                            if (rejectedDirections.Contains(dir) == false)
                            {
                                //Debug.Log("Adding Primary: " + dir);
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
                                //Debug.Log("Adding Secondary: " + dir);
                                possibleDirections.Add(dir);
                            }
                        }

                        if (possibleDirections.Count == 0)
                            primaryDirSelected = true;
                    }
                }

                attempedDir = CommonFunctions.randomlySelectFromList(possibleDirections);
            }

            moveAccepted = isNextMoveValid(attempedDir);

            if (moveAccepted == true)
            {
                this.currentState.setCurrentDir(attempedDir);
                this.currentState.setRejectedDir(rejectedDirections);
            }
            else if (moveAccepted == false)
            {
                // If the current direction will lead into a wall, then change the direction
                if (changeDirection == false)
                    changeDirection = true;
                rejectedDirections.Add(attempedDir);

                // If all directions are rejected, attempt to find any direction that works
                //      A direction can be rejected and not be locked.
                //      For example if the algorithm decided to change directions, the current direction is rejected. But doesn't mean that it's locked
                if (rejectedDirections.Count == 4)
                {
                    List<Direction> openDirections = getNotLockedDirections(this.currentState.getCurrentCoords());
                    List<Direction> newRejectedDirList = new List<Direction>();
                    foreach (var dir in rejectedDirections)
                    {
                        // If a direction in the rejectedDir list is NOT in the openDir list then keep the direction as rejected
                        if (openDirections.Contains(dir) == false)
                            newRejectedDirList.Add(dir);
                    }
                    rejectedDirections = newRejectedDirList;

                    if (rejectedDirections.Count == 4)
                    {
                        if (dirForRollback == true)
                            Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from Rollback");
                        else
                            Debug.LogError("ZoneVeinGenerator - determinNewDirection(): Failed to find a new direction. All directions are locked. \n" + "Failed from normal operation (not Rollback)");
                        rollBackState();
                    }
                }
            }
        }
    }

    // Check which directions are not locked
    List<Direction> getNotLockedDirections(CoordsInt coords)
    {
        List<Direction> notLockedDirections = new List<Direction>();
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
            bool pointIsInsideBounds = tileMapConnections.isInsideBounds(attemptedCoord);
            if (pointIsInsideBounds == true)
            {
                Double<TileTraveledToMarker, Tile> tileMapConnElement = this.tileMapConnections.getElement(attemptedCoord);
                if (tileMapConnElement.getOne().isPassLocked(currentVeinPass) == false)
                    notLockedDirections.Add(dir);
            }

            attemptedCoord = coords.deepCopyInt();
        }

        return notLockedDirections;
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
        Debug.Log("\tROLLBACK______________TOP");
        this.currentState.getCurrentCoords().print("\tPRE ROLLBACK COORDS: ");

        // Go to a previous state, it's set to go 2 states back or the last turn. Whichever one is less rollback
        this.currentState = stateHistory.rollBackState();
        this.currentState.getCurrentCoords().print("\tPOST ROLLBACK COORDS: ");



        // Revert any locked tiles

        // Get the rejected direction list from the recorded state
        //      Reject the direction of the next state, since that is where the path got stuck
        List<Direction> rejectedDirList = this.currentState.getRejectedDirList();
        rejectedDirList.Add(this.currentState.getNextDirection());


        Debug.Log("\tREJECTED DIRS: ");
        foreach (var dir in rejectedDirList)
        {
            Debug.Log("\t" + dir);
        }

        // Determine a new direction that can be attempted
        bool dirForRollback = true;
        determineNewDirection(rejectedDirList, dirForRollback);

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
