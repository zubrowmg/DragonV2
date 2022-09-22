using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Classes;
using Enums;

public class veinPresetManagerScript : MonoBehaviour
{
    

    const int dataStart = 0;

    const float smallLetterPercentChance = .15f;

    public enum POICoreType { Small, Medium, Large };
    public enum SmallVeinCore { One, Two, Three };
    public GameObject randProbabiliryManager;

    class POIPiece
    {
        public gridUnitScript.GeneralVeinDirection dir;
        public int x;
        public int y;
        public bool isBridge;

        public string LinkId;
        public string InvId;
        public string InvLinkId;
        public float PercentChance;

        public POIPiece(gridUnitScript.GeneralVeinDirection direction, int xStart, int yStart, bool isABridge, 
                        string idLink, string idI, string idInvertedLink, float percent)
        {
            this.dir = direction;
            this.x = xStart;
            this.y = yStart;
            this.isBridge = isABridge;

            this.LinkId = idLink;
            this.InvId = idI;
            this.InvLinkId = idInvertedLink;
            this.PercentChance = percent;
        }

        public void changePercentChance(float percent)
        {
            this.PercentChance = percent;
        }
        public void changeInvId(string id)
        {
            this.InvId = id;
        }
    }

    public void getCorePOI(ref List<List<int>> preset, ref int xStart, ref int yStart, POICoreType type, 
                                    gridUnitScript.GeneralVeinDirection dir, ref List<RoomPreset> bossRoomLocations, ZoneUnitProperties choosenZoneAndAbilities)
    {
        int xPresetStart = 0;
        int yPresetStart = 0;

        List<RoomPreset> bossRoomLocationsRelativeToPreset = new List<RoomPreset>();

        switch (type)
        {
            case POICoreType.Small:
                getSmallCore(ref preset, ref xPresetStart, ref yPresetStart, dir, ref bossRoomLocationsRelativeToPreset, choosenZoneAndAbilities);
                break;
            case POICoreType.Medium:
                Debug.Log("CHANGE POI CORE MEDIUM TO GET MEDIUM PRESETS");
                getSmallCore(ref preset, ref xPresetStart, ref yPresetStart, dir, ref bossRoomLocationsRelativeToPreset, choosenZoneAndAbilities);
                break;
            case POICoreType.Large:
                Debug.Log("CHANGE POI CORE LARGE TO GET LARGE PRESETS");
                getSmallCore(ref preset, ref xPresetStart, ref yPresetStart, dir, ref bossRoomLocationsRelativeToPreset, choosenZoneAndAbilities);
                break;
        }

        orientTemplate(ref preset, dir, ref xStart, ref yStart, ref xPresetStart, ref yPresetStart, ref bossRoomLocations, bossRoomLocationsRelativeToPreset);
    }


    void getSmallCore(ref List<List<int>> preset, ref int xStart, ref int yStart, 
                    gridUnitScript.GeneralVeinDirection dir, ref List<RoomPreset> bossRoomLocationsRelativeToPreset, ZoneUnitProperties choosenZoneAndAbilities)
    {
        string resourcePath = "VeinPresets/SmallCore/";

        SmallVeinCore type = SmallVeinCore.One;//(SmallVeinCore)Random.Range(0, 2);

        switch (type)
        {
            case SmallVeinCore.One:
                resourcePath = resourcePath + "1";
                break;
        }

        List<POIPiece> pieces = new List<POIPiece>();
        //print("HI0");
        parseCoreFile(resourcePath, dir, ref xStart, ref yStart, ref preset, ref pieces, ref bossRoomLocationsRelativeToPreset);
        //print("HI1");

        bool pieceInstalled = false;
        POIPiece piece = new POIPiece(gridUnitScript.GeneralVeinDirection.Up, 0, 0, true, "", "", "", 1f);

        choosePieces(ref pieces);



        for (int i = 0; i < pieces.Count; i++)
        {
            piece = pieces[i];

            pieceInstalled = getSmallPiece(ref preset, piece);

            if (pieceInstalled && piece.LinkId != "")
            {
                changeLinkedPiecesPercent(i, ref pieces, ref piece, 1f);
            }
            else if (!pieceInstalled && piece.LinkId != "")
            {
                changeLinkedPiecesPercent(i, ref pieces, ref piece, 0f);
            }
            pieceInstalled = false;
        }
        

    }

    void choosePieces(ref List<POIPiece> pieces)
    {
        List<int> visitedInvPieces = new List<int>();
        List<int> rejectedInvPieces = new List<int>();
        List<int> linkedPiecesWithInversion = new List<int>();

        List<POIPiece> newPiecesList = new List<POIPiece>();
        List<POIPiece> complicatedPiecesList = new List<POIPiece>();


        int biggestInvId = 0;


        POIPiece currentPiece = pieces[0];


        // Find the highest inverted id
        for (int i = 0; i < pieces.Count; i++)
        {
            currentPiece = pieces[i];
            if (currentPiece.InvId != "")
            {
                if (System.Convert.ToInt32(currentPiece.InvId) > biggestInvId)
                {
                    biggestInvId = System.Convert.ToInt32(currentPiece.InvId);
                }
                complicatedPiecesList.Add(currentPiece);
                if (currentPiece.LinkId != "")
                {
                    linkedPiecesWithInversion.Add(System.Convert.ToInt32(currentPiece.LinkId));
                }
            }
            else if (currentPiece.LinkId != "")
            {
                complicatedPiecesList.Add(currentPiece);
            }
            else
            {
                // Add all non complicated pieces to the new list
                newPiecesList.Add(currentPiece);
            }
        }

        // Need to add all normal linked pieces with no inversions
        for (int i = 0; i < pieces.Count; i++)
        {
            currentPiece = pieces[i];
            if (currentPiece.LinkId != "")
            {
                if (!linkedPiecesWithInversion.Contains(System.Convert.ToInt32(currentPiece.LinkId)))
                {
                    //Debug.Log("PLAIN LINK");
                    newPiecesList.Add(currentPiece);
                }
            }
        }

        List<POIPiece> tempInvPieces = new List<POIPiece>();

        // Start with inverted id 0 and go up
        for (int invId = 0; invId <= biggestInvId; invId++)
        {
            // If the inverted piece has been visited already skip it
            if (visitedInvPieces.Contains(invId))
            {
                continue;
            }
            visitedInvPieces.Add(invId);

            tempInvPieces = new List<POIPiece>();
            // Find all inverted pieces with the current inv id
            for (int i = 0; i < complicatedPiecesList.Count; i++)
            {
                if (complicatedPiecesList[i].InvId != "")
                {
                    if (System.Convert.ToInt32(complicatedPiecesList[i].InvId) == invId)
                    {
                        tempInvPieces.Add(complicatedPiecesList[i]);
                    }
                }
            }

            
            
            

            // Choose one of the inverted pieces
            int rand = Random.Range(0, tempInvPieces.Count);
            POIPiece choosenPiece = tempInvPieces[rand];

            string choosenLinkId = choosenPiece.LinkId;

            // Remove choosen piece from the temp inverted list
            tempInvPieces.RemoveAt(rand);

            for (int i = 0; i < tempInvPieces.Count; i++)
            {
                currentPiece = tempInvPieces[i];
                visitedInvPieces.Add(System.Convert.ToInt32(currentPiece.InvId));

                // if currentPiece.InvLinkId exists
                //    then find all InvLinkIds that match
            }

            //print("HI1");

            // Add all linked pieces to the new list
            for (int i = 0; i < complicatedPiecesList.Count; i++)
            {
                currentPiece = complicatedPiecesList[i];

                // Go through all pieces and add the choosen inverted piece links
                if (currentPiece.LinkId != "" && currentPiece.LinkId == choosenLinkId)
                {
                    
                    currentPiece.LinkId = "";
                    currentPiece.InvId = "";
                    currentPiece.PercentChance = 1f;

                    newPiecesList.Add(currentPiece);
                }
                else
                {
                    // Go through all of the rejected inverted pieces
                    for (int j = 0; j < tempInvPieces.Count; j++)
                    {

                        // If the link id is the same as a rejected piece don't add it to the list
                        if (currentPiece.LinkId != "" && currentPiece.LinkId == tempInvPieces[j].LinkId &&
                            currentPiece.InvId != tempInvPieces[j].InvId)
                        {
                            /*if (currentPiece.LinkId == "40")
                            {
                                print("---------START   LINK ID= " + currentPiece.LinkId + "   INVERTED ID" + currentPiece.InvId);
                            }*/

                            //print("start linkId: " + currentPiece.LinkId + "    " + currentPiece.dir + "      " + currentPiece.isBridge);
                            findRejectedInvertedLinks(currentPiece.LinkId, currentPiece.InvId, complicatedPiecesList, ref visitedInvPieces);
                            
                        }
                    }
                }
            }

            //print("HI2");

            // Add the choosen piece to the new list
            choosenPiece.LinkId = "";
            choosenPiece.InvId = "";
            choosenPiece.PercentChance = 1f;
            newPiecesList.Add(choosenPiece);
            
        }
        pieces = newPiecesList;
    }

    void findRejectedInvertedLinks(string linkId, string rejectedInvId, List<POIPiece> complicatedPiecesList, ref List<int> visitedInvPieces)
    {

        //print("LINK ID: " + linkId + "    REJECTED INV ID: " + rejectedInvId);

        POIPiece currentPiece = complicatedPiecesList[0];
        List<int> checkLinkPieces = new List<int>();
        //List<List<int>> checkLinkPiecesList = new List<List<int>>();

        string rejectedInvLinkId = "";

        List<string> rejectedInvLinkIds = new List<string>();

        // Go through all complicated pieces and find matching link ids that have inverted link ids
        for (int i = 0; i < complicatedPiecesList.Count; i++)
        {
            currentPiece = complicatedPiecesList[i];

            // Find the inverted link id
            if (currentPiece.LinkId != "" && currentPiece.LinkId == linkId)
            {
                if (currentPiece.InvLinkId != "" && currentPiece.InvId == rejectedInvId)
                {
                    rejectedInvLinkId = currentPiece.InvLinkId;
                    /*if (linkId == "40")
                    {
                        print("REJECTED INV LINKS: " + rejectedInvLinkId);
                    }*/
                    rejectedInvLinkIds.Add(rejectedInvLinkId);
                    
                }
            }
        }

        // Find all pieces that mention the inverted link id 
        //      And mark their inverted ids as visited
        //      Also go through any links of inverted link pieces
        //          If any linked pieces have another inverted id use findRejectedInvertedLinks() again
        //for (int k = 0; k < rejectedInvLinkIds.Count; k++)
        //{
            //string tempRejectedInvLinkId = rejectedInvLinkIds[k];
            for (int i = 0; i < complicatedPiecesList.Count; i++)
            {
                currentPiece = complicatedPiecesList[i];
                //checkLinkPieces = new List<int>();
                // Find the inverted link id
                if (currentPiece.InvLinkId != "" && currentPiece.InvLinkId == rejectedInvLinkId)
                {
                    if (currentPiece.InvId != "")
                    {
                        /*if (linkId == "40")
                        {
                            print("        Rejected Inv ID: " + currentPiece.InvId);
                        }*/
                        visitedInvPieces.Add(System.Convert.ToInt32(currentPiece.InvId));
                    }
                    if (currentPiece.LinkId != "" && currentPiece.LinkId != linkId)
                    {
                        /*if (linkId == "40")
                        {
                            print("        Found links: " + currentPiece.LinkId);
                        }*/
                        // Need to visit these links and check for inverted links
                        checkLinkPieces.Add(System.Convert.ToInt32(currentPiece.LinkId));
                    }
                }
            }
        //    checkLinkPiecesList.Add(checkLinkPieces);
        //}


        int linkedPiece = 0;

        //print("Main rejected: " + rejectedInvLinkId);

        for (int i = 0; i < complicatedPiecesList.Count; i++)
        {
            currentPiece = complicatedPiecesList[i];

            for (int j = 0; j < checkLinkPieces.Count; j++)
            {
                linkedPiece = checkLinkPieces[j];
                //print("Sub Link: " + currentPiece.LinkId);
                // If the current piece matches the link id of any rejected pieces
                if (currentPiece.LinkId != "" && System.Convert.ToInt32(currentPiece.LinkId) == linkedPiece)
                {
                    /*bool match = false;
                    for (int k = 0; k < rejectedInvLinkIds.Count; k++)
                    {
                        if (currentPiece.InvLinkId != rejectedInvLinkIds[k])
                        {
                            match = true;
                        }
                    }*/

                    //print("Sub Link Match: " + currentPiece.LinkId);
                    if (currentPiece.InvLinkId != "" && currentPiece.InvLinkId != rejectedInvLinkId)
                    {
                        //print("Sub Inverted Link: " + currentPiece.InvLinkId);
                        findRejectedInvertedLinks(currentPiece.LinkId, currentPiece.InvId, complicatedPiecesList, ref visitedInvPieces);
                    }
                }
            }
        }
    }

    void changeLinkedPiecesPercent(int i, ref List<POIPiece> pieces, ref POIPiece piece, float percent)
    {
        // Change all linked pieces percent chance of spawning to percentage
        
        pieces[i].changePercentChance(percent);
        for (int j = i + 1; j < pieces.Count; j++)
        {
            if (pieces[i].LinkId == pieces[j].LinkId)
            {
                pieces[j].changePercentChance(percent);
            }
        }
    }

    bool getSmallPiece(ref List<List<int>> preset, POIPiece piece)
    {
        bool pieceInstalled = false;

        string fullPath = "Assets/Resources/VeinPresets/SmallPiece/";
        string resourcePath = "VeinPresets/SmallPiece/";

        if (piece.isBridge)
        {
            fullPath = fullPath + "Bridge/";
            resourcePath = resourcePath + "Bridge/";
        }
        else
        {
            fullPath = fullPath + "Room/";
            resourcePath = resourcePath + "Room/";
        }

        // Get the amount of files in this directory
        DirectoryInfo dir = new DirectoryInfo(fullPath);
        FileInfo[] info = dir.GetFiles("*.txt");

        resourcePath = resourcePath + (Random.Range(0, info.Length)).ToString();


        int xStart = 0;
        int yStart = 0;
        List<List<int>> presetPiece = new List<List<int>>();

        int rand = randProbabiliryManager.GetComponent<randomProbabilityManagerScript>().getIntBasedOnPercentage(
                                new randomProbabilityManagerScript.RandomSelection(0, 0, 1f - piece.PercentChance),
                                new randomProbabilityManagerScript.RandomSelection(1, 1, piece.PercentChance));
        if (piece.LinkId != "")
        {
            //print("    Id: " + piece.LinkId);
            //print("    coors: " + piece.x + "," + piece.y);
            //print("        %: " + piece.PercentChance + "    Dir: " + piece.dir);
            //print("          ---------------------------------------- " );
        }
        if (rand == 1)
        {
            if (piece.LinkId != "")
            {
                //print("    coors: " + piece.x + "," + piece.y);
                //print("        %: " + piece.PercentChance + "    Dir: " + piece.dir);
                //print("    : " + piece.x + "," + piece.y);
            }
            // Read the pieces file
            parsePieceFile(resourcePath, piece.dir, ref xStart, ref yStart, ref presetPiece);
            attachPiece(ref preset, ref presetPiece, piece, ref xStart, ref yStart);
            pieceInstalled = true;
        }
        return pieceInstalled;
    }

    void attachPiece(ref List<List<int>> preset, ref List<List<int>> presetPiece, POIPiece piece, ref int xPieceStart, ref int yPieceStart)
    {
        
        // This function rotates the orientation for each piece direction
        switch (piece.dir)
        {
            // Don't need to change, since Right is the default direction
            case gridUnitScript.GeneralVeinDirection.Right:
                installPieceRightLeft(ref preset, ref presetPiece, piece, ref xPieceStart, ref yPieceStart);
                break;


            case gridUnitScript.GeneralVeinDirection.Left:
                installPieceRightLeft(ref preset, ref presetPiece, piece, ref xPieceStart, ref yPieceStart);

                break;


            case gridUnitScript.GeneralVeinDirection.Up:
                installPieceUpDown(ref preset, ref presetPiece, piece, ref xPieceStart, ref yPieceStart);

                break;


            case gridUnitScript.GeneralVeinDirection.Down:
                installPieceUpDown(ref preset, ref presetPiece, piece, ref xPieceStart, ref yPieceStart);

                break;
        }
        
    }

    void installPieceRightLeft(ref List<List<int>> preset, ref List<List<int>> presetPiece, POIPiece piece, ref int xPieceStart, ref int yPieceStart)
    {
        int x = piece.x - xPieceStart;
        int y = piece.y - yPieceStart;
        

        if (piece.dir == gridUnitScript.GeneralVeinDirection.Right)
        {
            x = piece.x - xPieceStart + 1;
        }
        else if (piece.dir == gridUnitScript.GeneralVeinDirection.Left)
        {
            x = piece.x + xPieceStart - 1;
        }
        //Debug.Log("Piece: " + piece.x + ", " + piece.y);
        //Debug.Log("StartPiece: " + xPieceStart + ", " + yPieceStart);
        //Debug.Log("Piece: " + x + ", " + y);


        for (int i = 0; i < presetPiece.Count; i++)
        {

            y = piece.y - yPieceStart;
            for (int j = 0; j < presetPiece[0].Count; j++)
            {

                //Debug.Log("Piece Coords: " + i + ", " + j);
                if (presetPiece[i][j] == 1)
                {
                    //Debug.Log("Coords: " + x + ", " + y);
                    //Debug.Log("Size: " + preset.Count + ", " + preset[0].Count);

                    // Don't go out of bounds of the preset
                    if (x >= preset.Count || x < 0 ||
                        y >= preset[0].Count || y < 0)
                    {
                        //Skip
                    }
                    else
                    {
                        preset[x][y] = 1;
                    }
                    
                }
                y++;
            }
            if (piece.dir == gridUnitScript.GeneralVeinDirection.Right)
            {
                x++;
            }
            else if (piece.dir == gridUnitScript.GeneralVeinDirection.Left)
            {
                x--;
            }
        }
    }

    void installPieceUpDown(ref List<List<int>> preset, ref List<List<int>> presetPiece, POIPiece piece, ref int xPieceStart, ref int yPieceStart)
    {
        // Need to rotate the piece upwards first
        List<List<int>> newPiece = new List<List<int>>();
        List<int> temp = new List<int>();
        for (int j = presetPiece[0].Count-1; j >= 0 ; j--)
        {
            temp = new List<int>();
            for (int i = 0; i < presetPiece.Count; i++)
            {
                temp.Add(presetPiece[i][j]);
            }
            newPiece.Add(temp);
        }
        presetPiece = newPiece;


        //Debug.Log("Old Start: " + xPieceStart + ", " + yPieceStart);

        // Calculate the new piece starts due to rotation
        int xTemp = xPieceStart;
        xPieceStart = (presetPiece.Count-1) - yPieceStart;
        yPieceStart = xTemp;

        int x = piece.x - xPieceStart;
        int y = piece.y - yPieceStart;

        if (piece.dir == gridUnitScript.GeneralVeinDirection.Up)
        {
            y = piece.y - yPieceStart + 1;
        }
        else if (piece.dir == gridUnitScript.GeneralVeinDirection.Down)
        {
            y = piece.y + yPieceStart - 1;
            x++;
        }

        //Debug.Log("Start coords: " + piece.x + ", " + piece.y);
        //Debug.Log("New StartPiece: " + xPieceStart + ", " + yPieceStart);
        //Debug.Log("New coords: " + x + ", " + y);
        
        for (int i = 0; i < presetPiece.Count; i++)
        {

            if (piece.dir == gridUnitScript.GeneralVeinDirection.Up)
            {
                y = piece.y - yPieceStart + 1;
            }
            else if (piece.dir == gridUnitScript.GeneralVeinDirection.Down)
            {
                y = piece.y + yPieceStart - 1;
            }
            for (int j = 0; j < presetPiece[0].Count; j++)
            {

                //Debug.Log("Piece Coords: " + i + ", " + j);
                if (presetPiece[i][j] == 1)
                {
                    //Debug.Log("Coords: " + x + ", " + y);
                    //Debug.Log("Size: " + preset[0].Count + ", " + preset.Count);

                    // Don't go out of bounds of the preset
                    if (x >= preset.Count || x < 0 ||
                        y >= preset[0].Count || y < 0)
                    {
                        //Skip
                    }
                    else
                    {
                        preset[x][y] = 1;
                    }
                }
                if (piece.dir == gridUnitScript.GeneralVeinDirection.Up)
                {
                    y++;
                }
                else if (piece.dir == gridUnitScript.GeneralVeinDirection.Down)
                {
                    y--;
                }
            }
            x++;
        }
    }

    void orientTemplate(ref List<List<int>> preset, gridUnitScript.GeneralVeinDirection dir, ref int xStart, 
                            ref int yStart, ref int xPresetStart, ref int yPresetStart, ref List<RoomPreset> bossRoomLocations, List<RoomPreset> bossRoomLocationsRelativeToPreset)
    {
        generalDirectionTwo direction = generalDirectionTwo.Right;

        // This function doesn't change the template for down/up
        //      Templates have 2 locations they can be placed for right and down
        switch (dir)
        {
            // Don't need to change, since Right is the default direction
            case gridUnitScript.GeneralVeinDirection.Right:
                xStart -= xPresetStart;
                yStart -= yPresetStart;
                direction = generalDirectionTwo.Right;
                break;


            case gridUnitScript.GeneralVeinDirection.Left:
                facePresetLeft(ref preset, ref xPresetStart, ref yPresetStart);
                xStart -= xPresetStart;
                yStart -= yPresetStart;
                direction = generalDirectionTwo.Left;
                break;


            case gridUnitScript.GeneralVeinDirection.Up:
                //facePresetUp(ref preset, ref xPresetStart, ref yPresetStart);
                xStart -= xPresetStart;
                yStart -= yPresetStart;
                direction = generalDirectionTwo.Up;
                break;


            case gridUnitScript.GeneralVeinDirection.Down:
                facePresetDown(ref preset, ref xPresetStart, ref yPresetStart);
                xStart -= xPresetStart;
                yStart -= yPresetStart;
                direction = generalDirectionTwo.Down;
                break;

        }

        // Update the bossRoomLocations relative to the grid using xStart and yStart
        for (int i = 0; i < bossRoomLocationsRelativeToPreset.Count; i++)
        {
            int newX = 0;
            int newY = 0;
            switch (dir)
            {
                case gridUnitScript.GeneralVeinDirection.Right:
                    newX = bossRoomLocationsRelativeToPreset[i].coords.x + xStart;
                    newY = bossRoomLocationsRelativeToPreset[i].coords.y + yStart;
                    break;
                case gridUnitScript.GeneralVeinDirection.Left:
                    newX = preset.Count - bossRoomLocationsRelativeToPreset[i].coords.x + xStart - 1;
                    newY = bossRoomLocationsRelativeToPreset[i].coords.y + yStart;
                    break;
                case gridUnitScript.GeneralVeinDirection.Up:
                    newX = bossRoomLocationsRelativeToPreset[i].coords.x + xStart;
                    newY = bossRoomLocationsRelativeToPreset[i].coords.y + yStart;
                    break;
                case gridUnitScript.GeneralVeinDirection.Down:
                    newX = bossRoomLocationsRelativeToPreset[i].coords.x + xStart;
                    newY = bossRoomLocationsRelativeToPreset[i].coords.y + yStart;
                    break;
            }
            

            string id = bossRoomLocationsRelativeToPreset[i].bossRoomId;
            bossRoomLocations.Add(new RoomPreset(newX, newY, id, direction));
        }

    }

    void facePresetLeft(ref List<List<int>> preset, ref int xPresetStart, ref int yPresetStart)
    {
        List<List<int>> translatedPreset = new List<List<int>>();

        List<int> temp = new List<int>();
        int xAccess = preset.Count - 1;

        for (int x = xAccess; x >= 0; x--)
        {
            // Reset yAccess and temp list
            //yAccess = preset[0].Count - 1;
            temp = new List<int>();
            for (int y = 0; y < preset[0].Count; y++)
            {
                //Debug.Log(yAccess);
                temp.Add(preset[x][y]);
                //yAccess--;
            }
            translatedPreset.Add(temp);
        }

        preset = translatedPreset;

        xPresetStart = preset.Count - xPresetStart;
    }

    void facePresetDown(ref List<List<int>> preset, ref int xPresetStart, ref int yPresetStart)
    {
        /*List<List<int>> translatedPreset = new List<List<int>>();

        List<int> temp = new List<int>();
        int xAccess = preset.Count - 1;

        for (int y = 0; y < preset[0].Count; y++)
        {
            // Reset xAccess and temp list
            xAccess = preset.Count - 1;
            temp = new List<int>();
            for (int x = 0; x < preset.Count; x++)
            {
                //Debug.Log(yAccess);
                temp.Add(preset[xAccess][y]);
                xAccess--;
            }
            translatedPreset.Add(temp);
        }

        preset = translatedPreset;
        */
        //int xTemp = xPresetStart;
        //xPresetStart = xPresetStart;
        yPresetStart = preset[0].Count - yPresetStart;
    }

    void facePresetUp(ref List<List<int>> preset, ref int xPresetStart, ref int yPresetStart)
    {
        /*List<List<int>> translatedPreset = new List<List<int>>();

        List<int> temp = new List<int>();
        int xAccess = preset.Count - 1;
        int yAccess = preset[0].Count - 1;

        for (int y = 0; y < preset[0].Count; y++)
        {
            // Reset xAccess and temp list
            xAccess = preset.Count - 1;
            temp = new List<int>();
            for (int x = 0; x < preset.Count; x++)
            {
                //Debug.Log(yAccess);
                temp.Add(preset[xAccess][yAccess]);
                xAccess--;
            }
            translatedPreset.Add(temp);
            yAccess--;
        }

        preset = translatedPreset;
        */
        int xTemp = xPresetStart;
        xPresetStart = yPresetStart;
        yPresetStart = xTemp;
    }

    void parseCoreFile(string path, gridUnitScript.GeneralVeinDirection dir, ref int xStart, ref int yStart, 
                                    ref List<List<int>> preset, ref List<POIPiece> pieces, ref List<RoomPreset> bossRoomLocationsRelativeToPreset)
    {
        var dataset = Resources.Load<TextAsset>(path);
        var dataLines = dataset.text.Split('\n'); 
        var lineData = dataLines[dataStart].Split(',');

        List<int> temp = new List<int>();

        float percentChance = 1f;
        POIPiece tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Up, 0, 0, true, "", "", "", 1f);

        for (int col = 0; col < lineData.Length; col++)
        {
            // Reset temp list and get next line
            temp = new List<int>();
            //
            for (int row = dataLines.Length - 1; row >= 0; row--) 
            {
                lineData = dataLines[row].Split(',');
                //Debug.Log(lineData[col]);
                if (lineData[col] == ">" && (dir == gridUnitScript.GeneralVeinDirection.Left || dir == gridUnitScript.GeneralVeinDirection.Right))
                {
                    xStart = col;
                    yStart = row;
                    //Debug.Log("Post: " + xStart + ", " + yStart);
                    temp.Add(1);
                }
                else if (lineData[col] == "V" && dir == gridUnitScript.GeneralVeinDirection.Down)
                {
                    xStart = col;
                    yStart = row;
                    temp.Add(1);
                    //Debug.Log("Post: " + xStart + ", " + yStart);
                }
                else
                {
                    
                    if (lineData[col] == "V" || lineData[col] == ">")
                    {
                        temp.Add(1);
                    }
                    else if (isPiece(lineData[col])) // W,A,S,D,X
                    {
                        if (containsLowerCasePiece(lineData[col]))
                        {
                            percentChance = smallLetterPercentChance;
                            temp.Add(1);
                        }
                        else
                        {
                            temp.Add(0);
                            percentChance = 1f;
                        }

                        //Debug.Log("Letter: " + lineData[col]);
                        if (lineData[col] == "W" || lineData[col] == "w")
                        {
                            tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Up, col, (dataLines.Length - 1) - row, true, "", "", "", percentChance);
                        }
                        else if (lineData[col] == "A" || lineData[col] == "a")
                        {
                            tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Left, col, (dataLines.Length - 1) - row, true, "", "", "", percentChance);
                        }
                        else if (lineData[col] == "S" || lineData[col] == "s")
                        {
                            tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Down, col, (dataLines.Length - 1) - row, true, "", "", "", percentChance);
                        }
                        else if (lineData[col] == "D" || lineData[col] == "d")
                        {
                            
                            tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Right, col, (dataLines.Length - 1) - row, true, "", "", "", percentChance);
                        }
                        else if (lineData[col] == "X" || lineData[col] == "x")
                        {
                            tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Right, col, (dataLines.Length - 1) - row, false, "", "", "", percentChance);
                        }
                        // Its a linked piece
                        else
                        {
                            //linkedPiece = true;
                            //Debug.Log("INNER: " + lineData[col]);
                            //Debug.Log("First: " + lineData[col][0]);

                            string linkId = "";
                            string invId = "";
                            string invLinkId = "";
                            if (lineData[col].Contains("L"))
                            {
                                for (int i = 0; i < lineData[col].Length; i++)
                                {
                                    if ((lineData[col][i] + "") == "L")
                                    {
                                        linkId = getNextILKInt(lineData[col], i);//lineData[col][i+1] + ""; // Need + "" to convert to string?
                                    }
                                }
                            }
                            if (lineData[col].Contains("I"))
                            {
                                for (int i = 0; i < lineData[col].Length; i++)
                                {
                                    if ((lineData[col][i] + "") == "I")
                                    {
                                        invId = getNextILKInt(lineData[col], i);//lineData[col][i+1] + ""; // Need + "" to convert to string?
                                    }
                                }
                            }
                            if (lineData[col].Contains("K"))
                            {
                                for (int i = 0; i < lineData[col].Length; i++)
                                {
                                    if ((lineData[col][i] + "") == "K")
                                    {
                                        invLinkId = getNextILKInt(lineData[col], i);//lineData[col][i + 1] + ""; // Need + "" to convert to string?
                                    }
                                }
                            }

                            //Debug.Log("I: " + invId + "|    L: " + linkId + "|   K: " + invLinkId);

                            //Debug.Log("Num: " + numOfLinks);
                            //Debug.Log("Id: " + invId);
                            if (lineData[col].Contains("W") || lineData[col].Contains("w"))
                            {
                                tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Up, col, (dataLines.Length - 1) - row, true, linkId, invId, invLinkId, percentChance);
                            }
                            else if (lineData[col].Contains("A") || lineData[col].Contains("a"))
                            {
                                tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Left, col, (dataLines.Length - 1) - row, true, linkId, invId, invLinkId, percentChance);
                            }
                            else if (lineData[col].Contains("S") || lineData[col].Contains("s"))
                            {
                                tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Down, col, (dataLines.Length - 1) - row, true, linkId, invId, invLinkId, percentChance);
                            }
                            else if (lineData[col].Contains("D") || lineData[col].Contains("d"))
                            {
                                tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Right, col, (dataLines.Length - 1) - row, true, linkId, invId, invLinkId, percentChance);
                            }
                            else if (lineData[col].Contains("X") || lineData[col].Contains("x"))
                            {
                                tempPiece = new POIPiece(gridUnitScript.GeneralVeinDirection.Right, col, (dataLines.Length - 1) - row, false, linkId, invId, invLinkId, percentChance);
                            }
                        }

                        //Debug.Log(percentChance);

                        pieces.Add(tempPiece);
                        
                    }
                    else if (isPresetBossRoom(lineData[col]))
                    {
                        bossRoomLocationsRelativeToPreset.Add(new RoomPreset(col, (dataLines.Length - 1) - row, lineData[col]));

                        // Still need to add a number to temp
                        temp.Add(1);
                    }
                    else
                    {
                        
                        temp.Add(System.Convert.ToInt32(lineData[col]));
                    }
                    
                }

            }
            preset.Add(temp);
        }

    }

    string getNextILKInt(string input, int index)
    {
        string output = "";
        //Debug.Log(input + "|");
        if (index + 1 >= input.Length)
        {
            output = "";
        }
        else if (input[index + 1] == '0' || input[index + 1] == '1' || input[index + 1] == '2' || input[index + 1] == '3' || input[index + 1] == '4' ||
            input[index + 1] == '5' || input[index + 1] == '6' || input[index + 1] == '7' || input[index + 1] == '8' || input[index + 1] == '9')
        {
            output = input[index + 1] + "" + getNextILKInt(input, index + 1);
        }

        return output;
    }

    public string getNextInt(string input, int index)
    {
        string output = "";
        //Debug.Log(input + "|");
        if (index + 1 >= input.Length)
        {
            output = "";
        }
        else if (input[index + 1] == '0' || input[index + 1] == '1' || input[index + 1] == '2' || input[index + 1] == '3' || input[index + 1] == '4' ||
            input[index + 1] == '5' || input[index + 1] == '6' || input[index + 1] == '7' || input[index + 1] == '8' || input[index + 1] == '9')
        {
            output = input[index + 1] + "" + getNextInt(input, index + 1);
        }

        return output;
    }

    bool isPiece(string input)
    {
        bool isAPiece = false;
        if (input.Contains("W") || input.Contains("A") || input.Contains("S") || input.Contains("D") ||
            input.Contains("w") || input.Contains("a") || input.Contains("s") || input.Contains("d") ||
            input.Contains("X") || input.Contains("x"))
        {
            isAPiece = true;
        }
        return isAPiece;
    }

    bool isPresetBossRoom(string input)
    {
        bool isaPresetBossRoom = false;
        if (input.Contains("B"))
        {
            isaPresetBossRoom = true;
        }
        return isaPresetBossRoom;
    }

    bool containsLowerCasePiece(string input)
    {
        bool isLowerCasePiece = false;
        if ( input.Contains("w") || input.Contains("a") || input.Contains("s") || input.Contains("d") || input.Contains("x"))
        {
            isLowerCasePiece = true;
        }
        return isLowerCasePiece;
    }

    void parsePieceFile(string path, gridUnitScript.GeneralVeinDirection dir, ref int xStart, ref int yStart, ref List<List<int>> preset)
    {
        var dataset = Resources.Load<TextAsset>(path);
        var dataLines = dataset.text.Split('\n');
        var lineData = dataLines[dataStart].Split(',');

        List<int> temp = new List<int>();
        for (int col = 0; col < lineData.Length; col++)
        {
            temp = new List<int>();

            for (int row = dataLines.Length - 1; row >= 0; row--)
            {
                // Reset temp list and get next line
                lineData = dataLines[row].Split(',');
                //Debug.Log(row + " , " + col);
                if (lineData[col] == ">")
                {
                    xStart = col;
                    yStart = dataLines.Length - 1 - row;
                    //Debug.Log("Post: " + xStart + ", " + yStart);
                    temp.Add(1);
                }
                else
                {
                    if (lineData[col] == "V" || lineData[col] == ">")
                    {
                        
                        temp.Add(1);
                    }
                    else
                    {
                        temp.Add(System.Convert.ToInt32(lineData[col]));
                    }
                }

            }
            preset.Add(temp);
        }
    }
}
