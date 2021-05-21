using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject board;

    ObjectiveManager objectiveManager;
    PieceMovement pieceMovement;
    BoardManager boardManager;
    
    void Start()
    {
        objectiveManager = board.GetComponent<ObjectiveManager>();
        pieceMovement = board.GetComponent<PieceMovement>();
        boardManager = board.GetComponent<BoardManager>();
    }
    
    void Update()
    {
        
    }

    public void HandleButton(string message)
    {
        switch(message)
        {
            case "Next Level":
                if(!objectiveManager.GoToNextLevel())
                {
                    goto case "Main Menu";
                }
                pieceMovement.enabled = true;
                pieceMovement.ResetPieces(20);
                break;
            case "Main Menu":
                SceneManager.LoadScene("Level Select");
                break;
                
        }
    }
}
