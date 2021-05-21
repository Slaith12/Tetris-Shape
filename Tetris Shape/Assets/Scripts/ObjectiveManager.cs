using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{
    public List<LevelData> levels = new List<LevelData>(); //the first item in the list shows {buffer thickness, buffer cells allowed}. the rest of the items are coordinates for the required cells.
    public int currentLevel;
    public float speedIncrease { get { return levels[currentLevel].speedIncrease; } }
    public int lineDrop { get { return levels[currentLevel].lineDrop; } }
    [SerializeField] Text bufferText;
    [SerializeField] Text levelText;
    [SerializeField] LevelTip tipText;
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
            for(int i = 0; i < bufferTiles.GetLength(0); i++)
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
    BoardManager boardManager;
    PieceMovement pieceMovement;
    

    // Use this for initilization
    public void Init()
    {
        boardManager = GetComponent<BoardManager>();
        pieceMovement = GetComponent<PieceMovement>();
        InitLevels();
        if (!PlayerPrefs.HasKey("Current Level"))
            PlayerPrefs.SetInt("Current Level", 0);
        StartLevel(PlayerPrefs.GetInt("Current Level"));
        bufferText.color = Color.green;
        bufferText.text = "0/" + levels[currentLevel].allowedBuffers + " buffers used.";
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            StartLevel(currentLevel);
        }
    }

    void GenerateTiles(LevelData objTiles)
    {
        foreach(int[] tile in objTiles.bufferTiles)
        {
            boardManager.GetTile(tile[0], tile[1]).obj = ObjectiveState.Buffer;
        }
        for(int tile = 0; tile < objTiles.requiredTiles.GetLength(0);tile++)
        {
            boardManager.GetTile(objTiles.requiredTiles[tile][0], objTiles.requiredTiles[tile][1]).obj = ObjectiveState.Required;
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
                    if(boardManager.GetTile(i, j).obj == ObjectiveState.Regular)
                    {
                        boardManager.GetTile(i, j).obj = ObjectiveState.Buffer;
                        objTiles.bufferTiles.Add(new int[] { i, j });
                        Debug.Log("Generating buffer tile at row " + j + " column " + i);
                    }
                }
            }
        }
    }
    void InitLevels()
    {
        //level 1
        levels.Add(new LevelData(1, 6 , new int[,]{ { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 },
                                                    { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 },
                                                    { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 },
                                                    { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 } }, 0.1f, 2));
        //level 2
        levels.Add(new LevelData(1, 8, new int[,] { { 4, 0 }, { 5, 0 }, { 3, 1 }, { 4, 1 }, { 5, 1 }, { 6, 1 }, { 2, 2 }, { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 7, 2 },
                                                    { 1, 3 }, { 2, 3 }, { 3, 3 }, { 4, 3 }, { 5, 3 }, { 6, 3 }, { 7, 3 }, { 8, 3 } }, 0.1f, 3));
        //level 3
        levels.Add(new LevelData(2, 10, new int[,] { { 3, 4 }, { 4, 4 }, { 5, 4 }, { 6, 4 },
                                                     { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 },
                                                     { 3, 6 }, { 4, 6 }, { 5, 6 }, { 6, 6 },
                                                     { 3, 7 }, { 4, 7 }, { 5, 7 }, { 6, 7 } }, 0.2f, 3));
        //level 4
        levels.Add(new LevelData(2, 10, new int[,] { { 3, 5 }, { 4, 6 }, { 5, 7 }, { 6, 8 }, { 6, 7 }, { 6, 6 }, { 6, 5 }, { 6, 4 }, { 6, 3 }, { 6, 2 }, { 5, 3 }, { 4, 4 }, { 4, 5 }, { 5, 4 },{ 5, 5 },{ 5, 6 } }, 0.2f, 3));
        //level 5
        levels.Add(new LevelData(1, 10, new int[,] { { 4, 3 }, { 5, 3 }, { 5, 4 }, { 6, 4 }, { 6, 5 },
                                                     { 7, 5 }, { 7, 6 }, { 6, 6 }, { 6, 7 }, { 5, 7 },
                                                     { 5, 8 }, { 4, 8 }, { 4, 7 }, { 3, 7 }, { 3, 6 }, { 2, 6 }, { 2, 5 }, { 3, 5 }, { 3, 4 }, { 4, 4 } }, 0.3f, 2));
        //level 6
        levels.Add(new LevelData(1, 0, new int[,] { { 4, 2 } }, 0.1f, 3));
        //level 7
        levels.Add(new LevelData(1, 0, new int[,] { { 3, 5 }, { 4, 5 }, { 5, 5 }, { 3, 6 }, { 4, 6 }, { 5, 6 }, { 4, 7 }, { 5, 7 } }, 0.05f, 5));
        //level 8
        levels.Add(new LevelData(new int[,] { { 3, 2 }, { 3, 3 }, { 3, 4 }, { 4, 2 }, { 4, 3 }, { 4, 4 }, { 5, 2 }, { 5, 3 }, { 5, 4 }, { 6, 2 }, { 6, 3 }, { 6, 4 } },
                                 0, new int[,] { { 3, 5 }, { 4, 5 }, { 5, 5 }, { 6, 5 } }, 0.2f, 3));
        //level 9
        levels.Add(new LevelData(new int[,] { { 3, 3 }, { 3, 2 }, { 4, 2 }, { 5, 2 }, { 6, 2 }, { 6, 3 }, { 7, 3 }, { 7, 4 }, { 8, 4 }, { 8, 5 },
                                              { 8, 6 }, { 8, 7 }, { 7, 7 }, { 7, 8 }, { 6, 8 }, { 6, 9 }, { 5, 9 }, { 4, 9 }, { 3, 9 }, { 3, 8 },
                                              { 2, 8 }, { 2, 7 }, { 1, 7 }, { 1, 6 }, { 1, 5 }, { 1, 4 }, { 2, 4 }, { 2, 3 } }, 4,
                                 new int[,] { { 4, 3 }, { 5, 3 }, { 6, 4 }, { 7, 5 }, { 7, 6 }, { 6, 7 }, { 5, 8 }, { 4, 8 }, { 3, 7 }, { 2, 6 }, { 2, 5 }, { 3, 4 } }, 0.2f, 2));
        //level 10
    }
    public void StartLevel(int level)
    {
        currentLevel = level;
        tipText.SetTip(levels[level].tip);
        levelText.text = "Level " + (level + 1);
        bufferText.text = "0/" + levels[currentLevel].allowedBuffers + " buffers used.";
        bufferText.color = Color.green;
        boardManager.ClearBoard();
        GenerateTiles(levels[level]);
        boardManager.BeginLevel(levels[level].startTop,levels[level].startSpeed);
    }
    public bool CheckObjective()
    {
        bool reqTilesFilled = true;
        foreach(int[] tiles in levels[currentLevel].requiredTiles)
        {
            switch(boardManager.GetTile(tiles[0],tiles[1])?.state)
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
        for (int i = 0; i < levels[currentLevel].bufferTiles.Count; i++)
        {
            switch (boardManager.GetTile(levels[currentLevel].bufferTiles[i][0], levels[currentLevel].bufferTiles[i][1])?.state)
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
        bufferText.text = buffers + "/" + levels[currentLevel].allowedBuffers + " buffers used.";
        bufferText.color = buffers > levels[currentLevel].allowedBuffers ? Color.red : Color.green;
        return reqTilesFilled && buffers <= levels[currentLevel].allowedBuffers;
    }
    public bool GoToNextLevel()
    {
        currentLevel++;
        if (levels.Count <= currentLevel)
        {
            return false;
        }
        PlayerPrefs.SetInt("Current Level", currentLevel);
        StartLevel(currentLevel);
        return true;
    }
}
