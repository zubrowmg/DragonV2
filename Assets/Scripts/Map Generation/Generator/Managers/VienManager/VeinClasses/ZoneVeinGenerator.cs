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
    TwoDList<Double<bool, Tile>> tileMapConnections = new TwoDList<Double<bool, Tile>>(); // Allocated dims, but only the tiles spaced out every x amount
    CoordsInt currentCoords = new CoordsInt(0, 0);
    CoordsInt prevCoords = new CoordsInt(0, 0);

    // Zone Connection Node Generation
    int gapBetweenNodes = 9;

    // Direction and momentum
    //      Momentum starts at 0 in any direction, has a percentage chance of changing direction after each increment. Once it hits the max then force direction change
    Direction currentDirection;
    int maxMomentum = 3;
    int currentMomentum = 0;
    float primaryDirectionPercentage = .65f;
    List<float> momentumPercentTable = new List<float> { .85f, .60f, .25f, .05f };
    List<Direction> primaryDir;
    List<Direction> secondaryDir;

    // Initial zone length
    int maxTrunkLength = 6;

    // Vein
    VeinZone currentVeinZone;
    List<CoordsInt> setCoords;
    SetCoordsVein newVein;
    int veinWidth = 5;


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
        determineStartDirection();

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
        this.currentCoords = new CoordsInt(0, 0);
        this.tileMapConnections = new TwoDList<Double<bool, Tile>>();
        this.setCoords = new List<CoordsInt>();
        this.newVein = new SetCoordsVein(ref getContainerInst(), 0, this.currentDirection, new CoordsInt(0, 0), new CoordsInt(0, 0), false, false, false, 5);

        setPrimaryAndSecondaryDir();
    }

    public void determineStartDirection()
    {
        // Randomly choose vertical or horizontal direction to start
        DirectionBias zoneDirBias = this.currentZone.getDirBias();
        int rand = Random.Range(0, 2);

        if (rand == 0)
        {
            this.currentDirection = zoneDirBias.getHorizontalDir();
            if (this.currentDirection == Direction.None)
                this.currentDirection = zoneDirBias.getVerticalDir();

        }
        else
        {
            this.currentDirection = zoneDirBias.getVerticalDir();
            if (this.currentDirection == Direction.None)
                this.currentDirection = zoneDirBias.getHorizontalDir();
        }

        if (this.currentDirection == Direction.None)
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
                    bool travelAllowed = true;

                    // Mark non vein points as not allowed to be traveled to
                    if (allocatedDimList.getGridVal(currentCoords) == 0)
                        travelAllowed = false;

                    Tile tileRef = allocatedTileMap.getElement(currentCoords);
                    Double<bool, Tile> newElement = new Double<bool, Tile>(travelAllowed, tileRef);
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
        this.currentCoords = startCoords.deepCopyInt();
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



    List<Direction> getNonePrimaryDir()
    {
        List<Direction> primaryDir = new List<Direction>();

        if (currentDirection != Direction.None)
            primaryDir.Add(currentZone.getDirBias().getHorizontalDir());
        if (currentDirection != Direction.None)
            primaryDir.Add(currentZone.getDirBias().getVerticalDir());

        return primaryDir;
    }

    // =====================================================================================
    //                              Create Zone Vein Functions
    // =====================================================================================

    public void createZoneVein()
    {
        createZoneVeinTrunk();

        bool done = false;

        while (done == false)
        {


            done = true;
        }
    }

    public void createZoneVeinTrunk()
    {
        int currentLength = 0;
        bool trunkFinished = false;

        this.currentCoords.print("Start Coords: ");
        CoordsInt currentWorldCoords = getTileMapCoordsFromTileMapConns(this.currentCoords);
        newVein.addSetCoord(currentWorldCoords);

        while (trunkFinished == false)
        {
            this.currentCoords.print("Current Coords: ");

            // Travel one unit in the current direction
            this.prevCoords = this.currentCoords;
            travelOneUnit(this.currentDirection);


            // Record the point
            currentWorldCoords = getTileMapCoordsFromTileMapConns(this.currentCoords);
            newVein.addSetCoord(currentWorldCoords);



            // Decide on a new direction
            determineNewDirection();

            // Determine if the trunk is too long
            currentLength++;
            if (currentLength >= maxTrunkLength)
                trunkFinished = true;
           // break;
        }

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
        CoordsInt attemptedTileMapCoords = this.currentCoords.deepCopyInt();

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
        Double<bool, Tile> attemptedTileMapConnElement = this.tileMapConnections.getElement(attemptedTileMapCoords);
        CoordsInt attemptedWorldTileMapCoords = attemptedTileMapConnElement.getTwo().getTileMapCoords();
        CoordsInt currentWorldTileMapCoords = this.tileMapConnections.getElement(currentCoords).getTwo().getTileMapCoords();

        // If the attempted to travel to coord is not exactly to the left in world coords, then reject
        //rejected = !checkGapDistance(attemptedWorldTileMapCoords, currentWorldTileMapCoords);
        checkGapDistance(attemptedWorldTileMapCoords, currentWorldTileMapCoords);
            //if (rejected == false)
                this.currentCoords = attemptedTileMapCoords.deepCopyInt();
        //}

        //return rejected;
    }



    bool checkTileMapConnPoint(CoordsInt coords)
    {
        bool rejected = false;

        //Debug.Log("BOUNDS_1: " + this.tileMapConnections.getXCount() + "," + this.tileMapConnections.getYCount(coords.getX()));
        //Debug.Log("BOUNDS_2: " + this.tileMapConnections.getXCount() + "," + this.tileMapConnections.getYCount());


        if (0 <= coords.getX() && coords.getX() < this.tileMapConnections.getXCount() &&
            0 <= coords.getY() && coords.getY() < this.tileMapConnections.getYCount())
        {
            // Do nothing, it's within bounds
        }
        else
        {
            rejected = true;
            return rejected;
        }

        Double<bool, Tile> attemptedTileMapConnElement = this.tileMapConnections.getElement(coords);

        if (attemptedTileMapConnElement.getOne() == false)
            rejected = true;

        return rejected;
    }

    void determineNewDirection()
    {
        //momentumMax = 4;
        //currentMomentum = 0;

        bool changeDirection = false;
        float randFloat = Random.Range(0f, 1f);
        Direction attempedDir = currentDirection;

        if (randFloat >= momentumPercentTable[currentMomentum])
            changeDirection = true;

        bool moveAccepted = false;
        bool primaryDirSelected = false;
        if (randFloat >= primaryDirectionPercentage)
            primaryDirSelected = true;

        List<Direction> rejectedDirections = new List<Direction>();
        randFloat = Random.Range(0f, 1f);

        // Determine which direction should be changed to
        //      Make sure the trunk doesn't run over itself, or will run out of bounds
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

                attempedDir = CommonFunctions.randomlySelectFromList(possibleDirections);
            }

            moveAccepted = isNextMoveValid(attempedDir);

            if (moveAccepted == true)
                currentDirection = attempedDir;
            else if (moveAccepted == false)
            {
                // If the current direction will lead into a wall, then change the direction
                if (changeDirection == false)
                    changeDirection = true;
                rejectedDirections.Add(currentDirection);
            }

        }
        
    }

    // Check if the next direction will hit the edge
    bool isNextMoveValid(Direction attempedDir)
    {
        CoordsInt attemptedTileMapCoords = this.currentCoords.deepCopyInt();

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

        return accepted;
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
