using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Text fullscreenText;
    [SerializeField] Text timerText;
    [SerializeField] Slider volumeSlider;
    [SerializeField] GameObject settings;

    // Start is called before the first frame update
    void Start()
    {
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            fullscreenText.text = "Fullscreen: On";
        }
        if (PlayerPrefs.HasKey("Timer") && PlayerPrefs.GetInt("Timer") == 1)
        {
            timerText.text = "Timer Mode: On";
        }
        if (PlayerPrefs.HasKey("Volume"))
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");
        }
        settings.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene("Level Select");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void FullScreen()
    {
        if(Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            fullscreenText.text = "Fullscreen: Off";
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            fullscreenText.text = "Fullscreen: On";
        }
    }

    public void ToggleTimer()
    {
        if (PlayerPrefs.HasKey("Timer") && PlayerPrefs.GetInt("Timer") == 1)
        {
            PlayerPrefs.SetInt("Timer", 0);
            timerText.text = "Timer Mode: Off";
        }
        else
        {
            PlayerPrefs.SetInt("Timer", 1);
            timerText.text = "Timer Mode: On";
        }
    }

    public void ResetProgress()
    {
        float volume = 0.5f;
        if(PlayerPrefs.HasKey("Volume"))
        {
            volume = PlayerPrefs.GetFloat("Volume");
        }
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetFloat("Volume", volume);
        timerText.text = "Timer Mode: Off";
    }

    public void Settings(bool enable)
    {
        settings.SetActive(enable);
    }

    public void ChangeMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
