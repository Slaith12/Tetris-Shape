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
    public TileState state;
    public Piece piece;
    public ObjectiveState obj;
    public bool allowChanges;
    [SerializeField] SpriteRenderer border;
    [SerializeField] SpriteRenderer tileFilling;
    [SerializeField] Color[] borders;
    [SerializeField] Color[] fillings;
    
    void Start()
    {
        allowChanges = true;
    }
    
    void Update()
    {
        border.sortingOrder = (int)obj + 1;
        allowChanges = state != TileState.Blocked;
        border.color = borders[(int)obj];
        if(state == TileState.Blocked)
        {
            tileFilling.color = new Color(30f / 255f, 30f / 255f, 30f / 255f);
            return;
        }
        if(state == TileState.Empty)
        {
            tileFilling.color = new Color() { a = 0 };
            return;
        }
        tileFilling.color = fillings[(int)piece];
    }
}
