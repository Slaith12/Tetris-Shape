using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A tile is 2 enums (TileState and ObjectiveState). Colors auto-update when enum is changed.
public enum TileState { Empty, Filled, Blocked, Active } //active is the current piece that the player controls.
public enum ObjectiveState { Regular, Buffer, Required }
public class Tile : MonoBehaviour
{
    /// <summary>
    /// The state of this tile. It's not recommended to modify this directly. Instead, use BoardManager.SetTile()
    /// </summary>
    public TileState State
    {
        get { return state; }
        set
        {
            state = value;
            allowChanges = state != TileState.Blocked;
            if (state == TileState.Blocked)
            {
                tileFilling.color = new Color(30f / 255f, 30f / 255f, 30f / 255f);
                return;
            }
            if (state == TileState.Empty)
            {
                tileFilling.color = new Color() { a = 0 };
                return;
            }
            tileFilling.color = fillings[(int)Piece];
        }
    }
    TileState state;
    public Piece Piece { get { return piece; } set { piece = value; State = State; } }
    Piece piece;
    public ObjectiveState ObjState
    {
        get { return obj; }
        set
        {
            obj = value;
            border.sortingOrder = (int)obj + 1;
            border.color = borders[(int)obj];
        }
    }
    ObjectiveState obj;
    public bool Ghost
    {
        get { return ghost; }
        set
        {
            ghost = value;
            ghostPiece.gameObject.SetActive(ghost);
        }
    }
    bool ghost;
    public bool allowChanges;
    [SerializeField] SpriteRenderer border;
    [SerializeField] SpriteRenderer tileFilling;
    [SerializeField] SpriteRenderer ghostPiece;
    [SerializeField] Color[] borders;
    [SerializeField] Color[] fillings;
    
    void Awake()
    {
        allowChanges = true;
        Ghost = false;
    }
    
    void Update()
    {

    }
}
