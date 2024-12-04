using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class UIManager : MonoBehaviour
{
    [field: Header("Counter UI")]
    [field: SerializeField] public GameObject turnCounter { get; private set; }
    [field: SerializeField] public GameObject goalCounter { get; private set; }
    [field: SerializeField] public GameObject enemyCounter { get; private set; }
    
    [field: Header("Pause Window")]
    [field: SerializeField] public GameObject gameResultWindow { get; private set; }
    [field: SerializeField] public GameObject gamePauseWindow { get; private set; }
    [field: SerializeField] public GameObject settingsWindow { get; private set; }
    [field: SerializeField] public Button gamePauseButton { get; private set; }


    [field: Header("Extra UI")]
    [field: SerializeField] public GameObject warningUI { get; private set; }
    [field: SerializeField] public GameObject phaseInformationUI { get; private set; }
    [field: SerializeField] public Button endTurnButton { get; private set; }

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private RefreshRate currentRefreshRate;
    private int currentResolutionIndex;

    private void Awake()
    {
        gamePauseButton.onClick.AddListener(Manager.Instance.gameManager.PauseGame);
        gamePauseButton.onClick.AddListener(Manager.Instance.uiManager.ToggleGamePauseWindow);

        Button[] gamePauseWindowButtons = gamePauseWindow.GetComponentsInChildren<Button>();
        foreach (Button button in gamePauseWindowButtons)
        {
            switch (button.name)
            {
                case "Resume":
                    button.onClick.AddListener(Manager.Instance.gameManager.ResumeGame);
                    button.onClick.AddListener(Manager.Instance.uiManager.ToggleGamePauseWindow);
                    break;
                case "Restart":
                    button.onClick.AddListener(Manager.Instance.gameManager.RestartGame);
                    break;
                case "Settings":
                    button.onClick.AddListener(Manager.Instance.uiManager.ToggleGamePauseWindow);
                    button.onClick.AddListener(Manager.Instance.uiManager.ToggleSettingsWindow);
                    break;
                default:
                    Debug.LogWarning($"Unknown button name \"{button.name}\" detected in Game Pause Window.");
                    break;
            }
        }

        Button[] gameResultWindowButtons = gameResultWindow.GetComponentsInChildren<Button>();
        foreach (Button button in gameResultWindowButtons)
        {
            switch (button.name)
            {
                case "Resume":
                    button.onClick.AddListener(Manager.Instance.gameManager.ResumeGame);
                    button.onClick.AddListener(Manager.Instance.uiManager.ToggleGamePauseWindow);
                    break;
                case "Restart":
                    button.onClick.AddListener(Manager.Instance.gameManager.RestartGame);
                    break;
                default:
                    Debug.LogWarning($"Unknown button name \"{button.name}\" detected in Game Result Window.");
                    break;
            }
        }

        Slider[] settingsWindowSliders = settingsWindow.GetComponentsInChildren<Slider>();
        foreach (Slider slider in settingsWindowSliders)
        {
            switch (slider.name)
            {
                case "Master Volume":
                    slider.onValueChanged.AddListener(Manager.Instance.soundFXManager.SetMasterVolume);
                    break;
                case "BGM Volume":
                    slider.onValueChanged.AddListener(Manager.Instance.soundFXManager.SetBGMVolume);
                    break;
                case "SFX Volume":
                    slider.onValueChanged.AddListener(Manager.Instance.soundFXManager.SetSoundFXVolume);
                    break;
                default:
                    Debug.LogWarning($"Unknown slider name \"{slider.name}\" detected in Game Settings Window.");
                    break;
            }
        }
        settingsWindow.GetComponentInChildren<Button>().onClick.AddListener(Manager.Instance.uiManager.ToggleSettingsWindow);
        settingsWindow.GetComponentInChildren<Button>().onClick.AddListener(Manager.Instance.uiManager.ToggleGamePauseWindow);
        TMP_Dropdown settingsWindowDropdown = settingsWindow.GetComponentInChildren<TMP_Dropdown>();

        settingsWindowDropdown.ClearOptions();
        
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        currentRefreshRate = Screen.currentResolution.refreshRateRatio;

        foreach (Resolution resolution in resolutions)
        {
            if (resolution.refreshRateRatio.Equals(currentRefreshRate))
            {
                filteredResolutions.Add(resolution);
            }
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "¡¿" + filteredResolutions[i].height;
            options.Add(resolutionOption);

            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        settingsWindowDropdown.AddOptions(options);
        settingsWindowDropdown.value = currentResolutionIndex;
        settingsWindowDropdown.RefreshShownValue();

        settingsWindowDropdown.onValueChanged.AddListener((index) =>
        {
            Resolution selectedResolution = filteredResolutions[index];
            Manager.Instance.gameManager.SetResolution(selectedResolution.width, selectedResolution.height);
            Debug.Log($"Resolution set to {selectedResolution.width} ¡¿ {selectedResolution.height}");
        });

        endTurnButton.onClick.AddListener(Manager.Instance.gameManager.TurnEndButton);
    }

    public async void ShowWarningUI(string warningTextContent)
    {
        if (Manager.Instance.gameManager.pieceDeploymentPhase)
        {
            return;
        }
    
        warningUI.SetActive(true);

        Image warningUIBackground = warningUI.GetComponent<Image>();
        TMP_Text warningText = warningUI.GetComponentInChildren<TMP_Text>();
        warningUIBackground.color = new Color(0.0f, 0.0f, 0.0f, 0.8f);
        warningText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        warningText.text = warningTextContent;
        await Task.Delay(3000);

        warningUIBackground.DOFade(0.0f, 3.0f);
        warningText.DOFade(0.0f, 3.0f);
        await Task.Delay(3000);
        warningUI.SetActive(false);
    }

    public void ShowPhaseInformationUI()
    {
        if (Manager.Instance.gameManager.pieceDeploymentPhase)
        {
            phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Player Phase";
            phaseInformationUI.SetActive(true);
        }
        else if (Manager.Instance.gameManager.battlePhase)
        {
            if (Manager.Instance.gameManager.playerPhase)
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Player Phase";
            }
            else if (Manager.Instance.gameManager.enemyPhase)
            {
                phaseInformationUI.GetComponentInChildren<TMP_Text>().text = "Enemy Phase";
            }

            phaseInformationUI.SetActive(true);
        }
    }

    public void ShowGameResultWindow(string resultText)
    {
        Manager.Instance.gameManager.PauseGame();
        gameResultWindow.SetActive(true);
        gameResultWindow.GetComponentInChildren<TMP_Text>().text = resultText;
    }

    public void SetCombatAbilityButtons()
    {
        foreach (Entity entity in Manager.Instance.gameManager.entities)
        {
            foreach (GameObject combatAbilityButton in entity.entityCombat.combatAbilityButtons)
            {
                combatAbilityButton.SetActive(entity.isSelected);
            }
        }
    }

    public void ToggleGamePauseWindow()
    {
        if (!gameResultWindow.activeSelf)
        {
            gamePauseWindow.SetActive(!gamePauseWindow.activeSelf);
            gamePauseButton.interactable = !gamePauseWindow.activeSelf;
        }
    }

    public void ToggleSettingsWindow()
    {
        if (!gameResultWindow.activeSelf)
        {
            settingsWindow.SetActive(!settingsWindow.activeSelf);
            gamePauseButton.interactable = false;
        }
    }

    public void SetGoalCounter(bool activeSelf, string content)
    {
        goalCounter.SetActive(activeSelf);
        goalCounter.GetComponentInChildren<TMP_Text>().text = content;
    }
}
