using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Piece { single = -1, J, S, L, Z, T, I, O}
public class PieceMovement : MonoBehaviour
{
    //references
    BoardManager boardManager;
    [SerializeField] Text holdText;

    //objective related variables
    public float gravitySpeed;
    public int topRow;

    //timers
    float DASTimer;
    float gravityTimer;

    //piece data
    int centerX;
    int centerY;
    Piece piece;
    int rotation;
    int[][] pieceTiles = new int[4][];

    //constants
    const float SOFTDROPSPEED = 10;
    const float DASDELAY = 0.5f; //the delay between when the button starts getting held and when DAS starts
    const float DASRATE = 0.05f; //the delay between each DAS movement
    /// <summary>
    /// The position of the tiles of each piece based on the center tile.
    /// I and O pieces are not included since their "center tile" is not where their pivot is.
    /// </summary>
    readonly int[,,] tileOffsets = new int[,,] { { { -1, 1 }, { -1, 0 }, { 0, 0 }, { 1, 0 } }, //J piece offsets
                                        { { 1, 1 }, { 0, 1 }, { 0, 0 }, { -1, 0 } }, //S piece offsets
                                        { { 1, 1 }, { 1, 0 }, { 0, 0 }, { -1, 0 } }, //L piece offsets
                                        { { -1, 1 }, { 0, 1 }, { 0, 0 }, { 1, 0 } }, //Z piece offsets
                                        { { -1, 0 }, { 0, 1 }, { 1, 0 }, { 0, 0 } } }; //T piece offsets

    readonly int[,] wallKickOffsets = new int[,] { { 0, 0 }, { -1, 0 }, { -1, 1 }, { 0, -2 }, { -1, -2 } };
    readonly int[,,] iPieceKickOffsets = new int[,,] { { { 0, 0 } , { -2, 0 }, { 1, 0 }, { -2, -1 } , { 1, 2 } },
                                                       { { 0, 0 }, { -1, 0 }, { 2, 0 }, { -1, 2 }, { 2, -1 } } };

    //misc
    public Piece heldPiece;
    bool holdCooldown;

    //Use this for initialization
    public void Init()
    {
        boardManager = GetComponent<BoardManager>();
        holdText.text = "Holding: None";
        heldPiece = Piece.single;
        DASTimer = 0;
        gravityTimer = 0;
        topRow = 20;
        //GetNewPiece(Piece.I);
    }
    
    void Update()
    {
        Gravity();
        DASTimer -= Time.deltaTime;
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if(Input.GetKey(KeyCode.RightArrow)) //prevent left and right at the same time. meant for Delayed Auto Shift. (not implemented yet)
        {
            MoveRight();
        }
        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(true);
        }
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            Rotate(false);
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Hold();
        }
    }

    //Main Control Functions

    void MoveLeft()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLocation(-1, 0);
            DASTimer = DASDELAY;
            return;
        }
        if(DASTimer > 0)
        {
            return;
        }
        ChangeLocation(-1, 0);
        DASTimer = DASRATE;
    }

    void MoveRight()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLocation(1, 0);
            DASTimer = DASDELAY;
            return;
        }
        if(DASTimer > 0)
        {
            return;
        }
        ChangeLocation(1, 0);
        DASTimer = DASRATE;
    }

    void Hold()
    {
        if(holdCooldown)
        {
            return;
        }
        SetPieceTiles(TileState.Empty);
        Piece placeholder = piece;
        if(heldPiece != Piece.single)
        {
            GetNewPiece(heldPiece);
        }
        else
        {
            boardManager.GetNewPiece();
        }
        heldPiece = placeholder;
        holdText.text = "Holding: " + heldPiece;
        holdCooldown = true;
    }

    void Rotate(bool clockwise)
    {
        rotation += clockwise ? 1 : -1;
        if(rotation > 3)
        {
            rotation = 0;
        }
        if(rotation < 0)
        {
            rotation = 3;
        }
        if (piece == Piece.single || piece == Piece.O) //the single and O pieces don't have any rotation
        {
            return;
        }
        SetPieceTiles(TileState.Empty);
        SetRotation(rotation);
        if(!WallKick(clockwise))
        {
            rotation -= clockwise ? 1 : -1;
            if (rotation > 3)
            {
                rotation = 0;
            }
            if (rotation < 0)
            {
                rotation = 3;
            }
            SetRotation(rotation);
        }
        SetPieceTiles(TileState.Active);
    }

    //Rotation Functions

    void SetRotation(int rotation)
    {
        if(piece == Piece.I)
        {
            switch (rotation) //the center piece is always the top right tile of the center 4 tiles
            {
                case 0:
                    for (int i = 0; i < 4; i++)
                    {
                        pieceTiles[i] = new int[] { centerX + (i - 2), centerY };
                    }
                    return;
                case 1:
                    for (int i = 0; i < 4; i++)
                    {
                        pieceTiles[i] = new int[] { centerX, centerY - (i - 1) };
                    }
                    return;
                case 2:
                    for (int i = 0; i < 4; i++)
                    {
                        pieceTiles[i] = new int[] { centerX + (i - 2), centerY - 1 };
                    }
                    return;
                case 3:
                    for (int i = 0; i < 4; i++)
                    {
                        pieceTiles[i] = new int[] { centerX - 1, centerY - (i - 1) };
                    }
                    return;
            }
        }
        if(piece == Piece.O)
        {
            return;
        }
        for(int i = 0; i < 4; i++)
        {
            int offsetX = tileOffsets[(int)piece - 1, i, 0];
            int offsetY = tileOffsets[(int)piece - 1, i, 1];
            if(rotation >= 2)
            {
                offsetX *= -1;
                offsetY *= -1;
            }
            if(rotation % 2 == 1)
            {
                int placeholder = offsetX;
                offsetX = offsetY;
                offsetY = -placeholder;
            }
            pieceTiles[i] = new int[] { centerX + offsetX, centerY + offsetY };
        }
    }

    bool WallKick(bool clockwise)
    {
        if (piece == Piece.I) //the I piece has a special wall kicks
        {
            return IPieceWallKick(clockwise);
        }
        //all of these calculations below is used to reduce what would be 8 arrays to 1 array
        int offsetXMult;
        int offsetYMult;
        if (clockwise)
        {
            offsetXMult = rotation < 2 ? 1 : -1;
            offsetYMult = rotation % 2 == 0 ? -1 : 1;
        }
        else
        {
            offsetXMult = rotation % 3 == 0 ? -1 : 1;
            offsetYMult = rotation % 2 == 0 ? -1 : 1;
        }
        for (int i = 0; i < 5; i++)
        {
            int offSetX = wallKickOffsets[i, 0] * offsetXMult;
            int offSetY = wallKickOffsets[i, 1] * offsetYMult;
            if (IsValidMove(offSetX, offSetY))
            {
                centerX += offSetX;
                centerY += offSetY;
                foreach (int[] pieceCoords in pieceTiles)
                {
                    pieceCoords[0] += offSetX;
                    pieceCoords[1] += offSetY;
                }
                return true;
            }
        }
        return false;
    }

    bool IPieceWallKick(bool clockwise)
    {
        //all of these calculations below is used to reduce what would be 8 arrays into 2 arrays
        int offsetMult;
        if (clockwise)
        {
            offsetMult = rotation % 3 == 0 ? -1 : 1;
        }
        else
        {
            offsetMult = rotation < 2 ? -1 : 1;
        }
        for (int i = 0; i < 5; i++)
        {
            int offSetX;
            int offSetY;
            if (clockwise)
            {
                offSetX = iPieceKickOffsets[(rotation + 1) % 2, i, 0] * offsetMult;
                offSetY = iPieceKickOffsets[(rotation + 1) % 2, i, 1] * offsetMult;
            }
            else
            {
                offSetX = iPieceKickOffsets[rotation % 2, i, 0] * offsetMult;
                offSetY = iPieceKickOffsets[rotation % 2, i, 1] * offsetMult;
            }
            if (IsValidMove(offSetX, offSetY))
            {
                centerX += offSetX;
                centerY += offSetY;
                foreach (int[] pieceCoords in pieceTiles)
                {
                    pieceCoords[0] += offSetX;
                    pieceCoords[1] += offSetY;
                }
                return true;
            }
        }
        return false;
    }

    //Gravity Functions

    void Gravity()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            gravityTimer -= Time.deltaTime * SOFTDROPSPEED;
        }
        else
        {
            gravityTimer -= Time.deltaTime;
        }
        if (gravityTimer <= 0f)
        {
            if (!ChangeLocation(0, -1))
            {
                PlacePiece();
            }
            gravityTimer = 1 / gravitySpeed;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            while (ChangeLocation(0, -1)) ;
            PlacePiece();
            gravityTimer = 1 / gravitySpeed;
        }
    }

    void PlacePiece()
    {
        holdCooldown = false;
        if (piece == Piece.single)
        {
            Debug.Log("Filling in a single piece at line " + centerY + " column " + centerX);
            boardManager.SetTile(centerX, centerY, TileState.Filled);
        }
        else
        {
            Debug.Log("Filling in a " + piece + " piece at line " + centerY + " column " + centerX);
            foreach (int[] pieceCoords in pieceTiles)
            {
                if (pieceCoords[1] >= topRow)
                {
                    //fail the level
                    Debug.Log("Game Over");
                    this.enabled = false;
                    return;
                }
                boardManager.SetTile(pieceCoords[0], pieceCoords[1], TileState.Filled);
            }
        }
        boardManager.UpdateBoard();
    }

    //Helper Functions

    public void GetNewPiece(Piece newPiece)
    {
        centerX = 5;
        centerY = topRow;
        piece = newPiece;
        rotation = 0;
        gravityTimer = 1 / gravitySpeed;
        Debug.Log("Getting new " + newPiece + " piece.");
        //the I and O pieces are special cases, so they're handled seperately
        if(piece == Piece.I)
        {
            pieceTiles = new int[][] { new int[] { 3, topRow }, new int[] { 4, topRow }, new int[] { 5, topRow }, new int[] { 6, topRow } };
            return;
        }
        if(piece == Piece.O)
        {
            pieceTiles = new int[][] { new int[] { 4, topRow }, new int[] { 5, topRow }, new int[] { 4, topRow + 1 }, new int[] { 5, topRow + 1 } };
            return;
        }
        pieceTiles = new int[4][];
        for(int i = 0; i < 4; i++)
        {
            pieceTiles[i] = new int[] { centerX + tileOffsets[(int)piece, i, 0], centerY + tileOffsets[(int)piece, i, 1] };
        }
    }

    bool ChangeLocation(int x, int y)
    {
        if (!IsValidMove(x, y))
            return false;
        if (piece == Piece.single)
        {
            boardManager.SetTile(centerX, centerY, TileState.Empty);
            boardManager.SetTile(centerX + x, centerY + y, TileState.Active);
        }
        else
        {
            SetPieceTiles(TileState.Empty);
            for(int i = 0; i < 4; i++)
            {
                int tileX = pieceTiles[i][0];
                int tileY = pieceTiles[i][1];
                boardManager.SetTile(tileX + x, tileY + y,TileState.Active);
                pieceTiles[i] = new int[] { tileX + x, tileY + y };
            }
        }
        centerX += x;
        centerY += y;
        return true;
    }

    bool IsValidMove(int x, int y)
    {
        if (piece == Piece.single)
        {
            Tile tile = boardManager.GetTile(centerX + x, centerY + y);
            if (tile == null || tile.state == TileState.Filled)
                return false;
        }
        else
        {
            foreach(int[] pieceCoords in pieceTiles)
            {
                int tileX = pieceCoords[0];
                int tileY = pieceCoords[1];
                if (tileY > 19)
                    continue;
                Tile tile = boardManager.GetTile(tileX + x, tileY + y);
                if(tile == null || tile.state == TileState.Filled)
                {
                    Debug.Log("Movement not accepted because the tile collided with another tile");
                    return false;
                }
            }
        }
        return true;
    }

    public void ResetPieces(int topRow)
    {
        this.topRow = topRow;
        ResetPieces();
    }

    public void ResetPieces()
    {
        heldPiece = Piece.single;
        holdText.text = "Holding: None";
        holdCooldown = false;
    }
    void SetPieceTiles(TileState state)
    {
        foreach (int[] tileCoords in pieceTiles)
        {
            boardManager.SetTile(tileCoords[0], tileCoords[1], state);
        }
    }
}
