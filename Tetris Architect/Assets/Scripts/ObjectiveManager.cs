using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] Text bufferText;
    [SerializeField] Text levelText;
    [SerializeField] LevelTip tipText;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject restartWarning;

    List<LevelData> levels = new List<LevelData>();
    List<TutorialLevel> tutorials = new List<TutorialLevel>();
    [HideInInspector] public int currentLevel;
    [HideInInspector] public bool onTutorial;

    public float speedIncrease
    {
        get
        {
            if (onTutorial)
                return tutorials[currentLevel].baseLevel.speedIncrease;
            return levels[currentLevel].speedIncrease;
        }
    }
    public int lineDrop
    {
        get
        {
            if (onTutorial)
                return tutorials[currentLevel].baseLevel.lineDrop;
            return levels[currentLevel].lineDrop;
        }
    }

    BoardManager boardManager;
    PieceMovement pieceMovement;

    public struct LevelData
    {
        public LevelData(int borderSize, int allowedBuffers, int[,] requiredTiles, string title, float speedIncrease, int lineDrop, float startSpeed = 1, int startTop = 20, string tip = "")
        {
            this.borderSize = borderSize;
            this.allowedBuffers = allowedBuffers;
            int[][] reqTiles = new int[requiredTiles.GetLength(0)][];
            for (int i = 0; i < requiredTiles.GetLength(0); i++)
            {
                reqTiles[i] = new int[] { requiredTiles[i, 0], requiredTiles[i, 1] };
            }
            this.requiredTiles = reqTiles;
            bufferTiles = new List<int[]>();
            generateBorders = true;
            this.title = title;
            this.speedIncrease = speedIncrease;
            this.lineDrop = lineDrop;
            this.startSpeed = startSpeed;
            this.startTop = startTop;
            this.tip = tip;
        }

        public LevelData(int[,] bufferTiles, int allowedBuffers, int[,] requiredTiles, string title, float speedIncrease, int lineDrop, float startSpeed = 1, int startTop = 20, string tip = "")
        {
            this.borderSize = 0;
            this.allowedBuffers = allowedBuffers;
            int[][] reqTiles = new int[requiredTiles.GetLength(0)][];
            for (int i = 0; i < requiredTiles.GetLength(0); i++)
            {
                reqTiles[i] = new int[] { requiredTiles[i, 0], requiredTiles[i, 1] };
            }
            this.requiredTiles = reqTiles;
            this.bufferTiles = new List<int[]>();
            for (int i = 0; i < bufferTiles.GetLength(0); i++)
            {
                this.bufferTiles.Add(new int[] { bufferTiles[i, 0], bufferTiles[i, 1] });
            }
            generateBorders = false;
            this.title = title;
            this.speedIncrease = speedIncrease;
            this.lineDrop = lineDrop;
            this.startSpeed = startSpeed;
            this.startTop = startTop;
            this.tip = tip;
        }
        public int borderSize;
        public int allowedBuffers;
        public int[][] requiredTiles;
        public List<int[]> bufferTiles;
        public string title;
        public float speedIncrease;
        public int lineDrop;
        public bool generateBorders;
        public float startSpeed;
        public int startTop;
        public string tip;
    }

    public struct TutorialLevel
    {
        public TutorialLevel(LevelData baseLevel, int[][,] pieceLocations, Piece[] initialQueue, int precedingLevel, string title)
        {
            this.baseLevel = baseLevel;
            this.pieceLocations = pieceLocations;
            this.initialQueue = initialQueue;
            this.precedingLevel = precedingLevel;
            this.title = title;
        }

        public LevelData baseLevel;
        public int[][,] pieceLocations; //each int[,] array corresponds to a different piece. it goes in the same order as the Piece enum.
        public Piece[] initialQueue;
        public int precedingLevel;
        public string title;
    }

    // Use this for initilization
    public void Init()
    {
        onTutorial = false;
        boardManager = GetComponent<BoardManager>();
        pieceMovement = GetComponent<PieceMovement>();
        restartWarning.SetActive(false);
        InitLevels();
        if (!PlayerPrefs.HasKey("Current Level"))
            PlayerPrefs.SetInt("Current Level", 0);
        StartLevel(PlayerPrefs.GetInt("Current Level"));
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ShowRestartWarning();
        }
    }

    public void ShowRestartWarning()
    {
        pieceMovement.enabled = false;
        gameManager.disablePausing = true;
        restartWarning.SetActive(true);
    }

    public void CloseRestartWarning()
    {
        pieceMovement.enabled = true;
        gameManager.disablePausing = false;
        restartWarning.SetActive(false);
    }

    public void Restart()
    {
        CloseRestartWarning();
        gameManager.Resume();
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
        //level 1, I might add a tutorial here just so the tutorial for level 6 is less surprising.
        levels.Add(new LevelData(1, 6, new int[,]{ { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 },
                                                   { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 },
                                                   { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 },
                                                   { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 } }, "A Square of Squares", 0.1f, 2));
        //level 2
        levels.Add(new LevelData(1, 6, new int[,] { { 4, 3 }, { 5, 3 }, { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 2, 1 }, { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 }, { 7, 1 },
                                                    { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 } }, "The Pyramid", 0.1f, 3));
        //level 3
        levels.Add(new LevelData(1, 5, new int[,] { { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 },
                                                     { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 },
                                                     { 3, 6 }, { 4, 6 }, { 5, 6 }, { 6, 6 },
                                                     { 3, 7 }, { 4, 7 }, { 5, 7 }, { 6, 7 } }, "The Floating Box", 0.2f, 3));
        //level 4
        levels.Add(new LevelData(2, 10, new int[,] { { 3, 5 }, { 4, 6 }, { 5, 7 }, { 6, 8 }, { 6, 7 }, { 6, 6 }, { 6, 5 }, { 6, 4 }, { 6, 3 }, { 6, 2 }, { 5, 3 }, { 4, 4 }, { 4, 5 }, { 5, 4 }, { 5, 5 }, { 5, 6 } }, "Another Triangle", 0.2f, 3));
        //level 5
        levels.Add(new LevelData(1, 10, new int[,] { { 4, 3 }, { 5, 3 }, { 5, 4 }, { 6, 4 }, { 6, 5 },
                                                     { 7, 5 }, { 7, 6 }, { 6, 6 }, { 6, 7 }, { 5, 7 },
                                                     { 5, 8 }, { 4, 8 }, { 4, 7 }, { 3, 7 }, { 3, 6 }, { 2, 6 }, { 2, 5 }, { 3, 5 }, { 3, 4 }, { 4, 4 } }, "The Ringer", 0.3f, 2));
        //level 6, an introduction to the "0 buffers" levels
        levels.Add(new LevelData(1, 0, new int[,] { { 4, 5 } }, "Just One Tile", 0.1f, 3));
        //tutorial for level 6, I don't like that this is the first and only tutorial.
        tutorials.Add(new TutorialLevel(levels[5], new int[][,] { new int[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 2, 0 }, { 9, 7 }, { 9, 8 }, { 8, 8 }, { 7, 8 } }, //J piece
                                                                  new int[,] { { 1, 1 }, { 2, 1 }, { 2, 2 }, { 3, 2 }, { 7, 5 }, { 8, 5 }, { 8, 6 }, { 9, 6 } }, //S piece
                                                                  new int[,] { { 1, 2 }, { 1, 3 }, { 2, 3 }, { 3, 3 }, { 6, 1 }, { 7, 1 }, { 8, 1 }, { 8, 2 }, { 2, 7 }, { 3, 7 }, { 4, 7 }, { 4, 8 } }, //L Piece
                                                                  new int[,] { { 1, 4 }, { 1, 5 }, { 2, 5 }, { 2, 6 }, { 7, 2 }, { 7, 3 }, { 8, 3 }, { 8, 4 } }, //Z piece
                                                                  new int[,] { }, //T piece
                                                                  new int[,] { { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 5, 7 }, { 6, 7 }, { 7, 7 }, { 8, 7 }, { 9, 1 }, { 9, 2 }, { 9, 3 }, { 9, 4 } }, //I Piece
                                                                  new int[,] { { 3, 0 }, { 4, 0 }, { 3, 1 }, { 4, 1 } } }, new Piece[] { Piece.O, Piece.T, Piece.L }, 5, "The Magical Floating Orange." ));
        //level 7
        levels.Add(new LevelData(1, 0, new int[,] { { 4, 5 }, { 5, 5 }, { 4, 6 }, { 5, 6 }, { 4, 7 }, { 5, 7 } }, "Half and Whole", 0.05f, 5));
        //level 8, an introduction to the concept of buffers being seperate from the objective and the concept of "piercing" through a buffer to get to the other side.
        levels.Add(new LevelData(new int[,] { { 2, 3 }, { 2, 4 }, { 2, 5 }, { 2, 6 }, { 2, 7 }, { 2, 8 }, { 3, 8 }, { 4, 8 }, { 5, 8 }, { 6, 8 }, { 7, 8 }, { 7, 7 }, { 7, 6 }, { 7, 5 }, { 7, 4 }, { 7, 3 }, { 6, 3 }, { 5, 3 }, { 4, 3 }, { 3, 3 }, { 2, 3 } },
                                 0, new int[,] { { 4, 6 }, { 4, 5 }, { 5, 5 }, { 5, 6 } }, "The Vault", 0.2f, 3));
        //level 9
        levels.Add(new LevelData(new int[,] { { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 }, { 1, 0 }, { 2, 1 }, { 3, 2 }, { 4, 3 }, { 5, 4 }, { 6, 5 }, { 7, 6 }, { 8, 7 }, { 9, 8 } }, 0,
                                 new int[,] { { 2, 8 }, { 4, 8 }, { 3, 7 }, { 3, 9 }, { 8, 2 }, { 8, 4 }, { 7, 3 }, { 9, 3 } }, "One in a Hundred", 0.1f, 3));
        //level 10
        levels.Add(new LevelData(new int[,] { { 0, 0 }, { 9, 0 }, { 1, 1 }, { 8, 1 }, { 2, 2 }, { 7, 2 }, { 3, 3 }, { 6, 3 }, { 4, 4 }, { 5, 4 },
                                              { 5, 5 }, { 4, 5 }, { 6, 6 }, { 3, 6 }, { 7, 7 }, { 2, 7 }, { 8, 8 }, { 1, 8 }, { 9, 9 }, { 0, 9 } }, 0,
                                 new int[,] { { 4, 18 }, { 5, 18 } }, "Double Cross", 0, 20));
        //level 11, an introduction to the concept of mutliple seperate objective regions. Was originally level 12 before the old level 11 was moved to the challenge levels.
        levels.Add(new LevelData(1, 0, new int[,] { { 2, 8 }, { 7, 8 } }, "The Abyss Stares Back", 0.1f, 4));
        //level 12, a combination of levels 8 and 11. Was originally 2 (very weirdly) intersecting diamonds, but top halves were cut off to allow for more room and make it easier. Was originally level 13 before the old level 11 was moved to the challenge levels.
        levels.Add(new LevelData(new int[,] { { 3, 7 }, { 6, 7 }, { 2, 8 }, { 4, 8 }, { 7, 8 }, { 5, 8 }, { 1, 9 }, { 5, 9 }, { 8, 9 }, { 4, 9 },
                                              { 0, 10 }, { 3, 10 }, { 6, 10 }, { 9, 10 } }, 0,
                                 new int[,] { { 1, 10 }, { 8, 10 } }, "A Poorly Drawn W (I Tried)", 0.1f, 4, tip: "It's actually 2 V's intersecting very weirdly. Oh you probably came here for a hint. The place where you go through the buffer will be very important, since it determines how you'll clear the line and how you'll build to the objective."));
        //level 13, ended looking even funnier than I imagined. Created as a replacement for the old level 11.
        levels.Add(new LevelData(new int[,] { { 4, 0 }, { 5, 0 }, { 3, 1 }, { 6, 1 }, { 2, 2 }, { 7, 2 }, { 1, 3 }, { 8, 3 }, { 0, 4 }, { 9, 4 },
                                              { 4, 9 }, { 5, 9 }, { 3, 8 }, { 6, 8 }, { 2, 7 }, { 7, 7 }, { 1, 6 }, { 8, 6 }, { 0, 5 }, { 9, 5 } }, 0,
                                 new int[,] { { 3, 4 }, { 3, 5 }, { 4, 6 }, { 5, 6 }, { 6, 5 }, { 6, 4 }, { 5, 3 }, { 4, 3 }, { 2, 10 }, { 7, 10 } }, "Open Wide!", 0.1f, 4));
        //level 14, an introduction to the "follow the path" levels.
        levels.Add(new LevelData(1, 5, new int[,] { { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 }, { 7, 3 }, { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 }, { 6, 5 }, { 6, 6 }, { 6, 7 }, { 6, 8 }, { 6, 9 },
                                                    { 7, 4 }, { 7, 5 }, { 7, 6 }, { 7, 7 }, { 7, 8 }, { 7, 9 }, { 5, 9 }, { 4, 9 }, { 3, 9 }, { 7, 10 }, { 6, 10 }, { 5, 10 }, { 4, 10 }, { 3, 10 } }, "Follow the Curve", 0.2f, 2, tip: "It's very hard to cleanly make a 2 tile wide line. Use buffers carefully."));
        //level 15
        levels.Add(new LevelData(new int[,] { { 4, 0 }, { 4, 1 }, { 4, 2 }, { 4, 3 }, { 4, 4 }, { 4, 5 }, { 4, 6 }, { 5, 6 }, { 6, 6 }, { 7, 6 }, { 8, 6 }, { 8, 7 }, { 9, 7 }, { 9, 8 },
                                              { 9, 14 }, { 9, 15 }, { 8, 15 }, { 8, 16 }, { 7, 16 }, { 6, 16 }, { 5, 16 }, { 4, 16 }, { 3, 16 }, { 2, 16 }, { 5, 10 }, { 6, 10 }, { 5, 11 }, { 6, 11 }, { 5, 12 }, { 6, 12 } }, 0, 
                                 new int[,] { { 2, 0 }, { 3, 0 }, { 2, 1 }, { 3, 1 }, { 2, 2 }, { 3, 2 }, { 2, 3 }, { 3, 3 }, { 2, 4 }, { 3, 4 }, { 2, 5 }, { 3, 5 }, { 2, 6 }, { 3, 6 }, { 2, 7 }, { 3, 7 },
                                              { 2, 8 }, { 3, 8 }, { 2, 9 }, { 3, 9 }, { 2, 10 }, { 3, 10 }, { 2, 11 }, { 3, 11 }, { 2, 12 }, { 3, 12 }, { 2, 13 }, { 3, 13 }, { 2, 14 }, { 3, 14 }, { 2, 15 }, { 3, 15 },
                                              { 4, 7 }, { 4, 8 }, { 5, 7 }, { 5, 8 }, { 6, 7 }, { 6, 8 }, { 7, 7 }, { 7, 8 }, { 7, 9 }, { 8, 8 }, { 8, 9 }, { 8, 10 }, { 8, 11 }, { 8, 12 }, { 8, 13 },
                                              { 9, 9 }, { 9, 10 }, { 9, 11 }, { 9, 12 }, { 9, 13 }, { 7, 13 }, { 8, 14 }, { 7, 14 }, { 6, 14 }, { 5, 14 }, { 4, 14 }, { 7, 15 }, { 6, 15 }, { 5, 15 }, { 4, 15 } }, "P for Perfect", 0.1f, 4));
        //level 16
        levels.Add(new LevelData(1, 4, new int[,] { { 4, 0 }, { 5, 0 }, { 8, 0 }, { 9, 0 }, { 4, 1 }, { 5, 1 }, { 8, 1 }, { 9, 1 }, { 4, 2 }, { 5, 2 }, { 8, 2 }, { 9, 2 }, { 4, 3 }, { 5, 3 }, { 8, 3 }, { 9, 3 }, { 4, 4 }, { 5, 4 }, { 8, 4 }, { 9, 4 }, { 6, 4 }, { 7, 4 }, { 6, 5 }, { 7, 5 },
                                                    { 4, 5 }, { 5, 5 }, { 8, 5 }, { 9, 5 }, { 4, 6 }, { 5, 6 }, { 8, 6 }, { 9, 6 }, { 4, 7 }, { 5, 7 }, { 8, 7 }, { 9, 7 }, { 4, 8 }, { 5, 8 }, { 8, 8 }, { 9, 8 }, { 4, 9 }, { 5, 9 }, { 8, 9 }, { 9, 9 }, { 6, 8 }, { 7, 8 }, { 6, 9 }, { 7, 9 },
                                                    { 2, 6 }, { 3, 6 }, { 2, 7 }, { 3, 7 }, { 2, 8 }, { 3, 8 }, { 2, 9 }, { 3, 9 }, { 2, 10 }, { 3, 10 }, { 2, 11 }, { 3, 11 }, { 2, 12 }, { 3, 12 }, { 2, 13 }, { 3, 13 },
                                                    { 0, 14 }, { 1, 14 }, { 2, 14 }, { 3, 14 }, { 4, 14 }, { 5, 14 }, { 0, 15 }, { 1, 15 }, { 2, 15 }, { 3, 15 }, { 4, 15 }, { 5, 15 } }, "Thanks For Playing", 0.05f, 7));
        //Challenge Level 1, based on levels 9, 10, 12, and 13 (an entire line is blocked off, requiring you to clear a line to get past it.)
        levels.Add(new LevelData(new int[,] { { 0, 7 }, { 1, 8 }, { 2, 7 }, { 3, 8 }, { 4, 7 }, { 5, 8 }, { 6, 7 }, { 7, 8 }, { 8, 7 }, { 9, 8 } }, 0, new int[,] { { 4, 10 }, { 4, 11 }, { 5, 10 }, { 5, 11 } }, "The Impenetrable Wall", 0.1f, 10, tip: "This is much harder than it initially seems. It may be easy to get to the first layer and place a piece to set up for the second layer, but clearing the line without making artifacts in either layer will be difficult. This time, you'll need to build up over the wall a bit and plan out how the lines will be cleared."));
        //Challenge Level 2, based on levels 14-16 (you're forced to follow a "path")
        levels.Add(new LevelData(1, 2, new int[,] { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 1, 1 }, { 2, 1 }, { 3, 1 }, { 1, 2 }, { 2, 2 }, { 3, 2 }, { 1, 3 }, { 2, 3 }, { 3, 3 }, { 1, 4 }, { 2, 4 }, { 3, 4 }, { 1, 5 }, { 2, 5 }, { 3, 5 }, { 1, 6 }, { 2, 6 }, { 3, 6 }, { 1, 7 }, { 2, 7 }, { 3, 7 }, { 0, 6 }, { 0, 7 },
                                                    { 4, 5 }, { 5, 5 }, { 4, 6 }, { 5, 6 }, { 4, 7 }, { 5, 7 }, { 3, 8 }, { 4, 8 }, { 5, 8 }, { 3, 9 }, { 4, 9 }, { 5, 9 }, { 3, 10 }, { 4, 10 }, { 5, 10 }, { 1, 10 }, { 1, 11 }, { 1, 12 }, { 2, 10 }, { 2, 11 }, { 2, 12 },
                                                    { 3, 11 }, { 3, 12 }, { 4, 11 }, { 4, 12 }, { 5, 11 }, { 5, 12 }, { 6, 10 }, { 6, 11 }, { 6, 12 }, { 7, 10 }, { 7, 11 }, { 7, 12 }, { 8, 10 }, { 8, 11 }, { 8, 12 }, { 9, 10 }, { 9, 11 }, { 9, 12 }, { 7, 13 }, { 8, 13 }, { 9, 13 }, { 7, 14 }, { 8, 14 }, { 9, 14 }, { 6, 15 }, { 7, 15 }, { 8, 15 }, { 9, 15 }, { 6, 16 }, { 7, 16 }, { 8, 16 }, { 9, 16 },
                                                    { 7, 9 }, { 8, 9 }, { 9, 9 }, { 7, 8 }, { 8, 8 }, { 9, 8 }, { 7, 7 }, { 8, 7 }, { 9, 7 }, { 7, 6 }, { 8, 6 }, { 9, 6 }, { 7, 5 }, { 8, 5 }, { 9, 5 }, { 7, 4 }, { 8, 4 }, { 9, 4 }, { 7, 3 }, { 8, 3 }, { 9, 3 }, { 7, 2 }, { 8, 2 }, { 9, 2 }, { 7, 1 }, { 8, 1 }, { 9, 1 }, { 5, 1 }, { 5, 2 }, { 5, 3 }, { 6, 1 }, { 6, 2 }, { 6, 3 } }, "The Snaking Maze", 0.1f, 5, tip: "Towering straight down is impossible in Tetris. Instead, try building across the gap at the bottom and clearing a line to get a foothold there. That way, you can use the buffers for something else. Also, when you get to the top, take advantage of the thin border by using a buffer to allow yourself to place junk tiles above the maze."));
        //Challenge Level 3, more tame version of old level 11.
        levels.Add(new LevelData(new int[,] { { 4, 0 }, { 5, 0 }, { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 4, 1 }, { 5, 1 }, { 6, 1 }, { 7, 1 }, { 8, 1 }, { 9, 1 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 7, 2 }, { 8, 2 }, { 9, 2 }, { 4, 3 }, { 5, 3 }, { 4, 4 }, { 5, 4 },
                                              { 0, 5 }, { 1, 5 }, { 2, 5 }, { 3, 5 }, { 4, 5 }, { 5, 5 }, { 0, 6 }, { 1, 6 }, { 2, 6 }, { 3, 6 }, { 4, 6 }, { 5, 6 }, { 0, 7 }, { 1, 7 }, { 2, 7 }, { 3, 7 }, { 4, 7 }, { 5, 7 }, { 0, 8 }, { 5, 8 }, { 6, 8 }, { 0, 9 }, { 5, 9 }, { 6, 9 },
                                              { 0, 10 }, { 5, 10 }, { 6, 10 }, { 7, 10 }, { 8, 10 }, { 9, 10 }, { 0, 11 }, { 5, 11 }, { 6, 11 }, { 7, 11 }, { 8, 11 }, { 9, 11 }, { 0, 12 }, { 5, 12 }, { 6, 12 }, { 7, 12 }, { 8, 12 }, { 9, 12 }, { 0, 13 }, { 5, 13 }, { 6, 13 }, 
                                              { 0, 14 }, { 1, 14 }, { 2, 14 }, { 3, 14 }, { 4, 14 }, { 5, 14 }, { 6, 14 }, { 0, 15 }, { 1, 15 }, { 2, 15 }, { 3, 15 }, { 4, 15 }, { 5, 15 }, { 6, 15 }, { 0, 16 }, { 1, 16 }, { 2, 16 }, { 3, 16 }, { 4, 16 }, { 5, 16 }, { 6, 16 }, { 0, 17 }, { 1, 17 }, { 2, 17 }, { 3, 17 }, { 4, 17 }, { 5, 17 }, { 6, 17 } }, 2, new int[,] { { 7, 17 }, { 8, 17 }, { 9, 17 } }, "Windows of Opportunity", 0.1f, 12, tip: "You'll need to bridge across the windows in a way that allows you to clear the lines once you've gotten there. I (light blue), J (blue), and L (orange) pieces can do well for most of the bridges, but for the last one you'll also need to use the S (green) piece to finish the bridge while still giving room for clearing the line. It's highly recommended you use 2-3 lines for each crossing."));
        //Challenge Level 4, based on level 8 (the vault)
        levels.Add(new LevelData(new int[,] { { 2, 7 }, { 2, 8 }, { 2, 9 }, { 2, 10 }, { 2, 11 }, { 3, 12 }, { 4, 12 }, { 5, 12 }, { 6, 12 }, { 7, 12 }, { 8, 11 }, { 8, 10 }, { 8, 9 }, { 8, 8 }, { 8, 7 }, { 7, 6 }, { 6, 6 }, { 5, 6 }, { 4, 6 }, { 3, 6 }, { 4, 9 }, { 5, 8 }, { 5, 10 }, { 6, 9 } }, 0, new int[,] { { 3, 7 }, { 3, 11 }, { 7, 7 }, { 7, 11 }, { 5, 9 } },
                                 "Bullseye", 0.1f, 5, tip: "Here, you have to worry about getting to the center tile while making sure you get the bottom corner tiles (trust me, you can miss those if you focus too hard on the center)." /*don't ask*/ + " It's recommended to tower on both sides so that it's easier to clear lines, but you should do most of the work on the left side where you have more room."));
        //Scrapped Level. Was originally going to be level 11 and was the entire reason that I made a challenge levels section, and yet was deemed too unfair to be a challenge level. 
        //Scrapped due to lack of room causing too much RNG dependency. Could not be fixed no matter how much I increased the room.
        //levels.Add(new LevelData(new int[,] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 0, 1 }, { 1, 1 }, { 2, 1 }, { 7, 1 }, { 8, 1 }, { 9, 1 }, { 0, 2 }, { 1, 2 }, { 2, 2 }, { 7, 2 }, { 8, 2 }, { 9, 2 },
        //                                      { 3, 3 }, { 6, 3 }, { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 }, { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 },
        //                                      { 2, 6 }, { 7, 6 }, { 0, 7 }, { 1, 7 }, { 2, 7 }, { 7, 7 }, { 8, 7 }, { 9, 7 }, { 0, 8 }, { 1, 8 }, { 2, 8 }, { 7, 8 }, { 8, 8 }, { 9, 8 },
        //                                      { 3, 9 }, { 6, 9 }, { 3, 10 }, { 4, 10 }, { 5, 10 }, { 6, 10 }, { 3, 11 }, { 4, 11 }, { 5, 11 }, { 6, 11 },
        //                                      { 2, 12 }, { 7, 12 }, { 0, 13 }, { 1, 13 }, { 2, 13 }, { 7, 13 }, { 8, 13 }, { 9, 13 }, { 0, 14 }, { 1, 14 }, { 2, 14 }, { 7, 14 }, { 8, 14 }, { 9, 14 },
        //                                      { 3, 15 }, { 6, 15 }, { 3, 16 }, { 4, 16 }, { 5, 16 }, { 6, 16 }, { 3, 17 }, { 4, 17 }, { 5, 17 }, { 6, 17 },
        //                                      { 2, 18 }, { 7, 18 }, { 0, 19 }, { 1, 19 }, { 2, 19 }, { 7, 19 }, { 8, 19 }, { 9, 19 } }, 2, new int[,] { { 4, 18 }, { 5, 18 } }, "The Great Tower", 0.05f, 14, tip: "Remember when you had to clear a line to get past a barrier? This is just like that, only you have to do it 6 times with much less space. You don't have much space to place junk pieces, so if you feel RNG dealt you a bad hand, you can use a buffer to skip a section or clear lines to cycle through pieces."));
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
        if (level < 16)
            levelText.text = "Level " + (level + 1) + ": " + levels[level].title;
        else
            levelText.text = "Challenge Level " + (level - 15) + ": " + levels[level].title;
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
        levelText.text = "Tutorial " + (level + 1) + ": " + tutorials[level].title;
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
