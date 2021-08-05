using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] GameObject level;
    [SerializeField] RectTransform levelContainer;
    [SerializeField] GameObject congratulation;

    void Start()
    {
        if(!PlayerPrefs.HasKey("Unlocked Levels"))
        {
            PlayerPrefs.SetInt("Unlocked Levels", 0);
        }
        for (int i = 0; i <= Mathf.Min(PlayerPrefs.GetInt("Unlocked Levels"), 15); i++)
        {
            RectTransform newLevel = (RectTransform)Instantiate(level, levelContainer).transform;
            newLevel.anchoredPosition = new Vector2(((i % 4) - 1.5f) * 200, ((i / 4) - 2f) * -100f);
            int placeholder = i;
            newLevel.GetComponent<Button>().onClick.AddListener( () => { LoadLevel(placeholder); }); //apparently, it doesn't consider what the number in the function is until it is called, so it was always called with i = 20.
            newLevel.GetComponentInChildren<Text>().text = "Level " + (i + 1);
        }
        if(PlayerPrefs.GetInt("Unlocked Levels") > 15)
        {
            for(int i = 0; i < 4; i++)
            {
                RectTransform newLevel = (RectTransform)Instantiate(level, levelContainer).transform;
                newLevel.anchoredPosition = new Vector2((i - 1.5f) * 200, -200f);
                int placeholder = 16 + i;
                newLevel.GetComponent<Button>().onClick.AddListener(() => { LoadLevel(placeholder); }); //apparently, it doesn't consider what the number in the function is until it is called, so it was always called with i = 20.
                newLevel.GetComponentInChildren<Text>().text = "Challenge " + (i + 1);
            }
        }
        if(PlayerPrefs.HasKey("Congratulation"))
        {
            congratulation.SetActive(true);
            PlayerPrefs.DeleteKey("Congratulation");
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

    public void ChangeLevel(string level)
    {
        for(int i = 0; i < levelContainer.childCount; i++)
        {
            Destroy(levelContainer.GetChild(0).gameObject);
        }
        PlayerPrefs.SetInt("Unlocked Levels", Mathf.Max(0, int.Parse(level) - 1));
        Start();
    }

    public void Quit()
    {
        SceneManager.LoadScene("Title Scene");
    }

    public void CloseCongrats()
    {
        congratulation.SetActive(false);
    }
}
