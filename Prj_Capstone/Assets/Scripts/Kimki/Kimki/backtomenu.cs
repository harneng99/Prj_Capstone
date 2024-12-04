using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class backtomenu : MonoBehaviour
{
    private int current_stage;

    private void Awake()
    {
        current_stage = int.Parse(SceneManager.GetActiveScene().name.Replace("Stage", ""));
    }

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
        PlayerPrefs.SetInt("stageclear", 0);
    }
}