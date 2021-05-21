using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    void Start()
    {
        if(!PlayerPrefs.HasKey("Unlocked Levels"))
        {
            PlayerPrefs.SetInt("Unlocked Levels", 0);
        }
    }
    
    void Update()
    {
        
    }

    public void LoadLevel(int level)
    {
        PlayerPrefs.SetInt("Current Level", level);
        SceneManager.LoadScene("Board");
    }
}
