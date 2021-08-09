using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject board;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject menuWarning;
    [SerializeField] AudioSource music;

    ObjectiveManager objectiveManager;
    PieceMovement pieceMovement;
    BoardManager boardManager;

    bool paused;
    [HideInInspector] public bool disablePausing;

    void Start()
    {
        objectiveManager = board.GetComponent<ObjectiveManager>();
        pieceMovement = board.GetComponent<PieceMovement>();
        boardManager = board.GetComponent<BoardManager>();
        pauseScreen.SetActive(false);
        menuWarning.SetActive(false);
        paused = false;
        disablePausing = false;
        if (!PlayerPrefs.HasKey("Volume"))
        {
            PlayerPrefs.SetFloat("Volume", 1f);
        }
        music.volume = PlayerPrefs.GetFloat("Volume");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void HandleButton(string message)
    {
        switch (message)
        {
            case "Next Level":
                if (!objectiveManager.GoToNextLevel())
                {
                    GoToLevelSelect();
                }
                break;
            case "Main Menu":
                Resume();
                ShowMenuWarning();
                break;
            case "Pause":
                Pause();
                break;
            case "Restart":
                Resume();
                objectiveManager.ShowRestartWarning();
                break;
            case "Resume":
                Resume();
                break;
        }
    }

    public void ShowMenuWarning()
    {
        pieceMovement.enabled = false;
        disablePausing = true;
        menuWarning.SetActive(true);
    }

    public void CloseMenuWarning()
    {
        pieceMovement.enabled = true;
        disablePausing = false;
        menuWarning.SetActive(false);
    }

    public void GoToLevelSelect()
    {
        PlayerPrefs.SetInt("Skip Tutorial", 0);
        SceneManager.LoadScene("Level Select");
    }

    public void Pause()
    {
        if (disablePausing)
            return;
        if (paused)
        {
            Resume();
            return;
        }
        paused = true;
        pauseScreen.SetActive(true);
        pieceMovement.enabled = false;
    }

    public void Resume()
    {
        if (!paused)
        {
            Pause();
        }
        paused = false;
        pauseScreen.SetActive(false);
        pieceMovement.enabled = true;
    } 
}
