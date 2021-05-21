using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum Piece { single, J, S, L, Z, T, I, O}
public class PieceMovement : MonoBehaviour
{
    //references
    BoardManager boardManager;
    [SerializeField] Text holdText;

    //objective related variables
    public float gravitySpeed = 1f;
    float gravityTimer;
    public int topRow;

    //piece data
    int centerX;
    int centerY;
    Piece piece;
    int rotation;
    int[][] tilePieces = new int[4][];

    //constants
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
    [SerializeField] float softDropSpeed = 10;

    // Use this for initialization
    public void Init()
    {
        boardManager = GetComponent<BoardManager>();
        holdText.text = "Holding: None";
        topRow = 20;
        //GetNewPiece(Piece.I);
    }
    
    void Update()
    {
        Gravity();
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLocation(-1, 0);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)) //prevent left and right at the same time. meant for Delayed Auto Shift. (not implemented yet)
        {
            ChangeLocation(1, 0);
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

    void Hold()
    {
        if(holdCooldown)
        {
            return;
        }
        foreach(int[] pieceCoords in tilePieces)
        {
            if(pieceCoords[1] < topRow)
                boardManager.SetTile(pieceCoords[0], pieceCoords[1], TileState.Empty);
        }
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

    public void ResetPieces()
    {
        heldPiece = Piece.single;
        holdText.text = "Holding: None";
        holdCooldown = false;
    }

    void Rotate(bool clockwise)
    {
        rotation += clockwise ? 1 : -1;
        if(rotation >= 4)
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
        for(int i = 0; i < 4; i++)
        {
            foreach (int[] pieceCoords in tilePieces)
            {
                boardManager.SetTile(pieceCoords[0], pieceCoords[1], TileState.Empty);
            }
        }
        SetRotation(rotation);
        
        if(piece == Piece.I) //the I piece has a special kick.
        {
            if(!IPieceWallKick(clockwise))
            {
                rotation -= clockwise ? 1 : -1;
                if (rotation >= 4)
                {
                    rotation = 0;
                }
                if (rotation < 0)
                {
                    rotation = 3;
                }
                SetRotation(rotation);
            }
        }
        else if (!WallKick(clockwise))
        {
            rotation -= clockwise ? 1 : -1;
            if (rotation >= 4)
            {
                rotation = 0;
            }
            if (rotation < 0)
            {
                rotation = 3;
            }
            SetRotation(rotation);
        }
        foreach(int[] tileCoords in tilePieces)
        {
            boardManager.SetTile(tileCoords[0], tileCoords[1], TileState.Active);
        }
    }

    void SetRotation(int rotation)
    {
        if(piece == Piece.I)
        {
            switch (rotation) //the center piece is always the top right tile of the center 4 tiles
            {
                case 0:
                    for (int i = 0; i < 4; i++)
                    {
                        tilePieces[i] = new int[] { centerX + (i - 2), centerY };
                    }
                    return;
                case 1:
                    for (int i = 0; i < 4; i++)
                    {
                        tilePieces[i] = new int[] { centerX, centerY - (i - 1) };
                    }
                    return;
                case 2:
                    for (int i = 0; i < 4; i++)
                    {
                        tilePieces[i] = new int[] { centerX + (i - 2), centerY - 1 };
                    }
                    return;
                case 3:
                    for (int i = 0; i < 4; i++)
                    {
                        tilePieces[i] = new int[] { centerX - 1, centerY - (i - 1) };
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
            tilePieces[i] = new int[] { centerX + offsetX, centerY + offsetY };
        }
    }

    bool IPieceWallKick(bool clockwise)
    {
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
                offSetX = iPieceKickOffsets[(rotation) % 2, i, 0] * offsetMult;
                offSetY = iPieceKickOffsets[(rotation) % 2, i, 1] * offsetMult;
            }
            if (IsValidMove(offSetX, offSetY))
            {
                centerX += offSetX;
                centerY += offSetY;
                foreach (int[] pieceCoords in tilePieces)
                {
                    pieceCoords[0] += offSetX;
                    pieceCoords[1] += offSetY;
                }
                return true;
            }
        }
        return false;
    }

    bool WallKick(bool clockwise)
    {
        int offsetXMult;
        int offsetYMult;
        if(clockwise)
        {
            offsetXMult = rotation < 2 ? 1 : -1;
            offsetYMult = rotation % 2 == 0 ? -1 : 1;
        }
        else
        {
            offsetXMult = rotation % 3 == 0 ? -1 : 1;
            offsetYMult = rotation % 2 == 0 ? -1 : 1;
        }
        for(int i = 0; i < 5; i++)
        {
            int offSetX = wallKickOffsets[i, 0]*offsetXMult;
            int offSetY = wallKickOffsets[i, 1]*offsetYMult;
            if (IsValidMove(offSetX,offSetY))
            {
                centerX += offSetX;
                centerY += offSetY;
                foreach (int[] pieceCoords in tilePieces)
                {
                    pieceCoords[0] += offSetX;
                    pieceCoords[1] += offSetY;
                }
                return true;
            }
        }
        return false;
    }

    void Gravity()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            gravityTimer -= Time.deltaTime * softDropSpeed;
        }
        else
        {
            gravityTimer -= Time.deltaTime;
        }
        if (gravityTimer <= 0f)
        {
            if (!ChangeLocation(0, -1))
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
                    foreach (int[] pieceCoords in tilePieces)
                    {
                        Debug.Log(pieceCoords[0]);
                        Debug.Log(pieceCoords[1]);
                        Debug.Log(topRow);
                        if(pieceCoords[1] >= topRow)
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
            gravityTimer = 1 / gravitySpeed;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            holdCooldown = false;
            Debug.Log("Sending piece down");
            while (ChangeLocation(0, -1)) ;
            Debug.Log("Piece is at the bottom");
            if (piece == Piece.single)
            {
                Debug.Log("Filling in a single piece at line " + centerY + " column " + centerX);
                boardManager.SetTile(centerX, centerY, TileState.Filled);
            }
            else
            {
                Debug.Log("Filling in a " + piece + " piece at line " + centerY + " column " + centerX);
                foreach (int[] pieceCoords in tilePieces)
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
            gravityTimer = 1 / gravitySpeed;
        }
    }

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
            tilePieces = new int[][] { new int[] { 3, topRow }, new int[] { 4, topRow }, new int[] { 5, topRow }, new int[] { 6, topRow } };
            return;
        }
        if(piece == Piece.O)
        {
            tilePieces = new int[][] { new int[] { 4, topRow }, new int[] { 5, topRow }, new int[] { 4, topRow + 1 }, new int[] { 5, topRow + 1 } };
            return;
        }
        tilePieces = new int[4][];
        for(int i = 0; i < 4; i++)
        {
            tilePieces[i] = new int[] { centerX + tileOffsets[(int)piece - 1, i, 0], centerY + tileOffsets[(int)piece - 1, i, 1] };
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
            foreach (int[] pieceCoords in tilePieces)
            {
                boardManager.SetTile(pieceCoords[0], pieceCoords[1], TileState.Empty);
            }
            for(int i = 0; i < 4; i++) //these for loops have to be seperate or else some tiles won't get filled in when moving in certain directions.
            {
                int tileX = tilePieces[i][0];
                int tileY = tilePieces[i][1];
                boardManager.SetTile(tileX + x, tileY + y,TileState.Active);
                tilePieces[i] = new int[] { tileX + x, tileY + y };
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
            for(int i = 0; i < 4; i++)
            {
                int tileX = tilePieces[i][0];
                int tileY = tilePieces[i][1];
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
        heldPiece = Piece.single; //if the single piece is held, it's treated like there's nothing being held
        holdText.text = "Holding: None";
        this.topRow = topRow;
    }
}
