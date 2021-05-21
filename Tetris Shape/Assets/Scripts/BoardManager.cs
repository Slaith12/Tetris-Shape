using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoardManager : MonoBehaviour
{
    public Tile[,] tiles = new Tile[10,20];
    [SerializeField] GameObject emptyTile; //prefab
    [SerializeField] GameObject canvas;

    int clearedLines;
    bool[] availablePieces = new bool[] { true, true, true, true, true, true, true };

    PieceMovement pieceMovement;
    ObjectiveManager objectiveManager;
    
    void Start()
    {
        canvas.SetActive(false);
        pieceMovement = GetComponent<PieceMovement>();
        objectiveManager = GetComponent<ObjectiveManager>();
        for(int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                tiles[i,j] = Instantiate(emptyTile,transform).GetComponent<Tile>();
                tiles[i,j].transform.position = new Vector3(i,j,0);
            }
        }
        pieceMovement.Init();
        objectiveManager.Init();
        clearedLines = 0;
    }
    
    void Update()
    {
        
    }

    public void ClearBoard()
    {
        for(int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                tiles[i, j].state = TileState.Empty;
                tiles[i, j].obj = ObjectiveState.Regular;
            }
        }
        canvas.SetActive(false);
    }

    public void UpdateBoard()
    {
        for(int i = 0; i < pieceMovement.topRow; i++) //go through each line to see if any of them should be cleared
        {
            if (CheckLine(i))
            {
                ClearLine(i); //if all tiles are filled, clear the line
                i--;
            }
        }
        if(objectiveManager.CheckObjective())
        {
            canvas.SetActive(true);
            if(!PlayerPrefs.HasKey("Unlocked Levels") || PlayerPrefs.GetInt("Unlocked Levels") < objectiveManager.currentLevel)
            {
                PlayerPrefs.SetInt("Unlocked Levels", objectiveManager.currentLevel);
            }
            pieceMovement.enabled = false;
            return;
        }
        GetNewPiece();
    }

    public void SetTile(int x, int y, TileState newState)
    {
        if(x < 0 || x > 9)
        {
            return;
        }
        if(y < 0 || y > 19)
        {
            return;
        }
        if(tiles[x,y].allowChanges)
        {
            tiles[x, y].state = newState;
            return;
        }
    }

    public Tile GetTile(int x, int y)
    {
        if(x < 0 || x > 9)
        {
            return null;
        }
        if(y < 0 || y > 19)
        {
            return null;
        }
        return tiles[x, y];
    }

    bool CheckLine(int line)
    {
        for (int j = 0; j < 10; j++) //go through each tile in the line to look at it's state
        {
            if (tiles[j, line].state != TileState.Filled)
            {
                return false;
            }
        }
        return true;
    }

    public void BeginLevel(int startingTopRow, float startSpeed)
    {
        pieceMovement.topRow = startingTopRow;
        for(int row = startingTopRow; row < 20; row++)
        {
            for(int tile = 0; tile < 10; tile++)
            {
                tiles[tile, row].state = TileState.Blocked;
            }
        }
        pieceMovement.gravitySpeed = startSpeed;
        pieceMovement.ResetPieces();
        availablePieces = new bool[] { true, true, true, true, true, true, true };
        GetNewPiece();
    }

    public void GetNewPiece()
    {
        int pieceNumber;
        do
        {
            Debug.Log("Getting new number");
            //get a new rng value to determine the next piece
            pieceNumber = Random.Range(0,7);
        }
        while (!availablePieces[pieceNumber]); //if the current rng value isn't allowed by the bag, get a new rng value
            
        availablePieces[pieceNumber] = false; //take the current rng value out of the bag
        pieceMovement.GetNewPiece((Piece)(pieceNumber + 1)); //spawn the new piece, +1 is because the single piece is the first piece in the enum
        foreach(bool piece in availablePieces)
        {
            if (piece)
                return;
        }
        availablePieces = new bool[] { true, true, true, true, true, true, true };
    }

    void ClearLine(int line)
    {
        clearedLines++;
        for(int i = 0; i < 10; i++)
        {
            tiles[i, line].state = TileState.Empty;
        }
        for(int i = line+1; i < 20; i++)
        {
            if(tiles[0,i].state == TileState.Blocked)
            {
                for(int j = 0; j < 10; j++)
                {
                    tiles[j, i - 1].state = TileState.Empty;
                }
                break;
            }
            for(int j = 0; j < 10; j++)
            {
                tiles[j, i - 1].state = tiles[j, i].state;
            }
        }
        pieceMovement.gravitySpeed += objectiveManager.speedIncrease;
        if(clearedLines % objectiveManager.lineDrop == 0)
        {
            pieceMovement.topRow--;
            for (int i = 0; i < 10; i++)
            {
                tiles[i, pieceMovement.topRow].state = TileState.Blocked;
            }
        }
    }
}