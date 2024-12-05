using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // TextMeshProUGUI를 사용하기 위해 추가

public class StageSelector : MonoBehaviour
{
    public int stage; // 현재 스테이지 번호
    private int stageActivated; // 활성화된 최대 스테이지
    private Text textComponent; // 일반 UI Text 컴포넌트
    private TextMeshProUGUI tmpTextComponent; // TextMeshProUGUI 컴포넌트

    void Start()
    {
        // 초기화: stageActivated 기본값을 1로 설정
        if (!PlayerPrefs.HasKey("stageActivated"))
        {
            PlayerPrefs.SetInt("stageActivated", 1);
            PlayerPrefs.Save();
        }

        // stageActivated 값을 가져옴
        stageActivated = PlayerPrefs.GetInt("stageActivated", 1);

        // 텍스트 컴포넌트 가져오기 (Text 또는 TextMeshProUGUI)
        textComponent = GetComponentInChildren<Text>();
        tmpTextComponent = GetComponentInChildren<TextMeshProUGUI>();

        // 현재 stage와 stageActivated를 비교하여 버튼 활성화 및 텍스트 색상 변경
        if (stage <= stageActivated)
        {
            GetComponent<Button>().interactable = true; // 버튼 활성화
            if (textComponent != null)
            {
                textComponent.color = Color.white; // 일반 Text 색상 변경
            }
            else if (tmpTextComponent != null)
            {
                tmpTextComponent.color = Color.white; // TextMeshProUGUI 색상 변경
            }
        }
        else
        {
            GetComponent<Button>().interactable = false; // 버튼 비활성화
            if (textComponent != null)
            {
                textComponent.color = Color.gray; // 일반 Text 색상 변경
            }
            else if (tmpTextComponent != null)
            {
                tmpTextComponent.color = Color.gray; // TextMeshProUGUI 색상 변경
            }
        }
    }

    public void OpenScene()
    {
        // 활성화된 스테이지라면 해당 씬으로 이동
        if (stage <= stageActivated)
        {
            SceneManager.LoadScene("Stage " + stage.ToString());
        }
    }
}