using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTip : MonoBehaviour
{
    [SerializeField] Image tip;
    [SerializeField] Text tipText;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetTip(string text)
    {
        tipText.text = text;
        if(text == "")
        {
            tipText.text = "No tip available.";
        }
    }

    public void ToggleTip()
    {
        tip.gameObject.SetActive(!tip.gameObject.activeSelf);
    }
}
