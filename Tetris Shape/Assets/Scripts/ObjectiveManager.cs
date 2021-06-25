using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    public int currentLevel;
    public bool onTutorial;
    public float speedIncrease { get
        {
            if (onTutorial)
                return tutorials[currentLevel].baseLevel.speedIncrease;
            return levels[currentLevel].speedIncrease;
        } }
    public int lineDrop { get
        {
            if (onTutorial)
                return tutorials[currentLevel].baseLevel.lineDrop;
            return levels[currentLevel].lineDrop;
        } }
    [SerializeField] Text bufferText;
    [SerializeField] Text levelText;
    [SerializeField] LevelTip tipText;
    [SerializeField] GameManager gameManager;

    List<LevelData> levels = new List<LevelData>();
    List<TutorialLevel> tutorials = new List<TutorialLevel>();

    BoardManager boardManager;
    PieceMovement pieceMovement;

    public struct LevelData
    {
        public LevelData(int borderSize, int allowedBuffers, int[,] requiredTiles, float speedIncrease, int lineDrop, float startSpeed = 1, int startTop = 20, string tip = "")
        {
            this.borderSize = borderSize;
            this.allowedBuffers = allowedBuffers;
            int[][] reqTiles = new int[requiredTiles.GetLength(0)][];
            for (int i = 0; i < requiredTiles.GetLength(0); i++)
            {
                reqTiles[i] = new int[] { requiredTiles[i, 0], requiredTiles[i, 1] };
            }
            this.requiredTiles = reqTiles;
            this.speedIncrease = speedIncrease;
            this.lineDrop = lineDrop;
            bufferTiles = new List<int[]>();
            generateBorders = true;
            this.startSpeed = startSpeed;
            this.startTop = startTop;
            this.tip = tip;
        }

        public LevelData(int[,] bufferTiles, int allowedBuffers, int[,] requiredTiles, float speedIncrease, int lineDrop, float startSpeed = 1, int startTop = 20, string tip = "")
        {
            this.borderSize = 0;
            this.allowedBuffers = allowedBuffers;
            int[][] reqTiles = new int[requiredTiles.GetLength(0)][];
            for (int i = 0; i < requiredTiles.GetLength(0); i++)
            {
                reqTiles[i] = new int[] { requiredTiles[i, 0], requiredTiles[i, 1] };
            }
            this.requiredTiles = reqTiles;
            this.speedIncrease = speedIncrease;
            this.lineDrop = lineDrop;
            this.bufferTiles = new List<int[]>();
            for (int i = 0; i < bufferTiles.GetLength(0); i++)
            {
                this.bufferTiles.Add(new int[] { bufferTiles[i, 0], bufferTiles[i, 1] });
            }
            generateBorders = false;
            this.startSpeed = startSpeed;
            this.startTop = startTop;
            this.tip = tip;
        }
        public int borderSize;
        public int allowedBuffers;
        public int[][] requiredTiles;
        public List<int[]> bufferTiles;
        public float speedIncrease;
        public int lineDrop;
        public bool generateBorders;
        public float startSpeed;
        public int startTop;
        public string tip;
    }

    public struct TutorialLevel
    {
        public TutorialLevel(LevelData baseLevel, int[][,] pieceLocations, Piece[] initialQueue, int precedingLevel)
        {
            this.baseLevel = baseLevel;
            this.pieceLocations = pieceLocations;
            this.initialQueue = initialQueue;
            this.precedingLevel = precedingLevel;
        }

        public LevelData baseLevel;
        public int[][,] pieceLocations;
        public Piece[] initialQueue;
        public int precedingLevel;
    }

    // Use this for initilization
    public void Init()
    {
        onTutorial = false;
        boardManager = GetComponent<BoardManager>();
        pieceMovement = GetComponent<PieceMovement>();
        InitLevels();
        if (!PlayerPrefs.HasKey("Current Level"))
            PlayerPrefs.SetInt("Current Level", 0);
        StartLevel(PlayerPrefs.GetInt("Current Level"));
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Restart();
        }
    }

    public void Restart()
    {
        gameManager.HandleButton("Resume");
        if (onTutorial)
            StartTutorial(currentLevel);
        else
            StartLevel(currentLevel);
    }

    void GenerateTiles(LevelData objTiles)
    {
        foreach(int[] tile in objTiles.bufferTiles)
        {
            boardManager.GetTile(tile[0], tile[1]).ObjState = ObjectiveState.Buffer;
        }
        for(int tile = 0; tile < objTiles.requiredTiles.GetLength(0);tile++)
        {
            boardManager.GetTile(objTiles.requiredTiles[tile][0], objTiles.requiredTiles[tile][1]).ObjState = ObjectiveState.Required;
        }
        for(int tileID = 0; tileID < objTiles.requiredTiles.GetLength(0); tileID++)
        {
            int[] tile = new int[] { objTiles.requiredTiles[tileID][0], objTiles.requiredTiles[tileID][1] };
            if (!objTiles.generateBorders)
                continue;
            for(int i = Mathf.Max(0, tile[0] - objTiles.borderSize); i <= Mathf.Min(9, tile[0] + objTiles.borderSize); i++)
            {
                for(int j = Mathf.Max(0, tile[1] - objTiles.borderSize); j <= Mathf.Min(19, tile[1] + objTiles.borderSize); j++)
                {
                    if(boardManager.GetTile(i, j).ObjState == ObjectiveState.Regular)
                    {
                        boardManager.GetTile(i, j).ObjState = ObjectiveState.Buffer;
                        objTiles.bufferTiles.Add(new int[] { i, j });
                        Debug.Log("Generating buffer tile at row " + j + " column " + i);
                    }
                }
            }
        }
    }
    void InitLevels() //I don't like how long these are.
    {
        //level 1
        levels.Add(new LevelData(1, 6, new int[,]{ { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 },
                                                   { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 },
                                                   { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 },
                                                   { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 } }, 0.1f, 2));
        //level 2
        levels.Add(new LevelData(1, 6, new int[,] { { 4, 3 }, { 5, 3 }, { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 2, 1 }, { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 }, { 7, 1 },
                                                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 } }, 0.1f, 3));
        //level 3
        levels.Add(new LevelData(1, 5, new int[,] { { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 },
                                                     { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 },
                                                     { 3, 6 }, { 4, 6 }, { 5, 6 }, { 6, 6 },
                                                     { 3, 7 }, { 4, 7 }, { 5, 7 }, { 6, 7 } }, 0.2f, 3));
        //level 4
        levels.Add(new LevelData(2, 10, new int[,] { { 3, 5 }, { 4, 6 }, { 5, 7 }, { 6, 8 }, { 6, 7 }, { 6, 6 }, { 6, 5 }, { 6, 4 }, { 6, 3 }, { 6, 2 }, { 5, 3 }, { 4, 4 }, { 4, 5 }, { 5, 4 }, { 5, 5 }, { 5, 6 } }, 0.2f, 3));
        //level 5
        levels.Add(new LevelData(1, 10, new int[,] { { 4, 3 }, { 5, 3 }, { 5, 4 }, { 6, 4 }, { 6, 5 },
                                                     { 7, 5 }, { 7, 6 }, { 6, 6 }, { 6, 7 }, { 5, 7 },
                                                     { 5, 8 }, { 4, 8 }, { 4, 7 }, { 3, 7 }, { 3, 6 }, { 2, 6 }, { 2, 5 }, { 3, 5 }, { 3, 4 }, { 4, 4 } }, 0.3f, 2));
        //level 6
        levels.Add(new LevelData(1, 0, new int[,] { { 4, 5 } }, 0.1f, 3));
        //tutorial for level 6
        tutorials.Add(new TutorialLevel(levels[5], new int[][,] { new int[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 2, 0 }, { 9, 7 }, { 9, 8 }, { 8, 8 }, { 7, 8 } },
                                                                  new int[,] { { 1, 1 }, { 2, 1 }, { 2, 2 }, { 3, 2 }, { 7, 5 }, { 8, 5 }, { 8, 6 }, { 9, 6 } },
                                                                  new int[,] { { 1, 2 }, { 1, 3 }, { 2, 3 }, { 3, 3 }, { 6, 1 }, { 7, 1 }, { 8, 1 }, { 8, 2 }, { 2, 7 }, { 3, 7 }, { 4, 7 }, { 4, 8 } },
                                                                  new int[,] { { 1, 4 }, { 1, 5 }, { 2, 5 }, { 2, 6 }, { 7, 2 }, { 7, 3 }, { 8, 3 }, { 8, 4 } },
                                                                  new int[,] { },
                                                                  new int[,] { { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 5, 7 }, { 6, 7 }, { 7, 7 }, { 8, 7 }, { 9, 1 }, { 9, 2 }, { 9, 3 }, { 9, 4 } },
                                                                  new int[,] { { 3, 0 }, { 4, 0 }, { 3, 1 }, { 4, 1 } } }, new Piece[] { Piece.O, Piece.T, Piece.L }, 5 ));
        //level 7
        levels.Add(new LevelData(1, 0, new int[,] { { 4, 5 }, { 5, 5 }, { 4, 6 }, { 5, 6 }, { 4, 7 }, { 5, 7 } }, 0.05f, 5));
        //level 8
        levels.Add(new LevelData(new int[,] { { 2, 3 }, { 2, 4 }, { 2, 5 }, { 2, 6 }, { 2, 7 }, { 2, 8 }, { 3, 8 }, { 4, 8 }, { 5, 8 }, { 6, 8 }, { 7, 8 }, { 7, 7 }, { 7, 6 }, { 7, 5 }, { 7, 4 }, { 7, 3 }, { 6, 3 }, { 5, 3 }, { 4, 3 }, { 3, 3 }, { 2, 3 } },
                                 0, new int[,] { { 4, 6 }, { 4, 5 }, { 5, 5 }, { 5, 6 } }, 0.2f, 3));
        //level 9
        levels.Add(new LevelData(new int[,] { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 }, { 1, 0 }, { 2, 1 }, { 3, 2 }, { 4, 3 }, { 5, 4 }, { 6, 5 }, { 7, 6 }, { 8, 7 }, { 9, 8 } }, 0, new int[,] { { 3, 7 }, { 5, 7 }, { 4, 6 }, { 4, 8 } }, 0.1f, 3));
        //level 10
        levels.Add(new LevelData(new int[,] { { 0, 0 }, { 9, 0 }, { 1, 1 }, { 8, 1 }, { 2, 2 }, { 7, 2 }, { 3, 3 }, { 6, 3 }, { 4, 4 }, { 5, 4 },
                                              { 5, 5 }, { 4, 5 }, { 6, 6 }, { 3, 6 }, { 7, 7 }, { 2, 7 }, { 8, 8 }, { 1, 8 }, { 9, 9 }, { 0, 9 } }, 0,
                                 new int[,] { { 4, 18 }, { 5, 18 } }, 0, 20));
        //level 11
        levels.Add(new LevelData(new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 0, 1 }, { 1, 1 }, { 2, 1 }, { 9, 0 }, { 8, 0 }, { 7, 0 }, { 9, 1 }, { 8, 1 }, { 7, 1 },
                                              { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 },
                                              { 0, 4 }, { 1, 4 }, { 2, 4 }, { 0, 5 }, { 1, 5 }, { 2, 5 }, { 9, 4 }, { 8, 4 }, { 7, 4 }, { 9, 5 }, { 8, 5 }, { 7, 5 },
                                              { 3, 6 }, { 4, 6 }, { 5, 6 }, { 6, 6 }, { 3, 7 }, { 4, 7 }, { 5, 7 }, { 6, 7 },
                                              { 0, 8 }, { 1, 8 }, { 2, 8 }, { 0, 9 }, { 1, 9 }, { 2, 9 }, { 9, 8 }, { 8, 8 }, { 7, 8 }, { 9, 9 }, { 8, 9 }, { 7, 9 },
                                              { 3, 10 }, { 4, 10 }, { 5, 10 }, { 6, 10 }, { 3, 11 }, { 4, 11 }, { 5, 11 }, { 6, 11 },
                                              { 0, 12 }, { 1, 12 }, { 2, 12 }, { 0, 13 }, { 1, 13 }, { 2, 13 }, { 9, 12 }, { 8, 12 }, { 7, 12 }, { 9, 13 }, { 8, 13 }, { 7, 13 },
                                              { 3, 14 }, { 4, 14 }, { 5, 14 }, { 6, 14 }, { 3, 15 }, { 4, 15 }, { 5, 15 }, { 6, 15 },
                                              { 0, 16 }, { 1, 16 }, { 2, 16 }, { 0, 17 }, { 1, 17 }, { 2, 17 }, { 9, 16 }, { 8, 16 }, { 7, 16 }, { 9, 17 }, { 8, 17 }, { 7, 17 },
                                              { 3, 18 }, { 6, 18 }, { 3, 19 }, { 4, 19 }, { 5, 19 }, { 6, 19 }}, 4, new int[,] { { 4, 18 }, { 5, 18 } }, 0.05f, 8));
        //level 12
        levels.Add(new LevelData(2, 2, new int[,] { { 2, 10 }, { 7, 10 } }, 0.1f, 4));
        //level 13
        levels.Add(new LevelData(new int[,] { { 0, 4 }, { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 }, { 9, 4 }, { 0, 5 }, { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 }, { 9, 5 },
                                              { 0, 6 }, { 3, 6 }, { 4, 6 }, { 5, 6 }, { 6, 6 }, { 9, 6 }, { 0, 7 }, { 3, 7 }, { 4, 7 }, { 5, 7 }, { 6, 7 }, { 9, 7 },
                                              { 0, 8 }, { 2, 8 }, { 4, 8 }, { 7, 8 }, { 5, 8 }, { 9, 8 }, { 0, 9 }, { 1, 9 }, { 5, 9 }, { 8, 9 }, { 4, 9 }, { 9, 9 },
                                              { 0, 10 }, { 3, 10 }, { 6, 10 }, { 9, 10 }, { 0, 11 }, { 1, 11 }, { 4, 11 }, { 5, 11 }, { 8, 11 }, { 9, 11 },
                                              { 0, 12 }, { 2, 12 }, { 4, 12 }, { 5, 12 }, { 7, 12 }, { 9, 12 }, { 0, 13 }, { 3, 13 }, { 6, 13 }, { 9, 13 } }, 0, new int[,] { { 1, 10 }, { 8, 10 } }, 0.1f, 4));
        //level 14
        levels.Add(new LevelData(1, 4, new int[,] { { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 }, { 6, 5 }, { 6, 6 }, { 6, 7 }, { 6, 8 }, { 6, 9 },
                                                    { 5, 9 }, { 4, 9 }, { 3, 9 } }, 0.2f, 2));
        //level 15
        levels.Add(new LevelData(2, 3, new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 2, 1 }, { 2, 2 }, { 2, 3 }, { 3, 2 }, { 4, 2 }, { 5, 2 },
                                                    { 5, 3 }, { 6, 3 }, { 7, 3 }, { 7, 2 }, { 7, 1 }, { 7, 0 }, { 6, 0 }, { 4, 0 } }, 0.1f, 4));
        //level 16
        levels.Add(new LevelData(2, 5, new int[,] { { 2, 2 }, { 3, 2 }, { 4, 2 }, { 5, 2 }, { 5, 3 }, { 5, 4 }, { 4, 4 }, { 3, 4 }, { 3, 5 }, { 3, 6 }, { 3, 7 }, { 3, 8 }, { 4, 8 }, { 5, 8 }, { 6, 8 }, { 7, 8 }, { 7, 7 }, { 7, 6 }, { 6, 6 }, { 5, 6 }, { 5, 5 }, { 7, 2 } }, 0.05f, 7));
        //level 17
        levels.Add(new LevelData(new int[,] { { 0, 4 }, { 1, 5 }, { 2, 4 }, { 3, 5 }, { 4, 4 }, { 5, 5 }, { 6, 4 }, { 7, 5 }, { 8, 4 }, { 9, 5 } }, 0,
                                 new int[,] { { 4, 7 }, { 5, 7 }, { 4, 8 }, { 5, 8 } }, 0.2f, 3));
        //level 18
        levels.Add(new LevelData());
        //level 19
        levels.Add(new LevelData());
        //level 20
        levels.Add(new LevelData());
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        if (!PlayerPrefs.HasKey("Skip Tutorial") || PlayerPrefs.GetInt("Skip Tutorial") == 0)
        {
            for(int i = 0; i < tutorials.Count; i++)
            {
                if (tutorials[i].precedingLevel == currentLevel)
                {
                    StartTutorial(i);
                    return;
                }
            }
        }
        onTutorial = false;
        Debug.Log(level);
        tipText.SetTip(levels[level].tip);
        levelText.text = "Level " + (level + 1);
        bufferText.text = "0/" + levels[currentLevel].allowedBuffers + " buffers used.";
        bufferText.color = Color.green;
        boardManager.ClearBoard();
        GenerateTiles(levels[level]);
        boardManager.BeginLevel(levels[level].startTop, levels[level].startSpeed);
    }

    public void StartTutorial(int level)
    {
        onTutorial = true;
        currentLevel = level;
        tipText.SetTip(tutorials[level].baseLevel.tip);
        levelText.text = "Tutorial " + (level + 1);
        bufferText.text = "0/" + tutorials[currentLevel].baseLevel.allowedBuffers + " buffers used.";
        bufferText.color = Color.green;
        boardManager.ClearBoard();
        GenerateTiles(tutorials[level].baseLevel);
        for(int i = 0; i < tutorials[level].pieceLocations.Length; i++)
        {
            int[,] locations = tutorials[level].pieceLocations[i];
            Debug.Log(locations.GetLength(0));
            for(int j = 0; j < locations.GetLength(0); j++)
            {
                Debug.Log("Getting location number " + j + " for piece type " + i);
                boardManager.SetTile(locations[j, 0], locations[j, 1], TileState.Filled, (Piece)i);
            }
        }
        boardManager.BeginLevel(tutorials[level].baseLevel.startTop, tutorials[level].baseLevel.startSpeed);
        if (tutorials[level].initialQueue.Length == 0)
            return;
        pieceMovement.GetNewPiece(tutorials[level].initialQueue[0]);
        for(int i = 1; i < Mathf.Min(boardManager.nextPieces.Count + 1, tutorials[level].initialQueue.Length); i++)
        {
            boardManager.nextPieces[i - 1] = tutorials[level].initialQueue[i];
        }
        boardManager.UpdateNextQueue();
    }

    public bool CheckObjective()
    {
        bool reqTilesFilled = true;
        LevelData level = levels[currentLevel];
        if (onTutorial)
            level = tutorials[currentLevel].baseLevel;
        foreach(int[] tiles in level.requiredTiles)
        {
            switch(boardManager.GetTile(tiles[0],tiles[1])?.State)
            {
                case TileState.Blocked:
                    Debug.Log("Game Over");
                    pieceMovement.enabled = false;
                    return false;
                case TileState.Empty:
                    reqTilesFilled = false;
                    break;
                case TileState.Filled:
                    break;
                default:
                    Debug.LogError("Tile at column " + tiles[0] + " and row " + tiles[1] + " had an invalid tile state.");
                    reqTilesFilled = false;
                    break;
            }
        }
        int buffers = 0;
        for (int i = 0; i < level.bufferTiles.Count; i++)
        {
            switch (boardManager.GetTile(level.bufferTiles[i][0], level.bufferTiles[i][1])?.State)
            {
                case TileState.Blocked:
                    //fail the level
                    break;
                case TileState.Empty:
                    break;
                case TileState.Filled:
                    //Debug.Log("Found filled buffer tile at coloumn " + levels[currentLevel].bufferTiles[i, 0] + " row " + levels[currentLevel].bufferTiles[i, 1]);
                    buffers += 1;
                    break;
                default:
                    //Debug.LogError("Tile at column " + levels[currentLevel].requiredTiles[i, 0] + " and row " + levels[currentLevel].requiredTiles[i, 1] + " had an invalid tile state.");
                    return false;
            }
        }
        bufferText.text = buffers + "/" + level.allowedBuffers + " buffers used.";
        bufferText.color = buffers > level.allowedBuffers ? Color.red : Color.green;
        return reqTilesFilled && buffers <= level.allowedBuffers;
    }
    public bool GoToNextLevel()
    {
        if (onTutorial)
        {
            currentLevel = tutorials[currentLevel].precedingLevel;
            PlayerPrefs.SetInt("Skip Tutorial", 1);
        }
        else
        {
            currentLevel++;
            PlayerPrefs.SetInt("Skip Tutorial", 0);
        }
        if (levels.Count <= currentLevel)
        {
            return false;
        }
        PlayerPrefs.SetInt("Current Level", currentLevel);
        StartLevel(currentLevel);
        return true;
    }
}
