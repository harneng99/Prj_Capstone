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
        // 현재 stageActivated 값을 가져와서 1 증가
        int currentStage = PlayerPrefs.GetInt("stageActivated", 1);
        PlayerPrefs.SetInt("stageActivated", currentStage + 1);

        // 변경된 값을 저장
        PlayerPrefs.Save();

        // 스테이지 선택 화면으로 이동
        SceneManager.LoadScene("stage_select");
    }
}