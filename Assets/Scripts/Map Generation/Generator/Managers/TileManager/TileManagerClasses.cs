using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonlyUsedClasses;
using VeinManagerClasses;
using CommonlyUsedDefinesAndEnums;

namespace TileManagerClasses
{
    // Tile map is used in the generation proccess to place room pieces and veins on
    public class TileMap
    {
        TwoDList<Tile> tileMap;
        GameObject tileMapGameObject;

        Tile selectedTile = null;

        // Properties
        Coords<int> minDim = new Coords<int>(0, 0);
        Coords<int> maxDim = new Coords<int>(147 * 2 * 2 * 2, 72 * 2 * 2 * 2);

        Dimensions tileMapDimensions;

        Coords<int> tileMapCenter;

        // Depth Markers
        int yAbove;
        int yLevel;
        int yShallow;
        int yDeep;
        int yVeryDeep;

        // Horizontal Displacement Markers
        int xFarLeft;
        int xLeft;
        int xRight;
        int xFarRight;


        public TileMap(GameObject tileMapGameObject)
        {
            this.tileMap = new TwoDList<Tile>();
            this.tileMapGameObject = tileMapGameObject;
            this.tileMapDimensions = new Dimensions(minDim, maxDim);
            this.tileMapCenter = new Coords<int>(tileMapDimensions.getMaxX()/2, 
                                                (tileMapDimensions.getMaxY()/2) + 50);

            // Depth Markers
            int yCenterToBottom = tileMapCenter.getY() - minDim.getY();
            this.yAbove      = tileMapCenter.getY() + 20;
            this.yShallow    = tileMapCenter.getY() - (yCenterToBottom / 4);
            this.yDeep       = tileMapCenter.getY() - ((yCenterToBottom * 2) / 4);
            this.yVeryDeep   = tileMapCenter.getY() - ((yCenterToBottom * 3) / 4);

            // Horizontal Displacement Markers
            this.xFarLeft = tileMapCenter.getX() - (tileMapCenter.getX() / 2);
            this.xLeft = tileMapCenter.getX() - 30;
            this.xRight = tileMapCenter.getX() + 30;
            this.xFarRight = tileMapCenter.getX() + (tileMapCenter.getX() / 2);
        }

        public void addTile(Coords<int> index, Tile item)
        {
            tileMap.addElement(index, item);
        }

        public void countTileDims()
        {
            Debug.Log("TileMap X Count: " + tileMap.getXCount() + 
                    "\n        Y Count: " + tileMap.getYCount());
        }



        // ======================================================================
        //                          Getters/Setters
        // ======================================================================

        public Depth getTileDepth(CoordsInt coords)
        {
            Depth depth = Depth.Level;

            // Above
            if (yAbove <= coords.getY())
                depth = Depth.Above;
            // Level
            else if (yShallow <= coords.getY() && coords.getY() < yAbove)
                depth = Depth.Level;
            // Deep
            else if (yDeep <= coords.getY() && coords.getY() < yShallow)
                depth = Depth.Deep;
            // Really Deep
            else if (coords.getY() < yDeep)
                depth = Depth.Very_Deep;

            return depth;
        }

        public HorizontalDisplacement getTileHorizontalDisplacement(CoordsInt coords)
        {
            HorizontalDisplacement xDisplacement = HorizontalDisplacement.Center;

            // Far Right
            if (xFarRight <= coords.getX())
                xDisplacement = HorizontalDisplacement.Far_Right;
            // Right
            else if (xRight <= coords.getX() && coords.getX() < xFarRight)
                xDisplacement = HorizontalDisplacement.Right;
            // Center
            else if (xLeft <= coords.getX() && coords.getX() < xRight)
                xDisplacement = HorizontalDisplacement.Center;
            // Left
            else if (xFarLeft <= coords.getX() && coords.getX() < xLeft)
                xDisplacement = HorizontalDisplacement.Left;
            // Far Left
            else if (coords.getX() < xFarLeft)
                xDisplacement = HorizontalDisplacement.Far_Left;

            return xDisplacement;
        }

        public Dimensions getTileMapDims()
        {
            return this.tileMapDimensions;
        }

        public Coords<int> getTileMapCenter()
        {
            return this.tileMapCenter;
        }

        public ref Tile getTile(Coords<int> coords, ref bool accessSuccessful)
        {
            if (tileMapDimensions.getMinX() <= coords.getX() && coords.getX() < tileMapDimensions.getMaxX() &&
                tileMapDimensions.getMinY() <= coords.getY() && coords.getY() < tileMapDimensions.getMaxY())
            {
                selectedTile = tileMap.getElement(coords);
                accessSuccessful = true;
            }
            else
            {
                Debug.Log("Tile Class - getTile() attempted to access out of bounds tile: " + coords.getX() + ", " + coords.getY());
                accessSuccessful = false;
            }

            return ref selectedTile;
        }

        public ref GameObject getTileMapGameObject()
        {
            return ref tileMapGameObject;
        }

    }

    public class Tile
    {
        bool gameObjectIsInstantiated = false;
        GameObject gameObject;

        GameObject tileMapGameObject;
        bool gameObjectTilesAreOn;
        bool enabledGameObjectIfTouched;

        // Coords
        CoordsInt tileMapIndex;
        Coords<float> worldCoords;

        // Properties
        string name;
        float tileHeight;
        float tileWidth;

        // Debug and Vein Generation
        bool isTouched = false;
        TileRoomType intendedRoomType = TileRoomType.None_Set; // Zone specific room, normal vein room, a vein biome room (GreatTunnel)

        bool isVein = false;
        bool isVeinMain = false;
        Vein associatedVein = null;

        // Room Generation
        bool isOccupiedByRoom = false;

        public Tile()
        {
            this.gameObject = null;
            this.tileMapGameObject = null;
            this.gameObjectTilesAreOn = false;

            this.tileHeight = 0f;
            this.tileWidth = 0f;

            this.tileMapIndex = null;
            this.worldCoords = null;

            this.name = "NULL";
        }

        public Tile(bool gameObjectTilesAreOn, bool enabledGameObjectIfTouched, float height, float width, 
                    Coords<float> worldCoords, CoordsInt tileMapCoords, ref GameObject tileMapGameObject)
        {
            this.gameObjectTilesAreOn = gameObjectTilesAreOn;
            this.tileMapGameObject = tileMapGameObject;
            this.enabledGameObjectIfTouched = enabledGameObjectIfTouched;

            this.tileHeight = height;
            this.tileWidth = width;

            this.tileMapIndex = tileMapCoords;
            this.worldCoords = worldCoords;

            this.name = "Tile__(" + tileMapIndex.getX().ToString() + "," + tileMapIndex.getY().ToString() + ")";

            // If gameobject tils are on, then we need to set additional gameobject setting
            if (gameObjectTilesAreOn == true)
            {
                instantiateTileGameObject();
            }
        }

        public void instantiateTileGameObject()
        {
            // If a gameobject instance exists then don't create another one
            if (this.gameObject != null)
                return;

            GameObject newTileGameObj = Singleton.instantiateTile();

            this.gameObject = newTileGameObj;
            this.gameObject.transform.position = new Vector3(worldCoords.getX(), worldCoords.getY(), 0);
            this.gameObject.transform.SetParent(tileMapGameObject.transform);
            this.gameObject.name = this.name;
            this.gameObjectIsInstantiated = true;
        }

        // ============================================================================
        //                               Setter/Getters
        // ============================================================================

        public void setTileAsVein(Vein veinInst)
        {
            if (this.isVein == false)
            {
                this.isVein = true;
                this.isTouched = true;
                this.associatedVein = veinInst;
                this.intendedRoomType = TileRoomType.Vein;

                if (gameObjectTilesAreOn == false)
                {
                    instantiateTileGameObject();
                }
            }
        }

        public void setTileAsVeinMain(Vein veinInst)
        {
            if (this.isVeinMain == false)
            {
                this.isVeinMain = true;
                setTileAsVein(veinInst);
            }
        }

        public ref GameObject getTileGameObject()
        {
            if (gameObjectIsInstantiated == true)
                return ref gameObject;
            else
            {
                Debug.LogError("Tile Class - getTileGameObject(): Attempted to grab a tile gameobject that is not instantiated");
                return ref gameObject;
            }
        }

        public bool getIsVeinMain()
        {
            return this.isVeinMain;
        }

        public bool getIsVein()
        {
            return this.isVein;
        }

        public CoordsInt getTileMapCoords()
        {
            return this.tileMapIndex;
        }

        public bool getIsOccupiedByRoom()
        {
            return this.isOccupiedByRoom;
        }
    }
}

