using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class resetbutton : MonoBehaviour
{

    void Start()
    {

    }

    public void resetstage()
    {
        PlayerPrefs.SetInt("stageActivated", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("stage_select");
    }
    public void openstage()
    {
        PlayerPrefs.SetInt("stageActivated", 20);
        PlayerPrefs.Save();
        SceneManager.LoadScene("stage_select");
    }
}