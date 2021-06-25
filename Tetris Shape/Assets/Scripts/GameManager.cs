using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject board;
    [SerializeField] GameObject pauseScreen;

    ObjectiveManager objectiveManager;
    PieceMovement pieceMovement;
    BoardManager boardManager;
    
    void Start()
    {
        objectiveManager = board.GetComponent<ObjectiveManager>();
        pieceMovement = board.GetComponent<PieceMovement>();
        boardManager = board.GetComponent<BoardManager>();
        pauseScreen.SetActive(false);
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
                break;
            case "Main Menu":
                SceneManager.LoadScene("Level Select");
                break;
            case "Pause":
                pauseScreen.SetActive(true);
                pieceMovement.enabled = false;
                break;
            case "Restart":
                objectiveManager.Restart();
                goto case "Resume";
            case "Resume":
                pauseScreen.SetActive(false);
                pieceMovement.enabled = true;
                break;
        }
    }
}
