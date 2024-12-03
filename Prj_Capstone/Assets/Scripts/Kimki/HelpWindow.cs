using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpWindow : MonoBehaviour
{
    public GameObject[] helpPages; // Help1, Help2, Help3, Help4 이미지 배열
    public Button nextButton;      // 오른쪽 화살표 버튼
    public Button previousButton;  // 왼쪽 화살표 버튼
    public Button helpToggleButton; // ? 버튼

    private int currentPage = 1;   // 현재 페이지 (1부터 시작)
    private const int maxPage = 4; // 최대 페이지
    private const int minPage = 1; // 최소 페이지
    private bool isHelpVisible = false; // 도움말 표시 여부

    void Start()
    {
        // 초기 상태 설정
        UpdateHelpPages();
        nextButton.gameObject.SetActive(false);
        previousButton.gameObject.SetActive(false);

        // 버튼 클릭 이벤트 추가
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
        helpToggleButton.onClick.AddListener(ToggleHelpPanel);
    }

    public void NextPage()
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            UpdateHelpPages();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > minPage)
        {
            currentPage--;
            UpdateHelpPages();
        }
    }

    public void ToggleHelpPanel()
    {
        isHelpVisible = !isHelpVisible;

        // 도움말 관련 UI 활성화/비활성화
        foreach (GameObject page in helpPages)
        {
            page.SetActive(false);
        }
        nextButton.gameObject.SetActive(isHelpVisible);
        previousButton.gameObject.SetActive(isHelpVisible);

        if (isHelpVisible)
        {
            UpdateHelpPages();
        }
    }

    private void UpdateHelpPages()
    {
        // 모든 페이지 비활성화
        foreach (GameObject page in helpPages)
        {
            page.SetActive(false);
        }

        // 현재 페이지 활성화
        helpPages[currentPage - 1].SetActive(true);

        // 화살표 버튼 활성화/비활성화
        nextButton.interactable = currentPage < maxPage;
        previousButton.interactable = currentPage > minPage;
    }
}
