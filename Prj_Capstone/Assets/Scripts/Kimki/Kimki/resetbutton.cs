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
        int currentStage = PlayerPrefs.GetInt("stageActivated", 1);
        PlayerPrefs.SetInt("stageActivated", currentStage + 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("stage_select");
    }
}