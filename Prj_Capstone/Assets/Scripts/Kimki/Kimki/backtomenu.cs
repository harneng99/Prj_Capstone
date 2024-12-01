using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class backtomenu : MonoBehaviour
{
    public int current_stage;

    public void backmenu()
    {
        // current_stage와 stageActivated를 비교하여 값 갱신
        if(PlayerPrefs.GetInt("stageclear", 1) == 0)
        {
            SceneManager.LoadScene("stage_select");
        }
        else
        {

            if (PlayerPrefs.GetInt("stageActivated", 1) <= current_stage)
            {
                PlayerPrefs.SetInt("stageActivated", current_stage + 1);
                PlayerPrefs.Save();
            }
            SceneManager.LoadScene("stage_select");
        }
    }
}