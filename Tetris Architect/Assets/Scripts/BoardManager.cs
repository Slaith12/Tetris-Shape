using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [SerializeField] GameObject emptyTile;
    [SerializeField] GameObject winScreen;
    [SerializeField] Text lineDropText;
    [SerializeField] Image[] pieceImages;
    public Sprite[] pieceSprites;

    public Tile[,] tiles = new Tile[10, 20];
    public List<Piece> nextPieces; //this is literally a queue but i'm using a list instead. i don't know why

    int clearedLines;
    bool[] availablePieces = new bool[] { true, true, true, true, true, true, true };

    PieceMovement pieceMovement;
    ObjectiveManager objectiveManager;
    
    void Start()
    {
        winScreen.SetActive(false);
        pieceMovement = GetComponent<PieceMovement>();
        objectiveManager = GetComponent<ObjectiveManager>();
        nextPieces = new List<Piece>();
        for (int i = 0; i < 10; i++)
        {
            for(int j = 0; j < 20; j++)
            {
                tiles[i, j] = Instantiate(emptyTile, transform).GetComponent<Tile>();
                tiles[i, j].transform.position = new Vector3(i, j, 0);
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
                tiles[i, j].State = TileState.Empty;
                tiles[i, j].ObjState = ObjectiveState.Regular;
            }
        }
        winScreen.SetActive(false);
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
            winScreen.SetActive(true);
            if(!objectiveManager.onTutorial && (!PlayerPrefs.HasKey("Unlocked Levels") || PlayerPrefs.GetInt("Unlocked Levels") <= objectiveManager.currentLevel))
            {
                PlayerPrefs.SetInt("Unlocked Levels", objectiveManager.currentLevel + 1);
            }
            pieceMovement.enabled = false;
            return;
        }
        pieceMovement.GetNewPiece(TakePiece());
    }

    public void SetTile(int x, int y, TileState newState)
    {
        if(GetTile(x,y)?.allowChanges == true)
            tiles[x,y].State = newState;
    }

    public void SetTile(int x, int y, Piece newPiece)
    {
        if (GetTile(x, y)?.allowChanges == true)
            tiles[x, y].Piece = newPiece;
    }

    public void SetTile(int x, int y, TileState newState, Piece newPiece)
    {
        if (GetTile(x, y)?.allowChanges == true)
        {
            tiles[x, y].State = newState;
            tiles[x, y].Piece = newPiece;
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x > 9)
        {
            return null;
        }
        if (y < 0 || y > 19)
        {
            return null;
        }
        return tiles[x, y];
    }

    bool CheckLine(int line)
    {
        for (int j = 0; j < 10; j++) //go through each tile in the line to look at it's state
        {
            if (tiles[j, line].State != TileState.Filled)
            {
                return false;
            }
        }
        return true;
    }

    public void BeginLevel(int startingTopRow, float startSpeed)
    {
        pieceMovement.enabled = true;
        clearedLines = 0;
        for(int row = startingTopRow; row < 20; row++)
        {
            for(int tile = 0; tile < 10; tile++)
            {
                tiles[tile, row].State = TileState.Blocked;
            }
        }
        availablePieces = new bool[] { true, true, true, true, true, true, true };
        nextPieces = new List<Piece>();
        for (int i = 0; i < 3; i++)
        {
            nextPieces.Add(GetNewPiece());
        }
        UpdateNextQueue();
        lineDropText.text = "Top line falls after\n" + objectiveManager.lineDrop + " line clears.";
        pieceMovement.gravitySpeed = startSpeed;
        pieceMovement.ResetPieces(startingTopRow);
        pieceMovement.GetNewPiece(TakePiece());
    }

    public Piece TakePiece()
    {
        Piece nextPiece = nextPieces[0];
        nextPieces.RemoveAt(0);
        nextPieces.Insert(2, GetNewPiece());
        UpdateNextQueue();
        return nextPiece; //spawn the new piece
    }

    Piece GetNewPiece()
    {
        int pieceNumber;
        do
        {
            Debug.Log("Getting new number");
            //get a new rng value to determine the next piece
            pieceNumber = Random.Range(0, 7);
        }
        while (!availablePieces[pieceNumber]); //if the current rng value isn't allowed by the bag, get a new rng value

        availablePieces[pieceNumber] = false; //take the current rng value out of the bag
        foreach (bool piece in availablePieces)
        {
            if (piece)
                return (Piece)pieceNumber;
        }
        availablePieces = new bool[] { true, true, true, true, true, true, true };
        return (Piece)pieceNumber;
    }

    public void UpdateNextQueue()
    {
        for(int i = 0; i < pieceImages.Length; i++)
        {
            pieceImages[i].sprite = pieceSprites[(int)nextPieces[i]];
        }
    }

    void ClearLine(int line)
    {
        clearedLines++;
        for(int i = 0; i < 10; i++)
        {
            tiles[i, line].State = TileState.Empty;
        }
        for(int i = line+1; i < 20; i++)
        {
            if(tiles[0,i].State == TileState.Blocked)
            {
                for(int j = 0; j < 10; j++)
                {
                    tiles[j, i - 1].State = TileState.Empty;
                }
                break;
            }
            for(int j = 0; j < 10; j++)
            {
                tiles[j, i - 1].State = tiles[j, i].State;
                tiles[j, i - 1].Piece = tiles[j, i].Piece;
            }
        }
        pieceMovement.gravitySpeed += objectiveManager.speedIncrease;
        if(clearedLines % objectiveManager.lineDrop == 0)
        {
            pieceMovement.topRow--;
            for (int i = 0; i < 10; i++)
            {
                tiles[i, pieceMovement.topRow].State = TileState.Blocked;
            }
        }
        lineDropText.text = "Top line falls after\n" + (objectiveManager.lineDrop - (clearedLines % objectiveManager.lineDrop)) + " line clears.";
    }
}